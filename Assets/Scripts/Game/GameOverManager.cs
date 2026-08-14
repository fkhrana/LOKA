using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Nama Scene")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "MainGameplay(Drawing)";

    [Header("Player & Lose Panel")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private LoseAksaraStatusUI loseAksaraStatusUI;

    private bool isGameOver;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.Died += HandlePlayerDied;
    }

    private void OnEnable()
    {
        RefreshLoseAksaraStatus();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Died -= HandlePlayerDied;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.Died -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (losePanel != null)
            losePanel.SetActive(true);

        Time.timeScale = 0f;
        RefreshLoseAksaraStatus();
    }

    public void RefreshLoseAksaraStatus()
    {
        if (loseAksaraStatusUI != null)
            loseAksaraStatusUI.RefreshStatus();
    }

    // Tombol "Home"
    public void GoToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    // Tombol "Again" (main ulang)
    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }

    // Tombol "Tutorial"
    public void OpenTutorial()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("OpenTutorial", 1);
        SceneManager.LoadScene(mainMenuScene);
    }
}