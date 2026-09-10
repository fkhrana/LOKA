using UnityEngine;

public static class GameProgressManager
{
    public static bool StartedFromMainMenu { get; private set; }

    public static void MarkStartedFromMainMenu()
    {
        StartedFromMainMenu = true;
    }

    private const string KEY_LAST_SCENE = "LastSceneName";
    private const string KEY_GAME_STATE = "GameState";
    private const string KEY_PUZZLE_INDEX = "LastPuzzleIndex";
    private const string KEY_CUTSCENE_COMPLETED = "CutsceneCompleted";
    private const string KEY_WAVE_INDEX = "LastWaveIndex";

    public static void SaveLastScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        PlayerPrefs.SetString(KEY_LAST_SCENE, sceneName);
        PlayerPrefs.Save();

        Debug.Log("[GameProgress] Scene saved: " + sceneName);
    }

    public static string GetLastScene(string defaultScene)
    {
        return PlayerPrefs.GetString(KEY_LAST_SCENE, defaultScene);
    }

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
        PlayerPrefs.Save();

        Debug.Log("[GameProgress] Game state berhasil dibersihkan.");
    }

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
        PlayerPrefs.Save();

        Debug.Log("[GameProgressManager] Progress level + puzzle + wave berhasil di-reset.");
    }

    public static void ResetProgress()
    {
        ResetLevelProgress();

        PlayerPrefs.DeleteKey(KEY_CUTSCENE_COMPLETED);
        PlayerPrefs.Save();

        Debug.Log("[GameProgressManager] Semua progress berhasil di-reset.");
    }
}