using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveCurrentProgress : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject canvas2;

    [Header("Panel")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject rewardPanel;

    [Header("Gameplay")]
    [SerializeField] private EnemyWaveSpawner enemyWaveSpawner;
    [SerializeField] private CameraIntroManager cameraIntroManager;

    private void Start()
    {
        string currentScene =
            SceneManager.GetActiveScene().name;

        GameProgressManager.SaveLastScene(
            currentScene
        );

        string savedState =
            GameProgressManager.GetGameState();

        if (savedState == "Puzzle")
        {
            RestorePuzzle();
            return;
        }

        if (savedState == "Reward")
        {
            RestoreReward();
            return;
        }

        if (savedState == "Gameplay")
        {
            RestoreGameplay();
            return;
        }

        StartNewGame();
    }

    private void RestoreGameplay()
    {
        Debug.Log(
            "[SaveCurrentProgress] Resume → Gameplay / Wave " +
            (GameProgressManager.GetWaveIndex() + 1)
        );

        if (cameraIntroManager != null)
        {
            cameraIntroManager.enabled = false;
        }

        if (canvas2 != null)
        {
            canvas2.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    private void RestorePuzzle()
    {
        Debug.Log(
            "[SaveCurrentProgress] Resume → Canvas 2 / Puzzle"
        );

        if (enemyWaveSpawner != null)
        {
            enemyWaveSpawner.StopWaveSequence();
        }

        if (cameraIntroManager != null)
        {
            cameraIntroManager.enabled = false;
        }

        if (canvas2 != null)
        {
            canvas2.SetActive(true);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    private void RestoreReward()
    {
        Debug.Log(
            "[SaveCurrentProgress] Resume → Canvas 2 / Reward"
        );

        if (enemyWaveSpawner != null)
        {
            enemyWaveSpawner.StopWaveSequence();
        }

        if (cameraIntroManager != null)
        {
            cameraIntroManager.enabled = false;
        }

        if (canvas2 != null)
        {
            canvas2.SetActive(true);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }
    }

    private void StartNewGame()
    {
        Debug.Log(
            "[SaveCurrentProgress] New Game → Gameplay normal"
        );

        if (canvas2 != null)
        {
            canvas2.SetActive(false);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }
}