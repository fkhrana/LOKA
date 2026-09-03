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
    [SerializeField] private Button playButton; // <-- Tambahan tombol Play/Resume (opsional)

    [Header("Cutscene")]
    [SerializeField] private CutsceneManager cutsceneManager;

    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "MainGameplay(Drawing)";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0.5f;

    private PanelType currentPanel = PanelType.None;
    private bool isClosing = false;
    private bool isTransitioning = false;

    private void Start()
    {
        Time.timeScale = 1f;
        CloseAllPanels();
        CleanupStaleTransitions();

        // Jika tombol play di-assign, pastikan listener-nya
        if (playButton != null)
            playButton.onClick.AddListener(ResumeGame);
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
        Time.timeScale = 1f;
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
        if (currentPanel == type || isClosing || isTransitioning) return;

        CloseAllPanels();

        if (type != PanelType.None)
        {
            Time.timeScale = 0f;
            cutsceneManager?.PauseVideo();
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
            currentPanel = PanelType.None;
        }
        else // Tutorial → kembali ke Pause dengan fade-in
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

    // ===== PUBLIC METHODS UNTUK TOMBOL =====

    public void OpenPause() => OpenPanel(PanelType.Pause);
    public void ClosePause() => CloseWithEffect(PanelType.Pause);

    public void OpenTutorial() => OpenPanel(PanelType.Tutorial);
    public void CloseTutorial() => CloseWithEffect(PanelType.Tutorial);

    /// <summary>
    /// Resume game (unpause) – melanjutkan permainan tanpa reset.
    /// Tombol Play/Resume panggil method ini.
    /// </summary>
    public void ResumeGame()
    {
        // Jika sedang dalam keadaan pause, tutup panel pause
        if (currentPanel == PanelType.Pause)
        {
            ClosePause();
        }
        // Jika sedang di tutorial, tutup tutorial dan pause (atau langsung resume)
        else if (currentPanel == PanelType.Tutorial)
        {
            // Opsi: langsung resume semua (tutup semua panel, set time scale 1)
            CloseAllPanels();
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
            currentPanel = PanelType.None;
        }
        else
        {
            // Jika tidak ada panel terbuka (misal karena bug), pastikan unpause
            Time.timeScale = 1f;
            cutsceneManager?.ResumeVideo();
        }
    }

    public void GoToMainMenu()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        if (pauseButton != null) pauseButton.interactable = false;

        LeanTween.cancel(gameObject);
        Time.timeScale = 1f;
        StopAllCoroutines();
        CloseAllPanels();

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[PauseOverlay] Main Menu Scene Name kosong!", this);
            isTransitioning = false;
            if (pauseButton != null) pauseButton.interactable = true;
            return;
        }

        CleanupStaleTransitions();

        var tm = TransitionManager.Instance();
        if (tm != null && transitionSettings != null)
        {
            tm.Transition(mainMenuSceneName, transitionSettings, loadDelay);
        }
        else
        {
            Debug.LogWarning("[PauseOverlay] TransitionManager atau TransitionSettings tidak ditemukan. Scene akan dibuka langsung.");
            SceneManager.LoadScene(mainMenuSceneName);
            isTransitioning = false;
            if (pauseButton != null) pauseButton.interactable = true;
        }
    }

    private void CleanupStaleTransitions()
    {
        var oldTransitions = FindObjectsByType<Transition>(FindObjectsSortMode.None);
        foreach (var t in oldTransitions)
        {
            if (t == null) continue;
            if (IsTransitionStillAnimating(t)) continue;
            Debug.LogWarning("[PauseOverlay] Menemukan Transition instance lama (selesai), menghapus: " + t.gameObject.name);
            Destroy(t.gameObject);
        }
    }

    private bool IsTransitionStillAnimating(Transition t)
    {
        Transform[] panels = { t.transitionPanelIN, t.transitionPanelOUT };
        foreach (var panel in panels)
        {
            if (panel == null || !panel.gameObject.activeInHierarchy) continue;
            foreach (var anim in panel.GetComponentsInChildren<Animator>(true))
            {
                if (anim == null || !anim.isActiveAndEnabled) continue;
                var state = anim.GetCurrentAnimatorStateInfo(0);
                if (state.normalizedTime < 1f && !anim.IsInTransition(0))
                    return true;
            }
        }
        return false;
    }
}