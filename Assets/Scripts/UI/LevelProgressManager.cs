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
    [SerializeField] private Image progressBarGlow;
    [SerializeField] private ParticleSystem progressBarVfx;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private float progressAnimationDuration = 0.25f;
    [SerializeField] private float progressBarGlowFadeSpeed = 2f;
    [Range(0f, 1f)]
    [SerializeField] private float progressBarGlowStartFill = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float progressBarGlowEndFill = 1f;
    [SerializeField] private GameObject[] milestoneVfxByWave;
    [SerializeField] private float milestoneVfxLifetime = 2f;

    [Header("Non-Collectible Item VFX")]
    [SerializeField] private Transform[] levelBarStarTargetsByWave;
    [SerializeField] private GameObject barItemTrailVfx;
    [SerializeField] private GameObject trailCollectItemVfx;
    [SerializeField] private Vector3 barItemTrailSpawnOffset = new Vector3(0f, 0f, 0.2f);
    [SerializeField] private float barItemTrailDuration = 0.8f;
    [SerializeField] private float trailCollectItemScale = 0.5f;
    [SerializeField] private float trailCollectItemFlightDuration = 1.2f;
    [SerializeField] private float trailCollectItemArcHeight = 3f;
    [SerializeField] private float trailCollectItemSideOffset = 1f;
    [SerializeField] private AnimationCurve trailCollectItemFlightCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float trailCollectItemArrivalDistance = 0.15f;
    [SerializeField] private float trailCollectItemEndDelay = 0.15f;

    [Header("Non-Collectible Item VFX SFX")]
    [SerializeField] private bool useNonCollectibleVfxSFX = true;
    [SerializeField] private string nonCollectibleVfxSFXName = "Success";

    [Range(0f, 1f)]
    [SerializeField] private float nonCollectibleVfxSFXVolume = 1f;

    [Header("Testing")]
    [SerializeField] private Button addProgressButton;
    [Min(1)]
    [SerializeField] private int testingProgressAmount = 1;

    [Header("Optional Events")]
    public UnityEvent OnReachedWaveMilestone;
    public UnityEvent OnReachedLevelComplete;

    private int totalEnemies = 1;
    private int processedEnemies = 0;
    private int pendingProgress = 0;

    private HashSet<int> triggeredMilestones =
        new HashSet<int>();

    private List<int> milestones =
        new List<int>();

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

    private void OnEnable()
    {
        if (addProgressButton != null)
            addProgressButton.onClick.AddListener(AddProgressForTesting);
    }

    private void OnDisable()
    {
        if (addProgressButton != null)
            addProgressButton.onClick.RemoveListener(AddProgressForTesting);
    }

    public void Initialize(
        int totalEnemiesInLevel,
        List<int> waveMilestones = null
    )
    {
        totalEnemies =
            Mathf.Max(1, totalEnemiesInLevel);

        processedEnemies = 0;
        pendingProgress = 0;

        if (waveMilestones != null)
        {
            milestones.Clear();
            milestones.AddRange(waveMilestones);
        }

        triggeredMilestones.Clear();

        if (progressBarGlow != null)
        {
            UpdateGlow(0f);
            SetGlowAlpha(0f);
        }

        UpdateUI();
    }

    public void OnEnemyProcessed()
    {
        pendingProgress = Mathf.Min(
            totalEnemies - processedEnemies,
            pendingProgress + 1
        );
    }

    public void CompletePendingProgress()
    {
        if (pendingProgress <= 0 || processedEnemies >= totalEnemies)
            return;

        pendingProgress--;
        processedEnemies++;

        SetGlowAlpha(1f);

        if (progressBarVfx != null)
        {
            progressBarVfx.Clear();
            progressBarVfx.Play();
        }

        UpdateUI();
    }

    public void AddProgressForTesting()
    {
        int amount = Mathf.Max(1, testingProgressAmount);

        for (int i = 0; i < amount; i++)
        {
            if (processedEnemies >= totalEnemies)
                break;

            CompleteProgressForTesting();
        }
    }

    private void CompleteProgressForTesting()
    {
        processedEnemies = Mathf.Min(totalEnemies, processedEnemies + 1);
        SetGlowAlpha(1f);

        if (progressBarVfx != null)
        {
            progressBarVfx.Clear();
            progressBarVfx.Play();
        }

        UpdateUI();
    }

    private void Update()
    {
        if (progressBar != null)
        {
            if (progressBarGlow != null)
                UpdateGlow(progressBar.value);
        }

        if (progressBarGlow != null)
        {
            Color glowColor = progressBarGlow.color;

            if (glowColor.a > 0f)
            {
                glowColor.a = Mathf.MoveTowards(
                    glowColor.a,
                    0f,
                    progressBarGlowFadeSpeed * Time.deltaTime
                );
                progressBarGlow.color = glowColor;
            }
        }
    }

    private void SetGlowAlpha(float alpha)
    {
        if (progressBarGlow == null)
            return;

        Color glowColor = progressBarGlow.color;
        glowColor.a = Mathf.Clamp01(alpha);
        progressBarGlow.color = glowColor;
    }

    private void UpdateGlow(float progress)
    {
        if (progressBarGlow == null)
            return;

        progressBarGlow.fillAmount = Mathf.Lerp(
            progressBarGlowStartFill,
            progressBarGlowEndFill,
            Mathf.Clamp01(progress)
        );
    }

    public void SetLevelBarTargetForWave(
        int waveIndex
    )
    {
        if (
            levelBarStarTargetsByWave == null ||
            waveIndex < 0 ||
            waveIndex >= levelBarStarTargetsByWave.Length ||
            levelBarStarTargetsByWave[waveIndex] == null
        )
            return;

        activeLevelBarStarTarget =
            levelBarStarTargetsByWave[waveIndex];
    }

    public void PlayNonCollectibleItemVfx(
        Vector3 itemPosition
    )
    {
        Transform target =
            activeLevelBarStarTarget != null
                ? activeLevelBarStarTarget
                : progressBar != null
                    ? progressBar.transform
                    : null;

        if (
            target == null ||
            barItemTrailVfx == null ||
            trailCollectItemVfx == null
        )
        {
            CompletePendingProgress();
            return;
        }

        StartCoroutine(
            PlayNonCollectibleItemVfxRoutine(
                itemPosition,
                target
            )
        );
    }

    private void PlayNonCollectibleVfxSFX()
    {
        if (
            useNonCollectibleVfxSFX &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PlaySFX(
                nonCollectibleVfxSFXName,
                nonCollectibleVfxSFXVolume
            );
        }
    }

    private IEnumerator PlayNonCollectibleItemVfxRoutine(
        Vector3 itemPosition,
        Transform target
    )
    {
        Vector3 spawnPosition =
            itemPosition + barItemTrailSpawnOffset;

        GameObject barTrail =
            Instantiate(
                barItemTrailVfx,
                spawnPosition,
                barItemTrailVfx.transform.rotation
            );

        // SFX dimainkan saat bar VFX muncul
        PlayNonCollectibleVfxSFX();

        PlayParticleSystems(barTrail);

        yield return new WaitForSeconds(
            barItemTrailDuration
        );

        if (barTrail != null)
            Destroy(barTrail);

        GameObject collectTrail =
            Instantiate(
                trailCollectItemVfx,
                spawnPosition,
                trailCollectItemVfx.transform.rotation
            );

        collectTrail.transform.localScale *=
            trailCollectItemScale;

        Vector3 collectTrailStartPosition =
            collectTrail.transform.position;
        float collectTrailElapsed = 0f;

        while (
            collectTrail != null &&
            target != null
        )
        {
            Vector3 targetPosition =
                GetWorldTargetPosition(
                    target,
                    collectTrail.transform.position.z
                );

            collectTrailElapsed += Time.deltaTime;

            float flightProgress =
                Mathf.Clamp01(
                    collectTrailElapsed /
                    Mathf.Max(0.01f, trailCollectItemFlightDuration)
                );

            float curvedProgress =
                trailCollectItemFlightCurve.Evaluate(
                    flightProgress
                );

            Vector3 midpoint =
                collectTrailStartPosition +
                (targetPosition - collectTrailStartPosition) /
                2f;
            Vector3 controlPoint =
                midpoint +
                new Vector3(
                    trailCollectItemSideOffset,
                    trailCollectItemArcHeight,
                    0f
                );

            Vector3 firstSegment =
                Vector3.Lerp(
                    collectTrailStartPosition,
                    controlPoint,
                    curvedProgress
                );
            Vector3 secondSegment =
                Vector3.Lerp(
                    controlPoint,
                    targetPosition,
                    curvedProgress
                );

            collectTrail.transform.position =
                Vector3.Lerp(
                    firstSegment,
                    secondSegment,
                    curvedProgress
                );

            if (
                flightProgress >= 1f ||
                Vector3.Distance(
                    collectTrail.transform.position,
                    targetPosition
                ) <= trailCollectItemArrivalDistance
            )
                break;

            yield return null;
        }

        if (collectTrail != null)
        {
            collectTrail.transform.position =
                GetWorldTargetPosition(
                    target,
                    collectTrail.transform.position.z
                );

            yield return new WaitForSeconds(
                trailCollectItemEndDelay
            );

            Destroy(collectTrail);
        }

        CompletePendingProgress();
    }

    private void PlayParticleSystems(
        GameObject effect
    )
    {
        ParticleSystem[] particleSystems =
            effect.GetComponentsInChildren<ParticleSystem>(
                true
            );

        foreach (
            ParticleSystem particleSystem
            in particleSystems
        )
        {
            particleSystem.Play(true);
        }
    }

    private Vector3 GetWorldTargetPosition(
        Transform target,
        float sourceZ
    )
    {
        RectTransform targetRect =
            target as RectTransform;

        Camera worldCamera = Camera.main;

        if (
            targetRect == null ||
            worldCamera == null
        )
            return target.position;

        Canvas canvas =
            targetRect.GetComponentInParent<Canvas>();

        Camera canvasCamera =
            canvas != null &&
            canvas.renderMode !=
                RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                canvasCamera,
                targetRect.position
            );

        float cameraDistance =
            Mathf.Abs(
                worldCamera.transform.position.z -
                sourceZ
            );

        Vector3 worldPosition =
            worldCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    cameraDistance
                )
            );

        worldPosition.z = sourceZ;

        return worldPosition;
    }

    private void UpdateUI()
    {
        if (progressBar != null)
        {
            float targetValue =
                (float)processedEnemies /
                (float)totalEnemies;

            if (progressBar.fillRect != null)
            {
                progressBar.fillRect.gameObject.SetActive(
                    processedEnemies > 0
                );
            }

            if (progressAnimation != null)
                StopCoroutine(progressAnimation);

            if (
                targetValue <= 0f ||
                progressAnimationDuration <= 0f
            )
            {
                progressBar.value = targetValue;
            }
            else
            {
                progressAnimation =
                    StartCoroutine(
                        AnimateProgressBar(
                            targetValue
                        )
                    );
            }
        }

        if (progressText != null)
        {
            progressText.text =
                $"{processedEnemies}/{totalEnemies}";
        }

        // Check milestones
        for (
            int i = 0;
            i < milestones.Count;
            i++
        )
        {
            int m = milestones[i];

            if (
                !triggeredMilestones.Contains(m) &&
                processedEnemies >= m
            )
            {
                triggeredMilestones.Add(m);

                PlayMilestoneVfx(i);

                OnReachedWaveMilestone?.Invoke();
            }
        }

        if (processedEnemies >= totalEnemies)
        {
            OnReachedLevelComplete?.Invoke();
        }
    }

    private void PlayMilestoneVfx(int milestoneIndex)
    {
        if (
            milestoneVfxByWave == null ||
            milestoneIndex < 0 ||
            milestoneIndex >= milestoneVfxByWave.Length ||
            milestoneVfxByWave[milestoneIndex] == null
        )
            return;

        GameObject milestoneVfx = milestoneVfxByWave[milestoneIndex];
        milestoneVfx.SetActive(true);
        PlayParticleSystems(milestoneVfx);

        StartCoroutine(DisableMilestoneVfxAfterDelay(milestoneVfx));
    }

    private IEnumerator DisableMilestoneVfxAfterDelay(GameObject milestoneVfx)
    {
        yield return new WaitForSeconds(
            Mathf.Max(0.1f, milestoneVfxLifetime)
        );

        if (milestoneVfx != null)
            milestoneVfx.SetActive(false);
    }

    private IEnumerator AnimateProgressBar(
        float targetValue
    )
    {
        float startValue =
            progressBar.value;

        float elapsed = 0f;

        while (
            elapsed <
            progressAnimationDuration
        )
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    progressAnimationDuration
                );

            t =
                t * t *
                (3f - 2f * t);

            progressBar.value =
                Mathf.Lerp(
                    startValue,
                    targetValue,
                    t
                );

            yield return null;
        }

        progressBar.value = targetValue;
        progressAnimation = null;
    }
}