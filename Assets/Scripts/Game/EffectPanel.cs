using UnityEngine;
using System;

public class EffectPanel : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private RectTransform box;
    [SerializeField] private CanvasGroup background;
    [SerializeField] private CanvasGroup boxCanvasGroup; // untuk fade box

    [Header("Animation")]
    [SerializeField] private float slideDistance = 500f;
    [SerializeField] private float fadeDuration = 0.3f;    // durasi fade background
    [SerializeField] private float slideDuration = 0.5f;   // durasi slide box

    [Header("Stack Settings")]
    [SerializeField] private bool isStacked = true;

    public Action onOpenComplete;
    public Action onCloseComplete;

    private Vector2 originalPosition;
    private bool isAnimating;
    private bool isOpen = false;

    private void Awake()
    {
        if (box == null) box = GetComponent<RectTransform>();
        originalPosition = box.anchoredPosition;

        // Auto-add CanvasGroup untuk box jika belum ada
        if (boxCanvasGroup == null)
        {
            boxCanvasGroup = box.GetComponent<CanvasGroup>();
            if (boxCanvasGroup == null)
                boxCanvasGroup = box.gameObject.AddComponent<CanvasGroup>();
        }

        if (background == null)
            Debug.LogWarning($"EffectPanel: Background belum diisi pada {gameObject.name}");
    }

    private void OnEnable()
    {
        if (isAnimating || box == null) return;

        isAnimating = true;
        isOpen = true;

        // Reset posisi ke bawah & alpha 0
        box.anchoredPosition = originalPosition + new Vector2(0, -slideDistance);
        boxCanvasGroup.alpha = 0f;

        // Fade in background
        if (isStacked && background != null)
        {
            background.alpha = 0f;
            background.blocksRaycasts = true;
            background.interactable = true;
            LeanTween.alphaCanvas(background, 1f, fadeDuration).setIgnoreTimeScale(true);
        }

        // Slide + Fade in box (bersamaan)
        LeanTween.move(box, originalPosition, slideDuration)
            .setEaseOutExpo()
            .setIgnoreTimeScale(true);

        LeanTween.alphaCanvas(boxCanvasGroup, 1f, slideDuration)
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
        if (!isOpen)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
            return;
        }

        if (isAnimating || box == null)
        {
            gameObject.SetActive(false);
            isOpen = false;
            onComplete?.Invoke();
            return;
        }

        isAnimating = true;

        int completed = 0;
        void TryFinish()
        {
            completed++;
            if (completed == 3) // box slide, box fade, background fade
            {
                gameObject.SetActive(false);
                isOpen = false;
                isAnimating = false;
                onCloseComplete?.Invoke();
                onComplete?.Invoke();
            }
        }

        // 1. Slide box ke bawah
        LeanTween.move(box, originalPosition + new Vector2(0, -slideDistance), slideDuration)
            .setEaseInExpo()
            .setIgnoreTimeScale(true)
            .setOnComplete(TryFinish);

        // 2. Fade out box
        LeanTween.alphaCanvas(boxCanvasGroup, 0f, slideDuration)
            .setEaseInExpo()
            .setIgnoreTimeScale(true)
            .setOnComplete(TryFinish);

        // 3. Fade out background
        if (isStacked && background != null)
        {
            LeanTween.alphaCanvas(background, 0f, fadeDuration)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    background.blocksRaycasts = false;
                    background.interactable = false;
                    TryFinish();
                });
        }
        else
        {
            // Jika tidak ada background, anggap selesai
            TryFinish();
        }
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);

        if (box != null)
        {
            box.anchoredPosition = originalPosition;
            boxCanvasGroup.alpha = 1f; // reset alpha
        }

        if (background != null)
        {
            background.alpha = 0f;
            background.blocksRaycasts = false;
            background.interactable = false;
        }

        isOpen = false;
        isAnimating = false;
    }
}