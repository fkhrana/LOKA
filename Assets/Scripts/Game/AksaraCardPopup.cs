using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AksaraCardPopup : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform cardTransform;

    [Header("Faces")]
    [SerializeField] private GameObject frontFace;
    [SerializeField] private GameObject backFace;

    [Header("Back Content")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private AudioSource audioSource;

    [Header("Sound")]
    [SerializeField] private AksaraSoundLibrary soundLibrary;

    [Header("Animation")]
    [SerializeField] private float flipDuration = 0.35f;

    [Header("Blur Background")]
    [SerializeField] private CanvasGroup blurOverlay; // overlay untuk efek blur/fade

    private AksaraData currentData;
    private Coroutine flipCoroutine;
    private bool isFlipping;

    private void Awake()
    {
        if (soundButton != null) soundButton.onClick.AddListener(PlayLetterSound);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (root != null) root.SetActive(false);
        if (blurOverlay != null)
        {
            blurOverlay.alpha = 0f;
            blurOverlay.gameObject.SetActive(false);
        }
    }

    public void Show(AksaraData data)
    {
        if (data == null || isFlipping) return;
        currentData = data;

        // Isi konten belakang
        if (cardImage != null)
        {
            cardImage.sprite = data.FragmentSprite != null ? data.FragmentSprite : data.IconSprite;
            cardImage.enabled = cardImage.sprite != null;
        }
        if (nameText != null) nameText.text = data.AksaraName;

        // Reset posisi & face
        SetFace(showFront: true);
        cardTransform.localScale = Vector3.one;
        root.SetActive(true);

        // Tampilkan blur overlay
        if (blurOverlay != null)
        {
            blurOverlay.gameObject.SetActive(true);
            LeanTween.alphaCanvas(blurOverlay, 1f, 0.2f).setIgnoreTimeScale(true);
        }

        // Mulai flip ke belakang
        if (flipCoroutine != null) StopCoroutine(flipCoroutine);
        flipCoroutine = StartCoroutine(FlipRoutine(toBack: true, hideAfter: false));
    }

    public void Hide()
    {
        if (isFlipping) return;
        if (flipCoroutine != null) StopCoroutine(flipCoroutine);
        flipCoroutine = StartCoroutine(FlipRoutine(toBack: false, hideAfter: true));
    }

    private IEnumerator FlipRoutine(bool toBack, bool hideAfter)
    {
        isFlipping = true;

        // Efek lift kartu sebelum flip (naik & membesar sedikit)
        if (toBack)
        {
            LeanTween.scale(cardTransform, Vector3.one * 1.05f, 0.1f).setEaseOutQuad();
            LeanTween.moveLocalY(cardTransform.gameObject, cardTransform.localPosition.y + 10f, 0.1f);
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Animasi flip (skala X)
        float half = flipDuration / 2f;
        float elapsed = 0f;

        // Stage 1: Pipihkan (scale.x -> 0)
        while (elapsed < half)
        {
            float t = elapsed / half;
            float scaleX = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, t));
            cardTransform.localScale = new Vector3(scaleX, cardTransform.localScale.y, 1f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cardTransform.localScale = new Vector3(0f, cardTransform.localScale.y, 1f);

        // Tukar muka
        SetFace(showFront: !toBack);

        // Stage 2: Lebarkan (scale.x -> 1)
        elapsed = 0f;
        while (elapsed < half)
        {
            float t = elapsed / half;
            float scaleX = Mathf.Lerp(0f, 1f, Mathf.SmoothStep(0f, 1f, t));
            cardTransform.localScale = new Vector3(scaleX, cardTransform.localScale.y, 1f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cardTransform.localScale = new Vector3(1f, cardTransform.localScale.y, 1f);

        // Efek turun & kembali ke skala normal
        if (toBack)
        {
            LeanTween.scale(cardTransform, Vector3.one, 0.1f).setEaseInQuad();
            LeanTween.moveLocalY(cardTransform.gameObject, cardTransform.localPosition.y - 10f, 0.1f);
        }

        isFlipping = false;

        if (hideAfter)
        {
            // Fade out blur overlay
            if (blurOverlay != null)
            {
                LeanTween.alphaCanvas(blurOverlay, 0f, 0.2f).setIgnoreTimeScale(true)
                    .setOnComplete(() => {
                        blurOverlay.gameObject.SetActive(false);
                        root.SetActive(false);
                    });
            }
            else
            {
                root.SetActive(false);
            }
        }
    }

    private void SetFace(bool showFront)
    {
        if (frontFace != null) frontFace.SetActive(showFront);
        if (backFace != null) backFace.SetActive(!showFront);
    }

    private void PlayLetterSound()
    {
        if (currentData == null || audioSource == null || soundLibrary == null) return;
        AudioClip clip = soundLibrary.GetClip(currentData.GestureShape);
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}