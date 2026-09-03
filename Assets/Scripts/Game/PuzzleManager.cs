using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyTransition;

public class PuzzleManager : MonoBehaviour
{
public static PuzzleManager Instance;
[Header("Drag semua slot (RectangleMini) ke sini")]
public List<DropZone> allSlots;

[Header("Referensi UI & VFX")]
public GameObject puzzlePanel;
public GameObject rewardPanel;
public GameObject winPanel;
public PowerManager powerManager;
public float delayBeforeWinPanel = 2f;

[Tooltip("Optional: GestureDrawer yang mengelola input gesture. Dinonaktifkan saat puzzle panel muncul.")]
public GestureDrawer gestureDrawer;

[Header("Transition")]
[SerializeField] private TransitionSettings transitionSettings;
[SerializeField] private float transitionDelay = 0.5f;

private TransitionManager transitionManager;

private bool wave1PuzzleShown;
private bool puzzleCompleted;

void Awake()
{
    Instance = this;

    if (gestureDrawer == null)
        gestureDrawer = FindAnyObjectByType<GestureDrawer>();
}

public void ShowPuzzleOnce()
{
    if (wave1PuzzleShown)
        return;

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
    if (puzzlePanel != null)
        puzzlePanel.SetActive(true);

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
        if (!slot.isFilled)
            return;
    }

    OnPuzzleComplete();
}

void OnPuzzleComplete()
{
    if (puzzleCompleted)
        return;

    Debug.Log("Puzzle selesai!");
    MarkPuzzleCompleted();
    StartCoroutine(PuzzleCompleteSequence());
}

IEnumerator PuzzleCompleteSequence()
{
    if (gestureDrawer != null)
        EnableGestureInput();

    PowerManager.UnlockPowerUp();

    if (powerManager != null)
    {
        powerManager.SetUnlocked();
        StartCoroutine(PopEffect(powerManager.transform));
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
    if (puzzlePanel != null)
        puzzlePanel.SetActive(false);

    if (rewardPanel != null)
        rewardPanel.SetActive(true);

    if (transitionManager != null)
        transitionManager.onTransitionCutPointReached -= ActivateRewardPanel;
}

IEnumerator PopEffect(Transform target)
{
    float duration = 0.4f;
    float time = 0f;

    Vector3 originalScale = target.localScale;
    Vector3 punchScale = originalScale * 1.3f;

    while (time < duration / 2)
    {
        time += Time.deltaTime;

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
        time += Time.deltaTime;

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
