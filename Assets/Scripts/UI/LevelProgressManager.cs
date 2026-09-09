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

    [Header("Non-Collectible Item VFX")]
    [SerializeField] private Transform[] levelBarStarTargetsByWave;
    [SerializeField] private GameObject barItemTrailVfx;
    [SerializeField] private GameObject trailCollectItemVfx;
    [SerializeField] private Vector3 barItemTrailSpawnOffset = new Vector3(0f, 0f, 0.2f);
    [SerializeField] private float barItemTrailDuration = 0.8f;
    [SerializeField] private float trailCollectItemScale = 0.5f;
    [SerializeField] private float trailCollectItemSpeed = 8f;
    [SerializeField] private float trailCollectItemArrivalDistance = 0.15f;
    [SerializeField] private float trailCollectItemEndDelay = 0.15f;

    [Header("Non-Collectible Item VFX SFX")]
    [SerializeField] private bool useNonCollectibleVfxSFX = true;
    [SerializeField] private string nonCollectibleVfxSFXName = "Success";
    [Range(0f, 1f)]
    [SerializeField] private float nonCollectibleVfxSFXVolume = 1f;

    [Header("Optional Events")]
    public UnityEvent OnReachedWaveMilestone; // invoked when reaching a milestone (e.g., show puzzle)
    public UnityEvent OnReachedLevelComplete; // invoked when full level complete (100%)

    private int totalEnemies = 1;
    private int processedEnemies = 0;
    private HashSet<int> triggeredMilestones = new HashSet<int>();
    private List<int> milestones = new List<int>();
    private Coroutine progressAnimation;
    private Transform activeLevelBarStarTarget;

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

    public void SetLevelBarTargetForWave(int waveIndex)
    {
        if (levelBarStarTargetsByWave == null ||
            waveIndex < 0 ||
            waveIndex >= levelBarStarTargetsByWave.Length ||
            levelBarStarTargetsByWave[waveIndex] == null)
            return;

        activeLevelBarStarTarget = levelBarStarTargetsByWave[waveIndex];
    }

    public void PlayNonCollectibleItemVfx(Vector3 itemPosition)
    {
        Transform target = activeLevelBarStarTarget != null
            ? activeLevelBarStarTarget
            : progressBar != null
                ? progressBar.transform
                : null;

        if (target == null || barItemTrailVfx == null || trailCollectItemVfx == null)
            return;

        // SFX ditambahkan tanpa mengubah logic VFX sebelumnya
        PlayNonCollectibleVfxSFX();

        StartCoroutine(PlayNonCollectibleItemVfxRoutine(itemPosition, target));
    }

    private void PlayNonCollectibleVfxSFX()
    {
        if (useNonCollectibleVfxSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                nonCollectibleVfxSFXName,
                nonCollectibleVfxSFXVolume
            );
        }
    }

    private IEnumerator PlayNonCollectibleItemVfxRoutine(Vector3 itemPosition, Transform target)
    {
        Vector3 spawnPosition = itemPosition + barItemTrailSpawnOffset;
        GameObject barTrail = Instantiate(
            barItemTrailVfx,
            spawnPosition,
            barItemTrailVfx.transform.rotation);
        PlayParticleSystems(barTrail);

        yield return new WaitForSeconds(barItemTrailDuration);

        if (barTrail != null)
            Destroy(barTrail);

        GameObject collectTrail = Instantiate(
            trailCollectItemVfx,
            spawnPosition,
            trailCollectItemVfx.transform.rotation);
        collectTrail.transform.localScale *= trailCollectItemScale;

        while (collectTrail != null && target != null)
        {
            Vector3 targetPosition = GetWorldTargetPosition(target, collectTrail.transform.position.z);
            collectTrail.transform.position = Vector3.MoveTowards(
                collectTrail.transform.position,
                targetPosition,
                trailCollectItemSpeed * Time.deltaTime);

            if (Vector3.Distance(collectTrail.transform.position, targetPosition) <= trailCollectItemArrivalDistance)
                break;

            yield return null;
        }

        if (collectTrail != null)
        {
            collectTrail.transform.position = GetWorldTargetPosition(
                target,
                collectTrail.transform.position.z);
            yield return new WaitForSeconds(trailCollectItemEndDelay);
            Destroy(collectTrail);
        }
    }

    private void PlayParticleSystems(GameObject effect)
    {
        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems)
            particleSystem.Play(true);
    }

    private Vector3 GetWorldTargetPosition(Transform target, float sourceZ)
    {
        RectTransform targetRect = target as RectTransform;
        Camera worldCamera = Camera.main;

        if (targetRect == null || worldCamera == null)
            return target.position;

        Canvas canvas = targetRect.GetComponentInParent<Canvas>();
        Camera canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(canvasCamera, targetRect.position);
        float cameraDistance = Mathf.Abs(worldCamera.transform.position.z - sourceZ);
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, cameraDistance));
        worldPosition.z = sourceZ;
        return worldPosition;
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
