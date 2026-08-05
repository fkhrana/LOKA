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

    [Header("Scene Settings")]

    [SerializeField] private int nextSceneIndex = 4;

    [Space]

    [Header("SFX")]

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickSound;

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
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    private void CloseWithAnimation(GameObject panel)
    {
        DialogBox dialog = panel.GetComponent<DialogBox>();

        if (dialog != null)
            dialog.CloseDialog();
        else
            panel.SetActive(false);
    }

    public void TapToStart()
    {
        PlayClickSFX();
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void OpenSetting()
    {
        PlayClickSFX();
        settingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
        PlayClickSFX();
        CloseWithAnimation(settingPanel);
    }

    public void OpenCollection()
    {
        PlayClickSFX();
        koleksiPanel.SetActive(true);
    }

    public void CloseCollection()
    {
        PlayClickSFX();
        CloseWithAnimation(koleksiPanel);
    }

    public void OpenLevel()
    {
        PlayClickSFX();
        levelPanel.SetActive(true);
    }

    public void CloseLevel()
    {
        PlayClickSFX();
        CloseWithAnimation(levelPanel);
    }

    public void OpenCredit()
    {
        PlayClickSFX();
        creditsPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        PlayClickSFX();
        CloseWithAnimation(creditsPanel);
    }

    public void OpenTutorial()
    {
        PlayClickSFX();
        tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        PlayClickSFX();
        CloseWithAnimation(tutorialPanel);
    }
}