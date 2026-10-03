using UnityEngine;

public class TutorialFinishButton : MonoBehaviour
{
    [Tooltip("Kalau kosong, akan auto-find di scene.")]
    [SerializeField] private GuidedTutorialManager tutorialManager;

    [Tooltip("Root FinishPanel. Akan disembunyikan saat tombol Ulang diklik.")]
    [SerializeField] private GameObject finishPanelRoot;

    // ✅ Dipasang ke tombol "YA"
    public void OnClickFinish()
    {
        if (tutorialManager == null)
            tutorialManager = FindFirstObjectByType<GuidedTutorialManager>();

        if (tutorialManager != null)
        {
            tutorialManager.OnTutorialFinishConfirmed();
            Debug.Log("[TutorialFinishButton] ✅ Tombol YA → konfirmasi dikirim.");
        }
        else
        {
            Debug.LogError("[TutorialFinishButton] ❌ GuidedTutorialManager tidak ditemukan!");
        }
    }

    // ✅ Dipasang ke tombol "ULANG"
    public void OnClickRestart()
    {
        Debug.Log("[TutorialFinishButton] 🔄 Tombol ULANG diklik → restart tutorial.");

        // Sembunyikan FinishPanel dulu
        if (finishPanelRoot != null)
            finishPanelRoot.SetActive(false);
        else
            Debug.LogWarning("[TutorialFinishButton] finishPanelRoot belum di-assign.");

        if (tutorialManager == null)
            tutorialManager = FindFirstObjectByType<GuidedTutorialManager>();

        if (tutorialManager != null)
        {
            tutorialManager.RestartTutorial();
        }
        else
        {
            Debug.LogError("[TutorialFinishButton] ❌ GuidedTutorialManager tidak ditemukan!");
        }
    }
}