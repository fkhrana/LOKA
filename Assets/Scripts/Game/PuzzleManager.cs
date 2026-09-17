using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyTransition;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Drag semua slot DropZone ke sini")]
    public List<DropZone> allSlots;

    [Header("Referensi UI")]
    public GameObject canvas2;
    public GameObject puzzlePanel;
    public GameObject rewardPanel;
    public PowerManager powerManager;

    [Header("Reward Power Up")]
    [SerializeField] private PowerManager.PowerUpType rewardPowerUpType = PowerManager.PowerUpType.Freeze;

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

    private TransitionManager transitionManager;
    private bool wave1PuzzleShown;
    private bool puzzleCompleted;

    private void Awake()
    {
        Instance = this;

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (saveCurrentProgress == null)
            saveCurrentProgress = FindFirstObjectByType<SaveCurrentProgress>();
    }

    public void ShowPuzzleOnce()
    {
        if (wave1PuzzleShown) return;

        wave1PuzzleShown = true;
        puzzleCompleted = false;

        if (puzzlePanel != null)
        {
            DisableGestureInput();

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
    }

    public void ShowPuzzlePanel()
    {
        if (puzzlePanel != null)
        {
            DisableGestureInput();

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
    }

    private void ActivatePuzzlePanel()
    {
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(true);
        if (rewardPanel != null) rewardPanel.SetActive(false);

        if (saveCurrentProgress != null)
            saveCurrentProgress.MarkPuzzleActive();
        else
            GameProgressManager.SaveGameState("Puzzle");

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
        if (gestureDrawer != null)
            gestureDrawer.enabled = true;
    }

    public bool IsPuzzleCompleted() => puzzleCompleted;
    public void MarkPuzzleCompleted() => puzzleCompleted = true;

    public void PlayWaveCompleteVfx()
    {
        PlayPuzzleCompleteVfx();
    }

    public IEnumerator PlayWaveCompleteSequence()
    {
        PlayWaveCompleteVfx();

        yield return new WaitForSeconds(delayBeforeWinPanel);

        if (puzzleCompleteVfx != null)
            puzzleCompleteVfx.SetActive(false);

        ShowPuzzleOnce();
    }

    public void CheckPuzzleComplete()
    {
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

    private void PlayPuzzleCompleteVfx()
    {
        if (puzzleCompleteVfx == null)
            return;

        puzzleCompleteVfx.SetActive(true);

        if (winSfx != null)
            AudioManager.Instance?.PlayUISFX(winSfx);

        EfekConfetti[] confettiEffects =
            puzzleCompleteVfx.GetComponentsInChildren<EfekConfetti>(true);

        foreach (EfekConfetti confettiEffect in confettiEffects)
        {
            confettiEffect.gameObject.SetActive(true); // FIX: aktifkan child dulu sebelum StartCoroutine
            confettiEffect.MuntahkanConfetti();
        }

        ParticleSystem[] particles =
            puzzleCompleteVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particle in particles)
        {
            particle.gameObject.SetActive(true);
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Debug.Log(
            "Puzzle complete VFX dimainkan: " +
            confettiEffects.Length + " efek terompet, " +
            particles.Length + " particle system."
        );
    }

    private IEnumerator PuzzleCompleteSequence()
    {
        if (gestureDrawer != null)
            EnableGestureInput();

        if (powerManager != null)
        {
            powerManager.SetUnlocked(rewardPowerUpType);

            Transform target = powerManager.GetSlotTransform(rewardPowerUpType);
            if (target != null)
                StartCoroutine(PopEffect(target));
        }
        else
        {
            PowerManager.UnlockPowerUp(rewardPowerUpType);
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

    private void ActivateRewardPanel()
    {
        if (canvas2 != null) canvas2.SetActive(true);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(true);

        if (saveCurrentProgress != null)
            saveCurrentProgress.MarkRewardActive();
        else
            GameProgressManager.SaveGameState("Reward");

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

            target.localScale = Vector3.Lerp(
                originalScale, punchScale, time / (duration / 2));

            yield return null;
        }

        time = 0f;

        while (time < duration / 2)
        {
            time += Time.unscaledDeltaTime;

            target.localScale = Vector3.Lerp(
                punchScale, originalScale, time / (duration / 2));

            yield return null;
        }

        target.localScale = originalScale;
    }
}