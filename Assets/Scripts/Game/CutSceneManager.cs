using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject tutorialMenu;
    public GameObject creditsMenu;

    public Button pauseButton;
    public Button skipButton;
    public Button creditsButton;
    public Button homeButton;
    public Button BackButton;

    public int nextSceneIndex = 3;

    public VideoPlayer videoPlayer;

    private bool isPaused = false;


    // =========================
    // START
    // =========================

    void Start()
    {
        Time.timeScale = 1f;

        // Sembunyikan semua panel
        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (tutorialMenu != null)
            tutorialMenu.SetActive(false);

        if (creditsMenu != null)
            creditsMenu.SetActive(false);

        // Aktifkan tombol utama
        if (pauseButton != null)
            pauseButton.interactable = true;

        if (skipButton != null)
            skipButton.interactable = true;

        if (creditsButton != null)
            creditsButton.interactable = true;

        if (homeButton != null)
            homeButton.interactable = true;

        isPaused = false;
    }


    // =========================
    // PAUSE BUTTON
    // =========================

    public void OnPauseClicked()
    {
        if (isPaused)
            return;

        isPaused = true;

        // Buka Settings Panel
        if (panelMenu != null)
            panelMenu.SetActive(true);

        PauseVideo();

        // Disable tombol di belakang
        DisableMainButtons();
    }


    // =========================
    // CONTINUE BUTTON
    // =========================

    public void OnContinueClicked()
    {
        ResumeVideo();
    }


    // =========================
    // CLOSE SETTINGS
    // =========================

    public void OnCloseClicked()
    {
        ResumeVideo();
    }


    // =========================
    // TUTORIAL BUTTON
    // =========================

    public void OnTutorialClicked()
    {
        // Tutup Settings
        if (panelMenu != null)
            panelMenu.SetActive(false);

        // Buka Tutorial
        if (tutorialMenu != null)
            tutorialMenu.SetActive(true);

        // Tetap pause
        PauseVideo();

        DisableMainButtons();
    }


    // =========================
    // BACK FROM TUTORIAL
    // =========================

    public void OnTutorialBackClicked()
    {
        // Tutup Tutorial
        if (tutorialMenu != null)
            tutorialMenu.SetActive(false);

        // Kembali ke Settings
        if (panelMenu != null)
            panelMenu.SetActive(true);

        // Tetap pause
        PauseVideo();

        DisableMainButtons();
    }


    // =========================
    // CREDITS BUTTON
    // =========================

    public void OnCreditsClicked()
    {
        // Tutup Settings
        if (panelMenu != null)
            panelMenu.SetActive(false);

        // Buka Credits
        if (creditsMenu != null)
            creditsMenu.SetActive(true);

        // Tetap pause
        PauseVideo();

        DisableMainButtons();
    }


    // =========================
    // BACK FROM CREDITS
    // =========================

    public void OnCreditsBackClicked()
    {
        // Tutup Credits
        if (creditsMenu != null)
            creditsMenu.SetActive(false);

        // Kembali ke Settings
        if (panelMenu != null)
            panelMenu.SetActive(true);

        // Tetap pause
        PauseVideo();

        DisableMainButtons();
    }


    // =========================
    // HOME / CURRENT SCENE
    // =========================

    public void OnHomeClicked()
    {
        // Kembalikan Time Scale
        Time.timeScale = 1f;

        // Stop video
        if (videoPlayer != null)
            videoPlayer.Stop();

        // Restart scene yang sedang aktif
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }


    // =========================
    // SKIP BUTTON
    // =========================

    public void OnSkipClicked()
    {
        if (skipButton != null && !skipButton.interactable)
            return;

        // Kembalikan Time Scale
        Time.timeScale = 1f;

        // Stop video
        if (videoPlayer != null)
            videoPlayer.Stop();

        // Pindah ke scene berikutnya
        SceneManager.LoadScene(nextSceneIndex);
    }


    // =========================
    // PAUSE VIDEO
    // =========================

    private void PauseVideo()
    {
        Time.timeScale = 0f;

        if (videoPlayer != null)
            videoPlayer.Pause();

        isPaused = true;
    }


    // =========================
    // RESUME VIDEO
    // =========================

    private void ResumeVideo()
    {
        // Tutup semua panel
        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (tutorialMenu != null)
            tutorialMenu.SetActive(false);

        if (creditsMenu != null)
            creditsMenu.SetActive(false);

        // Jalankan kembali waktu
        Time.timeScale = 1f;

        // Lanjutkan video
        if (videoPlayer != null)
            videoPlayer.Play();

        // Aktifkan tombol utama
        EnableMainButtons();

        isPaused = false;
    }


    // =========================
    // DISABLE MAIN BUTTONS
    // =========================

    private void DisableMainButtons()
    {
        if (pauseButton != null)
            pauseButton.interactable = false;

        if (skipButton != null)
            skipButton.interactable = false;
    }


    // =========================
    // ENABLE MAIN BUTTONS
    // =========================

    private void EnableMainButtons()
    {
        if (pauseButton != null)
            pauseButton.interactable = true;

        if (skipButton != null)
            skipButton.interactable = true;
    }
}