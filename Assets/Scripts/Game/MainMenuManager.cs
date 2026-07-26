using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingPanel;
    public GameObject koleksiPanel;
    public GameObject levelPanel;

    [Header("Scene Settings")]
        public int nextSceneIndex = 4;

    void Start()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (koleksiPanel != null)
            koleksiPanel.SetActive(false);

        if (levelPanel != null)
            levelPanel.SetActive(false);
    }

    //=========================
    // TAP UNTUK MULAI
    //=========================
    public void TapToStart()
    {
         SceneManager.LoadScene(1);
    }

    //=========================
    // PENGATURAN
    //=========================
    public void OpenSetting()
    {
        settingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
        settingPanel.SetActive(false);
    }

    //=========================
    // KOLEKSI
    //=========================
    public void OpenCollection()
    {
        koleksiPanel.SetActive(true);
    }

    public void CloseCollection()
    {
        koleksiPanel.SetActive(false);
    }

    //=========================
    // LEVEL
    //=========================
    public void OpenLevel()
    {
        levelPanel.SetActive(true);
    }

    public void CloseLevel()
    {
        levelPanel.SetActive(false);
    }
}