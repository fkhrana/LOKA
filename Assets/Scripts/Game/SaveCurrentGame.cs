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

    [Header("Player")]
    [Tooltip("Drag GameObject Player ke sini. Kalau kosong, akan dicari via tag 'Player'.")]
    [SerializeField] private Transform player;

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        GameProgressManager.SaveLastScene(currentScene);

        // === TUTORIAL GUARD ===
        // Kalau tutorial belum selesai, jangan restore state apapun.
        // TutorialManager yang akan atur semuanya dari awal.
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 0)
        {
            Debug.Log("[SaveCurrentProgress] Tutorial belum selesai → skip restore.");
            return;
        }

        RestorePlayerPosition();

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

    // ============================================================
    // PUBLIC ENTRY POINT — dipanggil dari PauseOverlay
    // ============================================================
    public void SavePlayerPositionNow()
    {
        if (player == null)
        {
            Debug.LogWarning("[SaveCurrentProgress] Player belum di-assign, skip save posisi.");
            return;
        }

        GameProgressManager.SavePlayerPosition(player.position);
    }

    // ============================================================
    // RESTORE
    // ============================================================
    private void RestorePlayerPosition()
    {
        if (player == null) return;

        if (GameProgressManager.TryGetPlayerPosition(out Vector3 pos))
        {
            player.position = pos;
            Debug.Log($"[SaveCurrentProgress] Player pos restored: {pos}");
        }
    }

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

    // ============================================================
    // PUBLIC ENTRY POINT — panggil dari script lain
    // ============================================================
    public void MarkPuzzleActive()
    {
        ApplyPuzzleUI();
        GameProgressManager.SaveGameState("Puzzle");
        Debug.Log("[SaveCurrentProgress] State disimpan: Puzzle");
    }

    public void MarkRewardActive()
    {
        ApplyRewardUI();
        GameProgressManager.SaveGameState("Reward");
        Debug.Log("[SaveCurrentProgress] State disimpan: Reward");
    }

    // ============================================================
    // UI HELPERS
    // ============================================================
    private void ApplyPuzzleUI()
    {
        if (enemyWaveSpawner != null) enemyWaveSpawner.StopWaveSequence();
        if (cameraIntroManager != null) cameraIntroManager.enabled = false;
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(true);
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    private void ApplyRewardUI()
    {
        if (enemyWaveSpawner != null) enemyWaveSpawner.StopWaveSequence();
        if (cameraIntroManager != null) cameraIntroManager.enabled = false;
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(true);
    }

    private void StartNewGame()
    {
        Debug.Log("[SaveCurrentProgress] New Game → Gameplay normal");

        if (canvas2 != null) canvas2.SetActive(false);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);

        if (GameProgressManager.HasEnteredGameplay())
        {
            Debug.Log("[SaveCurrentProgress] Skip cutscene (sudah pernah main)");
            if (cameraIntroManager != null) cameraIntroManager.enabled = false;
        }
        else
        {
            GameProgressManager.SetHasEnteredGameplay(true);
            Debug.Log("[SaveCurrentProgress] Cutscene dimulai.");
        }
    }
}