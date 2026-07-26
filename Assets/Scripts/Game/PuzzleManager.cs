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
    public PowerManager powerManager; 
    public float delayBeforeWave2 = 2f;

    void Awake()
    {
        Instance = this;
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
        Debug.Log("Puzzle selesai!");
        StartCoroutine(PuzzleCompleteSequence());
    }

    IEnumerator PuzzleCompleteSequence()
    {
        puzzlePanel.SetActive(false);

        if (powerManager != null)
        {
            powerManager.SetUnlocked();          // ganti tampilan jadi unlocked
            StartCoroutine(PopEffect(powerManager.transform)); // animasi pop
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