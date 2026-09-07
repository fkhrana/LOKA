using UnityEngine;
using UnityEngine.EventSystems;

public class LowHealthHelper : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("Idle Animation")]
    [SerializeField] private float floatHeight = 12f;
    [SerializeField] private float floatDuration = 1.8f;

    private LowHealthHelperController controller;
    private RectTransform helperRect;
    private Vector2 basePosition;
    private bool isInteractable;

    public void Initialize(LowHealthHelperController owner)
    {
        controller = owner;
        isInteractable = true;
        helperRect = transform as RectTransform;
        if (helperRect != null)
            basePosition = helperRect.anchoredPosition;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        LeanTween.cancel(gameObject);
        LeanTween.alphaCanvas(canvasGroup, 1f, fadeInDuration)
            .setEaseOutQuad()
            .setIgnoreTimeScale(true);
    }

    private void Update()
    {
        if (helperRect == null || floatDuration <= 0f)
            return;

        float cycle = (Time.unscaledTime / floatDuration) * Mathf.PI * 2f;
        helperRect.anchoredPosition = basePosition + Vector2.up * (Mathf.Sin(cycle) * floatHeight);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable)
            return;

        isInteractable = false;
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        controller?.ConsumeHelper(this);
    }

    public void FadeOutAndDestroy()
    {
        if (canvasGroup == null)
        {
            Destroy(gameObject);
            return;
        }

        LeanTween.cancel(gameObject);
        LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
            .setEaseInQuad()
            .setIgnoreTimeScale(true)
            .setOnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
        controller?.NotifyHelperDestroyed(this);
    }
}