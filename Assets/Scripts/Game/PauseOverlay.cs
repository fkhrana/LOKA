using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EasyTransition;

public class PauseOverlay : MonoBehaviour
{
    private enum PanelType { None, Pause, Tutorial }

    [System.Serializable]
    private class PanelData
    {
        public PanelType type;
        public GameObject panel;
    }

    [Header("Panels")]
    [SerializeField] private PanelData[] panels;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button playButton;

    [Header("Cutscene")]
    [SerializeField] private CutsceneManager cutsceneManager;

    [Header("Gesture")]
    [SerializeField] private GestureDrawer gestureDrawer;

    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "MainGameplay(Drawing)";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    private PanelType currentPanel = PanelType.None;
    private bool isClosing = false;
    private bool isTransitioning = false;

    private void Start()
    {
        Time.timeScale = 1f;
        CloseAllPanels();

        if (playButton != null)
            playButton.onClick.AddListener(ResumeGame);
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
        Time.timeScale = 1f;

        if (playButton != null)
            playButton.onClick.RemoveListener(ResumeGame);
    }

    private PanelData GetPanelData(PanelType type)
    {
        foreach (var p in panels)
            if (p.type == type) return p;

        return null;
    }

    private GameObject GetPanel(PanelType type) => GetPanelData(type)?.panel;

    private void OpenPanel(PanelType type)
    {
        if (currentPanel == type || isClosing || isTransitioning)
            return;

        CloseAllPanels();

        if (type != PanelType.None)
        {
            Time.timeScale = 0f;
            cutsceneManager?.PauseVideo();
            DisableGestureInput();
        }

        GetPanel(type)?.SetActive(true);
        currentPanel = type;
    }

    private void ClosePanel(PanelType type, System.Action onComplete = null)
    {
        if (currentPanel != type || isClosing || isTransitioning)
        {
            onComplete?.Invoke();
            return;
        }

        isClosing = true;

        if (type == PanelType.Pause)
        {
            CloseAllPanels();
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            EnableGestureInput();
            currentPanel = PanelType.None;
        }
        else
        {
            CloseAllPanels();

            var pausePanel = GetPanel(PanelType.Pause);
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                FadeIn(pausePanel);
            }

            currentPanel = PanelType.Pause;
        }

        isClosing = false;
        onComplete?.Invoke();
    }

    private void CloseAllPanels()
    {
        foreach (var data in panels)
            if (data.panel != null) data.panel.SetActive(false);
    }

    private void CloseWithEffect(PanelType type)
    {
        var panel = GetPanel(type);

        if (panel == null || currentPanel != type) return;

        var effect = panel.GetComponent<EffectPanel>();

        if (effect != null)
            effect.CloseDialog(() => ClosePanel(type));
        else
            ClosePanel(type);
    }

    private void FadeIn(GameObject obj)
    {
        if (obj == null) return;

        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        LeanTween.alphaCanvas(cg, 1f, 0.25f).setIgnoreTimeScale(true);
    }

    private void DisableGestureInput()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.ResetGestureInput();
            gestureDrawer.enabled = false;
        }
    }

    private void EnableGestureInput()
    {
        if (gestureDrawer != null)
            gestureDrawer.enabled = true;
    }

    public void OpenPause()    => OpenPanel(PanelType.Pause);
    public void ClosePause()   => CloseWithEffect(PanelType.Pause);
    public void OpenTutorial() => OpenPanel(PanelType.Tutorial);
    public void CloseTutorial()=> CloseWithEffect(PanelType.Tutorial);

    public void ResumeGame()
    {
        if (currentPanel == PanelType.Pause)
        {
            ClosePause();
        }
        else if (currentPanel == PanelType.Tutorial)
        {
            CloseAllPanels();
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            EnableGestureInput();
            currentPanel = PanelType.None;
        }
        else
        {
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            EnableGestureInput();
        }
    }

    public void GoToMainMenu()
    {
        if (isTransitioning)
            return;

        EnemyWaveSpawner enemyWaveSpawner =
            FindFirstObjectByType<EnemyWaveSpawner>();

        if (enemyWaveSpawner != null)
            enemyWaveSpawner.SaveCurrentWave();

        // === Save posisi player ===
        SaveCurrentProgress saveProgress =
            FindFirstObjectByType<SaveCurrentProgress>();

        if (saveProgress != null)
            saveProgress.SavePlayerPositionNow();

        // === Tandai sudah masuk gameplay ===
        GameProgressManager.SetHasEnteredGameplay(true);

        GameProgressManager.SaveLastScene(
            SceneManager.GetActiveScene().name
        );

        isTransitioning = true;

        if (pauseButton != null)
            pauseButton.interactable = false;

        LeanTween.cancel(gameObject);
        Time.timeScale = 1f;
        StopAllCoroutines();
        CloseAllPanels();
        EnableGestureInput();

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[PauseOverlay] Main Menu Scene Name kosong!", this);
            isTransitioning = false;

            if (pauseButton != null)
                pauseButton.interactable = true;

            return;
        }

        TransitionManager tm = TransitionManager.Instance();

        if (tm != null && transitionSettings != null)
        {
            tm.Transition(mainMenuSceneName, transitionSettings, loadDelay);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);

            isTransitioning = false;

            if (pauseButton != null)
                pauseButton.interactable = true;
        }
    }
}