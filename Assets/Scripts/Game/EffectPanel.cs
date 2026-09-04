using UnityEngine;
using System;

public class EffectPanel : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private RectTransform box;
    [SerializeField] private CanvasGroup background;
    [SerializeField] private CanvasGroup boxCanvasGroup;

    [Header("Animation")]
    [SerializeField] private float slideDistance = 500f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float slideDuration = 0.5f;

    [Header("Stack Settings")]
    [SerializeField] private bool isStacked = true;

    [Header("Sound")]
    [SerializeField] private bool useSound = true;
    [SerializeField] private string openSound = "Panel";
    [SerializeField] private string closeSound = "Panel";

    [Range(0f, 1f)]
    [SerializeField] private float panelSoundVolume = 0.6f;

    public Action onOpenComplete;
    public Action onCloseComplete;

    private Vector2 originalPosition;
    private bool isAnimating;
    private bool isOpen;

    private void Awake()
    {
        if (box == null)
            box = GetComponent<RectTransform>();

        originalPosition = box.anchoredPosition;

        if (boxCanvasGroup == null)
        {
            boxCanvasGroup = box.GetComponent<CanvasGroup>();

            if (boxCanvasGroup == null)
                boxCanvasGroup = box.gameObject.AddComponent<CanvasGroup>();
        }

        if (background == null)
            Debug.LogWarning(
                $"EffectPanel: Background belum diisi pada {gameObject.name}"
            );
    }

    private void OnEnable()
    {
        if (isAnimating || box == null)
            return;

        isAnimating = true;
        isOpen = true;

        if (useSound)
            AudioManager.Instance?.PlayUISFX(
                openSound,
                panelSoundVolume
            );

        box.anchoredPosition =
            originalPosition + new Vector2(0, -slideDistance);

        boxCanvasGroup.alpha = 0f;

        if (isStacked && background != null)
        {
            background.alpha = 0f;
            background.blocksRaycasts = true;
            background.interactable = true;

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
        .setIgnoreTimeScale(true);

        LeanTween.alphaCanvas(
            boxCanvasGroup,
            1f,
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

        if (useSound)
            AudioManager.Instance?.PlayUISFX(
                closeSound,
                panelSoundVolume
            );

        int completed = 0;

        void TryFinish()
        {
            completed++;

            if (completed == 3)
            {
                gameObject.SetActive(false);
                isOpen = false;
                isAnimating = false;

                onCloseComplete?.Invoke();
                onComplete?.Invoke();
            }
        }

        // Slide box
        LeanTween.move(
            box,
            originalPosition + new Vector2(0, -slideDistance),
            slideDuration
        )
        .setEaseInExpo()
        .setIgnoreTimeScale(true)
        .setOnComplete(TryFinish);

        // Fade box
        LeanTween.alphaCanvas(
            boxCanvasGroup,
            0f,
            slideDuration
        )
        .setEaseInExpo()
        .setIgnoreTimeScale(true)
        .setOnComplete(TryFinish);

        // Fade background
        if (isStacked && background != null)
        {
            LeanTween.alphaCanvas(
                background,
                0f,
                fadeDuration
            )
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
            TryFinish();
        }
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);

        if (box != null)
        {
            box.anchoredPosition = originalPosition;

            if (boxCanvasGroup != null)
                boxCanvasGroup.alpha = 1f;
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