using UnityEngine;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainGameplay(Drawing)";
    [SerializeField] private string homeSceneName = "MainMenu";

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

    // Pause video and play button SFX.
    public void PauseVideo()
    {
        if (isTransitioning) return;

        PlayButtonClickSFX();
        videoPlayer?.Pause();
    }

    // Resume video and play button SFX.
    public void ResumeVideo()
    {
        if (isTransitioning) return;

        PlayButtonClickSFX();
        videoPlayer?.Play();
    }

    // Skip the cutscene and transition to gameplay.
    public void OnSkipClicked()
    {
        if (isTransitioning) return;

        PlayButtonClickSFX();

        Time.timeScale = 1f;
        videoPlayer?.Stop();
        LoadNextScene();
    }

    // Return to the main menu.
    public void GoToHome()
    {
        if (isTransitioning) return;

        PlayButtonClickSFX();

        Time.timeScale = 1f;
        videoPlayer?.Stop();

        if (string.IsNullOrEmpty(homeSceneName))
        {
            Debug.LogError("homeSceneName is empty! Assign a valid scene name.", this);
            isTransitioning = false;
            return;
        }

        isTransitioning = true;
        CurtainsAnimation.TransitionToScene(homeSceneName);
    }

    // Play the button click SFX through the AudioManager.
    private void PlayButtonClickSFX()
    {
        AudioManager.Instance?.PlaySFX("Button Click (1)");
    }

    // Load the next scene when the video finishes.
    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isTransitioning) return;

        Time.timeScale = 1f;
        LoadNextScene();
    }

    // Transition to the next scene.
    private void LoadNextScene()
    {
        if (isTransitioning) return;

        isTransitioning = true;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("nextSceneName is empty! Assign a valid scene name.", this);
            isTransitioning = false;
            return;
        }

        CurtainsAnimation.TransitionToScene(nextSceneName);
    }
}