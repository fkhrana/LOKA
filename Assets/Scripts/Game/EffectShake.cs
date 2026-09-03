using UnityEngine;
using System.Collections;

public class ShakeEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float strength = 30f; // sudut goyang (derajat)

    public void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        Transform target = transform;
        Quaternion originalRotation = target.localRotation;

        float halfDuration = duration / 2f;

        // Goyang ke kanan
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / halfDuration;
            float angle = Mathf.Lerp(0, strength, t);
            target.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        // Goyang ke kiri
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / halfDuration;
            float angle = Mathf.Lerp(strength, -strength, t);
            target.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        // Kembali ke posisi awal
        target.localRotation = originalRotation;
        Debug.Log("ShakeEffect selesai di " + gameObject.name);
    }
}