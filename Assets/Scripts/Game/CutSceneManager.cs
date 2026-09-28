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

    [Tooltip("Path relatif dari folder StreamingAssets, contoh: Video/startscene.mp4")]
    [SerializeField] private string videoFileName = "Video/startscene.mp4";

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("Skip")]
    [SerializeField] private GameObject skipButton;

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    private bool isLoadingNextScene = false;
    private TransitionManager transitionManager;

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

        transitionManager = null;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        GameProgressManager.SaveLastScene(SceneManager.GetActiveScene().name);

        transitionManager = TransitionManager.Instance();

        if (skipButton != null)
            skipButton.SetActive(true);

        SetupAndPlayVideo();
    }

    private void SetupAndPlayVideo()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[CutsceneManager] VideoPlayer belum di-assign.");
            return;
        }

        // Jangan autoplay, kita play manual setelah Prepare selesai
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

        if (videoPlayer != null)
            videoPlayer.Stop();

        if (skipButton != null)
            skipButton.SetActive(false);

        GameProgressManager.SetCutsceneCompleted();
        LevelManager.Instance?.UnlockFirstLevel();

        Debug.Log($"[CutsceneManager] {cutsceneType} selesai → load '{nextSceneName}'.");

        TransitionManager tm = transitionManager;

        if (tm == null)
            tm = TransitionManager.Instance();

        if (tm != null && transitionSettings != null)
        {
            tm.Transition(nextSceneName, transitionSettings, loadDelay);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
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