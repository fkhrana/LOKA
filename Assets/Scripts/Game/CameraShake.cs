using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 0.45f;
    [SerializeField, Min(0f)] private float strength = 0.12f;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private bool shakeOnPlayerDamage = true;

    private Coroutine shakeCoroutine;

    private void OnEnable()
    {
        if (!shakeOnPlayerDamage)
            return;

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.DamageTaken += HandlePlayerDamageTaken;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.DamageTaken -= HandlePlayerDamageTaken;
    }

    private void HandlePlayerDamageTaken(int amount)
    {
        PlayShake();
    }

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
