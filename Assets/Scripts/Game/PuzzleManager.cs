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

    [Header("Puzzle Complete VFX")]
    [SerializeField] private GameObject puzzleCompleteVfx;

    [Header("Delay")]
    public float delayBeforeWinPanel = 2f;

    [Header("Gesture")]
    public GestureDrawer gestureDrawer;

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float transitionDelay = 0.5f;

    private TransitionManager transitionManager;
    private bool wave1PuzzleShown;
    private bool puzzleCompleted;

    private void Awake()
    {
        Instance = this;

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();
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
                transitionManager.Transition(
                    transitionSettings,
                    transitionDelay
                );
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
                transitionManager.Transition(
                    transitionSettings,
                    transitionDelay
                );
            }
            else
            {
                ActivatePuzzlePanel();
            }
        }
    }

    private void ActivatePuzzlePanel()
    {
        // Canvas 2 ON
        if (canvas2 != null)
            canvas2.SetActive(true);

        // Puzzle ON
        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);

        // Reward OFF
        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        // Save state resume
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

    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }

    public void MarkPuzzleCompleted()
    {
        puzzleCompleted = true;
    }

    public void CheckPuzzleComplete()
    {
        foreach (DropZone slot in allSlots)
        {
            if (slot == null) continue;

            if (!slot.isFilled)
                return;
        }

        OnPuzzleComplete();
    }

    private void OnPuzzleComplete()
    {
        if (puzzleCompleted)
            return;

        Debug.Log("✅ Puzzle selesai!");

        MarkPuzzleCompleted();
        PlayPuzzleCompleteVfx();

        // Jalankan sequence selesai
        StartCoroutine(PuzzleCompleteSequence());
    }

    private void PlayPuzzleCompleteVfx()
    {
        if (puzzleCompleteVfx == null)
            return;

        puzzleCompleteVfx.SetActive(true);

        EfekConfetti[] confettiEffects =
            puzzleCompleteVfx.GetComponentsInChildren<EfekConfetti>(true);

        foreach (EfekConfetti confettiEffect in confettiEffects)
            confettiEffect.MuntahkanConfetti();

        ParticleSystem[] particles =
            puzzleCompleteVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particle in particles)
        {
            particle.gameObject.SetActive(true);
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Debug.Log(
            "✨ Puzzle complete VFX dimainkan: " +
            confettiEffects.Length + " efek terompet, " +
            particles.Length + " particle system."
        );
    }

    private IEnumerator PuzzleCompleteSequence()
    {
        if (gestureDrawer != null)
            EnableGestureInput();

        PowerManager.UnlockPowerUp();

        if (powerManager != null)
        {
            powerManager.SetUnlocked();
            StartCoroutine(
                PopEffect(powerManager.transform)
            );
        }

        yield return new WaitForSeconds(
            delayBeforeWinPanel
        );

        transitionManager = TransitionManager.Instance();

        if (transitionManager != null && transitionSettings != null)
        {
            transitionManager.onTransitionCutPointReached += ActivateRewardPanel;

            transitionManager.Transition(
                transitionSettings,
                transitionDelay
            );
        }
        else
        {
            ActivateRewardPanel();
        }
    }

    private void ActivateRewardPanel()
    {
        // Canvas 2 ON
        if (canvas2 != null)
            canvas2.SetActive(true);

        // Puzzle OFF
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        // Reward ON
        if (rewardPanel != null)
            rewardPanel.SetActive(true);

        // Save state resume
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
                originalScale,
                punchScale,
                time / (duration / 2)
            );

            yield return null;
        }

        time = 0f;

        while (time < duration / 2)
        {
            time += Time.unscaledDeltaTime;

            target.localScale = Vector3.Lerp(
                punchScale,
                originalScale,
                time / (duration / 2)
            );

            yield return null;
        }

        target.localScale = originalScale;
    }
}