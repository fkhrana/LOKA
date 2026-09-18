using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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

    [Header("Testing")]
    [FormerlySerializedAs("keepPuzzlePanelStateOnSceneStart")]
    [SerializeField] private bool preservePanelStatesForDirectSceneTesting;

    private string previousSceneAtStartup;
    private string currentSceneAtStartup;

    private void Awake()
    {
        // Auto-cari player via tag kalau kosong.
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        previousSceneAtStartup = GameProgressManager.GetLastScene();
        currentSceneAtStartup = SceneManager.GetActiveScene().name;

        GameProgressManager.SaveLastScene(currentSceneAtStartup);

        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 0)
            return;

        string savedState = GameProgressManager.GetGameState();
        bool cameFromMainMenu = previousSceneAtStartup == "MainMenu";
        bool isFirstEntry = string.IsNullOrEmpty(previousSceneAtStartup);

        // Gameplay selalu dimulai dari awal saat scene dibuka kembali.
        // Progress wave dan level bar lama tidak dipakai.
        if (savedState == "Gameplay")
        {
            Debug.Log(
                "[SaveCurrentProgress] State Gameplay lama di-clear " +
                "→ reset progress wave dan level bar."
            );

            GameProgressManager.ClearGameState();
            return;
        }

        if ((savedState == "Puzzle" || savedState == "Reward") &&
            !cameFromMainMenu &&
            !isFirstEntry)
        {
            Debug.Log(
                $"[SaveCurrentProgress] State '{savedState}' stale " +
                $"(prevScene='{previousSceneAtStartup}') → clear sebelum Start."
            );

            GameProgressManager.ClearGameState();
        }
    }

    private void Start()
    {
        // Nilai scene sudah disimpan di Awake sebelum LastScene diperbarui.
        string previousScene = previousSceneAtStartup;
        string currentScene = currentSceneAtStartup;

        // Tutorial belum selesai → skip restore semua state.
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 0)
        {
            Debug.Log("[SaveCurrentProgress] Tutorial belum selesai → skip restore.");
            return;
        }

        if (preservePanelStatesForDirectSceneTesting)
        {
            if (canvas2 != null) canvas2.SetActive(true);
            Debug.Log("[SaveCurrentProgress] Direct scene testing → panel state dipertahankan.");
            return;
        }

        RestorePlayerPosition();

        string savedState = GameProgressManager.GetGameState();
        bool fromMainMenu = GameProgressManager.StartedFromMainMenu;

        Debug.Log($"[SaveCurrentProgress] scene='{currentScene}', " +
                  $"prevScene='{previousScene}', state='{savedState}', " +
                  $"fromMainMenu={fromMainMenu}");

        // Puzzle/Reward tetap bisa di-resume dari save yang valid.
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

    // Gameplay baru dimulai dari awal; canvas tetap aktif untuk VFX gameplay.
    private void RestoreGameplay()
    {
        Debug.Log("[SaveCurrentProgress] Resume → Gameplay");

        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);

        // CameraIntroManager tetap enabled untuk menjalankan intro/countdown.
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

    // Setup gameplay normal dengan panel Puzzle/Reward tersembunyi.
    private void StartNewGame()
    {
        Debug.Log("[SaveCurrentProgress] New Game → Gameplay normal");

        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);

        // CameraIntroManager tetap enabled agar intro/countdown dapat berjalan.
    }
}