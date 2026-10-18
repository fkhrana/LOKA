using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LatihanTulisController : MonoBehaviour
{
    [Header("Gesture")]
    [SerializeField] private CanvasGestureDrawer gestureDrawer;
    [SerializeField] private LatihanAksaraItem[] aksaraItems;

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private int startingIndex;

    [Header("Audio")]
    [SerializeField] private AksaraSoundLibrary soundLibrary;
    [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;

    [Header("Correct Gesture VFX")]
    [SerializeField] private EfekConfetti[] correctConfetti;
    [SerializeField] private AudioClip correctGestureSfx;
    [SerializeField, Range(0f, 1f)] private float correctGestureSfxVolume = 1f;
    [SerializeField] private TMP_Text correctFeedbackText;

    private int currentIndex;
    private readonly Dictionary<EfekConfetti, Coroutine> confettiHideCoroutines =
        new Dictionary<EfekConfetti, Coroutine>();
    private Coroutine feedbackTextHideCoroutine;

    public LatihanAksaraItem ActiveItem
    {
        get
        {
            if (aksaraItems == null ||
                currentIndex < 0 ||
                currentIndex >= aksaraItems.Length)
            {
                return null;
            }

            return aksaraItems[currentIndex];
        }
    }

    private void Awake()
    {
        if (correctFeedbackText != null)
            correctFeedbackText.gameObject.SetActive(false);

        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPrevious);

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNext);
    }

    private void Start()
    {
        if (gestureDrawer == null)
            Debug.LogError("LatihanTulisController membutuhkan CanvasGestureDrawer.", this);

        if (aksaraItems == null || aksaraItems.Length == 0)
        {
            Debug.LogError("LatihanTulisController tidak memiliki panel aksara.", this);
            return;
        }

        currentIndex = Mathf.Clamp(startingIndex, 0, aksaraItems.Length - 1);
        ShowCurrentItem();
    }

    private void OnEnable()
    {
        if (gestureDrawer != null)
            gestureDrawer.GestureRecognized += HandleGestureRecognized;
    }

    private void OnDisable()
    {
        if (gestureDrawer != null)
            gestureDrawer.GestureRecognized -= HandleGestureRecognized;
    }

    public void ShowNext()
    {
        if (!HasItems())
            return;

        currentIndex = (currentIndex + 1) % aksaraItems.Length;
        ShowCurrentItem();
    }

    public void ShowPrevious()
    {
        if (!HasItems())
            return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = aksaraItems.Length - 1;

        ShowCurrentItem();
    }

    public void PlayVoiceOver(GestureShape shape)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning("Sound Library belum diisi pada LatihanTulisController.", this);
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager tidak tersedia di scene.", this);
            return;
        }

        AudioClip clip = soundLibrary.GetClip(shape);
        if (clip == null)
        {
            Debug.LogWarning(
                "Audio aksara belum tersedia untuk GestureShape " + shape + ".",
                this);
            return;
        }

        float libraryVolume = soundLibrary.GetVolume(shape);
        float finalVolume = Mathf.Clamp(voiceVolume * libraryVolume * 8f, 0f, 10f);
        AudioManager.Instance.PlayLoudSFX(clip, finalVolume);
    }

    private void ShowCurrentItem()
    {
        LatihanAksaraItem activeItem = ActiveItem;
        if (activeItem == null)
            return;

        for (int i = 0; i < aksaraItems.Length; i++)
        {
            if (aksaraItems[i] != null)
                aksaraItems[i].gameObject.SetActive(i == currentIndex);
        }

        if (gestureDrawer != null)
            gestureDrawer.SetExpectedShape(activeItem.GestureShape);

        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        bool hasItems = HasItems();

        if (previousButton != null)
            previousButton.interactable = hasItems;

        if (nextButton != null)
            nextButton.interactable = hasItems;
    }

    private void HandleGestureRecognized(
        System.Collections.Generic.List<System.Collections.Generic.List<Vector2>> strokes,
        GestureRecognitionResult result)
    {
        LatihanAksaraItem activeItem = ActiveItem;
        if (activeItem == null ||
            result.DetectedShape != activeItem.GestureShape ||
            !result.IsRecognized ||
            !result.MatchesExpected)
        {
            return;
        }

        PlayCorrectFeedback();
    }

    private void PlayCorrectFeedback()
    {
        ShowCorrectFeedbackText();

        if (correctGestureSfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISFX(
                correctGestureSfx,
                correctGestureSfxVolume);
        }

        if (correctConfetti == null)
            return;

        foreach (EfekConfetti confetti in correctConfetti)
        {
            if (confetti == null)
                continue;

            StopConfettiHideCoroutine(confetti);
            confetti.gameObject.SetActive(true);
            confetti.MuntahkanConfetti();
            confettiHideCoroutines[confetti] = StartCoroutine(
                HideConfettiAfterDelay(confetti));
        }
    }

    private void ShowCorrectFeedbackText()
    {
        if (correctFeedbackText == null)
            return;

        if (feedbackTextHideCoroutine != null)
            StopCoroutine(feedbackTextHideCoroutine);

        correctFeedbackText.gameObject.SetActive(true);
        feedbackTextHideCoroutine = StartCoroutine(HideCorrectFeedbackTextAfterDelay());
    }

    private IEnumerator HideCorrectFeedbackTextAfterDelay()
    {
        float visibleDuration = correctGestureSfx != null
            ? correctGestureSfx.length
            : 0f;

        yield return new WaitForSecondsRealtime(visibleDuration);

        if (correctFeedbackText != null)
            correctFeedbackText.gameObject.SetActive(false);

        feedbackTextHideCoroutine = null;
    }

    private IEnumerator HideConfettiAfterDelay(EfekConfetti confetti)
    {
        float visibleDuration = correctGestureSfx != null
            ? correctGestureSfx.length
            : 0f;

        yield return new WaitForSecondsRealtime(visibleDuration);

        if (confetti != null)
            confetti.gameObject.SetActive(false);

        if (confetti != null)
            confettiHideCoroutines.Remove(confetti);
    }

    private void StopConfettiHideCoroutine(EfekConfetti confetti)
    {
        if (!confettiHideCoroutines.TryGetValue(confetti, out Coroutine coroutine))
            return;

        if (coroutine != null)
            StopCoroutine(coroutine);

        confettiHideCoroutines.Remove(confetti);
    }

    private bool HasItems()
    {
        return aksaraItems != null && aksaraItems.Length > 0;
    }

    private void OnDestroy()
    {
        if (feedbackTextHideCoroutine != null)
            StopCoroutine(feedbackTextHideCoroutine);

        foreach (Coroutine coroutine in confettiHideCoroutines.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        confettiHideCoroutines.Clear();

        if (previousButton != null)
            previousButton.onClick.RemoveListener(ShowPrevious);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(ShowNext);
    }
}
