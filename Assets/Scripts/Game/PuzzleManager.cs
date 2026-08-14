using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Drag semua slot (RectangleMini) ke sini")]
    public List<DropZone> allSlots;

    [Header("Referensi UI & VFX")]
    public GameObject puzzlePanel;
    public GameObject winPanel;
    public PowerManager powerManager; 
    public float delayBeforeWinPanel = 2f;
    public float delayBeforeWave2 = 2f;
    [Tooltip("Optional: GestureDrawer yang mengelola input gesture. Dinonaktifkan saat puzzle panel muncul.")]
    public GestureDrawer gestureDrawer;

    private bool wave1PuzzleShown;
    private bool puzzleCompleted;
    private bool winPanelShown;

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
            puzzlePanel.SetActive(true);
        }
    }

    public void ShowPuzzlePanel()
    {
        if (puzzlePanel != null)
        {
            DisableGestureInput();
            puzzlePanel.SetActive(true);
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
            if (!slot.isFilled) return;
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
        // Tetap tampilkan puzzle panel selama delay, jangan langsung di-nonaktifkan
        if (gestureDrawer != null)
            EnableGestureInput();

        if (powerManager != null)
        {
            powerManager.SetUnlocked();          // ganti tampilan jadi unlocked
            StartCoroutine(PopEffect(powerManager.transform)); // animasi pop
        }

        yield return new WaitForSeconds(delayBeforeWinPanel);

        // Tutup puzzle panel sebelum menampilkan win panel
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (winPanel != null && !winPanelShown)
        {
            winPanelShown = true;
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        yield return new WaitForSeconds(delayBeforeWave2);

        GoToWave2();
    }

    IEnumerator PopEffect(Transform target)
    {
        float duration = 0.4f;
        float time = 0f;
        Vector3 originalScale = target.localScale;
        Vector3 punchScale = originalScale * 1.3f; // sedikit membesar dulu

        // Membesar dulu
        while (time < duration / 2)
        {
            time += Time.deltaTime;
            target.localScale = Vector3.Lerp(originalScale, punchScale, time / (duration / 2));
            yield return null;
        }

        time = 0f;
        // Balik ke ukuran normal
        while (time < duration / 2)
        {
            time += Time.deltaTime;
            target.localScale = Vector3.Lerp(punchScale, originalScale, time / (duration / 2));
            yield return null;
        }

        target.localScale = originalScale;
    }

    void GoToWave2()
    {
        Debug.Log("Lanjut ke Wave 2");
        // WaveManager.Instance.StartWave(2);
    }
}