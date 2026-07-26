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
        {
            pauseButton.interactable = true;
        }

        if (skipButton != null)
        {
            skipButton.interactable = true;
        }
    }

// pause button
    public void OnPauseClicked()
    {
        if (isPaused) return;

        isPaused = true;

        if (panelMenu != null)
            panelMenu.SetActive(true);

        Time.timeScale = 0f;

        if (videoPlayer != null)
            videoPlayer.Pause();

        // Disable tombol tanpa menyembunyikan
        if (pauseButton != null)
            pauseButton.interactable = false;

        if (skipButton != null)
            skipButton.interactable = false;
    }

// continue button
    public void OnContinueClicked()
    {
        ResumeVideo();
    }

// close button
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
            skipButton.interactable = true;

        isPaused = false;
    }

//    tutorial panel
    public void OnTutorialClicked()
    {
        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (tutorialMenu != null)
            tutorialMenu.SetActive(true);

        // disable tombol selama tutorial
        if (pauseButton != null)
            pauseButton.interactable = false;

        if (skipButton != null)
            skipButton.interactable = false;
    }
// close button tutorial
    public void OnTutorialCloseClicked()
    {
        if (tutorialMenu != null)
            tutorialMenu.SetActive(false);

        if (panelMenu != null)
            panelMenu.SetActive(true);

        // Masih berada di menu pause
        if (pauseButton != null)
            pauseButton.interactable = false;

        if (skipButton != null)
            skipButton.interactable = false;
    }

    // skip button
    public void OnSkipClicked()
    {
        if (!skipButton.interactable)
            return;

        Time.timeScale = 1f;

        if (videoPlayer != null)
            videoPlayer.Stop();

        SceneManager.LoadScene(nextSceneIndex);
    }
}