using UnityEngine;

public class EffectTapMain : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private RectTransform tapImage;

    [Header("Animation Settings")]
    [SerializeField] private float scaleAmount = 1.05f;
    [SerializeField] private float moveAmount = 8f;
    [SerializeField] private float duration = 0.8f;

    private void Start()
    {
        if (tapImage == null)
        {
            Debug.LogWarning("TapToStartEffect: tapImage not assigned!");
            return;
        }

        // Animasi skala
        LeanTween.scale(tapImage, Vector3.one * scaleAmount, duration)
                 .setLoopPingPong()
                 .setEase(LeanTweenType.easeInOutSine);

        // Animasi posisi Y (naik turun)
        float targetY = tapImage.localPosition.y + moveAmount;
        LeanTween.moveLocalY(tapImage.gameObject, targetY, duration)
                 .setLoopPingPong()
                 .setEase(LeanTweenType.easeInOutSine);
    }

    // Optional: Method untuk menghentikan animasi (misal saat scene berganti)
    private void OnDestroy()
    {
        if (tapImage != null)
        {
            LeanTween.cancel(tapImage.gameObject);
        }
    }
}