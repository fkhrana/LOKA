using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Komponen reusable untuk animasi jari naik-turun.
/// Semua Graphic di dalam fingerIcon di-set raycastTarget=false supaya
/// tidak memblokir tap ke Button di bawahnya.
/// </summary>
public class TutorialFingerTap : MonoBehaviour
{
    [SerializeField] private GameObject fingerIcon;
    [SerializeField, Min(0f)] private float amplitude = 25f;
    [SerializeField, Min(0f)] private float speed = 4f;

    private Coroutine routine;
    private Vector2 basePos;

    private void Awake()
    {
        DisableRaycastOnIcon();
    }

    public void Show()
    {
        if (fingerIcon == null) return;

        DisableRaycastOnIcon();

        RectTransform rt = fingerIcon.GetComponent<RectTransform>();
        if (rt != null) basePos = rt.anchoredPosition;

        fingerIcon.SetActive(true);
        Stop();
        routine = StartCoroutine(RunRoutine());
    }

    public void Hide()
    {
        Stop();

        if (fingerIcon != null)
            fingerIcon.SetActive(false);
    }

    private void Stop()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    /// <summary>
    /// Matikan raycastTarget di semua Graphic (Image, RawImage, Text, dll)
    /// di dalam fingerIcon, supaya tap tembus ke Button di bawahnya.
    /// </summary>
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