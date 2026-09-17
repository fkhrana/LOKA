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
    [SerializeField] private Transform player;

    private void Awake()
    {
        // Auto-cari player via tag kalau kosong.
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        // Simpan scene sebelumnya SEBELUM update LastScene.
        string previousScene = GameProgressManager.GetLastScene();
        string currentScene = SceneManager.GetActiveScene().name;

        GameProgressManager.SaveLastScene(currentScene);

        // Tutorial belum selesai → skip restore semua state.
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 0)
        {
            Debug.Log("[SaveCurrentProgress] Tutorial belum selesai → skip restore.");
            return;
        }

        RestorePlayerPosition();

        string savedState = GameProgressManager.GetGameState();
        bool fromMainMenu = GameProgressManager.StartedFromMainMenu;

        Debug.Log($"[SaveCurrentProgress] scene='{currentScene}', " +
                  $"prevScene='{previousScene}', state='{savedState}', " +
                  $"fromMainMenu={fromMainMenu}");

        // Kalau state = Puzzle/Reward, valid resume cuma kalau:
        //   1. Datang dari MainMenu (player sengaja klik resume), ATAU
        //   2. First entry / crash recovery (previousScene kosong).
        // Selain itu (dari Latihan, Cutscene, scene lain) → stale, clear.
        if (savedState == "Puzzle" || savedState == "Reward")
        {
            bool cameFromMainMenu = (previousScene == "MainMenu");
            bool isFirstEntry = string.IsNullOrEmpty(previousScene);

            if (!cameFromMainMenu && !isFirstEntry)
            {
                Debug.Log($"[SaveCurrentProgress] State '{savedState}' stale " +
                          $"(prevScene='{previousScene}') → clear.");
                GameProgressManager.ClearGameState();
                savedState = "";
            }
        }

        if (savedState == "Puzzle") { RestorePuzzle(); return; }
        if (savedState == "Reward") { RestoreReward(); return; }
        if (savedState == "Gameplay") { RestoreGameplay(); return; }

        StartNewGame();
    }

    // Dipanggil dari PauseOverlay untuk simpan posisi player.
    public void SavePlayerPositionNow()
    {
        if (player == null)
        {
            Debug.LogWarning("[SaveCurrentProgress] Player belum di-assign, skip save posisi.");
            return;
        }

        GameProgressManager.SavePlayerPosition(player.position);
    }

    // Restore posisi player dari PlayerPrefs.
    private void RestorePlayerPosition()
    {
        if (player == null) return;

        if (GameProgressManager.TryGetPlayerPosition(out Vector3 pos))
        {
            player.position = pos;
            Debug.Log($"[SaveCurrentProgress] Player pos restored: {pos}");
        }
    }

    // Resume ke state Puzzle.
    private void RestorePuzzle()
    {
        Debug.Log("[SaveCurrentProgress] Resume → Canvas 2 / Puzzle");
        ApplyPuzzleUI();
    }

    // Resume ke state Reward.
    private void RestoreReward()
    {
        Debug.Log("[SaveCurrentProgress] Resume → Canvas 2 / Reward");
        ApplyRewardUI();
    }

    // Resume ke state Gameplay.
    private void RestoreGameplay()
    {
        Debug.Log("[SaveCurrentProgress] Resume → Gameplay");

        if (canvas2 != null) canvas2.SetActive(false);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);

        // CameraIntroManager TETAP enabled.
        // State = "Gameplay" → CameraIntroManager pakai countdown-only.
    }

    // Tandai state Puzzle + apply UI.
    public void MarkPuzzleActive()
    {
        ApplyPuzzleUI();
        GameProgressManager.SaveGameState("Puzzle");
        Debug.Log("[SaveCurrentProgress] State disimpan: Puzzle");
    }

    // Tandai state Reward + apply UI.
    public void MarkRewardActive()
    {
        ApplyRewardUI();
        GameProgressManager.SaveGameState("Reward");
        Debug.Log("[SaveCurrentProgress] State disimpan: Reward");
    }

    // Apply UI untuk state Puzzle.
    private void ApplyPuzzleUI()
    {
        if (enemyWaveSpawner != null) enemyWaveSpawner.StopWaveSequence();
        if (cameraIntroManager != null) cameraIntroManager.enabled = false;
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(true);
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    // Apply UI untuk state Reward.
    private void ApplyRewardUI()
    {
        if (enemyWaveSpawner != null) enemyWaveSpawner.StopWaveSequence();
        if (cameraIntroManager != null) cameraIntroManager.enabled = false;
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(true);
    }

    // Setup untuk new game / gameplay normal.
    private void StartNewGame()
    {
        Debug.Log("[SaveCurrentProgress] New Game → Gameplay normal");

        if (canvas2 != null) canvas2.SetActive(false);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);

        // CameraIntroManager TIDAK di-disable.
        // State kosong → CameraIntroManager fallback → panning + countdown.
        // Blok HasEnteredGameplay dihapus karena redundan dan bikin intro ke-skip.
    }
}