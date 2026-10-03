using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyTransition;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Drag semua slot DropZone ke sini (Gameplay)")]
    public List<DropZone> allSlots;

    [Header("Tutorial Slots (opsional)")]
    [Tooltip("Slot khusus untuk guided tutorial.")]
    [SerializeField] private List<DropZone> tutorialSlots = new List<DropZone>();

    [Header("Referensi UI")]
    public GameObject canvas2;
    public GameObject puzzlePanel;
    public GameObject rewardPanel;
    public PowerManager powerManager;

    [Header("Tutorial Panel")]
    [Tooltip("Panel PopUp untuk tutorial (SetActive false di awal).")]
    [SerializeField] private GameObject tutorialPuzzlePanel;

    [Header("Reward Power Up")]
    [SerializeField] private PowerManager.PowerUpType rewardPowerUpType =
        PowerManager.PowerUpType.Freeze;

    [Header("Puzzle Complete VFX")]
    [SerializeField] private GameObject puzzleCompleteVfx;

    [Header("Win SFX")]
    [SerializeField] private AudioClip winSfx;

    [Header("Delay")]
    public float delayBeforeWinPanel = 2f;

    [Header("Gesture")]
    public GestureDrawer gestureDrawer;

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float transitionDelay = 0.5f;

    [Header("Save Integration (opsional)")]
    [SerializeField] private SaveCurrentProgress saveCurrentProgress;

    [Header("Tutorial Mode")]
    [Tooltip("Kalau true, puzzle selesai → VFX saja, panel tidak pindah, save state di-skip.")]
    [SerializeField] private bool isTutorialMode = false;

    [Tooltip("Panel finish khusus tutorial.")]
    [SerializeField] private GameObject finishPanel;

    private TransitionManager transitionManager;
    private bool wave1PuzzleShown;
    private bool puzzleCompleted;

    private List<DropZone> originalGameplaySlots;

    private void Awake()
    {
        Instance = this;

        originalGameplaySlots = new List<DropZone>(allSlots);

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (powerManager == null)
            powerManager = FindAnyObjectByType<PowerManager>(FindObjectsInactive.Include);

        if (saveCurrentProgress == null)
            saveCurrentProgress = FindFirstObjectByType<SaveCurrentProgress>();
    }

    // ================================================================
    // TUTORIAL MODE API
    // ================================================================

    public void SetTutorialMode(bool value)
    {
        isTutorialMode = value;
        Debug.Log($"[PuzzleManager] Tutorial mode = {value}");
    }

    public void SetTutorialSlots()
    {
        Debug.Log("[PuzzleManager] SetTutorialSlots() dipanggil.");

        if (tutorialPuzzlePanel != null)
        {
            tutorialPuzzlePanel.SetActive(true);
            Debug.Log("[PuzzleManager] TutorialPuzzlePanel diaktifkan.");
        }
        else
        {
            Debug.LogWarning("[PuzzleManager] tutorialPuzzlePanel belum di-assign!");
        }

        if (tutorialSlots != null && tutorialSlots.Count > 0)
        {
            allSlots = new List<DropZone>(tutorialSlots);
            Debug.Log($"[PuzzleManager] Slot diganti ke tutorial ({allSlots.Count} slot).");
        }
        else
        {
            Debug.LogWarning("[PuzzleManager] tutorialSlots kosong — pakai gameplay slots.");
        }
    }

    public void RestoreGameplaySlots()
    {
        Debug.Log("[PuzzleManager] RestoreGameplaySlots() dipanggil.");

        if (tutorialPuzzlePanel != null)
        {
            tutorialPuzzlePanel.SetActive(false);
            Debug.Log("[PuzzleManager] TutorialPuzzlePanel dinonaktifkan.");
        }

        if (originalGameplaySlots != null && originalGameplaySlots.Count > 0)
        {
            allSlots = new List<DropZone>(originalGameplaySlots);
            Debug.Log($"[PuzzleManager] Slot dikembalikan ke gameplay ({allSlots.Count} slot).");
        }
    }

    public void ResetPuzzleForTutorial()
    {
        if (allSlots != null)
        {
            foreach (var slot in allSlots)
            {
                if (slot != null)
                    slot.isFilled = false;
            }
        }

        puzzleCompleted = false;
        wave1PuzzleShown = false;

        Debug.Log("[PuzzleManager] Puzzle di-reset untuk tutorial.");
    }

    public void ResetPuzzleStateForGameplay()
    {
        puzzleCompleted = false;
        Debug.Log("[PuzzleManager] Puzzle state di-reset untuk gameplay.");
    }

    public void ShowFinishPanelForTutorial()
    {
        ShowFinishPanel();
    }

    // ✅ Hide FinishPanel (dipanggil oleh GuidedTutorialManager)
    public void HideFinishPanel()
    {
        if (finishPanel != null)
            finishPanel.SetActive(false);
        Debug.Log("[PuzzleManager] ✅ Finish panel disembunyikan.");
    }

    private void ShowFinishPanel()
    {
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (tutorialPuzzlePanel != null) tutorialPuzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (finishPanel != null) finishPanel.SetActive(true);

        SetGameStarted(true);

        Debug.Log("[PuzzleManager] Finish panel (tutorial) ditampilkan.");
    }

    // ================================================================
    // SHOW PUZZLE
    // ================================================================

    public void ShowPuzzleOnce()
    {
        if (wave1PuzzleShown) return;

        wave1PuzzleShown = true;
        puzzleCompleted = false;

        if (puzzlePanel == null) return;

        DisableGestureInput();

        // ✅ SKIP TRANSISI kalau tutorial mode
        if (isTutorialMode)
        {
            Debug.Log("[PuzzleManager] Tutorial mode — skip transisi, langsung tampilkan panel.");
            ActivatePuzzlePanel();
            return;
        }

        // Transisi normal untuk gameplay
        transitionManager = TransitionManager.Instance();

        if (transitionManager != null && transitionSettings != null)
        {
            transitionManager.onTransitionCutPointReached += ActivatePuzzlePanel;
            transitionManager.Transition(transitionSettings, transitionDelay);
        }
        else
        {
            ActivatePuzzlePanel();
        }
    }

    public void ShowPuzzlePanel()
    {
        if (puzzlePanel == null) return;

        DisableGestureInput();

        // ✅ SKIP TRANSISI kalau tutorial mode
        if (isTutorialMode)
        {
            Debug.Log("[PuzzleManager] Tutorial mode — skip transisi, langsung tampilkan panel.");
            ActivatePuzzlePanel();
            return;
        }

        // Transisi normal untuk gameplay
        transitionManager = TransitionManager.Instance();

        if (transitionManager != null && transitionSettings != null)
        {
            transitionManager.onTransitionCutPointReached += ActivatePuzzlePanel;
            transitionManager.Transition(transitionSettings, transitionDelay);
        }
        else
        {
            ActivatePuzzlePanel();
        }
    }

    private void ActivatePuzzlePanel()
    {
        if (canvas2 != null) canvas2.SetActive(true);

        if (isTutorialMode)
        {
            if (tutorialPuzzlePanel != null)
                tutorialPuzzlePanel.SetActive(true);
        }
        else
        {
            if (puzzlePanel != null)
                puzzlePanel.SetActive(true);
        }

        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (finishPanel != null) finishPanel.SetActive(false);

        SetGameStarted(true);

        if (!isTutorialMode)
        {
            if (saveCurrentProgress != null)
                saveCurrentProgress.MarkPuzzleActive();
            else
                GameProgressManager.SaveGameState("Puzzle");
        }
        else
        {
            Debug.Log("[PuzzleManager] Tutorial mode — skip save state.");
        }

        if (transitionManager != null)
            transitionManager.onTransitionCutPointReached -= ActivatePuzzlePanel;
    }

    private void OnDestroy()
    {
        if (transitionManager != null)
        {
            transitionManager.onTransitionCutPointReached -= ActivatePuzzlePanel;
            transitionManager.onTransitionCutPointReached -= ActivateRewardPanel;
        }
    }

    private void DisableGestureInput()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.ResetGestureInput();
            gestureDrawer.enabled = false;
        }
    }

    private void EnableGestureInput()
    {
        if (gestureDrawer != null) gestureDrawer.enabled = true;
    }

    private void SetGameStarted(bool started)
    {
        CameraIntroManager.GameStarted = started;
    }

    // ================================================================
    // PUZZLE STATE
    // ================================================================

    public bool IsPuzzleCompleted() => puzzleCompleted;

    public void MarkPuzzleCompleted() => puzzleCompleted = true;

    public void PlayWaveCompleteVfx() => PlayPuzzleCompleteVfx();

    public IEnumerator PlayWaveCompleteSequence()
    {
        PlayWaveCompleteVfx();

        yield return new WaitForSeconds(delayBeforeWinPanel);

        if (puzzleCompleteVfx != null)
            puzzleCompleteVfx.SetActive(false);

        ShowPuzzleOnce();
    }

    public void PlayWaveCompleteSequenceFromProgress()
    {
        StartCoroutine(PlayWaveCompleteSequence());
    }

    public void CheckPuzzleComplete()
    {
        if (allSlots == null) return;

        foreach (DropZone slot in allSlots)
        {
            if (slot == null) continue;
            if (!slot.isFilled) return;
        }

        OnPuzzleComplete();
    }

    private void OnPuzzleComplete()
    {
        if (puzzleCompleted) return;

        Debug.Log("Puzzle selesai!");

        MarkPuzzleCompleted();

        PlayPuzzleCompleteVfx();

        StartCoroutine(PuzzleCompleteSequence());
    }

    // ================================================================
    // PUZZLE COMPLETE VFX
    // ================================================================

    private void PlayPuzzleCompleteVfx()
    {
        if (puzzleCompleteVfx == null) return;

        puzzleCompleteVfx.SetActive(true);

        if (winSfx != null)
            AudioManager.Instance?.PlayUISFX(winSfx);

        EfekConfetti[] confettiEffects =
            puzzleCompleteVfx.GetComponentsInChildren<EfekConfetti>(true);

        foreach (EfekConfetti confettiEffect in confettiEffects)
        {
            if (confettiEffect == null) continue;
            confettiEffect.gameObject.SetActive(true);
            confettiEffect.MuntahkanConfetti();
        }

        ParticleSystem[] particles =
            puzzleCompleteVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particle in particles)
        {
            if (particle == null) continue;

            particle.gameObject.SetActive(true);
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);
        }

        Debug.Log(
            "Puzzle complete VFX dimainkan: " +
            confettiEffects.Length + " confetti, " +
            particles.Length + " particle."
        );
    }

    // ================================================================
    // PUZZLE COMPLETE SEQUENCE
    // ================================================================

    private IEnumerator PuzzleCompleteSequence()
    {
        if (gestureDrawer != null)
            EnableGestureInput();

        SetGameStarted(true);

        if (isTutorialMode)
        {
            yield return new WaitForSeconds(delayBeforeWinPanel);

            if (puzzleCompleteVfx != null)
                puzzleCompleteVfx.SetActive(false);

            Debug.Log("[PuzzleManager] Tutorial puzzle selesai (VFX saja).");
            yield break;
        }

        if (powerManager != null)
        {
            powerManager.SetUnlocked(rewardPowerUpType);

            int levelIndex = LevelManager.Instance != null
                ? LevelManager.Instance.GetCurrentLevelIndex()
                : PlayerPrefs.GetInt("CurrentLevelIndex", 0);

            BooksFinal.SaveAutomaticPowerUpReward(
                levelIndex,
                rewardPowerUpType,
                powerManager.GetPowerUpSprite(rewardPowerUpType),
                powerManager.GetPowerUpNameSprite(rewardPowerUpType),
                powerManager.GetPowerUpRewardDescription(rewardPowerUpType)
            );

            Transform target = powerManager.GetSlotTransform(rewardPowerUpType);
            if (target != null)
                StartCoroutine(PopEffect(target));
        }
        else
        {
            PowerManager.UnlockPowerUp(rewardPowerUpType);

            int levelIndex = LevelManager.Instance != null
                ? LevelManager.Instance.GetCurrentLevelIndex()
                : PlayerPrefs.GetInt("CurrentLevelIndex", 0);

            BooksFinal.SaveAutomaticPowerUpReward(
                levelIndex, rewardPowerUpType, null, null, null
            );

            Debug.LogWarning("PuzzleManager: PowerManager tidak terhubung.");
        }

        yield return new WaitForSeconds(delayBeforeWinPanel);

        transitionManager = TransitionManager.Instance();

        if (transitionManager != null && transitionSettings != null)
        {
            transitionManager.onTransitionCutPointReached += ActivateRewardPanel;
            transitionManager.Transition(transitionSettings, transitionDelay);
        }
        else
        {
            ActivateRewardPanel();
        }
    }

    // ================================================================
    // REWARD PANEL (NORMAL)
    // ================================================================

    private void ActivateRewardPanel()
    {
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (tutorialPuzzlePanel != null) tutorialPuzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(true);
        if (finishPanel != null) finishPanel.SetActive(false);

        SetGameStarted(true);

        if (!isTutorialMode)
        {
            if (saveCurrentProgress != null)
                saveCurrentProgress.MarkRewardActive();
            else
                GameProgressManager.SaveGameState("Reward");
        }

        if (transitionManager != null)
            transitionManager.onTransitionCutPointReached -= ActivateRewardPanel;
    }

    private IEnumerator PopEffect(Transform target)
    {
        float duration = 0.4f;
        float time = 0f;

        Vector3 originalScale = target.localScale;
        Vector3 punchScale = originalScale * 1.3f;

        while (time < duration / 2)
        {
            time += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(originalScale, punchScale, time / (duration / 2));
            yield return null;
        }

        time = 0f;

        while (time < duration / 2)
        {
            time += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(punchScale, originalScale, time / (duration / 2));
            yield return null;
        }

        target.localScale = originalScale;
    }
}