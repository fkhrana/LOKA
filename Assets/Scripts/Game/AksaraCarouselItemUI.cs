using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single Aksara card in the carousel.
/// Handles visual setup, lock state, click events, and bounce effect.
/// Gambar card diambil dari AksaraCardVisualLibrary (BUKAN dari data.FragmentSprite/IconSprite),
/// supaya carousel bisa pakai gambar card yang beda dari yang dipakai di halaman detail lain.
/// </summary>
[RequireComponent(typeof(Button))]
public class AksaraCarouselItemUI : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private AksaraCardVisualLibrary cardVisualLibrary;

    [Header("Audio")]
    [SerializeField] private Button soundButton;
    [SerializeField] private AksaraSoundLibrary soundLibrary;

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
        mainButton.onClick.AddListener(OnClick);
        if (soundButton)
            soundButton.onClick.AddListener(PlayLetterSound);
    }

    public void Setup(AksaraData newData, AksaraCarouselUI parent, bool collected)
    {
        data = newData;
        carousel = parent;
        isCollected = collected;

        if (data == null) return;

        Sprite sprite = cardVisualLibrary != null ? cardVisualLibrary.GetCardSprite(data) : null;

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
        if (!isAnimating)
            transform.localScale = Vector3.one * scale;
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

    private void OnClick()
    {
        if (!isCollected || data == null || carousel == null) return;
        carousel.OnItemSelected(this);
    }

    private void PlayLetterSound()
    {
        if (!isCollected || data == null || soundLibrary == null) return;
        AudioClip clip = soundLibrary.GetClip(data.GestureShape);
        if (clip != null)
            AudioManager.Instance?.PlayUISFX(clip);
    }
}