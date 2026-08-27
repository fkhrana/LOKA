using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainGameplay(Drawing)";

    private void OnEnable()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        videoPlayer?.Play();
    }

    public void PauseVideo()
    {
        videoPlayer?.Pause();
    }

    public void ResumeVideo()
    {
        videoPlayer?.Play();
    }

    public void OnSkipClicked()
    {
        Time.timeScale = 1f;
        videoPlayer?.Stop();
        LoadNextScene();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Time.timeScale = 1f;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        CurtainsAnimation.TransitionToScene(nextSceneName);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}