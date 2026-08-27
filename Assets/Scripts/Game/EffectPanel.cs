using UnityEngine;
using System;

public class EffectPanel : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private RectTransform box;
    [SerializeField] private CanvasGroup background;

    [Header("Animation")]
    [SerializeField] private float slideDistance = 500f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float slideDuration = 0.5f;

    public Action onOpenComplete;
    public Action onCloseComplete;

    private Vector2 originalPosition;
    private bool isAnimating;

    private void Awake()
    {
        if (box == null)
            box = GetComponent<RectTransform>();

        originalPosition = box.anchoredPosition;

        if (background == null)
            Debug.LogWarning($"EffectPanel: Background belum diisi pada {gameObject.name}");
    }

    private void OnEnable()
    {
        if (isAnimating || box == null)
            return;

        isAnimating = true;

        box.anchoredPosition =
            originalPosition + new Vector2(0, -slideDistance);

        if (background != null)
        {
            background.alpha = 0f;

            LeanTween.alphaCanvas(
                background,
                1f,
                fadeDuration
            )
            .setIgnoreTimeScale(true);
        }

        LeanTween.move(
            box,
            originalPosition,
            slideDuration
        )
        .setEaseOutExpo()
        .setIgnoreTimeScale(true)
        .setOnComplete(() =>
        {
            isAnimating = false;
            onOpenComplete?.Invoke();
        });
    }

    public void CloseDialog(Action onComplete = null)
    {
        if (isAnimating || box == null)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
            return;
        }

        isAnimating = true;

        if (background != null)
        {
            LeanTween.alphaCanvas(
                background,
                0f,
                fadeDuration
            )
            .setIgnoreTimeScale(true);
        }

        LeanTween.move(
            box,
            originalPosition + new Vector2(0, -slideDistance),
            slideDuration
        )
        .setEaseInExpo()
        .setIgnoreTimeScale(true)
        .setOnComplete(() =>
        {
            gameObject.SetActive(false);

            isAnimating = false;

            onCloseComplete?.Invoke();
            onComplete?.Invoke();
        });
    }

    private void OnDisable()
    {
        if (box != null)
            box.anchoredPosition = originalPosition;

        if (background != null)
            background.alpha = 1f;
    }
}