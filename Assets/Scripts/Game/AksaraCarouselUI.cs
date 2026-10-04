using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AksaraCarouselUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<AksaraData> allAksaraData;

    [Header("Scroll Settings")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private AksaraCarouselItemUI itemPrefab;
    [SerializeField] private HorizontalLayoutGroup contentLayoutGroup;

    [Header("Navigation")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    [Header("Scaling")]
    [SerializeField] private float centerScale = 1.15f;
    [SerializeField] private float edgeScale = 0.85f;
    [SerializeField] private float scaleFalloffDistance = 300f;

    [Header("Snap")]
    [SerializeField] private float snapLerpSpeed = 10f;

    [Header("Audio")]
    [Tooltip("Satu-satunya sumber audio untuk semua item carousel.")]
    [SerializeField] private AksaraSoundLibrary soundLibrary;

    [Tooltip("Multiplier volume, sama seperti aksaraSFXVolume di BossEnemy. " +
             "Formula: aksaraSFXVolume × library volume × 8, clamp 0..10.")]
    [SerializeField, Range(0f, 2f)] private float aksaraSFXVolume = 1.5f;

    [Header("Gesture")]
    [SerializeField] private GestureDrawer gestureDrawer;

    private readonly List<AksaraCarouselItemUI> spawnedItems = new();
    public List<AksaraData> AllAksaraData => allAksaraData;

    private bool isSnapping;
    private float snapTargetNormalized;
    private int currentIndex = 0;

    private void Awake()
    {
        if (leftArrowButton) leftArrowButton.onClick.AddListener(() => SnapStep(-1));
        if (rightArrowButton) rightArrowButton.onClick.AddListener(() => SnapStep(1));
    }

    private void OnEnable()
    {
        BuildList();
        isSnapping = false;
        if (scrollRect != null)
            scrollRect.StopMovement();

        SetNavigationAvailable(spawnedItems.Count > 1);
        StartCoroutine(RefreshStateAfterLayout());
        DisableGestureInput();
    }

    private void OnDisable()
    {
        EnableGestureInput();
    }

    private IEnumerator RefreshStateAfterLayout()
    {
        // BuildList destroys old cards deferred, so wait until Unity has removed them
        // and the layout group has arranged the newly spawned cards.
        yield return null;

        if (content != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
        SyncIndexToNearestItem();
        SyncIndexToNearestItem();
    }

    private void SetNavigationAvailable(bool available)
    {
        if (leftArrowButton) leftArrowButton.interactable = available;
        if (rightArrowButton) rightArrowButton.interactable = available;
    }

    private void DisableGestureInput()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.ResetGestureInput();
            gestureDrawer.enabled = false;
        }
    }

    private void EnableGestureInput()
    {
        if (gestureDrawer != null) gestureDrawer.enabled = true;
    }

    private void BuildList()
    {
        if (content == null || itemPrefab == null || allAksaraData == null)
        {
            Debug.LogWarning("[AksaraCarouselUI] Missing references.");
            return;
        }

        foreach (Transform child in content) Destroy(child.gameObject);
        spawnedItems.Clear();

        foreach (AksaraData data in allAksaraData)
        {
            if (data == null) continue;

            bool collected = PermanentCollectionManager.IsCollected(data);
            AksaraCarouselItemUI item = Instantiate(itemPrefab, content);
            item.Setup(data, this, collected);
            spawnedItems.Add(item);
        }

        ApplyCenteringPadding();
        currentIndex = 0;
    }

    private void ApplyCenteringPadding()
    {
        if (contentLayoutGroup == null || viewport == null || itemPrefab == null) return;

        Canvas.ForceUpdateCanvases();

        float viewportWidth = viewport.rect.width;
        float itemWidth = ((RectTransform)itemPrefab.transform).rect.width;

        int padding = Mathf.Max(0, Mathf.RoundToInt((viewportWidth - itemWidth) * 0.5f));

        contentLayoutGroup.padding.left = padding;
        contentLayoutGroup.padding.right = padding;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void Update()
    {
        UpdateScales();

        if (!isSnapping || scrollRect == null) return;

        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            snapTargetNormalized,
            Time.unscaledDeltaTime * snapLerpSpeed);

        if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - snapTargetNormalized) < 0.001f)
        {
            scrollRect.horizontalNormalizedPosition = snapTargetNormalized;
            isSnapping = false;
            // Update tombol sudah dilakukan instant di SnapStep/OnEndDrag
        }
    }

    private void UpdateScales()
    {
        if (viewport == null || spawnedItems.Count == 0) return;

        float viewportCenterX = viewport.rect.center.x;

        foreach (AksaraCarouselItemUI item in spawnedItems)
        {
            Vector3 localPos = viewport.InverseTransformPoint(item.RectTransform.position);
            float distance = Mathf.Abs(localPos.x - viewportCenterX);
            float t = Mathf.Clamp01(distance / scaleFalloffDistance);
            item.SetScale(Mathf.Lerp(centerScale, edgeScale, t));
        }
    }

    private AksaraCarouselItemUI GetNearestCenterItem()
    {
        if (viewport == null || spawnedItems.Count == 0) return null;

        float viewportCenterX = viewport.rect.center.x;
        AksaraCarouselItemUI nearest = null;
        float minDistance = float.MaxValue;

        foreach (AksaraCarouselItemUI item in spawnedItems)
        {
            Vector3 localPos = viewport.InverseTransformPoint(item.RectTransform.position);
            float distance = Mathf.Abs(localPos.x - viewportCenterX);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = item;
            }
        }

        return nearest;
    }

    // ============================================================
    // NAVIGATION — state-driven (instant button update)
    // ============================================================

    private void SnapStep(int direction)
    {
        if (spawnedItems.Count == 0) return;

        SyncIndexToNearestItem();
        int targetIndex = Mathf.Clamp(currentIndex + direction, 0, spawnedItems.Count - 1);
        if (targetIndex == currentIndex) return;

        currentIndex = targetIndex;
        ScrollToItem(spawnedItems[currentIndex]);

        // Update tombol INSTANT — tidak tunggu animasi selesai
        UpdateButtonsByIndex(currentIndex);
    }

    private void ScrollToItem(AksaraCarouselItemUI item)
    {
        if (scrollRect == null || content == null || viewport == null) return;

        Canvas.ForceUpdateCanvases();

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        if (contentWidth <= viewportWidth) return;

        RectTransform itemRect = item.RectTransform;
        float itemCenterX = itemRect.anchoredPosition.x + itemRect.rect.width * (0.5f - itemRect.pivot.x);

        float targetX = Mathf.Clamp(itemCenterX - viewportWidth * 0.5f, 0f, contentWidth - viewportWidth);
        snapTargetNormalized = targetX / (contentWidth - viewportWidth);
        isSnapping = true;
    }

    private void UpdateButtonsByIndex(int index)
    {
        if (leftArrowButton) leftArrowButton.interactable = index > 0;
        if (rightArrowButton) rightArrowButton.interactable = index < spawnedItems.Count - 1;
    }

    private void SyncIndexToNearestItem()
    {
        AksaraCarouselItemUI nearest = GetNearestCenterItem();
        if (nearest == null) return;

        currentIndex = spawnedItems.IndexOf(nearest);
        UpdateButtonsByIndex(currentIndex);
    }

    // Dipanggil dari event ScrollRect.OnEndDrag di Inspector
    public void OnEndDrag()
    {
        SyncIndexToNearestItem();
    }

    // ============================================================
    // AUDIO — single source of truth
    // ============================================================

    /// <summary>
    /// Klik kartu: play audio + bounce + snap ke tengah.
    /// </summary>
    public void OnItemSelected(AksaraCarouselItemUI item)
    {
        if (item == null || item.Data == null) return;

        if (!PermanentCollectionManager.IsCollected(item.Data))
        {
            Debug.Log($"[AksaraCarouselUI] {item.Data.name} locked.");
            return;
        }

        PlayAksaraAudio(item.Data);

        item.PlayBounceEffect();

        // Sync currentIndex dengan kartu yang diklik
        int clickedIndex = spawnedItems.IndexOf(item);
        if (clickedIndex >= 0)
        {
            currentIndex = clickedIndex;
            UpdateButtonsByIndex(currentIndex);
        }

        AksaraCarouselItemUI centerItem = GetNearestCenterItem();
        if (item != centerItem) ScrollToItem(item);
    }

    /// <summary>
    /// Klik tombol sound: play audio saja (tanpa bounce/snap).
    /// </summary>
    public void OnSoundButtonClicked(AksaraCarouselItemUI item)
    {
        if (item == null || item.Data == null) return;
        if (!PermanentCollectionManager.IsCollected(item.Data)) return;

        PlayAksaraAudio(item.Data);
    }

    /// <summary>
    /// Helper audio terpusat — SAMA PERSIS dengan BossEnemy.PlayAksaraSFX().
    /// </summary>
    private void PlayAksaraAudio(AksaraData data)
    {
        if (soundLibrary == null || data == null) return;
        if (AudioManager.Instance == null) return;

        AudioClip clip = soundLibrary.GetClip(data.GestureShape);
        if (clip == null) return;

        float libraryVolume = soundLibrary.GetVolume(data.GestureShape);
        float finalVolume = Mathf.Clamp(aksaraSFXVolume * libraryVolume * 8f, 0f, 10f);

        AudioManager.Instance.PlayLoudSFX(clip, finalVolume);
    }
}