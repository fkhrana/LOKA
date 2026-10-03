using UnityEngine;

public class TutorialFinishButton : MonoBehaviour
{
    [Tooltip("Kalau kosong, akan auto-find di scene.")]
    [SerializeField] private GuidedTutorialManager tutorialManager;

    [Tooltip("Root FinishPanel. Akan disembunyikan saat tombol YA / Ulang diklik.")]
    [SerializeField] private GameObject finishPanelRoot;

    // ✅ Dipasang ke tombol "YA"
    public void OnClickFinish()
    {
        Debug.Log("[TutorialFinishButton] ✅ Tombol YA diklik.");

        // ✅ FIX: Sembunyikan FinishPanel dulu
        if (finishPanelRoot != null)
            finishPanelRoot.SetActive(false);

        if (tutorialManager == null)
            tutorialManager = FindFirstObjectByType<GuidedTutorialManager>();

        if (tutorialManager != null)
        {
            tutorialManager.OnTutorialFinishConfirmed();
            Debug.Log("[TutorialFinishButton] ✅ Konfirmasi dikirim.");
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

        if (finishPanelRoot != null)
            finishPanelRoot.SetActive(false);

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