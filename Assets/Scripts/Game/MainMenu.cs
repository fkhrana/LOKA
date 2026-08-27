using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickSound;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "CutScenee";

    private PanelType currentPanel = PanelType.None;

    private void Awake() => Time.timeScale = 1f;

    private void Start()
    {
        CloseAllPanels();

        if (PlayerPrefs.GetInt("OpenTutorial", 0) == 1)
        {
            OpenTutorial();
            PlayerPrefs.SetInt("OpenTutorial", 0);
        }
    }

    // ---------- PANEL MANAGEMENT ----------

    private void OpenPanel(GameObject panel, GameObject button, PanelType type)
    {
        if (currentPanel == type) return; // sudah terbuka

        PlayClickSFX();
        CloseAllPanels(); // tutup semua panel lain

        panel?.SetActive(true);
        button?.SetActive(false);
        currentPanel = type;
    }

    private void ClosePanel(GameObject panel, GameObject button, PanelType type)
    {
        if (currentPanel != type) return; // tidak terbuka

        PlayClickSFX();

        // Jika ada EffectPanel, gunakan animasi tutup
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
        CurtainsAnimation.TransitionToScene(nextSceneName);
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
        if (sfxSource != null && clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }
}