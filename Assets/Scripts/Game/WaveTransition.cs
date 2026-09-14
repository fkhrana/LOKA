using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Efek dramatis untuk banner transisi wave.
/// Attach ke GameObject banner (yang di-toggle oleh EnemyWaveSpawner).
/// Semua animasi jalan otomatis dari OnEnable / OnDisable.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class WaveTransitionBannerEffect : MonoBehaviour
{
    [Header("Pop-Up Intro")]
    [Tooltip("Durasi total overshoot pop (masuk + settle).")]
    [SerializeField] private float popDuration = 0.45f;
    [Tooltip("Skala overshoot sebelum settle ke 1.")]
    [SerializeField] private float overshootScale = 1.3f;
    [Tooltip("Skala awal saat banner muncul. 0 = dari nol (pop-in).")]
    [SerializeField] private float startScale = 0f;

    [Header("Alert Blink")]
    [Tooltip("Selang antar kedipan warna.")]
    [SerializeField] private float blinkInterval = 0.12f;
    [Tooltip("Warna A — misal merah terang (alert).")]
    [SerializeField] private Color colorA = new Color(1f, 0.15f, 0.15f, 1f);
    [Tooltip("Warna B — misal kuning/orange (emergency).")]
    [SerializeField] private Color colorB = new Color(1f, 0.85f, 0.1f, 1f);

    [Header("Shake")]
    [Tooltip("Amplitudo getaran posisi (pixel UI).")]
    [SerializeField] private float shakeAmplitude = 8f;
    [Tooltip("Frekuensi getaran (semakin besar = semakin cepat).")]
    [SerializeField] private float shakeFrequency = 22f;

    [Header("Pulse")]
    [Tooltip("Seberapa besar skala membesar-menurun terus-menerus.")]
    [SerializeField] private float pulseAmplitude = 0.06f;
    [Tooltip("Kecepatan pulse.")]
    [SerializeField] private float pulseFrequency = 6f;

    [Header("Screen Flash (opsional)")]
    [Tooltip("Image fullscreen (awalnya inactive). Akan flash putih saat banner muncul.")]
    [SerializeField] private Image screenFlashImage;
    [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private float flashDuration = 0.18f;

    [Header("Camera Shake (opsional)")]
    [Tooltip("Drag Main Camera. Kamera akan bergetar singkat saat banner muncul.")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraShakeAmplitude = 0.15f;
    [SerializeField] private float cameraShakeDuration = 0.35f;

    [Header("Audio (opsional)")]
    [Tooltip("SFX alarm/impact yang dimainkan sekali saat banner muncul.")]
    [SerializeField] private AudioClip alarmSFX;
    [Range(0f, 1f)]
    [SerializeField] private float alarmVolume = 1f;

    [Header("Target Grafik")]
    [Tooltip("Graphic yang warnanya di-blink. Kosong = auto ambil dari komponen ini / anak-anak.")]
    [SerializeField] private Graphic targetGraphic;

    private RectTransform rectTransform;
    private Vector2 baseAnchoredPos;
    private Color baseColor = Color.white;
    private Coroutine routine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseAnchoredPos = rectTransform.anchoredPosition;

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
        if (targetGraphic == null)
            targetGraphic = GetComponentInChildren<Graphic>(true);

        if (targetGraphic != null)
            baseColor = targetGraphic.color;
    }

    private void OnEnable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayEffectRoutine());
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        // Reset state supaya tidak "kotor" saat dipakai lagi
        rectTransform.anchoredPosition = baseAnchoredPos;
        rectTransform.localScale = Vector3.one;

        if (targetGraphic != null)
            targetGraphic.color = baseColor;

        if (screenFlashImage != null)
            screenFlashImage.gameObject.SetActive(false);
    }

    private IEnumerator PlayEffectRoutine()
    {
        rectTransform.localScale = Vector3.one * startScale;
        rectTransform.anchoredPosition = baseAnchoredPos;

        // Audio sekali
        if (alarmSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(alarmSFX, alarmVolume);

        // Efek samping
        if (screenFlashImage != null)
            StartCoroutine(FlashScreenRoutine());

        if (cameraTransform != null)
            StartCoroutine(ShakeCameraRoutine());

        // ==== Fase 1: Pop-in overshoot (easeOutBack) ====
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

        // ==== Fase 2: Settle ke skala 1 ====
        t = 0f;
        float settleDuration = popDuration * 0.4f;
        float from = rectTransform.localScale.x;
        while (t < settleDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / settleDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f); // easeOutCubic
            float s = Mathf.Lerp(from, 1f, eased);
            rectTransform.localScale = Vector3.one * s;
            yield return null;
        }
        rectTransform.localScale = Vector3.one;

        // ==== Fase 3: Loop alert — blink warna + shake + pulse ====
        bool toggle = false;
        float blinkTimer = 0f;

        while (true)
        {
            float time = Time.unscaledTime;

            // Kedip warna
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

            // Getar posisi
            float shakeX = Mathf.Sin(time * shakeFrequency) * shakeAmplitude;
            float shakeY = Mathf.Cos(time * shakeFrequency * 0.7f) * shakeAmplitude * 0.5f;
            rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(shakeX, shakeY);

            // Pulse skala
            float pulse = 1f + Mathf.Sin(time * pulseFrequency) * pulseAmplitude;
            rectTransform.localScale = Vector3.one * pulse;

            yield return null;
        }
    }

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