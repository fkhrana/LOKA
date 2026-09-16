using UnityEngine;

public static class GameProgressManager
{
    public static bool StartedFromMainMenu { get; private set; }
    public static void MarkStartedFromMainMenu() { StartedFromMainMenu = true; }

    // Global keys.
    private const string KEY_LAST_SCENE = "LastSceneName";
    private const string KEY_CUTSCENE_COMPLETED = "CutsceneCompleted";
    private const string KEY_CURRENT_LEVEL_INDEX = "CurrentLevelIndex";

    // Per-level prefixes (suffix dengan level index).
    private const string PREFIX_GAME_STATE = "GameState_L";
    private const string PREFIX_PUZZLE_INDEX = "LastPuzzleIndex_L";
    private const string PREFIX_WAVE_INDEX = "LastWaveIndex_L";
    private const string PREFIX_POS_X = "SavedPos_X_L";
    private const string PREFIX_POS_Y = "SavedPos_Y_L";
    private const string PREFIX_POS_Z = "SavedPos_Z_L";
    private const string PREFIX_HAS_POS = "HasPlayerPos_L";
    private const string PREFIX_HAS_ENTERED = "HasEnteredGameplay_L";

    // Migrasi dari key lama ke key per-level sekali saja.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void MigrateLegacyKeys()
    {
        if (!PlayerPrefs.HasKey("GameState") && !PlayerPrefs.HasKey("LastWaveIndex"))
            return;

        int currentLevel = PlayerPrefs.GetInt(KEY_CURRENT_LEVEL_INDEX, 0);

        MigrateString("GameState", PREFIX_GAME_STATE + currentLevel);
        MigrateInt("LastPuzzleIndex", PREFIX_PUZZLE_INDEX + currentLevel);
        MigrateInt("LastWaveIndex", PREFIX_WAVE_INDEX + currentLevel);
        MigrateFloat("SavedPos_X", PREFIX_POS_X + currentLevel);
        MigrateFloat("SavedPos_Y", PREFIX_POS_Y + currentLevel);
        MigrateFloat("SavedPos_Z", PREFIX_POS_Z + currentLevel);
        MigrateInt("HasPlayerPos", PREFIX_HAS_POS + currentLevel);
        MigrateInt("HasEnteredGameplay", PREFIX_HAS_ENTERED + currentLevel);

        PlayerPrefs.Save();
        Debug.Log($"[GameProgressManager] Legacy keys migrated ke level {currentLevel}.");
    }

    private static void MigrateString(string oldKey, string newKey)
    {
        if (!PlayerPrefs.HasKey(oldKey)) return;
        PlayerPrefs.SetString(newKey, PlayerPrefs.GetString(oldKey, ""));
        PlayerPrefs.DeleteKey(oldKey);
    }

    private static void MigrateInt(string oldKey, string newKey)
    {
        if (!PlayerPrefs.HasKey(oldKey)) return;
        PlayerPrefs.SetInt(newKey, PlayerPrefs.GetInt(oldKey, 0));
        PlayerPrefs.DeleteKey(oldKey);
    }

    private static void MigrateFloat(string oldKey, string newKey)
    {
        if (!PlayerPrefs.HasKey(oldKey)) return;
        PlayerPrefs.SetFloat(newKey, PlayerPrefs.GetFloat(oldKey, 0f));
        PlayerPrefs.DeleteKey(oldKey);
    }

