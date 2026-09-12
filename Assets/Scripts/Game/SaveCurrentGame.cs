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
        // Simpan scene gameplay saat ini
        string currentScene = SceneManager.GetActiveScene().name;
        GameProgressManager.SaveLastScene(currentScene);

        // Ambil state terakhir
        string savedState = GameProgressManager.GetGameState();

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

        StartNewGame();
    }

    // =========================
    // RESTORE (dipanggil saat scene BARU di-load, tidak perlu save ulang
    // karena state sudah sesuai dengan yang tersimpan)
    // =========================

    private void RestorePuzzle()
    {
        Debug.Log("[SaveCurrentProgress] Resume → Canvas 2 / Puzzle");
        ApplyPuzzleUI();
    }

    private void RestoreReward()
    {
        Debug.Log("[SaveCurrentProgress] Resume → Canvas 2 / Reward");
        ApplyRewardUI();
    }

    // =========================
    // PUBLIC ENTRY POINT — panggil ini dari script lain
    // (misal EnemyWaveSpawner saat wave terakhir selesai,
    // atau PuzzleManager saat puzzle mulai/menang)
    // =========================

    public void MarkPuzzleActive()
    {
        ApplyPuzzleUI();

        // Ini kuncinya: UI berubah SEKALIGUS state tersimpan
        GameProgressManager.SaveGameState("Puzzle");

        Debug.Log("[SaveCurrentProgress] State disimpan: Puzzle");
    }

    public void MarkRewardActive()
    {
        ApplyRewardUI();

        GameProgressManager.SaveGameState("Reward");

        Debug.Log("[SaveCurrentProgress] State disimpan: Reward");
    }

    // =========================
    // UI HELPERS (murni ubah tampilan, tidak menyentuh PlayerPrefs)
    // =========================

    private void ApplyPuzzleUI()
    {
        if (enemyWaveSpawner != null)
            enemyWaveSpawner.StopWaveSequence();

        if (cameraIntroManager != null)
            cameraIntroManager.enabled = false;

        if (canvas2 != null)
            canvas2.SetActive(true);

        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    private void ApplyRewardUI()
    {
        if (enemyWaveSpawner != null)
            enemyWaveSpawner.StopWaveSequence();

        if (cameraIntroManager != null)
            cameraIntroManager.enabled = false;

        if (canvas2 != null)
            canvas2.SetActive(true);

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (rewardPanel != null)
            rewardPanel.SetActive(true);
    }

    private void StartNewGame()
    {
        Debug.Log("[SaveCurrentProgress] New Game → Gameplay normal");

#if UNITY_EDITOR
        if (!GameProgressManager.StartedFromMainMenu)
        {
            Debug.Log("[SaveCurrentProgress] Direct Editor Play → kondisi panel dipertahankan");
            return;
        }
#endif

        if (canvas2 != null)
            canvas2.SetActive(false);

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }
}