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

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "MainGameplay(Drawing)";

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
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;

        transitionManager = null;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        GameProgressManager.SaveLastScene(
            SceneManager.GetActiveScene().name
        );

        StartCoroutine(InitializeAfterTransition());
    }

    // Tunggu transisi selesai baru play video.
    private IEnumerator InitializeAfterTransition()
    {
        transitionManager = TransitionManager.Instance();

        if (transitionManager == null)
        {
            yield return StartCoroutine(PrepareAndPlayVideo());
            yield break;
        }

        while (transitionManager.IsTransitionRunning())
            yield return null;

        yield return StartCoroutine(PrepareAndPlayVideo());
    }

    // Prepare video lalu play.
    private IEnumerator PrepareAndPlayVideo()
    {
        if (videoPlayer == null)
            yield break;

        videoPlayer.Stop();
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        if (isLoadingNextScene)
            yield break;

        videoPlayer.Play();

        if (skipButton != null)
            skipButton.SetActive(true);
    }

    // Callback saat video selesai.
    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isLoadingNextScene)
            return;

        LoadNextScene();
    }

    // Tombol skip cutscene.
    public void SkipCutscene()
    {
        if (isLoadingNextScene)
            return;

        LoadNextScene();
    }

    // Stop video dan load scene berikutnya.
    private void LoadNextScene()
    {
        if (isLoadingNextScene)
            return;

        isLoadingNextScene = true;

        if (videoPlayer != null)
            videoPlayer.Stop();

        if (skipButton != null)
            skipButton.SetActive(false);

        Debug.Log(
            $"[CutsceneManager] {cutsceneType} selesai → load '{nextSceneName}'."
        );

        TransitionManager tm = transitionManager;

        if (tm == null)
            tm = TransitionManager.Instance();

        if (tm != null && transitionSettings != null)
        {
            tm.Transition(
                nextSceneName,
                transitionSettings,
                loadDelay
            );
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // Pause video.
    public void PauseVideo()
    {
        if (videoPlayer == null)
            return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
    }

    // Resume video.
    public void ResumeVideo()
    {
        if (videoPlayer == null)
            return;

        if (videoPlayer.isPrepared &&
            !videoPlayer.isPlaying &&
            !isLoadingNextScene)
        {
            videoPlayer.Play();
        }
    }
}