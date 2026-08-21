using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject koleksiPanel;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject tutorialPanel;

    [Space]
    [Header("Buttons")]
    [SerializeField] private GameObject settingButton;
    [SerializeField] private GameObject koleksiButton;
    [SerializeField] private GameObject levelButton;
    [SerializeField] private GameObject creditsButton;
    [SerializeField] private GameObject tutorialButton;

    [Space]
    [Header("Scene Settings")]
    [SerializeField] private int nextSceneIndex = 2;

    [Space]
    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickSound;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        settingPanel?.SetActive(false);
        koleksiPanel?.SetActive(false);
        levelPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
        tutorialPanel?.SetActive(false);

        if (PlayerPrefs.GetInt("OpenTutorial", 0) == 1)
        {
            OpenTutorial();
            PlayerPrefs.SetInt("OpenTutorial", 0);
        }
    }

    private void PlayClickSFX()
    {
        if (sfxSource != null && clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }

    // ---------- GENERIC OPEN / CLOSE ----------

    private void OpenPanel(GameObject panel, GameObject openButton)
    {
        PlayClickSFX();
        panel?.SetActive(true);
        openButton?.SetActive(false);
    }

    private void ClosePanel(GameObject panel, GameObject openButton)
    {
        PlayClickSFX();

        DialogBox dialog = panel != null ? panel.GetComponent<DialogBox>() : null;

        if (dialog != null)
            dialog.CloseDialog();
        else
            panel?.SetActive(false);

        openButton?.SetActive(true);
    }

    // ---------- SCENE ----------

    public void TapToStart()
    {
        PlayClickSFX();
        SceneManager.LoadScene(nextSceneIndex);
    }

    // ---------- SETTING ----------
    public void OpenSetting() => OpenPanel(settingPanel, settingButton);
    public void CloseSetting() => ClosePanel(settingPanel, settingButton);

    // ---------- COLLECTION ----------
    public void OpenCollection() => OpenPanel(koleksiPanel, koleksiButton);
    public void CloseCollection() => ClosePanel(koleksiPanel, koleksiButton);

    // ---------- LEVEL ----------
    public void OpenLevel() => OpenPanel(levelPanel, levelButton);
    public void CloseLevel() => ClosePanel(levelPanel, levelButton);

    // ---------- CREDIT ----------
    public void OpenCredit() => OpenPanel(creditsPanel, creditsButton);
    public void CloseCredit() => ClosePanel(creditsPanel, creditsButton);

    // ---------- TUTORIAL ----------
    public void OpenTutorial() => OpenPanel(tutorialPanel, tutorialButton);
    public void CloseTutorial() => ClosePanel(tutorialPanel, tutorialButton);
}