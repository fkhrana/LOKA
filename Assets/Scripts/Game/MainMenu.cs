using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;

public class MainMenu : MonoBehaviour
{
    private enum PanelType { None, Setting, Collection, Level, Credits, Tutorial }

    [Header("Panels")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject koleksiPanel;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject tutorialPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject settingButton;
    [SerializeField] private GameObject koleksiButton;
    [SerializeField] private GameObject levelButton;
    [SerializeField] private GameObject creditsButton;
    [SerializeField] private GameObject tutorialButton;

    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "CutScenee";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0.5f;

    [Header("Main Menu Content (Entry Animation)")]
    [Tooltip("GameObject 'Main Menu Content' yang punya Animator untuk animasi masuk (Buttons, Title, dsb).")]
    [SerializeField] private GameObject mainMenuContent;
    [Tooltip("Nama state Animator yang jadi entry animation, misal 'Show' atau 'Enter'. Kosongkan untuk pakai default state layer 0.")]
    [SerializeField] private string entryStateName = "";

    private PanelType currentPanel = PanelType.None;

    // ============================================
    //  AWAKE - RESET STATE (SCOPE DIBATASI!)
    // ============================================
    private void Awake()
    {
        Time.timeScale = 1f;

        // Hanya reset Animator & CanvasGroup DI DALAM PANEL, bukan di Main Menu Content.
        ResetPanelState(settingPanel);
        ResetPanelState(koleksiPanel);
        ResetPanelState(levelPanel);
        ResetPanelState(creditsPanel);
        ResetPanelState(tutorialPanel);

        CloseAllPanels();
    }

    private void ResetPanelState(GameObject panel)
    {
        if (panel == null) return;

        Animator[] anims = panel.GetComponentsInChildren<Animator>(true);
        foreach (var anim in anims)
        {
            if (anim == null) continue;
            anim.Rebind();
            anim.Update(0f);
        }

        CanvasGroup[] cgs = panel.GetComponentsInChildren<CanvasGroup>(true);
        foreach (var cg in cgs)
        {
            if (cg == null) continue;
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }

    // ============================================
    //  START - INISIALISASI TAMBAHAN
    // ============================================
    private void Start()
    {
        CloseAllPanels();
        PlayEntryAnimation();

        if (PlayerPrefs.GetInt("OpenTutorial", 0) == 1)
        {
            OpenTutorial();
            PlayerPrefs.SetInt("OpenTutorial", 0);
        }
    }

    // Paksa restart animasi entry Main Menu Content dari frame 0,
    // TIDAK mengandalkan auto-play/Culling Mode saja. Ini menghindari kasus
    // di mana Animator sudah "kepakai" momentumnya duluan sebelum curtain
    // transisi selesai fade-out (race condition timing).
    private void PlayEntryAnimation()
    {
        if (mainMenuContent == null) return;

        Animator[] anims = mainMenuContent.GetComponentsInChildren<Animator>(true);
        foreach (var anim in anims)
        {
            if (anim == null) continue;

            // Rebind dulu supaya bersih dari state sebelumnya
            anim.Rebind();
            anim.Update(0f);

            if (!string.IsNullOrEmpty(entryStateName))
            {
                // Paksa play state tertentu dari awal (normalizedTime = 0)
                anim.Play(entryStateName, 0, 0f);
            }
            else
            {
                // Kalau nama state tidak diisi, paksa play default state layer 0 dari awal
                anim.Play(0, 0, 0f);
            }

            anim.Update(0f);
        }
    }

    // ---------- PANEL MANAGEMENT ----------

    private void OpenPanel(GameObject panel, GameObject button, PanelType type)
    {
        if (currentPanel == type) return;

        PlayClickSFX();
        CloseAllPanels();

        panel?.SetActive(true);
        button?.SetActive(false);
        currentPanel = type;
    }

    private void ClosePanel(GameObject panel, GameObject button, PanelType type)
    {
        if (currentPanel != type) return;

        PlayClickSFX();

        EffectPanel dialog = panel?.GetComponent<EffectPanel>();
        if (dialog != null)
            dialog.CloseDialog();
        else
            panel?.SetActive(false);

        button?.SetActive(true);
        currentPanel = PanelType.None;
    }

    private void CloseAllPanels()
    {
        settingPanel?.SetActive(false);
        koleksiPanel?.SetActive(false);
        levelPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
        tutorialPanel?.SetActive(false);

        settingButton?.SetActive(true);
        koleksiButton?.SetActive(true);
        levelButton?.SetActive(true);
        creditsButton?.SetActive(true);
        tutorialButton?.SetActive(true);

        currentPanel = PanelType.None;
    }

    // ---------- SCENE ----------

    public void TapToStart()
    {
        PlayClickSFX();

        var tm = TransitionManager.Instance();
        if (tm != null && transitionSettings != null)
        {
            tm.Transition(nextSceneName, transitionSettings, loadDelay);
        }
        else
        {
            Debug.LogWarning("[MainMenu] TransitionManager or Settings missing, loading directly.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // ---------- SETTING ----------
    public void OpenSetting() => OpenPanel(settingPanel, settingButton, PanelType.Setting);
    public void CloseSetting() => ClosePanel(settingPanel, settingButton, PanelType.Setting);

    // ---------- COLLECTION ----------
    public void OpenCollection() => OpenPanel(koleksiPanel, koleksiButton, PanelType.Collection);
    public void CloseCollection() => ClosePanel(koleksiPanel, koleksiButton, PanelType.Collection);

    // ---------- LEVEL ----------
    public void OpenLevel() => OpenPanel(levelPanel, levelButton, PanelType.Level);
    public void CloseLevel() => ClosePanel(levelPanel, levelButton, PanelType.Level);

    // ---------- CREDIT ----------
    public void OpenCredit() => OpenPanel(creditsPanel, creditsButton, PanelType.Credits);
    public void CloseCredit() => ClosePanel(creditsPanel, creditsButton, PanelType.Credits);

    // ---------- TUTORIAL ----------
    public void OpenTutorial() => OpenPanel(tutorialPanel, tutorialButton, PanelType.Tutorial);
    public void CloseTutorial() => ClosePanel(tutorialPanel, tutorialButton, PanelType.Tutorial);

    // ---------- SFX ----------
    private void PlayClickSFX()
    {
        if (clickSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clickSound);
    }
}