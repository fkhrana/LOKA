using System.Collections;
using UnityEngine;

public class ProgressBarRevealAnimation : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Coroutine animationRoutine;
    private Vector3 originalScale;
    private float originalAlpha;
    private bool hasOriginalValues;

    public void Play(float duration, float startScale)
    {
        ResetAnimation();
        CacheOriginalValues();

        if (canvasGroup == null || duration <= 0f)
            return;

        animationRoutine = StartCoroutine(PlayRoutine(duration, startScale));
    }

    public void ResetAnimation()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        RestoreOriginalValues();
    }

    private void OnDisable()
    {
        ResetAnimation();
    }

    private void CacheOriginalValues()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = transform.localScale;
        originalAlpha = canvasGroup.alpha;
        hasOriginalValues = true;
    }

    private IEnumerator PlayRoutine(float duration, float startScale)
    {
        float elapsed = 0f;
        Vector3 initialScale = originalScale * startScale;

        canvasGroup.alpha = 0f;
        transform.localScale = initialScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            canvasGroup.alpha = Mathf.Lerp(0f, originalAlpha, easedProgress);
            transform.localScale = Vector3.Lerp(initialScale, originalScale, easedProgress);

            yield return null;
        }

        RestoreOriginalValues();
        animationRoutine = null;
    }

    private void RestoreOriginalValues()
    {
        if (!hasOriginalValues)
            return;

        canvasGroup.alpha = originalAlpha;
        transform.localScale = originalScale;
    }
}