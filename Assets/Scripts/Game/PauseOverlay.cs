using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EasyTransition;

public class PauseOverlay : MonoBehaviour
{
    private enum PanelType { None, Pause, Tutorial, Guided }

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

    // ✅ BARU: Hide guided tutorial panel + circle saat pause
    [SerializeField] private GameObject guidedTutorialPanelToHide;
    [SerializeField] private TutorialHintManager tutorialHintManager;

    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "MainGameplay(Drawing)";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;
    [SerializeField] private float bgmFadeOutDuration = 0.8f;

    private PanelType currentPanel = PanelType.None;
    private bool isClosing = false;
    private bool isTransitioning = false;

    private TutorialManager cachedTutorialManager;
    private bool levelCompletionSaved = false;

    private void Start()
    {
        Time.timeScale = 1f;
        CloseAllPanels();

        if (playButton != null)
            playButton.onClick.AddListener(ResumeGame);

        SubscribeLevelComplete();
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
        Time.timeScale = 1f;

        if (playButton != null)
            playButton.onClick.RemoveListener(ResumeGame);

        UnsubscribeLevelComplete();
    }

    private void SubscribeLevelComplete()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogWarning("[PauseOverlay] LevelProgressManager.Instance null — skip subscribe.");
            return;
        }

        LevelProgressManager.Instance.OnReachedLevelComplete.RemoveListener(OnLevelComplete);
        LevelProgressManager.Instance.OnReachedLevelComplete.AddListener(OnLevelComplete);

        Debug.Log("[PauseOverlay] Subscribe OnReachedLevelComplete ✅");
    }

    private void UnsubscribeLevelComplete()
    {
        if (LevelProgressManager.Instance == null) return;
        LevelProgressManager.Instance.OnReachedLevelComplete.RemoveListener(OnLevelComplete);
    }

    private void OnLevelComplete()
    {
        if (levelCompletionSaved) return;
        levelCompletionSaved = true;

        Debug.Log("[PauseOverlay] OnReachedLevelComplete fired ✅");
    }

    private TutorialManager GetTutorialManager()
    {
        if (cachedTutorialManager == null)
            cachedTutorialManager = FindFirstObjectByType<TutorialManager>();

        return cachedTutorialManager;
    }

    private PanelData GetPanelData(PanelType type)
    {
        foreach (var p in panels)
        {
            if (p.type == type) return p;
        }
        return null;
    }

    private GameObject GetPanel(PanelType type) => GetPanelData(type)?.panel;

    private void SetIntroUIVisible(bool visible)
    {
        if (CameraIntroManager.Instance != null)
            CameraIntroManager.Instance.SetIntroUIVisible(visible);
    }

    private void SetGuidedTutorialPanelVisible(bool visible)
    {
        if (guidedTutorialPanelToHide != null)
            guidedTutorialPanelToHide.SetActive(visible);
    }

    private void OpenPanel(PanelType type)
    {
        if (currentPanel == type || isClosing || isTransitioning) return;

        CloseAllPanels();

        if (type != PanelType.None)
        {
            Time.timeScale = 0f;
            cutsceneManager?.PauseVideo();
            DisableGestureInput();
            SetIntroUIVisible(false);
            GetTutorialManager()?.SetTutorialVisualsVisible(false);

            // ✅ Hide guided panel + circle
            SetGuidedTutorialPanelVisible(false);

            if (tutorialHintManager != null)
                tutorialHintManager.HideCircleTemporarily();
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
            SetIntroUIVisible(true);
            GetTutorialManager()?.SetTutorialVisualsVisible(true);

            // ✅ Restore guided panel + circle
            SetGuidedTutorialPanelVisible(true);

            if (tutorialHintManager != null)
                tutorialHintManager.RestoreCircle();

            currentPanel = PanelType.None;
        }
        else if (type == PanelType.Guided)
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
        {
            if (data.panel != null) data.panel.SetActive(false);
        }
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
        if (gestureDrawer != null) gestureDrawer.enabled = true;
    }

    // ---------------------------------------------------------------
    // PUBLIC API
    // ---------------------------------------------------------------

    public void OpenPause() => OpenPanel(PanelType.Pause);
    public void ClosePause() => CloseWithEffect(PanelType.Pause);

    public void OpenTutorial() => OpenPanel(PanelType.Tutorial);
    public void CloseTutorial() => CloseWithEffect(PanelType.Tutorial);

    public void OpenGuidedTutorial() => OpenPanel(PanelType.Guided);
    public void CloseGuidedTutorial() => CloseWithEffect(PanelType.Guided);

    public void ResumeGame()
    {
        if (currentPanel == PanelType.Pause)
        {
            ClosePause();
        }
        else if (currentPanel == PanelType.Tutorial ||
                 currentPanel == PanelType.Guided)
        {
            CloseAllPanels();
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            EnableGestureInput();
            SetIntroUIVisible(true);
            GetTutorialManager()?.SetTutorialVisualsVisible(true);

            SetGuidedTutorialPanelVisible(true);

            if (tutorialHintManager != null)
                tutorialHintManager.RestoreCircle();

            currentPanel = PanelType.None;
        }
        else
        {
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            EnableGestureInput();
            SetIntroUIVisible(true);

            SetGuidedTutorialPanelVisible(true);

            if (tutorialHintManager != null)
                tutorialHintManager.RestoreCircle();
        }
    }

    public void GoToMainMenu()
    {
        if (isTransitioning) return;

        if (!levelCompletionSaved &&
            LevelProgressManager.Instance != null &&
            LevelProgressManager.Instance.IsProgressBarFilled())
        {
            int currentLevel = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
            PlayerPrefs.SetInt("LevelCompleted_" + currentLevel, 1);
            PlayerPrefs.SetInt("LevelUnlocked_" + currentLevel, 1);
            PlayerPrefs.SetInt("LevelUnlocked_" + (currentLevel + 1), 1);
            PlayerPrefs.Save();

            LevelManager.Instance?.CompleteLevel(currentLevel);
            levelCompletionSaved = true;

            Debug.Log($"[PauseOverlay] ✅ Safety save Level {currentLevel + 1} COMPLETED.");
        }

        SaveGameplayProgress();

        isTransitioning = true;

        if (pauseButton != null)
            pauseButton.interactable = false;

        LeanTween.cancel(gameObject);
        CloseAllPanels();
        DisableGestureInput();
        cutsceneManager?.PauseVideo();
        SetIntroUIVisible(false);
        GetTutorialManager()?.SetTutorialVisualsVisible(false);
        Time.timeScale = 0f;

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            isTransitioning = false;
            Time.timeScale = 1f;
            if (pauseButton != null) pauseButton.interactable = true;
            return;
        }

        StartCoroutine(FadeAndLoadScene(mainMenuSceneName));
    }

    private void SaveGameplayProgress()
    {
        EnemyWaveSpawner enemyWaveSpawner = FindFirstObjectByType<EnemyWaveSpawner>();
        if (enemyWaveSpawner != null) enemyWaveSpawner.SaveCurrentWave();

        SaveCurrentProgress saveProgress = FindFirstObjectByType<SaveCurrentProgress>();
        if (saveProgress != null) saveProgress.SavePlayerPositionNow();

        GameProgressManager.SetHasEnteredGameplay(true);
        GameProgressManager.SaveLastScene(SceneManager.GetActiveScene().name);
        GameProgressManager.SaveGameState("Gameplay");
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (AudioManager.Instance != null)
            yield return AudioManager.Instance.FadeOutBGMAndWait(bgmFadeOutDuration);

        TransitionManager tm = TransitionManager.Instance();

        if (tm != null && transitionSettings != null)
        {
            tm.Transition(sceneName, transitionSettings, loadDelay);
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            isTransitioning = false;
        }
    }
}