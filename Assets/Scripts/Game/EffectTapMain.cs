using UnityEngine;
using UnityEngine.EventSystems;

public class EffectTapMain : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Image")]
    [SerializeField] private RectTransform tapImage;

    [Header("Animation Settings")]
    [SerializeField] private float scaleAmount = 1.05f;
    [SerializeField] private float moveAmount = 8f;
    [SerializeField] private float duration = 0.8f;

    [Header("SFX")]
    [SerializeField] private string hoverSFXName = "Hover";
    [SerializeField] private string clickSFXName = "Click";

    private void Start()
    {
        if (tapImage == null)
        {
            Debug.LogWarning("EffectTapMain: tapImage not assigned!");
            return;
        }

        // Animasi skala
        LeanTween.scale(tapImage, Vector3.one * scaleAmount, duration)
                 .setLoopPingPong()
                 .setEase(LeanTweenType.easeInOutSine);

        // Animasi posisi Y (naik turun)
        float targetY = tapImage.localPosition.y + moveAmount;

        LeanTween.moveLocalY(
            tapImage.gameObject,
            targetY,
            duration
        )
        .setLoopPingPong()
        .setEase(LeanTweenType.easeInOutSine);
    }

    // =========================
    // HOVER
    // =========================

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null &&
            !string.IsNullOrEmpty(hoverSFXName))
        {
            AudioManager.Instance.PlayHoverSFX(hoverSFXName);
        }
    }

    // =========================
    // CLICK
    // =========================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null &&
            !string.IsNullOrEmpty(clickSFXName))
        {
            AudioManager.Instance.PlaySFX(clickSFXName);
        }
    }

    // =========================
    // CLEANUP
    // =========================

    private void OnDestroy()
    {
        if (tapImage != null)
        {
            LeanTween.cancel(tapImage.gameObject);
        }
    }
}