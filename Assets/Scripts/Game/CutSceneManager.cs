using UnityEngine;
using UnityEngine.Video;
using EasyTransition;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainGameplay(Drawing)";
    [SerializeField] private string homeSceneName = "MainMenu";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0.5f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
            Debug.LogWarning("VideoPlayer not assigned in CutsceneManager!", this);
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        videoPlayer?.Play();
    }

    public void PauseVideo()
    {
        if (isTransitioning) return;
        PlayButtonClickSFX();
        videoPlayer?.Pause();
    }

    public void ResumeVideo()
    {
        if (isTransitioning) return;
        PlayButtonClickSFX();
        videoPlayer?.Play();
    }

    public void OnSkipClicked()
    {
        if (isTransitioning) return;

        PlayButtonClickSFX();
        Time.timeScale = 1f;
        videoPlayer?.Stop();

        // Hanya panggil LoadNextScene() – di dalamnya sudah urus transisi
        LoadNextScene();
    }

    private void PlayButtonClickSFX()
    {
        AudioManager.Instance?.PlaySFX("Button Click (1)");
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isTransitioning) return;
        Time.timeScale = 1f;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isTransitioning) return;

        isTransitioning = true;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("nextSceneName is empty!", this);
            isTransitioning = false;
            return;
        }

        var tm = TransitionManager.Instance();
        if (tm != null && transitionSettings != null)
        {
            tm.Transition(nextSceneName, transitionSettings, loadDelay);
        }
        else
        {
            Debug.LogWarning("TransitionManager or Settings missing, loading scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}