using System.Collections;
using UnityEngine;

public class TutorialFlowController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject tutorialCarouselPanel;
    [SerializeField] private GameObject guidedTutorialPanel;

    [Header("Guided Tutorial Manager")]
    [SerializeField] private GuidedTutorialManager guidedTutorialManager;

    [Header("Pause Overlay (opsional)")]
    [SerializeField] private PauseOverlay pauseOverlay;

    [Header("Auto Open (Player Baru)")]
    [SerializeField] private bool autoOpenForNewPlayer = true;
    [SerializeField, Min(0f)] private float autoOpenDelay = 1.5f;

    [Header("Skip Carousel Delay")]
    [SerializeField, Min(0f)] private float skipCarouselDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private void Start()
    {
        // ✅ Kalau SkipCarouselOnLoad → langsung guided (karena sudah lihat di MainMenu)
        if (GameProgressManager.SkipCarouselOnLoad)
        {
            Log("⏭️ Skip carousel (sudah dilihat di MainMenu) → langsung guided tutorial.");
            GameProgressManager.SetSkipCarouselOnLoad(false);   // reset flag
            GameProgressManager.SetGuidedTutorialActive(true);

            if (tutorialCarouselPanel != null)
                tutorialCarouselPanel.SetActive(false);

            StartCoroutine(StartGuidedDelayed());
            return;
        }

        if (!autoOpenForNewPlayer) return;

        bool isTutorialDone = GameProgressManager.IsGuidedTutorialCompleted();
        bool isFirstTime = !GameProgressManager.HasProgress();
        bool isNewPlayer = isFirstTime || !isTutorialDone;

        if (isNewPlayer)
        {
            Log("🆕 Player baru — auto-open carousel.");
            StartCoroutine(WaitAndOpenRoutine(isTutorialDone));
        }
        else
        {
            Log("👤 Player lama — skip auto-open.");
        }
    }

    private IEnumerator StartGuidedDelayed()
    {
        yield return null;
        yield return new WaitForSeconds(skipCarouselDelay);

        if (guidedTutorialPanel != null)
            guidedTutorialPanel.SetActive(true);

        if (guidedTutorialManager != null)
            guidedTutorialManager.BeginTutorial();
        else
            Log("⚠️ guidedTutorialManager belum di-assign!");
    }

    private IEnumerator WaitAndOpenRoutine(bool isTutorialDone)
    {
        if (isTutorialDone)
            yield return new WaitUntil(() => CameraIntroManager.GameStarted);
        else
            yield return new WaitForSeconds(autoOpenDelay);

        AutoOpenTutorial();
    }

    private void AutoOpenTutorial()
    {
        if (tutorialCarouselPanel == null) { Log("⚠️ tutorialCarouselPanel null — skip."); return; }
        if (tutorialCarouselPanel.activeSelf) { Log("ℹ️ Carousel sudah terbuka — skip."); return; }

        Log("📖 Auto-open carousel tutorial.");
        tutorialCarouselPanel.SetActive(true);
    }

    public void OnStartLearningClicked()
    {
        Log("✅ Tombol YA diklik → mulai guided tutorial.");

        Time.timeScale = 1f;

        // ✅ Set flag guided tutorial aktif
        GameProgressManager.SetGuidedTutorialActive(true);

        // Stop countdown kalau ada
        if (CameraIntroManager.Instance != null)
            CameraIntroManager.Instance.StopCountdown();

        if (tutorialCarouselPanel != null) tutorialCarouselPanel.SetActive(false);
        if (guidedTutorialPanel != null) guidedTutorialPanel.SetActive(true);

        if (guidedTutorialManager != null)
            guidedTutorialManager.BeginTutorial();
        else
            Log("⚠️ guidedTutorialManager belum di-assign!");
    }

    public void CloseTutorialCarousel()
    {
        Log("❌ Tutup carousel tanpa lanjut.");
        GameProgressManager.SetGuidedTutorialActive(false);

        if (tutorialCarouselPanel != null) tutorialCarouselPanel.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.OpenPause();
    }

    private void Log(string msg) { if (debugLog) Debug.Log($"[TutorialFlow] {msg}"); }
}