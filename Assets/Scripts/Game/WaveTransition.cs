using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class WaveTransitionBannerEffect : MonoBehaviour
{
    [Header("Pop-Up Intro")]
    [SerializeField] private float popDuration = 0.45f;
    [SerializeField] private float overshootScale = 1.3f;
    [SerializeField] private float startScale = 0f;

    [Header("Alert Blink")]
    [SerializeField] private float blinkInterval = 0.12f;
    [SerializeField] private Color colorA = new Color(1f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color colorB = new Color(1f, 0.85f, 0.1f, 1f);

    [Header("Shake")]
    [SerializeField] private float shakeAmplitude = 8f;
    [SerializeField] private float shakeFrequency = 22f;

    [Header("Pulse")]
    [SerializeField] private float pulseAmplitude = 0.06f;
    [SerializeField] private float pulseFrequency = 6f;

    [Header("Screen Flash (opsional)")]
    [SerializeField] private Image screenFlashImage;
    [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private float flashDuration = 0.18f;

    [Header("Camera Shake (opsional)")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraShakeAmplitude = 0.15f;
    [SerializeField] private float cameraShakeDuration = 0.35f;

    [Header("Audio (opsional)")]
    [SerializeField] private AudioClip alarmSFX;
    [Range(0f, 1f)] [SerializeField] private float alarmVolume = 1f;

    [Header("Target Grafik")]
    [SerializeField] private Graphic targetGraphic;

    private RectTransform rectTransform;
    private Vector2 baseAnchoredPos;
    private Color baseColor = Color.white;
    private Coroutine routine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseAnchoredPos = rectTransform.anchoredPosition;

        if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
        if (targetGraphic == null) targetGraphic = GetComponentInChildren<Graphic>(true);
        if (targetGraphic != null) baseColor = targetGraphic.color;
    }

    private void OnEnable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayEffectRoutine());
    }

    private void OnDisable()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }

        // Reset state biar tidak kotor saat dipakai ulang.
        rectTransform.anchoredPosition = baseAnchoredPos;
        rectTransform.localScale = Vector3.one;

        if (targetGraphic != null) targetGraphic.color = baseColor;
        if (screenFlashImage != null) screenFlashImage.gameObject.SetActive(false);
    }

    // Routine utama efek banner.
    private IEnumerator PlayEffectRoutine()
    {
        rectTransform.localScale = Vector3.one * startScale;
        rectTransform.anchoredPosition = baseAnchoredPos;

        if (alarmSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(alarmSFX, alarmVolume);

        if (screenFlashImage != null) StartCoroutine(FlashScreenRoutine());
        if (cameraTransform != null) StartCoroutine(ShakeCameraRoutine());

        // Fase 1: pop-in overshoot (easeOutBack).
        float t = 0f;
        float popInDuration = popDuration * 0.6f;
        while (t < popInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popInDuration);

            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float eased = 1f + c3 * Mathf.Pow(p - 1f, 3f) + c1 * Mathf.Pow(p - 1f, 2f);

            float s = Mathf.LerpUnclamped(startScale, overshootScale, eased);
            rectTransform.localScale = Vector3.one * s;

            yield return null;
        }

        // Fase 2: settle ke skala 1 (easeOutCubic).
        t = 0f;
        float settleDuration = popDuration * 0.4f;
        float from = rectTransform.localScale.x;
        while (t < settleDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / settleDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            float s = Mathf.Lerp(from, 1f, eased);
            rectTransform.localScale = Vector3.one * s;
            yield return null;
        }
        rectTransform.localScale = Vector3.one;

        // Fase 3: loop alert — blink warna + shake + pulse.
        bool toggle = false;
        float blinkTimer = 0f;

        while (true)
        {
            float time = Time.unscaledTime;

            if (targetGraphic != null)
            {
                blinkTimer += Time.unscaledDeltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    toggle = !toggle;
                    targetGraphic.color = toggle ? colorA : colorB;
                }
            }

            float shakeX = Mathf.Sin(time * shakeFrequency) * shakeAmplitude;
            float shakeY = Mathf.Cos(time * shakeFrequency * 0.7f) * shakeAmplitude * 0.5f;
            rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(shakeX, shakeY);

            float pulse = 1f + Mathf.Sin(time * pulseFrequency) * pulseAmplitude;
            rectTransform.localScale = Vector3.one * pulse;

            yield return null;
        }
    }

    // Flash putih fullscreen sebentar.
    private IEnumerator FlashScreenRoutine()
    {
        if (screenFlashImage == null) yield break;

        screenFlashImage.gameObject.SetActive(true);
        screenFlashImage.color = flashColor;

        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / flashDuration);

            Color c = flashColor;
            c.a = Mathf.Lerp(flashColor.a, 0f, p);
            screenFlashImage.color = c;

            yield return null;
        }

        screenFlashImage.gameObject.SetActive(false);
    }

    // Shake kamera singkat saat banner muncul.
    private IEnumerator ShakeCameraRoutine()
    {
        if (cameraTransform == null) yield break;

        Vector3 originalPos = cameraTransform.localPosition;
        float t = 0f;

        while (t < cameraShakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float damper = 1f - Mathf.Clamp01(t / cameraShakeDuration);

            float x = (Random.value * 2f - 1f) * cameraShakeAmplitude * damper;
            float y = (Random.value * 2f - 1f) * cameraShakeAmplitude * damper;

            cameraTransform.localPosition = originalPos + new Vector3(x, y, 0f);
            yield return null;
        }

        cameraTransform.localPosition = originalPos;
    }
}