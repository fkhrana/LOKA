using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AksaraCarouselItemUI : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private AksaraCardVisualLibrary cardVisualLibrary;

    [Header("Audio")]
    [Tooltip("Tombol terpisah untuk memutar suara aksara. " +
             "Event-nya di-forward ke AksaraCarouselUI.OnSoundButtonClicked().")]
    [SerializeField] private Button soundButton;

    [Header("Colors")]
    [SerializeField] private Color collectedTint = Color.white;
    [SerializeField] private Color lockedTint = new Color(0.55f, 0.55f, 0.55f, 1f);

    private AksaraData data;
    private AksaraCarouselUI carousel;
    private bool isCollected;
    private Button mainButton;
    private bool isAnimating = false;

    public RectTransform RectTransform => (RectTransform)transform;
    public AksaraData Data => data;

    private void Awake()
    {
        mainButton = GetComponent<Button>();
        mainButton.onClick.AddListener(OnMainClicked);

        if (soundButton) soundButton.onClick.AddListener(OnSoundClicked);
    }

    public void Setup(AksaraData newData, AksaraCarouselUI parent, bool collected)
    {
        data = newData;
        carousel = parent;
        isCollected = collected;

        if (data == null) return;

        Sprite sprite = cardVisualLibrary != null
            ? cardVisualLibrary.GetCardSprite(data)
            : null;

        if (cardBackground)
        {
            cardBackground.sprite = sprite;
            cardBackground.enabled = sprite != null;
            cardBackground.color = isCollected ? collectedTint : lockedTint;
        }

        if (lockIcon) lockIcon.SetActive(!isCollected);
        if (soundButton) soundButton.interactable = isCollected;
    }

    public void SetScale(float scale)
    {
        if (!isAnimating) transform.localScale = Vector3.one * scale;
    }

    public void PlayBounceEffect()
    {
        if (!isCollected || data == null) return;

        LeanTween.cancel(gameObject);
        isAnimating = true;

        Vector3 startScale = transform.localScale;

        LeanTween.scale(gameObject, startScale * 1.2f, 0.1f)
            .setEasePunch()
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, startScale, 0.1f)
                    .setEaseOutQuad()
                    .setOnComplete(() => isAnimating = false);
            });
    }

    // Klik kartu utama → forward ke carousel (audio + bounce + snap).
    private void OnMainClicked()
    {
        if (!isCollected || data == null || carousel == null) return;
        carousel.OnItemSelected(this);
    }

    // Klik sound button → forward ke carousel (audio saja).
    private void OnSoundClicked()
    {
        if (!isCollected || data == null || carousel == null) return;
        carousel.OnSoundButtonClicked(this);
    }
}