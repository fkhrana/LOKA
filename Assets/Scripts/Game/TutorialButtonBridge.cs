using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialButtonBridge : MonoBehaviour
{
    [Tooltip("Nama scene Level 1 yang akan di-load dari MainMenu.")]
    [SerializeField] private string level1SceneName = "MainGameplay(Drawing)";

    [Tooltip("Nama scene MainMenu — di sini tutorial akan load ke Level 1.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private TutorialFlowController tutorialFlowController;

    public void SetFlowController(TutorialFlowController controller)
    {
        tutorialFlowController = controller;
    }

    public void OnStartLearningClicked()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // ✅ Deteksi berdasarkan nama scene
        if (currentScene == mainMenuSceneName)
        {
            // Kita di MainMenu → set flag + load Level 1
            Debug.Log($"[TutorialButtonBridge] Di MainMenu ('{currentScene}') → load Level 1 dengan guided.");

            GameProgressManager.SetGuidedTutorialActive(true);
            GameProgressManager.SetSkipCarouselOnLoad(true);

            if (string.IsNullOrEmpty(level1SceneName))
            {
                Debug.LogError("[TutorialButtonBridge] level1SceneName belum di-assign!");
                return;
            }

            SceneManager.LoadScene(level1SceneName);
        }
        else
        {
            // Kita di Level 1 (atau scene lain) → pakai flow controller
            Debug.Log($"[TutorialButtonBridge] Di scene '{currentScene}' → pakai TutorialFlowController.");

            if (tutorialFlowController == null)
            {
                tutorialFlowController = FindFirstObjectByType<TutorialFlowController>();
            }

            if (tutorialFlowController != null)
                tutorialFlowController.OnStartLearningClicked();
            else
                Debug.LogWarning("[TutorialButtonBridge] TutorialFlowController tidak ditemukan!");
        }
    }
}