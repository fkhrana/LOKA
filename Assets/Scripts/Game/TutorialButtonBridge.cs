using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialButtonBridge : MonoBehaviour
{
    [Tooltip("Nama scene Level 1 yang akan di-load dari MainMenu.")]
    [SerializeField] private string level1SceneName = "MainGameplay(Drawing)";

    [Tooltip("Nama scene MainMenu.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Tooltip("Nama scene Cutscene intro.")]
    [SerializeField] private string cutsceneSceneName = "Cutscene";

    private TutorialFlowController tutorialFlowController;

    public void SetFlowController(TutorialFlowController controller)
    {
        tutorialFlowController = controller;
    }

    public void OnStartLearningClicked()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == mainMenuSceneName)
        {
            GameProgressManager.SetGuidedTutorialActive(true);
            GameProgressManager.SetSkipCarouselOnLoad(true);

            if (!GameProgressManager.IsCutsceneCompleted())
            {
                if (string.IsNullOrEmpty(cutsceneSceneName))
                {
                    Debug.LogError("[TutorialButtonBridge] cutsceneSceneName belum di-assign!");
                    return;
                }

                Debug.Log("[TutorialButtonBridge] Cutscene belum ditonton → load cutscene dulu.");

                GameProgressManager.SetPendingGuidedAfterCutscene(true);
                SceneManager.LoadScene(cutsceneSceneName);
                return;
            }

            Debug.Log("[TutorialButtonBridge] Cutscene sudah lewat → langsung Level 1.");

            if (string.IsNullOrEmpty(level1SceneName))
            {
                Debug.LogError("[TutorialButtonBridge] level1SceneName belum di-assign!");
                return;
            }

            SceneManager.LoadScene(level1SceneName);
        }
        else
        {
            if (tutorialFlowController == null)
                tutorialFlowController = FindFirstObjectByType<TutorialFlowController>();

            if (tutorialFlowController != null)
                tutorialFlowController.OnStartLearningClicked();
            else
                Debug.LogWarning("[TutorialButtonBridge] TutorialFlowController tidak ditemukan!");
        }
    }
}