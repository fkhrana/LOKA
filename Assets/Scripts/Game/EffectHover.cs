using UnityEngine;
using UnityEngine.EventSystems;

public class EffectHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Target")]
    [SerializeField] private RectTransform targetTransform;

    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverMoveY = 10f;
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;

    [Header("Sound")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private float sfxCooldown = 0.5f;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isHovering;
    private bool hasCapturedOriginal;
    private float lastSFXTime = -Mathf.Infinity;

    private void Awake()
    {
        // Hanya pastikan referensi target ada. JANGAN capture originalScale/Position
        // di sini, karena urutan Awake() antar-script tidak dijamin oleh Unity.
        // Kalau ada Animator di GameObject yang sama dan belum sempat settle,
        // scale/posisi yang ke-capture bisa salah (misal 0), dan nanti dipakai
        // lagi di OnDisable() -> tombol jadi invisible/mengecil permanen.
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Start() dijamin jalan setelah semua Awake() di scene selesai,
        // termasuk Animator.Rebind() kalau ada -> nilai yang di-capture di sini aman.
        CaptureOriginalIfNeeded();
    }

    private void OnEnable()
    {
        // Jaga-jaga: kalau objek ini di-nonaktifkan lalu diaktifkan lagi sebelum
        // Start() sempat jalan (mis. lewat OpenPanel/CloseAllPanels di frame yang sama),
        // pastikan tetap ada nilai original yang valid.
        CaptureOriginalIfNeeded();
    }

    private void CaptureOriginalIfNeeded()
    {
        if (hasCapturedOriginal || targetTransform == null) return;

        originalScale = targetTransform.localScale;
        originalPosition = targetTransform.localPosition;
        hasCapturedOriginal = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering) return;
        isHovering = true;

        LeanTween.cancel(targetTransform.gameObject);

        LeanTween.scale(targetTransform, originalScale * hoverScale, animDuration)
            .setEase(easeType)
            .setIgnoreTimeScale(true);

        if (hoverMoveY != 0f)
        {
            LeanTween.moveLocalY(targetTransform.gameObject, originalPosition.y + hoverMoveY, animDuration)
                .setEase(easeType)
                .setIgnoreTimeScale(true);
        }

        if (hoverSound != null && Time.unscaledTime - lastSFXTime >= sfxCooldown)
        {
            lastSFXTime = Time.unscaledTime;
            AudioManager.Instance?.PlayHoverSFX(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering) return;
        isHovering = false;

        LeanTween.cancel(targetTransform.gameObject);

        LeanTween.scale(targetTransform, originalScale, animDuration)
            .setEase(easeType)
            .setIgnoreTimeScale(true);

        if (hoverMoveY != 0f)
        {
            LeanTween.moveLocalY(targetTransform.gameObject, originalPosition.y, animDuration)
                .setEase(easeType)
                .setIgnoreTimeScale(true);
        }
        // Hover sound tidak di-stop di sini, biar tetap smooth
    }

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
            targetTransform.localPosition = originalPosition;
            LeanTween.cancel(targetTransform.gameObject);
        }
        isHovering = false;
    }
}