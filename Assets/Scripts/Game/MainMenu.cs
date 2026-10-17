using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;

public class MainMenu : MonoBehaviour
{
    private enum PanelType { None, Setting, Collection, Level, Credits, Tutorial }

    [Serializable]
    private class PanelData
    {
        public PanelType type;
        public GameObject panel;
        public GameObject button;
    }

    [Header("Panels")]
    [SerializeField] private PanelData[] panels;

    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "CutScenee";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0.5f;
    [SerializeField] private float bgmFadeOutDuration = 1.2f;

    [Header("Main Menu Content")]
    [SerializeField] private GameObject mainMenuContent;
    [SerializeField] private string entryStateName = "";

    [Header("Reset Progress")]
    [Tooltip("Fallback total level kalau LevelManager tidak ditemukan.")]
    [SerializeField, Min(1)] private int totalLevels = 4;

    private PanelType currentPanel = PanelType.None;
    private bool isTransitioning = false;
    private bool isPanelAnimating = false;

    private void Awake()
    {
        Time.timeScale = 1f;

        TutorialManager.IsTrainingMode = false;
        PowerUpTutorialManager.ForceReset();
        BossLevelPowerUpTutorial.ForceReset();
        GuidedTutorialManager.ForceReset();
        GameProgressManager.SetGuidedTutorialActive(false);

        CloseAllPanels();
    }

    private void Start()
    {
        StartCoroutine(DeferredInit());
    }

    private IEnumerator DeferredInit()
    {
        yield return null;

        ResetAllCanvases();
        ForceShowMainMenu();
        CloseAllPanels();
        PlayEntryAnimation();

        StartCoroutine(EnsureMainMenuVisible());
    }

    private PanelData GetPanelData(PanelType type)
    {
        foreach (var p in panels)
            if (p.type == type) return p;

        return null;
    }

    private void OpenPanel(PanelType type)
    {
        if (currentPanel == type || isPanelAnimating || isTransitioning) return;

        PlayClickSFX();

        if (currentPanel != PanelType.None)
        {
            isPanelAnimating = true;

            ClosePanel(currentPanel, () =>
            {
                isPanelAnimating = false;
                OpenPanelDirect(type);
            });
        }
        else
        {
            OpenPanelDirect(type);
        }
    }

    private void OpenPanelDirect(PanelType type)
    {
        var data = GetPanelData(type);
        if (data == null) return;

        data.panel?.SetActive(true);
        data.button?.SetActive(false);
        currentPanel = type;
    }

    private void ClosePanel(PanelType type, Action onComplete = null)
    {
        if (currentPanel != type)
        {
            onComplete?.Invoke();
            return;
        }

        PlayClickSFX();

        var data = GetPanelData(type);

        if (data == null)
        {
            currentPanel = PanelType.None;
            onComplete?.Invoke();
            return;
        }

        var effect = data.panel?.GetComponent<EffectPanel>();

        if (effect != null)
        {
            effect.CloseDialog(() =>
            {
                data.button?.SetActive(true);
                data.panel?.SetActive(false);
                currentPanel = PanelType.None;
                onComplete?.Invoke();
            });
        }
        else
        {
            data.button?.SetActive(true);
            data.panel?.SetActive(false);
            currentPanel = PanelType.None;
            onComplete?.Invoke();
        }
    }

    private void CloseAllPanels()
    {
        foreach (var data in panels)
        {
            if (data.panel != null) data.panel.SetActive(false);
            if (data.button != null) data.button.SetActive(true);
        }

        currentPanel = PanelType.None;
    }

    public void OpenSetting()    => OpenPanel(PanelType.Setting);
    public void CloseSetting()   => ClosePanel(PanelType.Setting);
    public void OpenCollection() => OpenPanel(PanelType.Collection);
    public void CloseCollection()=> ClosePanel(PanelType.Collection);
    public void OpenLevel()      => OpenPanel(PanelType.Level);
    public void CloseLevel()     => ClosePanel(PanelType.Level);
    public void OpenCredit()     => OpenPanel(PanelType.Credits);
    public void CloseCredit()    => ClosePanel(PanelType.Credits);
    public void OpenTutorial()   => OpenPanel(PanelType.Tutorial);
    public void CloseTutorial()  => ClosePanel(PanelType.Tutorial);

    public void TapToStart()
    {
        if (isTransitioning) return;

        PlayClickSFX();

        string targetScene = nextSceneName;

        if (!GameProgressManager.IsCutsceneCompleted())
        {
            Debug.Log($"[MainMenu] Cutscene belum nonton → ke Cutscenee: {targetScene}");
        }
        else if (GameProgressManager.HasLastScene())
        {
            string saved = GameProgressManager.GetLastScene();

            if (!string.IsNullOrEmpty(saved))
            {
                targetScene = saved;
                Debug.Log($"[MainMenu] RESUME ke: {targetScene}");
            }
        }
        else
        {
            targetScene = "MainGameplay(Drawing)";
            Debug.Log($"[MainMenu] Cutscene udah nonton, langsung gameplay: {targetScene}");
        }

        isTransitioning = true;
        StartCoroutine(FadeAndLoadScene(targetScene));
    }

