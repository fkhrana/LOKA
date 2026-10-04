using UnityEngine;
using UnityEngine.SceneManagement;

public static class TrainingFlagGuard
{
    private const string MainMenuScene = "MainMenu";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MainMenuScene)
            TutorialManager.IsTrainingMode = false;
    }
}