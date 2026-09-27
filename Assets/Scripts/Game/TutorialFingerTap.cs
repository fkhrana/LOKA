using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFingerTap : MonoBehaviour
{
    [Header("Icon")]
    [SerializeField] private GameObject fingerIcon;

    [Header("Idle Animation")]
    [SerializeField, Min(0f)] private float amplitude = 25f;
    [SerializeField, Min(0f)] private float speed = 4f;

    [Header("Appear Animation")]
    [Tooltip("Durasi animasi fade + scale saat finger muncul (detik). " +
             "0 = muncul instan tanpa animasi.")]
    [SerializeField, Min(0f)] private float appearDuration = 0.25f;

    [Tooltip("Scale awal saat finger mulai muncul (0 = mengecil dari 0).")]
    [SerializeField, Range(0f, 1f)] private float appearStartScale = 0.5f;

    private Coroutine routine;
    private Coroutine appearRoutine;
    private CanvasGroup canvasGroup;
    private Vector2 basePos;
    private bool basePosCached;

    private void Awake()
    {
        DisableRaycastOnIcon();
        EnsureCanvasGroup();
        CacheBasePosition();
    }

    public void Show()
    {
        if (fingerIcon == null) return;

        DisableRaycastOnIcon();
        EnsureCanvasGroup();
        CacheBasePosition();

        RectTransform rt = fingerIcon.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = basePos;

        fingerIcon.SetActive(true);

        Stop();
        StopAppear();

        if (appearDuration > 0f)
            appearRoutine = StartCoroutine(AppearRoutine());
        else
        {
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (rt != null) rt.localScale = Vector3.one;
            routine = StartCoroutine(RunRoutine());
        }
    }

    public void Hide()
    {
        Stop();
        StopAppear();

        if (fingerIcon != null)
            fingerIcon.SetActive(false);
    }

    private IEnumerator AppearRoutine()
    {
        RectTransform rt = fingerIcon.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float t = 0f;
        while (t < appearDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / appearDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = p;

            float scale = Mathf.Lerp(appearStartScale, 1f, p);
            rt.localScale = Vector3.one * scale;

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        rt.localScale = Vector3.one;

        appearRoutine = null;
        routine = StartCoroutine(RunRoutine());
    }

    private void EnsureCanvasGroup()
    {
        if (fingerIcon == null) return;
        if (canvasGroup != null) return;

        canvasGroup = fingerIcon.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = fingerIcon.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void CacheBasePosition()
    {
        if (basePosCached) return;
        if (fingerIcon == null) return;

        RectTransform rt = fingerIcon.GetComponent<RectTransform>();
        if (rt == null) return;

        basePos = rt.anchoredPosition;
        basePosCached = true;
    }

    private void Stop()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void StopAppear()
    {
        if (appearRoutine != null)
        {
            StopCoroutine(appearRoutine);
            appearRoutine = null;
        }
    }

    private void DisableRaycastOnIcon()
    {
        if (fingerIcon == null) return;

        foreach (var g in fingerIcon.GetComponentsInChildren<Graphic>(true))
            if (g != null) g.raycastTarget = false;
    }

    private IEnumerator RunRoutine()
    {
        RectTransform rt = fingerIcon.GetComponent<RectTransform>();
        if (rt == null) yield break;

        while (true)
        {
            float sin = Mathf.Sin(Time.unscaledTime * speed);
            float offset = Mathf.Abs(sin) * amplitude;
            rt.anchoredPosition = basePos - new Vector2(0f, offset);
            yield return null;
        }
    }
}