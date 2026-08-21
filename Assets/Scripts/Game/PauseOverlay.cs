using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseOverlay : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;

    [Header("Retry Button")]
    [SerializeField] private GameObject retryButton;

    [Header("Tutorial & Credits")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject creditPanel;

    [Header("Cutscene")]
    [SerializeField] private CutsceneManager cutsceneManager;

    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;


    // =========================
    // START
    // =========================

    private void Start()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (creditPanel != null)
            creditPanel.SetActive(false);

        UpdateRetryButton();
    }


    // =========================
    // OPEN PAUSE
    // =========================

    public void OpenPause()
    {
        if (isPaused)
            return;

        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        // Kalau sedang Cutscene, pause video
        if (cutsceneManager != null)
            cutsceneManager.PauseVideo();
    }


    // =========================
    // CLOSE PAUSE
    // =========================

    public void ClosePause()
    {
        if (!isPaused)
            return;

        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        // Kalau sedang Cutscene, lanjutkan video
        if (cutsceneManager != null)
            cutsceneManager.ResumeVideo();
    }


    // =========================
    // TUTORIAL
    // =========================

    public void OpenTutorial()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (creditPanel != null)
            creditPanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        Time.timeScale = 0f;

        if (cutsceneManager != null)
            cutsceneManager.PauseVideo();
    }


    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        if (cutsceneManager != null)
            cutsceneManager.PauseVideo();
    }


    // =========================
    // CREDITS
    // =========================

    public void OpenCredits()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (creditPanel != null)
            creditPanel.SetActive(true);

        Time.timeScale = 0f;

        if (cutsceneManager != null)
            cutsceneManager.PauseVideo();
    }


    public void CloseCredits()
    {
        if (creditPanel != null)
            creditPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        if (cutsceneManager != null)
            cutsceneManager.PauseVideo();
    }


    // =========================
    // MAIN MENU
    // =========================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }


    // =========================
    // RETRY
    // =========================

    public void Retry()
    {
        if (!IsGameplayScene())
            return;

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }


    // =========================
    // CHECK GAMEPLAY
    // =========================

    private bool IsGameplayScene()
    {
        return SceneManager.GetActiveScene().name == gameplaySceneName;
    }


    // =========================
    // RETRY VISIBILITY
    // =========================

    private void UpdateRetryButton()
    {
        if (retryButton != null)
            retryButton.SetActive(IsGameplayScene());
    }


    // =========================
    // CLEANUP
    // =========================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}