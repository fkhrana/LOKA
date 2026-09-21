using UnityEngine;
using UnityEngine.EventSystems;

public class EffectHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Target")]
    [SerializeField] private RectTransform targetTransform;

    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverMoveY = 10f;
    [SerializeField] private float animDuration = 0.2f;

    [Header("Collect Book Pulse")]
    [SerializeField] private float collectPulseScale = 1.2f;
    [SerializeField] private float collectPulseDuration = 0.5f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;

    [Header("Sound")]
    [SerializeField] private string hoverSound = "Hover";
    [SerializeField] private string clickSound = "ButtonHover";
    [Range(0f, 1f)] [SerializeField] private float hoverVolume = 0.1f;
    [Range(0f, 1f)] [SerializeField] private float clickVolume = 1f;

    private Vector3 originalScale;
    private Vector2 originalAnchoredPosition;

    private bool isHovering;
    private bool hasCapturedOriginal;

    private void Awake()
    {
        if (targetTransform == null) targetTransform = GetComponent<RectTransform>();
    }

    private void Start() => CaptureOriginalIfNeeded();
    private void OnEnable() => CaptureOriginalIfNeeded();

    // Simpan posisi & scale awal sekali saja.
    private void CaptureOriginalIfNeeded()
    {
        if (hasCapturedOriginal || targetTransform == null) return;

        originalScale = targetTransform.localScale;
        originalAnchoredPosition = targetTransform.anchoredPosition;
        hasCapturedOriginal = true;
    }

    // Efek hover: scale + move + SFX.
    public void OnPointerEnter(PointerEventData eventData)
    {
        DragItem dragItem = GetComponent<DragItem>();

        if (dragItem != null && dragItem.IsDragging) return;
        if (isHovering) return;

        isHovering = true;

        LeanTween.cancel(targetTransform.gameObject);

        // Scale up.
        LeanTween.scale(targetTransform, originalScale * hoverScale, animDuration)
            .setEase(easeType)
            .setIgnoreTimeScale(true);

        // Move Y.
        if (hoverMoveY != 0f)
        {
            LeanTween.moveY(targetTransform, originalAnchoredPosition.y + hoverMoveY, animDuration)
                .setEase(easeType)
                .setIgnoreTimeScale(true);
        }

        // PlayHoverSFX sudah otomatis stop hover yang sedang main.
        AudioManager.Instance?.PlayHoverSFX(hoverSound, hoverVolume);
    }

    // Reset scale + posisi saat keluar hover.
    public void OnPointerExit(PointerEventData eventData)
    {
        DragItem dragItem = GetComponent<DragItem>();

        if (dragItem != null && dragItem.IsDragging) return;
        if (!isHovering) return;

        isHovering = false;

        LeanTween.cancel(targetTransform.gameObject);

        // Return scale.
        LeanTween.scale(targetTransform, originalScale, animDuration)
            .setEase(easeType)
            .setIgnoreTimeScale(true);

        // Return position.
        if (hoverMoveY != 0f)
        {
            LeanTween.moveY(targetTransform, originalAnchoredPosition.y, animDuration)
                .setEase(easeType)
                .setIgnoreTimeScale(true);
        }
    }

    // Forward click ke OnClick dan reset hover untuk touch device.
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
        StopHoverEffect();
    }

    // Putar SFX klik.
    public void OnClick()
    {
        AudioManager.Instance?.PlaySFX(clickSound, clickVolume);
    }

    // Pulse scale sebentar (untuk item collect).
    public void PlayCollectPulse()
    {
        if (targetTransform == null) return;

        CaptureOriginalIfNeeded();
        LeanTween.cancel(targetTransform.gameObject);
        targetTransform.localScale = originalScale;

        LeanTween.scale(targetTransform, originalScale * collectPulseScale, collectPulseDuration * 0.4f)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                LeanTween.scale(targetTransform, originalScale, collectPulseDuration * 0.6f)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setIgnoreTimeScale(true)
                    .setOnComplete(() => targetTransform.localScale = originalScale);
            });
    }

    // Stop hover + reset transform.
    public void StopHoverEffect()
    {
        isHovering = false;

        if (targetTransform == null || !hasCapturedOriginal) return;

        LeanTween.cancel(targetTransform.gameObject);

        targetTransform.localScale = originalScale;
        targetTransform.anchoredPosition = originalAnchoredPosition;

        AudioManager.Instance?.StopHoverSFX();
    }

    // Stop hover sound saja.
    public void StopHoverSound()
    {
        AudioManager.Instance?.StopHoverSFX();
        isHovering = false;
    }

    private void OnDisable()
    {
        StopHoverSound();

        if (targetTransform != null && hasCapturedOriginal)
        {
            targetTransform.localScale = originalScale;
            targetTransform.anchoredPosition = originalAnchoredPosition;

            LeanTween.cancel(targetTransform.gameObject);
        }

        isHovering = false;
    }
}