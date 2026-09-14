using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using EasyTransition;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Next Scene")]
    [Tooltip("Scene tujuan setelah cutscene selesai/skip. Isi dengan scene Tutorial (Latihan).")]
    [SerializeField] private string nextSceneName = "Latihan";

    [Header("Skip")]
    [SerializeField] private GameObject skipButton;

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    private bool videoFinished = false;
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
            SceneManager.GetActiveScene().name);

        StartCoroutine(InitializeAfterTransition());
    }

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

    private IEnumerator PrepareAndPlayVideo()
    {
        if (videoPlayer == null) yield break;

        videoFinished = false;

        videoPlayer.Stop();
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        if (isLoadingNextScene) yield break;

        videoPlayer.Play();

        if (skipButton != null)
            skipButton.SetActive(true);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isLoadingNextScene) return;
        LoadNextScene();
    }

    public void SkipCutscene()
    {
        if (isLoadingNextScene) return;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isLoadingNextScene) return;

        isLoadingNextScene = true;

        if (videoPlayer != null)
            videoPlayer.Stop();

        if (skipButton != null)
            skipButton.SetActive(false);

        // === Selalu ke Tutorial (Latihan) ===
        // Skip atau selesai, keduanya tetap masuk Tutorial.
        // Level 1 di-unlock nanti oleh TutorialManager saat player klik Main.
        Debug.Log($"[CutsceneManager] Cutscene selesai/skip → load '{nextSceneName}'.");

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

        if (videoPlayer.isPrepared &&
            !videoPlayer.isPlaying &&
            !isLoadingNextScene)
        {
            videoPlayer.Play();
        }
    }
}