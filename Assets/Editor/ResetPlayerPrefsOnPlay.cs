using UnityEditor;
using UnityEngine;
using System;

[InitializeOnLoad]
public static class ResetPlayerPrefsOnPlay
{
    static ResetPlayerPrefsOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        DeleteGameProgressKeys();
        PlayerPrefs.Save();
        Debug.Log("[Editor] Save progress game dihapus sebelum Play Mode.");
    }

    private static void DeleteGameProgressKeys()
    {
        string[] fixedKeys =
        {
            "LastSceneName",
            "GameState",
            "LastPuzzleIndex",
            "LastWaveIndex",
            "CutsceneCompleted",
            "CurrentLevelIndex",
            "OpenTutorial",

            // Power Up keys
            "PowerUp_Freeze_Unlocked",
            "PowerUp_Freeze_Consumed",
            "PowerUp_Combo_Unlocked",
            "PowerUp_Combo_Consumed",
            "PowerUp_Boost_Unlocked",   // ← NEW untuk Kacapi
            "PowerUp_Boost_Consumed"    // ← NEW untuk Kacapi
        };

        foreach (string key in fixedKeys)
            PlayerPrefs.DeleteKey(key);

        for (int levelIndex = 0; levelIndex < 3; levelIndex++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + levelIndex);
            PlayerPrefs.DeleteKey("LevelCompleted_" + levelIndex);

            // Progress keys sekarang disimpan per level.
            PlayerPrefs.DeleteKey("GameState_L" + levelIndex);
            PlayerPrefs.DeleteKey("LastPuzzleIndex_L" + levelIndex);
            PlayerPrefs.DeleteKey("LastWaveIndex_L" + levelIndex);
            PlayerPrefs.DeleteKey("ProcessedEnemies_L" + levelIndex);
            PlayerPrefs.DeleteKey("SavedPos_X_L" + levelIndex);
            PlayerPrefs.DeleteKey("SavedPos_Y_L" + levelIndex);
            PlayerPrefs.DeleteKey("SavedPos_Z_L" + levelIndex);
            PlayerPrefs.DeleteKey("HasPlayerPos_L" + levelIndex);
            PlayerPrefs.DeleteKey("HasEnteredGameplay_L" + levelIndex);
        }

        foreach (string shape in Enum.GetNames(typeof(GestureShape)))
            PlayerPrefs.DeleteKey("PermanentCollected_" + shape);
    }
}