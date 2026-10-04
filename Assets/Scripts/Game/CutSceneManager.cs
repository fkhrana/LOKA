using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using EasyTransition;

public class CutsceneManager : MonoBehaviour
{
    public enum CutsceneType
    {
        Opening,
        Ending
    }

    [Header("Cutscene")]
    [SerializeField] private CutsceneType cutsceneType = CutsceneType.Opening;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Tooltip("Path relatif dari folder StreamingAssets, contoh: Video/startscene_new.mp4")]
    [SerializeField] private string videoFileName = "Video/startscene_new.mp4";

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Tooltip("Scene tujuan kalau cutscene dipicu dari tombol Mulai Belajar.")]
    [SerializeField] private string guidedLevelSceneName = "MainGameplay(Drawing)";

    [Header("Skip")]
    [SerializeField] private GameObject skipButton;

    [Tooltip("Tombol pause (opsional).")]
    [SerializeField] private GameObject pauseButton;

    [Tooltip("Delay sebelum tombol skip & pause muncul (detik, dihitung dari video play).")]
    [SerializeField, Min(0f)] private float buttonAppearDelay = 3f;

    [Tooltip("Durasi fade in tombol.")]
    [SerializeField, Min(0f)] private float buttonFadeDuration = 0.3f;

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    private bool isLoadingNextScene = false;
    private TransitionManager transitionManager;
    private Coroutine showButtonsCoroutine;

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        StopShowButtonsCoroutine();
        transitionManager = null;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        GameProgressManager.SaveLastScene(SceneManager.GetActiveScene().name);

        transitionManager = TransitionManager.Instance();

        HideCutsceneButtons();

        SetupAndPlayVideo();
    }

    private void SetupAndPlayVideo()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[CutsceneManager] VideoPlayer belum di-assign.");
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Application.streamingAssetsPath + "/" + videoFileName;

        Debug.Log($"[CutsceneManager] Video URL: {videoPlayer.url}");

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (isLoadingNextScene)
            return;

        vp.Play();

        StopShowButtonsCoroutine();
        showButtonsCoroutine = StartCoroutine(ShowButtonsAfterDelay());
    }

    private IEnumerator ShowButtonsAfterDelay()
    {
        float elapsed = 0f;

        while (elapsed < buttonAppearDelay && !isLoadingNextScene)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (isLoadingNextScene)
        {
            showButtonsCoroutine = null;
            yield break;
        }

        yield return FadeInButton(skipButton);
        yield return FadeInButton(pauseButton);

        showButtonsCoroutine = null;
    }

    private IEnumerator FadeInButton(GameObject button)
    {
        if (button == null) yield break;

        button.SetActive(true);

        var cg = button.GetComponent<CanvasGroup>();
        if (cg == null) cg = button.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        if (buttonFadeDuration <= 0f)
        {
            cg.alpha = 1f;
            yield break;
        }

        float t = 0f;

        while (t < buttonFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / buttonFadeDuration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    private void HideCutsceneButtons()
    {
        SetButtonVisibleInstant(skipButton, false);
        SetButtonVisibleInstant(pauseButton, false);
    }

    private void SetButtonVisibleInstant(GameObject button, bool visible)
    {
        if (button == null) return;

        var cg = button.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = visible ? 1f : 0f;

        button.SetActive(visible);
    }

    private void StopShowButtonsCoroutine()
    {
        if (showButtonsCoroutine != null)
        {
            StopCoroutine(showButtonsCoroutine);
            showButtonsCoroutine = null;
        }
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[CutsceneManager] VideoPlayer error: {message}");
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isLoadingNextScene)
            return;

        LoadNextScene();
    }

    public void SkipCutscene()
    {
        if (isLoadingNextScene)
            return;

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isLoadingNextScene)
            return;

        isLoadingNextScene = true;

        StopShowButtonsCoroutine();

        if (videoPlayer != null)
            videoPlayer.Stop();

        HideCutsceneButtons();

        GameProgressManager.SetCutsceneCompleted();
        LevelManager.Instance?.UnlockFirstLevel();

        string targetScene = ResolveNextScene();

        Debug.Log($"[CutsceneManager] {cutsceneType} selesai → load '{targetScene}'.");

        TransitionManager tm = transitionManager;

        if (tm == null)
            tm = TransitionManager.Instance();

        if (tm != null && transitionSettings != null)
        {
            tm.Transition(targetScene, transitionSettings, loadDelay);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    private string ResolveNextScene()
    {
        if (GameProgressManager.PendingGuidedAfterCutscene)
        {
            GameProgressManager.SetPendingGuidedAfterCutscene(false);

            if (string.IsNullOrEmpty(guidedLevelSceneName))
            {
                Debug.LogWarning("[CutsceneManager] guidedLevelSceneName kosong → fallback ke nextSceneName.");
                return nextSceneName;
            }

            Debug.Log("[CutsceneManager] PendingGuidedAfterCutscene → lanjut ke level guided.");
            return guidedLevelSceneName;
        }

        return nextSceneName;
    }

    public void PauseVideo()
    {
        if (videoPlayer == null) return;
        if (videoPlayer.isPlaying) videoPlayer.Pause();
    }

    public void ResumeVideo()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPrepared && !videoPlayer.isPlaying && !isLoadingNextScene)
        {
            videoPlayer.Play();
        }
    }
}