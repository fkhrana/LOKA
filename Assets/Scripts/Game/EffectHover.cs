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
    private float lastSFXTime = -Mathf.Infinity;

    private void Awake()
    {
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();

        originalScale = targetTransform.localScale;
        originalPosition = targetTransform.localPosition;
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
        // Hover sound tidak di‑stop di sini, biar tetap smooth
    }

    public void StopHoverSound()
    {
        AudioManager.Instance?.StopHoverSFX();
        isHovering = false;
    }

    private void OnDisable()
    {
        StopHoverSound();

        if (targetTransform != null)
        {
            targetTransform.localScale = originalScale;
            targetTransform.localPosition = originalPosition;
            LeanTween.cancel(targetTransform.gameObject);
        }
        isHovering = false;
    }
}