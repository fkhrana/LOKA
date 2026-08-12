using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelUI : MonoBehaviour, IPointerClickHandler
{
    // === Komponen UI ===
    public Image levelIconImage;     // Gambar angka/bintang level
    public Image backgroundImage;    // Background card (opsional, untuk warna kuning/abu)
    public GameObject lockOverlay;   // Overlay gelap (aktif jika locked)
    public GameObject lockIcon;      // Gambar gembok (aktif jika locked)

    // === Warna (hanya dipakai jika backgroundImage diisi) ===
    public Color unlockedColor = new Color(1f, 0.84f, 0f); // Kuning
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f); // Abu-abu

    private LevelData currentData;

    // Dipanggil oleh LevelManager untuk mengisi data
    public void Setup(LevelData data)
    {
        currentData = data;

        // Set icon level
        if (data.levelIcon != null && levelIconImage != null)
            levelIconImage.sprite = data.levelIcon;

        // Atur warna background (jika ada)
        if (backgroundImage != null)
        {
            backgroundImage.color = data.isUnlocked ? unlockedColor : lockedColor;
        }

        // Tampilkan/sembunyikan lock
        bool isLocked = !data.isUnlocked;
        if (lockOverlay != null) lockOverlay.SetActive(isLocked);
        if (lockIcon != null) lockIcon.SetActive(isLocked);
    }

    // Saat card diklik (karena IPointerClickHandler)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentData == null || !currentData.isUnlocked) return;

        // Langsung main (tanpa popup)
        PlayLevel();
    }

    // Fungsi untuk memulai level
    void PlayLevel()
    {
        Debug.Log("Memulai " + currentData.levelName);
        // Ganti scene atau panggil sistem game
        // SceneManager.LoadScene("Level_" + currentData.levelIndex);
    }
}