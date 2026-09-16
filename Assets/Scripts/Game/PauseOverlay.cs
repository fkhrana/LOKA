using System.Collections;
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

    [Tooltip("Scene latihan. Dipakai oleh tombol Tutorial.")]
    [SerializeField] private string tutorialSceneName = "Latihan";

    [Tooltip("Scene cutscene. Dipakai kalau player belum pernah nonton cutscene.")]
    [SerializeField] private string cutsceneSceneName = "CutScenee";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    [Tooltip("Durasi fade out BGM sebelum pindah scene.")]
    [SerializeField] private float bgmFadeOutDuration = 0.8f;

    private PanelType currentPanel = PanelType.None;
    private bool isClosing = false;
    private bool isTransitioning = false;

    private TutorialManager cachedTutorialManager;

    private void Start()
    {
        Time.timeScale = 1f;
        CloseAllPanels();

        if (playButton != null) playButton.onClick.AddListener(ResumeGame);
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
        Time.timeScale = 1f;

        if (playButton != null) playButton.onClick.RemoveListener(ResumeGame);
    }

    // Cari TutorialManager di scene (cache).
    private TutorialManager GetTutorialManager()
    {
        if (cachedTutorialManager == null)
            cachedTutorialManager = FindFirstObjectByType<TutorialManager>();

        return cachedTutorialManager;
    }

    // Cari data panel by tipe.
    private PanelData GetPanelData(PanelType type)
    {
        foreach (var p in panels)
            if (p.type == type) return p;

        return null;
    }

    // Ambil GameObject panel by tipe.
    private GameObject GetPanel(PanelType type) => GetPanelData(type)?.panel;

    // Buka panel & pause game.
    private void OpenPanel(PanelType type)
    {
        if (currentPanel == type || isClosing || isTransitioning) return;

        CloseAllPanels();

        if (type != PanelType.None)
        {
            Time.timeScale = 0f;
            cutsceneManager?.PauseVideo();
            DisableGestureInput();

            // Sembunyikan visual tutorial (hand + dots) saat pause.
            GetTutorialManager()?.SetTutorialVisualsVisible(false);
        }

        GetPanel(type)?.SetActive(true);
        currentPanel = type;
    }

    // Tutup panel dan/atau resume game.
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

            // Tampilkan kembali visual tutorial.
            GetTutorialManager()?.SetTutorialVisualsVisible(true);

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

    // Matikan semua panel.
    private void CloseAllPanels()
    {
        foreach (var data in panels)
            if (data.panel != null) data.panel.SetActive(false);
    }

    // Tutup panel via EffectPanel kalau ada.
    private void CloseWithEffect(PanelType type)
    {
        var panel = GetPanel(type);

        if (panel == null || currentPanel != type) return;

        var effect = panel.GetComponent<EffectPanel>();

        if (effect != null) effect.CloseDialog(() => ClosePanel(type));
        else ClosePanel(type);
    }

    // Fade in CanvasGroup objek.
    private void FadeIn(GameObject obj)
    {
        if (obj == null) return;

        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        LeanTween.alphaCanvas(cg, 1f, 0.25f).setIgnoreTimeScale(true);
    }

    // Matikan gesture input sementara.
    private void DisableGestureInput()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.ResetGestureInput();
            gestureDrawer.enabled = false;
        }
    }

    // Nyalakan gesture input.
    private void EnableGestureInput()
    {
        if (gestureDrawer != null) gestureDrawer.enabled = true;
    }

    public void OpenPause()    => OpenPanel(PanelType.Pause);
    public void ClosePause()   => CloseWithEffect(PanelType.Pause);

    // Tombol Tutorial: save progress, fade BGM, lalu load Latihan/Cutscene.
    public void OpenTutorial()
    {
        if (isTransitioning) return;

        SaveGameplayProgress();

        bool cutsceneCompleted = GameProgressManager.IsCutsceneCompleted();
        string targetScene = cutsceneCompleted ? tutorialSceneName : cutsceneSceneName;

        Debug.Log($"[PauseOverlay] Tutorial button → target: {targetScene} " +
                  $"(cutsceneCompleted={cutsceneCompleted})");

        PrepareForTransition();

        StartCoroutine(FadeAndLoadScene(targetScene));
    }

    // Tetap ada untuk backward compatibility (mis. tombol close panel).
    public void CloseTutorial() => CloseWithEffect(PanelType.Tutorial);

    // Resume game dari panel apapun.
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

            // Tampilkan kembali visual tutorial.
            GetTutorialManager()?.SetTutorialVisualsVisible(true);

            currentPanel = PanelType.None;
        }
        else
        {
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            EnableGestureInput();
        }
    }

    // Simpan progress lalu kembali ke main menu.
    public void GoToMainMenu()
    {
        if (isTransitioning) return;

        SaveGameplayProgress();

        PrepareForTransition();

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[PauseOverlay] Main Menu Scene Name kosong!", this);
            isTransitioning = false;

            if (pauseButton != null) pauseButton.interactable = true;

            return;
        }

        StartCoroutine(FadeAndLoadScene(mainMenuSceneName));
    }

    // Simpan wave + posisi player + state gameplay.
    private void SaveGameplayProgress()
    {
        EnemyWaveSpawner enemyWaveSpawner = FindFirstObjectByType<EnemyWaveSpawner>();
        if (enemyWaveSpawner != null) enemyWaveSpawner.SaveCurrentWave();

        SaveCurrentProgress saveProgress = FindFirstObjectByType<SaveCurrentProgress>();
        if (saveProgress != null) saveProgress.SavePlayerPositionNow();

        GameProgressManager.SetHasEnteredGameplay(true);
        GameProgressManager.SaveLastScene(SceneManager.GetActiveScene().name);

        // Tandai state Gameplay supaya CameraIntroManager tahu ini resume.
        GameProgressManager.SaveGameState("Gameplay");
    }

    // Cleanup state sebelum pindah scene (tween, timeScale, panel, gesture).
    private void PrepareForTransition()
    {
        isTransitioning = true;

        if (pauseButton != null) pauseButton.interactable = false;

        LeanTween.cancel(gameObject);
        Time.timeScale = 1f;
        StopAllCoroutines();
        CloseAllPanels();
        EnableGestureInput();
    }

    // Fade out BGM lalu load scene via TransitionManager.
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
            SceneManager.LoadScene(sceneName);
            isTransitioning = false;
        }
    }
}