    public void ResetProgressAndReload()
    {
        if (isTransitioning) return;

        PlayClickSFX();

        int levels = totalLevels;
        if (LevelManager.Instance != null)
            levels = Mathf.Max(levels, LevelManager.Instance.TotalLevels);

        string[] progressKeys =
        {
            "LastSceneName", "CutsceneCompleted", "CurrentLevelIndex",
            "OpenTutorial", "TutorialPowerUpDone",
            "GuidedTutorialDone_Global", "PendingGuidedAfterCutscene",
            "PowerUp_Freeze_Unlocked", "PowerUp_Freeze_Consumed",
            "PowerUp_Combo_Unlocked",  "PowerUp_Combo_Consumed",
            "PowerUp_Shield_Unlocked", "PowerUp_Shield_Consumed",
            "PowerUp_Boost_Unlocked",  "PowerUp_Boost_Consumed",
        };

        foreach (var k in progressKeys)
            PlayerPrefs.DeleteKey(k);

        for (int i = 0; i < levels; i++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + i);
            PlayerPrefs.DeleteKey("LevelCompleted_" + i);

            PlayerPrefs.DeleteKey("GameState_L" + i);
            PlayerPrefs.DeleteKey("LastPuzzleIndex_L" + i);
            PlayerPrefs.DeleteKey("LastWaveIndex_L" + i);
            PlayerPrefs.DeleteKey("ProcessedEnemies_L" + i);

            PlayerPrefs.DeleteKey("SavedPos_X_L" + i);
            PlayerPrefs.DeleteKey("SavedPos_Y_L" + i);
            PlayerPrefs.DeleteKey("SavedPos_Z_L" + i);
            PlayerPrefs.DeleteKey("HasPlayerPos_L" + i);
            PlayerPrefs.DeleteKey("HasEnteredGameplay_L" + i);

            PlayerPrefs.DeleteKey("TutorialPowerUpDone_L" + i);
            PlayerPrefs.DeleteKey("GuidedTutorialDone_L" + i);
            PlayerPrefs.DeleteKey("BossTutorialDone_L" + i);
            PlayerPrefs.DeleteKey("LevelCollectedAksara_L" + i);
        }

        for (int i = 1; i <= levels; i++)
        {
            PlayerPrefs.DeleteKey("FinalBook_Reward_L" + i);
            PlayerPrefs.DeleteKey("FinalBook_Reward_L" + i + "_Name");
            PlayerPrefs.DeleteKey("FinalBook_RewardDesc_L" + i);
            PlayerPrefs.DeleteKey("FinalBook_Aksara_L" + i);
        }

        foreach (string shape in Enum.GetNames(typeof(GestureShape)))
            PlayerPrefs.DeleteKey("PermanentCollected_" + shape);

        PlayerPrefs.Save();

        Debug.Log($"[MainMenu] Progress di-reset untuk {levels} level. Settings aman.");

        isTransitioning = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
            SceneManager.LoadScene(sceneName);
            isTransitioning = false;
        }
    }

    private void ResetAllCanvases()
    {
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas == null || canvas.gameObject.scene.name != gameObject.scene.name)
                continue;

            canvas.sortingOrder = 0;
            canvas.gameObject.SetActive(true);

            var cg = canvas.GetComponent<CanvasGroup>();

            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    private void ForceShowMainMenu()
    {
        if (mainMenuContent == null) return;

        mainMenuContent.SetActive(true);

        foreach (var cg in mainMenuContent.GetComponentsInChildren<CanvasGroup>(true))
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        foreach (Transform child in mainMenuContent.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.SetActive(true);
        }
    }

    private IEnumerator EnsureMainMenuVisible()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if (mainMenuContent == null) yield break;

        foreach (var cg in mainMenuContent.GetComponentsInChildren<CanvasGroup>(true))
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        mainMenuContent.SetActive(true);
    }

    private void PlayEntryAnimation()
    {
        if (mainMenuContent == null) return;

        foreach (var anim in mainMenuContent.GetComponentsInChildren<Animator>(true))
        {
            if (anim == null) continue;

            anim.Rebind();
            anim.Update(0f);

            if (!string.IsNullOrEmpty(entryStateName))
                anim.Play(entryStateName, 0, 0f);
            else
                anim.Play(0, 0, 0f);

            anim.Update(0f);
        }
    }

    private void PlayClickSFX()
    {
        if (clickSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clickSound);
    }
}