    // Simpan nama scene terakhir (global).
    public static void SaveLastScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        PlayerPrefs.SetString(KEY_LAST_SCENE, sceneName);
        PlayerPrefs.Save();
    }

    // Ambil nama scene terakhir.
    public static string GetLastScene() => PlayerPrefs.GetString(KEY_LAST_SCENE, "");

    // Ambil nama scene terakhir dengan default.
    public static string GetLastScene(string defaultScene) => PlayerPrefs.GetString(KEY_LAST_SCENE, defaultScene);

    // Cek apakah ada scene terakhir tersimpan.
    public static bool HasLastScene() => !string.IsNullOrEmpty(PlayerPrefs.GetString(KEY_LAST_SCENE, ""));

    // Tandai cutscene sudah selesai (global).
    public static void SetCutsceneCompleted()
    {
        PlayerPrefs.SetInt(KEY_CUTSCENE_COMPLETED, 1);
        PlayerPrefs.Save();
    }

    // Cek cutscene sudah selesai.
    public static bool IsCutsceneCompleted() => PlayerPrefs.GetInt(KEY_CUTSCENE_COMPLETED, 0) == 1;

    // Simpan game state untuk level tertentu.
    public static void SaveGameState(int levelIndex, string state)
    {
        if (string.IsNullOrEmpty(state)) return;
        PlayerPrefs.SetString(PREFIX_GAME_STATE + levelIndex, state);
        PlayerPrefs.Save();
    }

    // Ambil game state level tertentu.
    public static string GetGameState(int levelIndex) => PlayerPrefs.GetString(PREFIX_GAME_STATE + levelIndex, "");

    // Bersihkan semua state level tertentu.
    public static void ClearGameState(int levelIndex)
    {
        PlayerPrefs.DeleteKey(PREFIX_GAME_STATE + levelIndex);
        PlayerPrefs.DeleteKey(PREFIX_PUZZLE_INDEX + levelIndex);
        PlayerPrefs.DeleteKey(PREFIX_WAVE_INDEX + levelIndex);
        ClearPlayerPosition(levelIndex);
        PlayerPrefs.DeleteKey(PREFIX_HAS_ENTERED + levelIndex);
        PlayerPrefs.Save();
    }

    // Simpan puzzle index level tertentu.
    public static void SavePuzzleIndex(int levelIndex, int puzzleIndex)
    {
        PlayerPrefs.SetInt(PREFIX_PUZZLE_INDEX + levelIndex, puzzleIndex);
        PlayerPrefs.Save();
    }

    // Ambil puzzle index level tertentu.
    public static int GetPuzzleIndex(int levelIndex) => PlayerPrefs.GetInt(PREFIX_PUZZLE_INDEX + levelIndex, 0);

    // Simpan wave index level tertentu.
    public static void SaveWaveIndex(int levelIndex, int waveIndex)
    {
        if (waveIndex < 0) return;
        PlayerPrefs.SetInt(PREFIX_WAVE_INDEX + levelIndex, waveIndex);
        PlayerPrefs.Save();
    }

    // Ambil wave index level tertentu.
    public static int GetWaveIndex(int levelIndex) => PlayerPrefs.GetInt(PREFIX_WAVE_INDEX + levelIndex, 0);

    // Simpan posisi player level tertentu.
    public static void SavePlayerPosition(int levelIndex, Vector3 pos)
    {
        PlayerPrefs.SetFloat(PREFIX_POS_X + levelIndex, pos.x);
        PlayerPrefs.SetFloat(PREFIX_POS_Y + levelIndex, pos.y);
        PlayerPrefs.SetFloat(PREFIX_POS_Z + levelIndex, pos.z);
        PlayerPrefs.SetInt(PREFIX_HAS_POS + levelIndex, 1);
        PlayerPrefs.Save();
    }

    // Ambil posisi player level tertentu.
    public static bool TryGetPlayerPosition(int levelIndex, out Vector3 pos)
    {
        pos = Vector3.zero;

        if (PlayerPrefs.GetInt(PREFIX_HAS_POS + levelIndex, 0) != 1)
            return false;

        pos = new Vector3(
            PlayerPrefs.GetFloat(PREFIX_POS_X + levelIndex, 0f),
            PlayerPrefs.GetFloat(PREFIX_POS_Y + levelIndex, 0f),
            PlayerPrefs.GetFloat(PREFIX_POS_Z + levelIndex, 0f)
        );

        return true;
    }

    // Hapus posisi player level tertentu.
    public static void ClearPlayerPosition(int levelIndex)
    {
        PlayerPrefs.DeleteKey(PREFIX_POS_X + levelIndex);
        PlayerPrefs.DeleteKey(PREFIX_POS_Y + levelIndex);
        PlayerPrefs.DeleteKey(PREFIX_POS_Z + levelIndex);
        PlayerPrefs.DeleteKey(PREFIX_HAS_POS + levelIndex);
    }

    // Simpan flag sudah masuk gameplay level tertentu.
    public static void SetHasEnteredGameplay(int levelIndex, bool value)
    {
        PlayerPrefs.SetInt(PREFIX_HAS_ENTERED + levelIndex, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Cek flag sudah masuk gameplay level tertentu.
    public static bool HasEnteredGameplay(int levelIndex) => PlayerPrefs.GetInt(PREFIX_HAS_ENTERED + levelIndex, 0) == 1;

    // Level aktif untuk wrapper backward-compatible.
    private static int ActiveLevel() => PlayerPrefs.GetInt(KEY_CURRENT_LEVEL_INDEX, 0);

    // Wrapper lama tanpa index, otomatis pakai CurrentLevelIndex.
    public static void SaveGameState(string state) => SaveGameState(ActiveLevel(), state);
    public static string GetGameState() => GetGameState(ActiveLevel());
    public static void ClearGameState() => ClearGameState(ActiveLevel());
    public static void SavePuzzleIndex(int puzzleIndex) => SavePuzzleIndex(ActiveLevel(), puzzleIndex);
    public static int GetPuzzleIndex() => GetPuzzleIndex(ActiveLevel());
    public static void SaveWaveIndex(int waveIndex) => SaveWaveIndex(ActiveLevel(), waveIndex);
    public static int GetWaveIndex() => GetWaveIndex(ActiveLevel());
    public static void SavePlayerPosition(Vector3 pos) => SavePlayerPosition(ActiveLevel(), pos);
    public static bool TryGetPlayerPosition(out Vector3 pos) => TryGetPlayerPosition(ActiveLevel(), out pos);
    public static void ClearPlayerPosition() => ClearPlayerPosition(ActiveLevel());
    public static void SetHasEnteredGameplay(bool value) => SetHasEnteredGameplay(ActiveLevel(), value);
    public static bool HasEnteredGameplay() => HasEnteredGameplay(ActiveLevel());

    // Cek ada progress tersimpan.
    public static bool HasProgress() => PlayerPrefs.HasKey(KEY_LAST_SCENE);

    // Reset state satu level saja.
    public static void ResetLevelProgress(int levelIndex) => ClearGameState(levelIndex);

    // Reset state semua level.
    public static void ResetAllLevelProgress(int totalLevels)
    {
        for (int i = 0; i < totalLevels; i++)
            ClearGameState(i);
    }

    // Reset global progress.
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_SCENE);
        PlayerPrefs.DeleteKey(KEY_CUTSCENE_COMPLETED);
        PlayerPrefs.Save();

        Debug.Log("[GameProgressManager] Global progress di-reset.");
    }
}