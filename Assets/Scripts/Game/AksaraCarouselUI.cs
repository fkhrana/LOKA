using System.Collections.Generic;
using UnityEngine;

public class AksaraCarouselUI : MonoBehaviour
{
    [Header("Data Huruf")] [SerializeField] private List<AksaraData> allAksaraData;
    [Header("Carousel Setup")] [SerializeField] private Transform contentParent;
    [SerializeField] private AksaraCarouselItemUI itemPrefab;
    [Header("Popup")] [SerializeField] private AksaraCardPopup cardPopup;

    private void OnEnable() => RefreshCarousel();

    public void RefreshCarousel()
    {
        if (contentParent == null || itemPrefab == null)
        {
            Debug.LogWarning("[AksaraCarouselUI] contentParent atau itemPrefab belum di-assign.");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (allAksaraData == null || allAksaraData.Count == 0)
        {
            Debug.LogWarning("[AksaraCarouselUI] allAksaraData masih kosong.");
            return;
        }

        foreach (AksaraData data in allAksaraData)
        {
            if (data == null) continue;

            // 🔥 Baca dari permanent (bukan dari CollectedAksaraManager)
            bool isCollected = PermanentCollectionManager.IsCollected(data);

            AksaraCarouselItemUI item = Instantiate(itemPrefab, contentParent);
            item.Setup(data, this, isCollected);
        }
    }

    public void OnItemSelected(AksaraData data)
    {
        if (cardPopup != null)
            cardPopup.Show(data);
    }
}