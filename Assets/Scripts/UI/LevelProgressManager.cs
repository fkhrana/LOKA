using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    [Header("Optional Events")]
    public UnityEvent OnReachedWaveMilestone; // invoked when reaching a milestone (e.g., show puzzle)
    public UnityEvent OnReachedLevelComplete; // invoked when full level complete (100%)

    private int totalEnemies = 1;
    private int processedEnemies = 0; // number of enemies spawned/processed
    private HashSet<int> triggeredMilestones = new HashSet<int>();
    private List<int> milestones = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialize(int totalEnemiesInLevel, List<int> waveMilestones = null)
    {
        totalEnemies = Mathf.Max(1, totalEnemiesInLevel);
        processedEnemies = 0;
        triggeredMilestones.Clear();
        milestones.Clear();

        if (waveMilestones != null)
            milestones.AddRange(waveMilestones);

        UpdateUI();
    }

    public void OnEnemySpawned()
    {
        processedEnemies = Mathf.Min(totalEnemies, processedEnemies + 1);
        UpdateUI();
    }

    public void OnEnemyProcessed() // for future use if needed (e.g., defeated)
    {
        // Could track processed differently; for now treat same as spawn
        processedEnemies = Mathf.Min(totalEnemies, processedEnemies + 1);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (progressBar != null)
            progressBar.value = (float)processedEnemies / (float)totalEnemies;

        if (progressText != null)
            progressText.text = $"{processedEnemies}/{totalEnemies}";

        // Check milestones
        for (int i = 0; i < milestones.Count; i++)
        {
            int m = milestones[i];
            if (!triggeredMilestones.Contains(m) && processedEnemies >= m)
            {
                triggeredMilestones.Add(m);
                OnReachedWaveMilestone?.Invoke();
            }
        }

        if (processedEnemies >= totalEnemies)
        {
            OnReachedLevelComplete?.Invoke();
        }
    }
}
