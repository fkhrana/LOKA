using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultButton : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "MainGameplay(Drawing)";
    [SerializeField] private string nextScene = "Level2";

    // HOME
    public void Home()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    // REPLAY
    public void Replay()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    // NEXT
    public void Next()
    {
        Time.timeScale = 1f;

        string targetScene = nextScene;

        if (LevelManager.Instance != null)
        {
            int currentLevel = LevelManager.Instance.GetCurrentLevelIndex();
            int nextLevel = currentLevel + 1;

            // Tandai selesai → unlock level berikutnya + clear GameState
            LevelManager.Instance.CompleteLevel(currentLevel);

            // Update current level ke level berikutnya
            LevelManager.Instance.SetCurrentLevel(nextLevel);

            string configuredScene =
                LevelManager.Instance.GetSceneNameForLevel(nextLevel);

            if (!string.IsNullOrEmpty(configuredScene))
                targetScene = configuredScene;

            GameProgressManager.ClearGameState(nextLevel);

            Debug.Log($"[ResultButton] Level {currentLevel + 1} selesai → " +
                      $"Level {currentLevel + 2} di-unlock.");
        }
        else
        {
            Debug.LogWarning("[ResultButton] LevelManager.Instance tidak ditemukan.");
        }

        SceneManager.LoadScene(targetScene);
    }

    // BACK
    public void Back()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex - 1);
    }
}