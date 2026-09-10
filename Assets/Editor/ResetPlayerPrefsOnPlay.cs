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
            "PowerUp_Freeze_Unlocked",
            "PowerUp_Freeze_Consumed",
            "PowerUp_Combo_Unlocked",
            "PowerUp_Combo_Consumed"
        };

        foreach (string key in fixedKeys)
            PlayerPrefs.DeleteKey(key);

        for (int levelIndex = 0; levelIndex < 3; levelIndex++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + levelIndex);
            PlayerPrefs.DeleteKey("LevelCompleted_" + levelIndex);
        }

        foreach (string shape in Enum.GetNames(typeof(GestureShape)))
            PlayerPrefs.DeleteKey("PermanentCollected_" + shape);
    }
}
