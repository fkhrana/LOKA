using UnityEngine;

public static class GameProgressManager
{
    private const string KEY_LAST_SCENE = "LastSceneName";
    private const string KEY_GAME_STATE = "GameState";
    private const string KEY_PUZZLE_INDEX = "LastPuzzleIndex";
    private const string KEY_CUTSCENE_COMPLETED = "CutsceneCompleted";
    private const string KEY_WAVE_INDEX = "LastWaveIndex";

    // Player position
    private const string KEY_POS_X = "SavedPos_X";
    private const string KEY_POS_Y = "SavedPos_Y";
    private const string KEY_POS_Z = "SavedPos_Z";
    private const string KEY_HAS_POS = "HasPlayerPos";

    // Flag sudah pernah masuk gameplay
    private const string KEY_HAS_ENTERED = "HasEnteredGameplay";

    // ===========================
    // SCENE
    // ===========================
    public static void SaveLastScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        PlayerPrefs.SetString(KEY_LAST_SCENE, sceneName);
        PlayerPrefs.Save();

        Debug.Log("[GameProgress] Scene saved: " + sceneName);
    }

    public static string GetLastScene()
    {
        return PlayerPrefs.GetString(KEY_LAST_SCENE, "");
    }

    public static string GetLastScene(string defaultScene)
    {
        return PlayerPrefs.GetString(KEY_LAST_SCENE, defaultScene);
    }

    public static bool HasLastScene()
    {
        string s = PlayerPrefs.GetString(KEY_LAST_SCENE, "");
        return !string.IsNullOrEmpty(s);
    }

    // ===========================
    // GAME STATE
    // ===========================
    public static void SaveGameState(string state)
    {
        if (string.IsNullOrEmpty(state))
            return;

        PlayerPrefs.SetString(KEY_GAME_STATE, state);
        PlayerPrefs.Save();

        Debug.Log("[GameProgress] State saved: " + state);
    }

    public static string GetGameState()
    {
        return PlayerPrefs.GetString(KEY_GAME_STATE, "");
    }

    public static void ClearGameState()
    {
        PlayerPrefs.DeleteKey(KEY_GAME_STATE);
        PlayerPrefs.DeleteKey(KEY_PUZZLE_INDEX);
        PlayerPrefs.DeleteKey(KEY_WAVE_INDEX);

        ClearPlayerPosition();
        PlayerPrefs.DeleteKey(KEY_HAS_ENTERED);

        PlayerPrefs.Save();

        Debug.Log("[GameProgress] Game state berhasil dibersihkan.");
    }

    // ===========================
    // PUZZLE
    // ===========================
    public static void SavePuzzleIndex(int puzzleIndex)
    {
        PlayerPrefs.SetInt(KEY_PUZZLE_INDEX, puzzleIndex);
        PlayerPrefs.Save();

        Debug.Log("[GameProgress] Puzzle index saved: " + puzzleIndex);
    }

    public static int GetPuzzleIndex()
    {
        return PlayerPrefs.GetInt(KEY_PUZZLE_INDEX, 0);
    }

    // ===========================
    // WAVE
    // ===========================
    public static void SaveWaveIndex(int waveIndex)
    {
        if (waveIndex < 0)
            return;

        PlayerPrefs.SetInt(KEY_WAVE_INDEX, waveIndex);
        PlayerPrefs.Save();

        Debug.Log("[GameProgress] Wave index saved: " + waveIndex);
    }

    public static int GetWaveIndex()
    {
        return PlayerPrefs.GetInt(KEY_WAVE_INDEX, 0);
    }

    // ===========================
    // CUTSCENE
    // ===========================
    public static void SetCutsceneCompleted()
    {
        PlayerPrefs.SetInt(KEY_CUTSCENE_COMPLETED, 1);
        PlayerPrefs.Save();

        Debug.Log("[GameProgress] Cutscene selesai.");
    }

    public static bool IsCutsceneCompleted()
    {
        return PlayerPrefs.GetInt(KEY_CUTSCENE_COMPLETED, 0) == 1;
    }

    // ===========================
    // PLAYER POSITION
    // ===========================
    public static void SavePlayerPosition(Vector3 pos)
    {
        PlayerPrefs.SetFloat(KEY_POS_X, pos.x);
        PlayerPrefs.SetFloat(KEY_POS_Y, pos.y);
        PlayerPrefs.SetFloat(KEY_POS_Z, pos.z);
        PlayerPrefs.SetInt(KEY_HAS_POS, 1);
        PlayerPrefs.Save();

        Debug.Log($"[GameProgress] Player pos saved: {pos}");
    }

    public static bool TryGetPlayerPosition(out Vector3 pos)
    {
        pos = Vector3.zero;

        if (PlayerPrefs.GetInt(KEY_HAS_POS, 0) != 1)
            return false;

        pos = new Vector3(
            PlayerPrefs.GetFloat(KEY_POS_X, 0f),
            PlayerPrefs.GetFloat(KEY_POS_Y, 0f),
            PlayerPrefs.GetFloat(KEY_POS_Z, 0f)
        );

        return true;
    }

    public static void ClearPlayerPosition()
    {
        PlayerPrefs.DeleteKey(KEY_POS_X);
        PlayerPrefs.DeleteKey(KEY_POS_Y);
        PlayerPrefs.DeleteKey(KEY_POS_Z);
        PlayerPrefs.DeleteKey(KEY_HAS_POS);
    }

    // ===========================
    // ENTERED GAMEPLAY FLAG
    // ===========================
    public static void SetHasEnteredGameplay(bool value)
    {
        PlayerPrefs.SetInt(KEY_HAS_ENTERED, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool HasEnteredGameplay()
    {
        return PlayerPrefs.GetInt(KEY_HAS_ENTERED, 0) == 1;
    }

    // ===========================
    // PROGRESS
    // ===========================
    public static bool HasProgress()
    {
        return PlayerPrefs.HasKey(KEY_LAST_SCENE);
    }

    public static void ResetLevelProgress()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_SCENE);
        PlayerPrefs.DeleteKey(KEY_GAME_STATE);
        PlayerPrefs.DeleteKey(KEY_PUZZLE_INDEX);
        PlayerPrefs.DeleteKey(KEY_WAVE_INDEX);
        PlayerPrefs.DeleteKey(KEY_HAS_ENTERED);

        ClearPlayerPosition();
        PlayerPrefs.Save();

        Debug.Log("[GameProgressManager] Progress level di-reset.");
    }

    public static void ResetProgress()
    {
        ResetLevelProgress();

        PlayerPrefs.DeleteKey(KEY_CUTSCENE_COMPLETED);
        PlayerPrefs.Save();

        Debug.Log("[GameProgressManager] Semua progress di-reset.");
    }
}