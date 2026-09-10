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
        // Simpan scene gameplay
        string currentScene =
            SceneManager.GetActiveScene().name;

        GameProgressManager.SaveLastScene(currentScene);

        // Ambil state terakhir
        string savedState =
            GameProgressManager.GetGameState();

        // Resume puzzle
        if (savedState == "Puzzle")
        {
            RestorePuzzle();
            return;
        }

        // Resume reward
        if (savedState == "Reward")
        {
            RestoreReward();
            return;
        }

        // Game baru
        StartNewGame();
    }

    private void RestorePuzzle()
    {
        Debug.Log(
            "[SaveCurrentProgress] Resume → Canvas 2 / Puzzle"
        );

        // Jangan jalankan enemy/wave lagi
        if (enemyWaveSpawner != null)
            enemyWaveSpawner.StopWaveSequence();

        // Pastikan intro tidak berjalan
        if (cameraIntroManager != null)
            cameraIntroManager.enabled = false;

        // Canvas 2 ON
        if (canvas2 != null)
            canvas2.SetActive(true);

        // Puzzle ON
        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);

        // Reward OFF
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    private void RestoreReward()
    {
        Debug.Log(
            "[SaveCurrentProgress] Resume → Canvas 2 / Reward"
        );

        // Jangan jalankan enemy/wave lagi
        if (enemyWaveSpawner != null)
            enemyWaveSpawner.StopWaveSequence();

        // Pastikan intro tidak berjalan
        if (cameraIntroManager != null)
            cameraIntroManager.enabled = false;

        // Canvas 2 ON
        if (canvas2 != null)
            canvas2.SetActive(true);

        // Puzzle OFF
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        // Reward ON
        if (rewardPanel != null)
            rewardPanel.SetActive(true);
    }

    private void StartNewGame()
    {
        Debug.Log(
            "[SaveCurrentProgress] New Game → Gameplay normal"
        );

        // Canvas 2 OFF
        if (canvas2 != null)
            canvas2.SetActive(false);

        // Puzzle OFF
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        // Reward OFF
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }
}