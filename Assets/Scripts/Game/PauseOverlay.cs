using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseOverlay : MonoBehaviour
{
    private enum PanelType { None, Pause, Tutorial, Credits }

    [Header("Panels")] [SerializeField] private GameObject pausePanel, tutorialPanel, creditPanel;
    [Header("Buttons")] [SerializeField] private GameObject retryButton;
    [Header("Cutscene")] [SerializeField] private CutsceneManager cutsceneManager;
    [Header("Scene Names")] [SerializeField] private string gameplaySceneName = "Gameplay", mainMenuSceneName = "MainMenu";

    private PanelType currentPanel = PanelType.None;
    private bool isClosing = false;

    private void Start()
    {
        Time.timeScale = 1f;
        CloseAllPanels();
        UpdateRetryButton();
    }
    private void OnDestroy() => Time.timeScale = 1f;

    private void OpenPanel(PanelType type)
    {
        if (currentPanel == type || isClosing) return;
        CloseAllPanels();
        if (type != PanelType.None)
        {
            Time.timeScale = 0f;
            cutsceneManager?.PauseVideo();
        }
        switch (type)
        {
            case PanelType.Pause:   pausePanel?.SetActive(true); break;
            case PanelType.Tutorial: tutorialPanel?.SetActive(true); break;
            case PanelType.Credits: creditPanel?.SetActive(true); break;
        }
        currentPanel = type;
    }

    private void ClosePanel(PanelType type)
    {
        if (currentPanel != type || isClosing) return;
        isClosing = true;
        if (type == PanelType.Pause)
        {
            CloseAllPanels();
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            currentPanel = PanelType.None;
        }
        else
        {
            CloseAllPanels();
            pausePanel?.SetActive(true);
            currentPanel = PanelType.Pause;
        }
        isClosing = false;
    }

    private void CloseAllPanels()
    {
        pausePanel?.SetActive(false);
        tutorialPanel?.SetActive(false);
        creditPanel?.SetActive(false);
    }

    private void CloseWithEffect(GameObject panel, PanelType type)
    {
        if (panel == null || currentPanel != type) return;
        EffectPanel effect = panel.GetComponent<EffectPanel>();
        if (effect != null)
            effect.CloseDialog(() => ClosePanel(type));
        else
            ClosePanel(type);
    }

    public void OpenPause() => OpenPanel(PanelType.Pause);
    public void ClosePause() => CloseWithEffect(pausePanel, PanelType.Pause);

    public void OpenTutorial() => OpenPanel(PanelType.Tutorial);
    public void CloseTutorial() => CloseWithEffect(tutorialPanel, PanelType.Tutorial);

    public void OpenCredits() => OpenPanel(PanelType.Credits);
    public void CloseCredits() => CloseWithEffect(creditPanel, PanelType.Credits);

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        CurtainsAnimation.TransitionToScene(mainMenuSceneName);
    }

    public void Retry()
    {
        if (!IsGameplayScene()) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool IsGameplayScene() => SceneManager.GetActiveScene().name == gameplaySceneName;
    private void UpdateRetryButton() { if (retryButton) retryButton.SetActive(IsGameplayScene()); }
}