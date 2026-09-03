using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AksaraCardPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Text nameText;
    [SerializeField] private GameObject frontFace;
    [SerializeField] private GameObject backFace;
    [SerializeField] private CanvasGroup blurOverlay;
    [SerializeField] private RectTransform cardTransform;

    [Header("Animasi")]
    [SerializeField] private float flipDuration = 0.5f;

    private AksaraData currentData;
    private bool isFlipping = false;
    private Coroutine flipCoroutine;
    private bool isShowingFront = true;

    private void Awake()
    {
        if (cardTransform == null)
            cardTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
        if (blurOverlay != null) blurOverlay.gameObject.SetActive(false);
    }

    // OVERLOAD 1: tanpa sourceRect (langsung di tengah)
    public void Show(AksaraData data)
    {
        Show(data, null);
    }

    // OVERLOAD 2: dengan sourceRect (animasi dari posisi card)
    public void Show(AksaraData data, RectTransform sourceRect)
    {
        if (data == null || isFlipping) return;
        currentData = data;

        // Set gambar & teks
        if (cardImage != null)
        {
            cardImage.sprite = data.FragmentSprite != null ? data.FragmentSprite : data.IconSprite;
            cardImage.enabled = cardImage.sprite != null;
        }
        if (nameText != null) nameText.text = data.AksaraName;

        SetFace(showFront: true);
        gameObject.SetActive(true);

        // Tampilkan blur
        if (blurOverlay != null)
        {
            blurOverlay.gameObject.SetActive(true);
            LeanTween.alphaCanvas(blurOverlay, 1f, 0.2f).setIgnoreTimeScale(true);
        }

        if (flipCoroutine != null) StopCoroutine(flipCoroutine);

        // ===== ANIMASI DARI POSISI CARD (untuk Canvas Overlay) =====
        if (sourceRect != null && cardTransform.parent is RectTransform parentRect)
        {
            // 🔥 Perbaikan: konversi WorldToScreenPoint dengan kamera null (overlay)
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, sourceRect.position);

            // Konversi screen point ke local position di parentRect (Canvas)
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPoint,
                null, // kamera null untuk overlay
                out Vector2 localPoint
            );

            if (success)
            {
                // Set posisi & skala awal (kecil di posisi card)
                cardTransform.anchoredPosition = localPoint;
                cardTransform.localScale = Vector3.one * 0.3f;

                // Animasi membesar & bergerak ke tengah
                LeanTween.move(cardTransform, Vector2.zero, 0.25f)
                    .setEaseOutQuad()
                    .setIgnoreTimeScale(true);

                LeanTween.scale(cardTransform, Vector3.one, 0.25f)
                    .setEaseOutQuad()
                    .setIgnoreTimeScale(true)
                    .setOnComplete(() =>
                    {
                        flipCoroutine = StartCoroutine(FlipRoutine(toBack: true, hideAfter: false));
                    });

                return;
            }
        }

        // Fallback: langsung di tengah (tanpa animasi dari card)
        cardTransform.anchoredPosition = Vector2.zero;
        cardTransform.localScale = Vector3.one;
        flipCoroutine = StartCoroutine(FlipRoutine(toBack: true, hideAfter: false));
    }

    public void Hide()
    {
        if (isFlipping) return;
        if (flipCoroutine != null) StopCoroutine(flipCoroutine);

        SetFace(showFront: true);
        isShowingFront = true;

        if (blurOverlay != null)
        {
            LeanTween.alphaCanvas(blurOverlay, 0f, 0.2f)
                .setIgnoreTimeScale(true)
                .setOnComplete(() => blurOverlay.gameObject.SetActive(false));
        }

        LeanTween.scale(cardTransform, Vector3.one * 0.8f, 0.15f)
            .setEaseInQuad()
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);
                cardTransform.localScale = Vector3.one;
                cardTransform.anchoredPosition = Vector2.zero;
            });
    }

    private void SetFace(bool showFront)
    {
        if (frontFace != null) frontFace.SetActive(showFront);
        if (backFace != null) backFace.SetActive(!showFront);
        isShowingFront = showFront;
    }

    private IEnumerator FlipRoutine(bool toBack, bool hideAfter)
    {
        isFlipping = true;

        float elapsed = 0f;
        Vector3 startScale = cardTransform.localScale;
        Vector3 midScale = new Vector3(0f, startScale.y, startScale.z);

        // Fase 1: mengecil ke 0 di sumbu X
        while (elapsed < flipDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (flipDuration / 2f);
            float scaleX = Mathf.Lerp(startScale.x, 0f, t);
            cardTransform.localScale = new Vector3(scaleX, startScale.y, startScale.z);
            yield return null;
        }
        cardTransform.localScale = midScale;

        // Ganti sisi
        SetFace(showFront: !toBack);

        // Fase 2: membesar kembali
        elapsed = 0f;
        while (elapsed < flipDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (flipDuration / 2f);
            float scaleX = Mathf.Lerp(0f, startScale.x, t);
            cardTransform.localScale = new Vector3(scaleX, startScale.y, startScale.z);
            yield return null;
        }
        cardTransform.localScale = startScale;

        isFlipping = false;

        if (hideAfter) Hide();
    }
}