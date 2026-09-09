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

    [Header("Lose SFX")]
    [SerializeField] private bool useLoseSFX = true;
    [SerializeField] private string loseSFXName = "Lose";
    
    [Range(0f, 1f)]
    [SerializeField] private float loseSFXVolume = 1f;

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

        if (useLoseSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                loseSFXName,
                loseSFXVolume
            );
        }

        Time.timeScale = 0f;

        RefreshLoseAksaraStatus();
    }

    public void RefreshLoseAksaraStatus()
    {
        if (loseAksaraStatusUI != null)
            loseAksaraStatusUI.RefreshStatus();
    }

    public void GoToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }

    public void OpenTutorial()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("OpenTutorial", 1);
        SceneManager.LoadScene(mainMenuScene);
    }
}