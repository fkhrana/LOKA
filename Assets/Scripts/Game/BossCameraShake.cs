using System.Collections;
using UnityEngine;

public class BossCameraShake : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 0.45f;
    [SerializeField, Min(0f)] private float strength = 0.12f;

    private Coroutine shakeCoroutine;

    public void PlayShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            Vector2 offset = Random.insideUnitCircle * strength * damper;
            transform.localPosition = originalPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
