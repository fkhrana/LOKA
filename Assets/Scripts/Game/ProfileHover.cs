using UnityEngine;
using UnityEngine.EventSystems;

public class ProfileHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float speed = 8f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private float sfxCooldown = 0.5f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private float lastSFXTime = -Mathf.Infinity;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;

        PlayHoverSFX();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    private void PlayHoverSFX()
    {
        if (sfxSource == null || hoverSound == null)
            return;

        // Cegah SFX diputar terlalu sering
        if (Time.unscaledTime - lastSFXTime < sfxCooldown)
            return;

        lastSFXTime = Time.unscaledTime;

        sfxSource.PlayOneShot(hoverSound);
    }
}