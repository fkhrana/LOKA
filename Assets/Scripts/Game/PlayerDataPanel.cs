using UnityEngine;
using UnityEngine.UI;

public class PlayerDataPanel : MonoBehaviour
{
    private static bool hasCheckedThisAppSession;

    [SerializeField] private GameObject panel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button resetButton;

    private MainMenu mainMenu;

    private void Awake()
    {
        mainMenu = FindFirstObjectByType<MainMenu>();

        if (panel == null)
            panel = gameObject;

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetGame);

        if (!hasCheckedThisAppSession)
        {
            hasCheckedThisAppSession = true;
            RefreshVisibility();
        }
        else if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueGame);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetGame);
    }

    private void RefreshVisibility()
    {
        if (panel != null)
            panel.SetActive(AnalyticsConsentUI.HasSavedChoice && HasSavedPlayerData());
    }

    private bool HasSavedPlayerData()
    {
        return PlayerPrefs.HasKey("LastSceneName") ||
               PlayerPrefs.HasKey("CutsceneCompleted") ||
               PlayerPrefs.HasKey("CurrentLevelIndex");
    }

    public void ContinueGame()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ResetGame()
    {
        if (panel != null)
            panel.SetActive(false);

        if (mainMenu != null)
            mainMenu.ResetProgressAndReload();
    }
}
