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
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        previousSceneAtStartup = GameProgressManager.GetLastScene();
        currentSceneAtStartup = SceneManager.GetActiveScene().name;

        GameProgressManager.SaveLastScene(currentSceneAtStartup);

        if (!GameProgressManager.IsTutorialCompleted())
            return;

        string savedState = GameProgressManager.GetGameState();
        bool cameFromMainMenu = previousSceneAtStartup == "MainMenu";
        bool isFirstEntry = string.IsNullOrEmpty(previousSceneAtStartup);

        if (savedState == "Gameplay")
        {
            GameProgressManager.ClearGameState();
            return;
        }

        if ((savedState == "Puzzle" || savedState == "Reward") &&
            !cameFromMainMenu &&
            !isFirstEntry)
        {
            GameProgressManager.ClearGameState();
        }
    }

    private void Start()
    {
        string previousScene = previousSceneAtStartup;
        string currentScene = currentSceneAtStartup;

        if (!GameProgressManager.IsTutorialCompleted())
        {
            Debug.Log("[SaveCurrentProgress] Tutorial belum selesai → skip restore.");
            return;
        }

        if (preservePanelStatesForDirectSceneTesting)
        {
            if (canvas2 != null) canvas2.SetActive(true);
            return;
        }

        RestorePlayerPosition();

        string savedState = GameProgressManager.GetGameState();
        bool fromMainMenu = GameProgressManager.StartedFromMainMenu;

        if (savedState == "Puzzle") { RestorePuzzle(); return; }
        if (savedState == "Reward") { RestoreReward(); return; }
        if (savedState == "Gameplay") { RestoreGameplay(); return; }

        StartNewGame();
    }

    public void SavePlayerPositionNow()
    {
        if (player == null) return;
        GameProgressManager.SavePlayerPosition(player.position);
    }

    private void RestorePlayerPosition()
    {
        if (player == null) return;

        if (GameProgressManager.TryGetPlayerPosition(out Vector3 pos))
        {
            player.position = pos;
        }
    }

    private void RestorePuzzle() { ApplyPuzzleUI(); }
    private void RestoreReward() { ApplyRewardUI(); }

    private void RestoreGameplay()
    {
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    public void MarkPuzzleActive()
    {
        ApplyPuzzleUI();
        GameProgressManager.SaveGameState("Puzzle");
    }

    public void MarkRewardActive()
    {
        ApplyRewardUI();
        GameProgressManager.SaveGameState("Reward");
    }

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
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }
}