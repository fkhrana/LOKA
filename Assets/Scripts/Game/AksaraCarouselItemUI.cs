using UnityEngine;
using UnityEngine.UI;

// Ditempel di prefab "AksaraCard" yang di-instantiate ke Content (ScrollRect horizontal).
// Scale-nya diatur dari luar oleh AksaraCarouselUI tiap frame (makin dekat tengah, makin besar).
[RequireComponent(typeof(Button))]
public class AksaraCarouselItemUI : MonoBehaviour
{
    [Header("Card Visual")]
    [SerializeField] private Image cardBackground;        // sprite dari data.FragmentSprite/IconSprite
    [SerializeField] private GameObject lockIcon;

    [Header("Audio")]
    [SerializeField] private Button soundButton;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AksaraSoundLibrary soundLibrary;

    [Header("Warna Status")]
    [SerializeField] private Color collectedTint = Color.white;
    [SerializeField] private Color lockedTint = new Color(0.55f, 0.55f, 0.55f, 1f);

    private AksaraData aksaraData;
    private AksaraCarouselUI parentCarousel;
    private Button button;
    private bool isCollected;

    public RectTransform RectTransform => (RectTransform)transform;
    public AksaraData Data => aksaraData;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        if (soundButton != null) soundButton.onClick.AddListener(PlayLetterSound);
    }

    public void Setup(AksaraData data, AksaraCarouselUI carousel, bool collected)
    {
        aksaraData = data;
        parentCarousel = carousel;
        isCollected = collected;

        if (data == null) return;

        Sprite fragment = data.FragmentSprite != null ? data.FragmentSprite : data.IconSprite;
        if (cardBackground != null)
        {
            cardBackground.sprite = fragment;
            cardBackground.enabled = fragment != null;
            cardBackground.color = isCollected ? collectedTint : lockedTint;
        }

        if (lockIcon != null) lockIcon.SetActive(!isCollected);
        if (soundButton != null) soundButton.interactable = isCollected;
    }

    // Dipanggil AksaraCarouselUI tiap frame berdasar jarak ke tengah viewport.
    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    private void OnClick()
    {
        if (!isCollected || aksaraData == null || parentCarousel == null) return;
        parentCarousel.OnItemSelected(this);
    }

    private void PlayLetterSound()
    {
        if (!isCollected || aksaraData == null || audioSource == null || soundLibrary == null) return;
        AudioClip clip = soundLibrary.GetClip(aksaraData.GestureShape);
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}