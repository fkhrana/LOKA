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
    [SerializeField] private AksaraSoundLibrary soundLibrary;

    private readonly List<AksaraCarouselItemUI> spawnedItems = new();

    private bool isSnapping;
    private float snapTargetNormalized;

    private void Awake()
    {
        if (leftArrowButton)
            leftArrowButton.onClick.AddListener(() => SnapStep(-1));

        if (rightArrowButton)
            rightArrowButton.onClick.AddListener(() => SnapStep(1));
    }

    private void OnEnable()
    {
        BuildList();
        UpdateButtons();
    }

    private void BuildList()
    {
        if (content == null || itemPrefab == null || allAksaraData == null)
        {
            Debug.LogWarning("[AksaraCarouselUI] Missing references.");
            return;
        }

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        spawnedItems.Clear();

        foreach (AksaraData data in allAksaraData)
        {
            if (data == null)
                continue;

            bool collected = PermanentCollectionManager.IsCollected(data);

            AksaraCarouselItemUI item =
                Instantiate(itemPrefab, content);

            item.Setup(data, this, collected);

            spawnedItems.Add(item);
        }

        ApplyCenteringPadding();
    }

    // Menambahkan padding agar item pertama dan terakhir bisa berada di tengah
    private void ApplyCenteringPadding()
    {
        if (
            contentLayoutGroup == null ||
            viewport == null ||
            itemPrefab == null
        )
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        float viewportWidth = viewport.rect.width;

        float itemWidth =
            ((RectTransform)itemPrefab.transform).rect.width;

        int padding = Mathf.Max(
            0,
            Mathf.RoundToInt((viewportWidth - itemWidth) * 0.5f)
        );

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

            if (
                Mathf.Abs(
                    scrollRect.horizontalNormalizedPosition -
                    snapTargetNormalized
                ) < 0.001f
            )
            {
                scrollRect.horizontalNormalizedPosition =
                    snapTargetNormalized;

                isSnapping = false;

                UpdateButtons();
            }
        }
    }

    private void UpdateScales()
    {
        if (viewport == null || spawnedItems.Count == 0)
            return;

        float viewportCenterX = viewport.rect.center.x;

        foreach (AksaraCarouselItemUI item in spawnedItems)
        {
            Vector3 localPos =
                viewport.InverseTransformPoint(
                    item.RectTransform.position
                );

            float distance =
                Mathf.Abs(localPos.x - viewportCenterX);

            float t =
                Mathf.Clamp01(
                    distance / scaleFalloffDistance
                );

            float scale =
                Mathf.Lerp(centerScale, edgeScale, t);

            item.SetScale(scale);
        }
    }

    private AksaraCarouselItemUI GetNearestCenterItem()
    {
        if (viewport == null || spawnedItems.Count == 0)
            return null;

        float viewportCenterX = viewport.rect.center.x;

        AksaraCarouselItemUI nearest = null;
        float minDistance = float.MaxValue;

        foreach (AksaraCarouselItemUI item in spawnedItems)
        {
            Vector3 localPos =
                viewport.InverseTransformPoint(
                    item.RectTransform.position
                );

            float distance =
                Mathf.Abs(localPos.x - viewportCenterX);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = item;
            }
        }

        return nearest;
    }

    private void SnapStep(int direction)
    {
        AksaraCarouselItemUI current =
            GetNearestCenterItem();

        if (current == null)
            return;

        int index = spawnedItems.IndexOf(current);

        int targetIndex = Mathf.Clamp(
            index + direction,
            0,
            spawnedItems.Count - 1
        );

        ScrollToItem(spawnedItems[targetIndex]);
    }

    private void ScrollToItem(AksaraCarouselItemUI item)
    {
        if (
            scrollRect == null ||
            content == null ||
            viewport == null
        )
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        if (contentWidth <= viewportWidth)
            return;

        RectTransform itemRect = item.RectTransform;

        float itemCenterX =
            itemRect.anchoredPosition.x +
            itemRect.rect.width *
            (0.5f - itemRect.pivot.x);

        float targetX = Mathf.Clamp(
            itemCenterX - viewportWidth * 0.5f,
            0f,
            contentWidth - viewportWidth
        );

        snapTargetNormalized =
            targetX / (contentWidth - viewportWidth);

        isSnapping = true;
    }

    public void OnItemSelected(AksaraCarouselItemUI item)
    {
        if (!PermanentCollectionManager.IsCollected(item.Data))
        {
            Debug.Log(
                $"[AksaraCarouselUI] {item.Data.name} locked."
            );

            return;
        }

        if (soundLibrary != null)
        {
            AudioClip clip =
                soundLibrary.GetClip(
                    item.Data.GestureShape
                );

            if (clip != null)
                AudioManager.Instance?.PlayUISFX(clip);
        }

        item.PlayBounceEffect();

        AksaraCarouselItemUI centerItem =
            GetNearestCenterItem();

        if (item != centerItem)
        {
            ScrollToItem(item);
        }
    }

    // Mengupdate status tombol navigasi
    private void UpdateButtons()
    {
        if (spawnedItems.Count == 0)
            return;

        int currentIndex =
            spawnedItems.IndexOf(
                GetNearestCenterItem()
            );

        if (currentIndex < 0)
            return;

        if (leftArrowButton)
            leftArrowButton.interactable = currentIndex > 0;

        if (rightArrowButton)
        {
            rightArrowButton.interactable =
                currentIndex < spawnedItems.Count - 1;
        }
    }

    // Dipanggil saat user selesai melakukan drag
    public void OnEndDrag()
    {
        UpdateButtons();
    }
}