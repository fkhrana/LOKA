using System;
using System.Collections;
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
    [SerializeField] private float progressAnimationDuration = 0.25f;

    [Header("Optional Events")]
    public UnityEvent OnReachedWaveMilestone; // invoked when reaching a milestone (e.g., show puzzle)
    public UnityEvent OnReachedLevelComplete; // invoked when full level complete (100%)

    private int totalEnemies = 1;
    private int processedEnemies = 0;
    private HashSet<int> triggeredMilestones = new HashSet<int>();
    private List<int> milestones = new List<int>();
    private Coroutine progressAnimation;

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

    public void OnEnemyProcessed()
    {
        processedEnemies = Mathf.Min(totalEnemies, processedEnemies + 1);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (progressBar != null)
        {
            float targetValue = (float)processedEnemies / (float)totalEnemies;

            if (progressBar.fillRect != null)
                progressBar.fillRect.gameObject.SetActive(processedEnemies > 0);

            if (progressAnimation != null)
                StopCoroutine(progressAnimation);

            if (targetValue <= 0f || progressAnimationDuration <= 0f)
            {
                progressBar.value = targetValue;
            }
            else
            {
                progressAnimation = StartCoroutine(AnimateProgressBar(targetValue));
            }
        }

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

    private IEnumerator AnimateProgressBar(float targetValue)
    {
        float startValue = progressBar.value;
        float elapsed = 0f;

        while (elapsed < progressAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / progressAnimationDuration);
            t = t * t * (3f - 2f * t);
            progressBar.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }

        progressBar.value = targetValue;
        progressAnimation = null;
    }
}
