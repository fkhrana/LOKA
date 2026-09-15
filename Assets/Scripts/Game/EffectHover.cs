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

    [SerializeField] private LeanTweenType easeType =
        LeanTweenType.easeOutBack;

    [Header("Sound")]

    [SerializeField] private string hoverSound = "Hover";

    [SerializeField] private string clickSound = "ButtonHover";

    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.1f;

    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1f;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private bool isHovering;
    private bool hasCapturedOriginal;

    private void Awake()
    {
        if (targetTransform == null)
            targetTransform =
                GetComponent<RectTransform>();
    }

    private void Start()
    {
        CaptureOriginalIfNeeded();
    }

    private void OnEnable()
    {
        CaptureOriginalIfNeeded();
    }

    private void CaptureOriginalIfNeeded()
    {
        if (hasCapturedOriginal ||
            targetTransform == null)
        {
            return;
        }

        originalScale =
            targetTransform.localScale;

        originalPosition =
            targetTransform.localPosition;

        hasCapturedOriginal = true;
    }

    // ==================================================
    // POINTER ENTER
    // ==================================================

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        DragItem dragItem =
            GetComponent<DragItem>();

        if (dragItem != null &&
            dragItem.IsDragging)
        {
            return;
        }

        if (isHovering)
            return;

        isHovering = true;

        LeanTween.cancel(
            targetTransform.gameObject
        );

        // SCALE
        LeanTween.scale(
            targetTransform,
            originalScale * hoverScale,
            animDuration
        )
        .setEase(easeType)
        .setIgnoreTimeScale(true);

        // MOVE Y
        if (hoverMoveY != 0f)
        {
            LeanTween.moveLocalY(
                targetTransform.gameObject,
                originalPosition.y + hoverMoveY,
                animDuration
            )
            .setEase(easeType)
            .setIgnoreTimeScale(true);
        }

        // HOVER SOUND
        AudioManager.Instance?.StopHoverSFX();

        AudioManager.Instance?.PlayHoverSFX(
            hoverSound,
            hoverVolume
        );
    }

    // ==================================================
    // POINTER EXIT
    // ==================================================

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        DragItem dragItem =
            GetComponent<DragItem>();

        if (dragItem != null &&
            dragItem.IsDragging)
        {
            return;
        }

        if (!isHovering)
            return;

        isHovering = false;

        LeanTween.cancel(
            targetTransform.gameObject
        );

        // RETURN SCALE
        LeanTween.scale(
            targetTransform,
            originalScale,
            animDuration
        )
        .setEase(easeType)
        .setIgnoreTimeScale(true);

        // RETURN POSITION
        if (hoverMoveY != 0f)
        {
            LeanTween.moveLocalY(
                targetTransform.gameObject,
                originalPosition.y,
                animDuration
            )
            .setEase(easeType)
            .setIgnoreTimeScale(true);
        }
    }

    // ==================================================
    // POINTER CLICK
    // ==================================================

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        OnClick();
    }

    public void OnClick()
    {
        AudioManager.Instance?.PlaySFX(
            clickSound,
            clickVolume
        );
    }

    public void PlayCollectPulse()
    {
        if (targetTransform == null)
            return;

        Vector3 startScale = targetTransform.localScale;

        LeanTween.scale(
            targetTransform,
            startScale * collectPulseScale,
            collectPulseDuration * 0.4f
        )
        .setEase(LeanTweenType.easeOutBack)
        .setIgnoreTimeScale(true)
        .setOnComplete(() =>
        {
            LeanTween.scale(
                targetTransform,
                startScale,
                collectPulseDuration * 0.6f
            )
            .setEase(LeanTweenType.easeInOutSine)
            .setIgnoreTimeScale(true);
        });
    }

    // ==================================================
    // STOP HOVER EFFECT
    // ==================================================

    public void StopHoverEffect()
    {
        isHovering = false;

        if (targetTransform == null ||
            !hasCapturedOriginal)
        {
            return;
        }

        LeanTween.cancel(
            targetTransform.gameObject
        );

        targetTransform.localScale =
            originalScale;

        targetTransform.localPosition =
            originalPosition;

        AudioManager.Instance?.StopHoverSFX();
    }

    public void StopHoverSound()
    {
        AudioManager.Instance?.StopHoverSFX();

        isHovering = false;
    }

    // ==================================================
    // DISABLE
    // ==================================================

    private void OnDisable()
    {
        StopHoverSound();

        if (targetTransform != null &&
            hasCapturedOriginal)
        {
            targetTransform.localScale =
                originalScale;

            targetTransform.localPosition =
                originalPosition;

            LeanTween.cancel(
                targetTransform.gameObject
            );
        }

        isHovering = false;
    }
}