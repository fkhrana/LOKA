using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject panelMenu;
    public GameObject tutorialMenu;

    [Header("Buttons")]
    public Button pauseButton;
    public Button skipButton;

    [Header("Scene Settings")]
    public int nextSceneIndex = 4;

    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (tutorialMenu != null)
            tutorialMenu.SetActive(false);

        if (pauseButton != null)
            pauseButton.interactable = true;

        if (skipButton != null)
            skipButton.gameObject.SetActive(true);
    }

    // =========================
    // PAUSE
    // =========================
    public void OnPauseClicked()
    {
        if (isPaused) return;

        isPaused = true;

        if (panelMenu != null)
            panelMenu.SetActive(true);

        Time.timeScale = 0f;

        if (videoPlayer != null)
            videoPlayer.Pause();

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    // =========================
    // CONTINUE
    // =========================
    public void OnContinueClicked()
    {
        ResumeVideo();
    }

    // =========================
    // CLOSE MENU
    // =========================
    public void OnCloseClicked()
    {
        ResumeVideo();
    }

    private void ResumeVideo()
    {
        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (tutorialMenu != null)
            tutorialMenu.SetActive(false);

        Time.timeScale = 1f;

        if (videoPlayer != null)
            videoPlayer.Play();

        if (pauseButton != null)
            pauseButton.interactable = true;

        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        isPaused = false;
    }

    // =========================
    // OPEN TUTORIAL
    // =========================
    public void OnTutorialClicked()
    {
        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (tutorialMenu != null)
            tutorialMenu.SetActive(true);

        if (pauseButton != null)
            pauseButton.interactable = false;

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    // =========================
    // CLOSE TUTORIAL
    // =========================
    public void OnTutorialCloseClicked()
    {
        if (tutorialMenu != null)
            tutorialMenu.SetActive(false);

        if (panelMenu != null)
            panelMenu.SetActive(true);

        if (pauseButton != null)
            pauseButton.interactable = true;

        // Skip tetap disembunyikan karena masih berada di menu pause
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    // =========================
    // SKIP
    // =========================
    public void OnSkipClicked()
    {
        Time.timeScale = 1f;

        if (videoPlayer != null)
            videoPlayer.Stop();

        SceneManager.LoadScene(nextSceneIndex);
    }
}