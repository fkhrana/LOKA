using UnityEngine;
using UnityEngine.UI;

// Ditempel di prefab satu slot carousel.
// Menampilkan huruf dalam dua kondisi: kekumpul (kuning, bisa diklik)
// atau terkunci (gelap, tidak bisa diklik) — sesuai desain panel Koleksi.
[RequireComponent(typeof(Button))]
public class AksaraCarouselItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;

    [Header("Warna Status")]
    [SerializeField] private Color collectedColor = new Color(1f, 0.85f, 0.1f, 1f);   // kuning
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.22f, 0.15f, 1f);   // gelap/coklat tua

    private AksaraData aksaraData;
    private AksaraCarouselUI parentCarousel;
    private Button button;
    private bool isCollected;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void Setup(AksaraData data, AksaraCarouselUI carousel, bool collected)
    {
        aksaraData = data;
        parentCarousel = carousel;
        isCollected = collected;

        if (iconImage != null && data != null)
        {
            // Sprite huruf tetap ditampilkan walau belum kekumpul (bentuknya kelihatan, cuma gelap)
            iconImage.sprite = data.IconSprite;
            iconImage.enabled = data.IconSprite != null;
            iconImage.color = isCollected ? collectedColor : lockedColor;
        }

        // Huruf yang belum kekumpul tidak bisa dibuka popup-nya
        if (button != null)
            button.interactable = isCollected;
    }

    private void OnClick()
    {
        if (isCollected && aksaraData != null && parentCarousel != null)
        {
            parentCarousel.OnItemSelected(aksaraData);
        }
    }
}