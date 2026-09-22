using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;

public class GameOverManager : MonoBehaviour
{
    [Header("Nama Scene")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "MainGameplay(Drawing)";
    public string tutorialScene = "Latihan";

    [Header("Player & Lose Panel")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private LoseAksaraStatusUI loseAksaraStatusUI;

    [Header("Lose SFX")]
    [SerializeField] private bool useLoseSFX = true;
    [SerializeField] private string loseSFXName = "Lose";

    [Range(0f, 1f)]
    [SerializeField] private float loseSFXVolume = 1f;

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    [Tooltip("Durasi fade out BGM sebelum pindah scene.")]
    [SerializeField] private float bgmFadeOutDuration = 0.8f;

    private bool isGameOver;
    private bool isTransitioning;

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

        Time.timeScale = 1f;
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

        string targetScene = gameplayScene;

        if (LevelManager.Instance != null)
        {
            int currentLevel = LevelManager.Instance.GetCurrentLevelIndex();
            GameProgressManager.ClearGameState(currentLevel);

            string configuredScene =
                LevelManager.Instance.GetSceneNameForLevel(currentLevel);

            if (!string.IsNullOrEmpty(configuredScene))
                targetScene = configuredScene;
        }
        else
        {
            GameProgressManager.ClearGameState();
        }

        SceneManager.LoadScene(targetScene);
    }

    public void OpenTutorial()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        Time.timeScale = 1f;

        StartCoroutine(
            FadeAndLoadScene(tutorialScene)
        );
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (AudioManager.Instance != null)
        {
            yield return
                AudioManager.Instance
                    .FadeOutBGMAndWait(
                        bgmFadeOutDuration
                    );
        }

        TransitionManager tm =
            TransitionManager.Instance();

        if (tm != null &&
            transitionSettings != null)
        {
            tm.Transition(
                sceneName,
                transitionSettings,
                loadDelay
            );
        }
        else
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene(sceneName);

            isTransitioning = false;
        }
    }
}
