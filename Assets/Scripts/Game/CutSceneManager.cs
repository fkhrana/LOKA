using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainGameplay(Drawing)";


    // =========================
    // START
    // =========================

    private void Start()
    {
        Time.timeScale = 1f;

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }


    // =========================
    // PAUSE VIDEO
    // =========================

    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }


    // =========================
    // RESUME VIDEO
    // =========================

    public void ResumeVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }


    // =========================
    // SKIP CUTSCENE
    // =========================

    public void OnSkipClicked()
    {
        Time.timeScale = 1f;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

         TransisiManager.Instance.LoadScene(nextSceneName);
    }


    // =========================
    // VIDEO FINISHED
    // =========================

    public void OnVideoFinished(VideoPlayer vp)
    {
        Time.timeScale = 1f;

         TransisiManager.Instance.LoadScene(nextSceneName);
    }


    // =========================
    // CLEANUP
    // =========================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}