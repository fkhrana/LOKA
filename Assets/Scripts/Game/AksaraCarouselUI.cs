using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AksaraCarouselUI : MonoBehaviour
{
    [Header("Data Huruf")]
    [SerializeField] private List<AksaraData> allAksaraData;

    [Header("Scroll Setup")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private AksaraCarouselItemUI itemPrefab;

    [Header("Navigasi (opsional, buat snap 1 langkah)")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    [Header("Scaling berdasar jarak ke tengah")]
    [SerializeField] private float centerScale = 1.15f;
    [SerializeField] private float edgeScale = 0.85f;
    [SerializeField] private float scaleFalloffDistance = 300f; // px, jarak sampai scale minimum tercapai

    [Header("Snap")]
    [SerializeField] private float snapLerpSpeed = 10f;

    [Header("Popup Detail")]
    [SerializeField] private AksaraCardPopup cardPopup;

    private readonly List<AksaraCarouselItemUI> spawnedItems = new List<AksaraCarouselItemUI>();
    private bool isSnapping;
    private float snapTargetNormalized;

    private void Awake()
    {
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(() => SnapStep(-1));
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(() => SnapStep(1));
    }

    private void OnEnable() => BuildList();

    private void BuildList()
    {
        if (content == null || itemPrefab == null || allAksaraData == null)
        {
            Debug.LogWarning("[AksaraCarouselUI] content/itemPrefab/allAksaraData belum lengkap.");
            return;
        }

        foreach (Transform child in content)
            Destroy(child.gameObject);
        spawnedItems.Clear();

        foreach (AksaraData data in allAksaraData)
        {
            if (data == null) continue;
            bool isCollected = PermanentCollectionManager.IsCollected(data);
            AksaraCarouselItemUI item = Instantiate(itemPrefab, content);
            item.Setup(data, this, isCollected);
            spawnedItems.Add(item);
        }
    }

    private void Update()
    {
        UpdateScales();

        if (isSnapping && scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition, snapTargetNormalized, Time.unscaledDeltaTime * snapLerpSpeed);

            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - snapTargetNormalized) < 0.001f)
            {
                scrollRect.horizontalNormalizedPosition = snapTargetNormalized;
                isSnapping = false;
            }
        }
    }

    // Scale tiap card berdasar seberapa dekat dia ke tengah viewport.
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
        float minDistance = float.MaxValue;

        foreach (var item in spawnedItems)
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

    private void SnapStep(int direction)
    {
        AksaraCarouselItemUI current = GetNearestCenterItem();
        if (current == null) return;

        int index = spawnedItems.IndexOf(current);
        int targetIndex = Mathf.Clamp(index + direction, 0, spawnedItems.Count - 1);
        ScrollToItem(spawnedItems[targetIndex]);
    }

    private void ScrollToItem(AksaraCarouselItemUI item)
{
    if (scrollRect == null || content == null || viewport == null) return;

    Canvas.ForceUpdateCanvases(); // <-- fix di sini

    float contentWidth = content.rect.width;
    float viewportWidth = viewport.rect.width;
    if (contentWidth <= viewportWidth) return; // gak ada yang perlu di-scroll

    RectTransform itemRect = item.RectTransform;
    float itemCenterX = itemRect.anchoredPosition.x + itemRect.rect.width * (0.5f - itemRect.pivot.x);

    float targetX = Mathf.Clamp(itemCenterX - viewportWidth * 0.5f, 0f, contentWidth - viewportWidth);
    snapTargetNormalized = targetX / (contentWidth - viewportWidth);
    isSnapping = true;
}

    // Dipanggil AksaraCarouselItemUI saat card yang sudah kekumpul diklik.
    public void OnItemSelected(AksaraCarouselItemUI item)
    {
        AksaraCarouselItemUI nearestCenter = GetNearestCenterItem();

        if (item == nearestCenter)
        {
            if (cardPopup != null) cardPopup.Show(item.Data);
        }
        else
        {
            ScrollToItem(item);
        }
    }
}