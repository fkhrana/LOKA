using System.Collections.Generic;
using UnityEngine;

// Menggantikan CollectManager lama.
// Carousel menampilkan SEMUA huruf aksara yang ada (dari allAksaraData),
// lalu tiap slot dicek statusnya ke CollectedAksaraManager.Instance.IsCollected(data)
// untuk menentukan tampilannya (kuning = sudah kekumpul, gelap = masih terkunci).
public class AksaraCarouselUI : MonoBehaviour
{
    [Header("Data Huruf")]
    [Tooltip("Semua AksaraData yang ada di game, urutannya sesuai urutan tampil di panel Koleksi.")]
    [SerializeField] private List<AksaraData> allAksaraData;

    [Header("Carousel Setup")]
    [SerializeField] private Transform contentParent;      // parent dengan Horizontal/Grid Layout Group
    [SerializeField] private AksaraCarouselItemUI itemPrefab;

    [Header("Popup")]
    [SerializeField] private AksaraCardPopup cardPopup;

    private void OnEnable()
    {
        // Refresh tiap kali UI buku dibuka, supaya status huruf terbaru langsung muncul
        RefreshCarousel();
    }

    public void RefreshCarousel()
    {
        if (contentParent == null || itemPrefab == null)
        {
            Debug.LogWarning("[AksaraCarouselUI] contentParent atau itemPrefab belum di-assign.");
            return;
        }

        // Bersihkan slot lama sebelum di-render ulang
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (allAksaraData == null || allAksaraData.Count == 0)
        {
            Debug.LogWarning("[AksaraCarouselUI] allAksaraData masih kosong, isi dulu di Inspector.");
            return;
        }

        bool managerReady = CollectedAksaraManager.Instance != null;
        if (!managerReady)
            Debug.LogWarning("[AksaraCarouselUI] CollectedAksaraManager belum ada di scene, semua slot dianggap terkunci.");

        foreach (AksaraData data in allAksaraData)
        {
            if (data == null)
                continue;

            bool isCollected = managerReady && CollectedAksaraManager.Instance.IsCollected(data);

            AksaraCarouselItemUI item = Instantiate(itemPrefab, contentParent);
            item.Setup(data, this, isCollected);
        }
    }

    // Dipanggil oleh AksaraCarouselItemUI saat slot yang sudah kekumpul diklik
    public void OnItemSelected(AksaraData data)
    {
        if (cardPopup != null)
        {
            cardPopup.Show(data);
        }
    }
}