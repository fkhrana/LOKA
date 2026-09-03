using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a horizontal scrollable carousel of Aksara cards.
/// - Builds cards from data on enable.
/// - Scales cards based on distance to viewport center.
/// - Snaps to the nearest card with smooth lerp.
/// - Handles card selection (sound feedback, bounce effect, snap).
/// - Applies centering padding so first/last cards can reach viewport center.
/// </summary>
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
    [SerializeField] private AksaraSoundLibrary soundLibrary;

    private readonly List<AksaraCarouselItemUI> spawnedItems = new();
    private bool isSnapping;
    private float snapTargetNormalized;

    private void Awake()
    {
        if (leftArrowButton) leftArrowButton.onClick.AddListener(() => SnapStep(-1));
        if (rightArrowButton) rightArrowButton.onClick.AddListener(() => SnapStep(1));
    }

    private void OnEnable() => BuildList();

    private void BuildList()
    {
        if (content == null || itemPrefab == null || allAksaraData == null)
        {
            Debug.LogWarning("[AksaraCarouselUI] Missing references.");
            return;
        }

        foreach (Transform child in content)
            Destroy(child.gameObject);
        spawnedItems.Clear();

        foreach (var data in allAksaraData)
        {
            if (data == null) continue;
            bool collected = PermanentCollectionManager.IsCollected(data);
            var item = Instantiate(itemPrefab, content);
            item.Setup(data, this, collected);
            spawnedItems.Add(item);
        }

        ApplyCenteringPadding();
    }

    // Menambahkan padding kiri-kanan di Content, supaya card pertama & terakhir
    // tetap bisa digeser sampai benar-benar di tengah viewport.
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

        if (isSnapping && scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition,
                snapTargetNormalized,
                Time.unscaledDeltaTime * snapLerpSpeed
            );

            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - snapTargetNormalized) < 0.001f)
            {
                scrollRect.horizontalNormalizedPosition = snapTargetNormalized;
                isSnapping = false;
            }
        }
    }

    private void UpdateScales()
    {
        if (viewport == null || spawnedItems.Count == 0) return;

        float viewportCenterX = viewport.rect.center.x;

        foreach (var item in spawnedItems)
        {
            Vector3 localPos = viewport.InverseTransformPoint(item.RectTransform.position);
            float distance = Mathf.Abs(localPos.x - viewportCenterX);
            float t = Mathf.Clamp01(distance / scaleFalloffDistance);
            float scale = Mathf.Lerp(centerScale, edgeScale, t);
            item.SetScale(scale);
        }
    }

    private AksaraCarouselItemUI GetNearestCenterItem()
    {
        if (viewport == null || spawnedItems.Count == 0) return null;

        float viewportCenterX = viewport.rect.center.x;
        AksaraCarouselItemUI nearest = null;
        float minDist = float.MaxValue;

        foreach (var item in spawnedItems)
        {
            Vector3 localPos = viewport.InverseTransformPoint(item.RectTransform.position);
            float dist = Mathf.Abs(localPos.x - viewportCenterX);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = item;
            }
        }
        return nearest;
    }

    private void SnapStep(int direction)
    {
        var current = GetNearestCenterItem();
        if (current == null) return;

        int index = spawnedItems.IndexOf(current);
        int targetIndex = Mathf.Clamp(index + direction, 0, spawnedItems.Count - 1);
        ScrollToItem(spawnedItems[targetIndex]);
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

    public void OnItemSelected(AksaraCarouselItemUI item)
    {
        if (!PermanentCollectionManager.IsCollected(item.Data))
        {
            Debug.Log($"[AksaraCarouselUI] {item.Data.name} locked.");
            return;
        }

        if (soundLibrary != null)
        {
            AudioClip clip = soundLibrary.GetClip(item.Data.GestureShape);
            if (clip != null)
                AudioManager.Instance?.PlayUISFX(clip);
        }

        item.PlayBounceEffect();

        var centerItem = GetNearestCenterItem();
        if (item != centerItem)
            ScrollToItem(item);
    }
}