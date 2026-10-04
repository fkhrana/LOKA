using UnityEngine;

public class TutorialFinishButton : MonoBehaviour
{
    [Tooltip("Kalau kosong, akan auto-find di scene.")]
    [SerializeField] private GuidedTutorialManager tutorialManager;

    [Tooltip("Root FinishPanel. Akan disembunyikan saat tombol YA / Ulang diklik.")]
    [SerializeField] private GameObject finishPanelRoot;

    public void OnClickFinish()
    {
        Debug.Log("[TutorialFinishButton] ✅ Tombol YA diklik.");

        // ⬇️ TAMBAH — log diagnostik (hapus setelah bug selesai)
        if (finishPanelRoot == null)
        {
            Debug.LogError("[TutorialFinishButton] ❌ finishPanelRoot BELUM di-assign di Inspector!");
        }
        else
        {
            Debug.Log($"[TutorialFinishButton] finishPanelRoot='{finishPanelRoot.name}', " +
                      $"activeSelf={finishPanelRoot.activeSelf}, " +
                      $"activeInHierarchy={finishPanelRoot.activeInHierarchy}, " +
                      $"parent='{(finishPanelRoot.transform.parent != null ? finishPanelRoot.transform.parent.name : "null")}'");
        }
        // ⬆️ SAMPAI SINI

        // Sembunyikan FinishPanel
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