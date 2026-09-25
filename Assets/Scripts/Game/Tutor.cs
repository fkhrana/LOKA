using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpTutorialManager : MonoBehaviour
{
    #region Static
    public static bool IsPowerUpTutorial { get; private set; }
    #endregion

    #region Tutorial State
    private enum TutorialStep
    {
        Idle,
        WaitingForPowerUpTap,
        WaitingOutcome,
        Success,
        Fail
    }

    private TutorialStep currentStep = TutorialStep.Idle;
    #endregion

    #region Inspector Fields
    [Header("Reusable Components")]
    [SerializeField] private TutorialOverlay overlay;
    [SerializeField] private TutorialFingerTap fingerTap;

    [Header("Cover")]
    [SerializeField] private float coverDuration = 1.2f;

    [Header("Power Up")]
    [SerializeField] private Button powerUpButton;
    [SerializeField] private PowerManager.PowerUpType powerUpType = PowerManager.PowerUpType.Freeze;
    [SerializeField] private bool requiresAksaraPath = true;

    [Header("References")]
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private TutorialHintManager hintManager;
    [SerializeField] private TutorialEnemySpawner tutorialSpawner;
    [SerializeField] private AksaraData tutorialAksara;
    [SerializeField] private EnemyData tutorialEnemyData;

    [Header("Enemy Spawner")]
    [SerializeField, Min(1)] private int tutorialEnemyCount = 4;
    [SerializeField, Min(0.1f)] private float enemyNormalSpeed = 2f;
    [SerializeField] private bool spawnNearCameraTarget = true;
    [SerializeField] private Vector3 manualSpawnCenter = Vector3.zero;
    [SerializeField, Min(0.1f)] private float spawnRadius = 1.5f;

    [Header("Blink Effect")]
    [SerializeField, Min(0.05f)] private float blinkInterval = 0.25f;

    [Header("Outcome Panel")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button yaButton;
    [SerializeField] private Button ulangButton;

    [Header("Intro Wait")]
    [SerializeField] private bool waitForCameraIntro = true;
    [SerializeField, Min(1f)] private float maxWaitForIntro = 30f;
    [SerializeField, Min(0f)] private float fallbackIntroDelay = 10f;

    [Header("Boss Level Mode")]
    [Tooltip("Kalau true, tutorial di boss level. Setelah sukses = SpawnBoss, bukan resume wave.")]
    [SerializeField] private bool isBossLevelTutorial = false;
    [Tooltip("Wajib diisi kalau isBossLevelTutorial = true.")]
    [SerializeField] private EnemyWaveSpawner bossSpawner;

    [Header("Debug")]
    [SerializeField] private bool resetTutorialOnPlay = false;
    [SerializeField] private bool autoUncheckReset = true;
    [SerializeField] private bool debugLog = true;
    #endregion

    #region Runtime State
    private Coroutine blinkRoutine;
    private Coroutine revealRoutine;

    private int lastScreenW = -1;
    private int lastScreenH = -1;
    private ScreenOrientation lastOrientation;

    private EnemyWaveSpawner waveSpawner;
    private bool waveSpawnerPaused = false;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        HandleDebugResetTutorial();

        if (ShouldSkipTutorial())
        {
            if (isBossLevelTutorial && bossSpawner != null)
            {
                if (debugLog)
                    Debug.Log("[PowerUpTutorialManager] Sudah tutorial + boss level → spawn boss.");

                StartCoroutine(SpawnBossDelayed());
            }
            else
            {
                if (debugLog)
                    Debug.Log("[PowerUpTutorialManager] Tutorial sudah selesai — skip.");
            }

            gameObject.SetActive(false);
            return;
        }

        if (gestureDrawer == null)
            gestureDrawer = FindFirstObjectByType<GestureDrawer>();

        if (overlay == null)
            overlay = GetComponentInChildren<TutorialOverlay>(true);

        if (fingerTap == null)
            fingerTap = GetComponentInChildren<TutorialFingerTap>(true);

        overlay?.Build();

        HookButtons();

        DisableRaycastOnTutorialUI();
    }

    private void OnEnable()
    {
        PowerManager.OnAnyPowerUpEnded += HandlePowerUpEnded;
    }

    private void OnDisable()
    {
        PowerManager.OnAnyPowerUpEnded -= HandlePowerUpEnded;
    }

    private void Start()
    {
        PauseWaveSpawner();

        SpawnTutorialEnemies();
        SetEnemiesSpeed(enemyNormalSpeed);
        HideAllInitially();

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        if (waitForCameraIntro)
            StartCoroutine(WaitForIntroThenStart());
        else
            BeginTutorialFlow();
    }

    private void Update()
    {
        if (Screen.width != lastScreenW ||
            Screen.height != lastScreenH ||
            Screen.orientation != lastOrientation)
        {
            lastScreenW = Screen.width;
            lastScreenH = Screen.height;
            lastOrientation = Screen.orientation;

            overlay?.RefreshHoleIfActive();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        PowerManager.OnAnyPowerUpEnded -= HandlePowerUpEnded;
    }
    #endregion

    #region Boss Spawn (skip path)
    private IEnumerator SpawnBossDelayed()
    {
        yield return null;

        if (bossSpawner != null)
            bossSpawner.SpawnBoss();
        else
            Debug.LogWarning("[PowerUpTutorialManager] bossSpawner belum di-set.");
    }
    #endregion

    #region Wave Spawner Control
    private void PauseWaveSpawner()
    {
        if (waveSpawnerPaused) return;
        if (isBossLevelTutorial) return;

        waveSpawner = FindFirstObjectByType<EnemyWaveSpawner>();
        if (waveSpawner != null)
        {
            waveSpawner.enabled = false;
            waveSpawnerPaused = true;

            if (debugLog)
                Debug.Log("[PowerUpTutorialManager] Wave spawner DI-PAUSE.");
        }
    }

    private void ResumeWaveSpawner(bool startSequenceIfIdle = false)
    {
        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<EnemyWaveSpawner>();

        if (waveSpawner != null)
        {
            waveSpawner.enabled = true;

            if (startSequenceIfIdle)
                waveSpawner.StartWaveSequence();

            if (debugLog)
                Debug.Log("[PowerUpTutorialManager] Wave spawner DI-RESUME.");
        }

        waveSpawnerPaused = false;
    }
    #endregion

    #region Tutorial Flow
    public void RestartTutorial()
    {
        StopAllCoroutines();

        IsPowerUpTutorial = true;
        TutorialManager.IsTrainingMode = true;

        overlay?.HideAll();
        fingerTap?.Hide();
        SetActive(startPanel, false);
        hintManager?.HideAll();

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        ClearAndRespawnEnemies();
        SetEnemiesSpeed(enemyNormalSpeed);

        tutorialSpawner?.ActivateAll();
        currentStep = TutorialStep.WaitingForPowerUpTap;
        SetPowerUpButtonInteractable(true);

        revealRoutine = StartCoroutine(RevealHoleRoutine());

        if (debugLog)
            Debug.Log("[PowerUpTutorialManager] 🔄 Restart — WaitingForPowerUpTap.");
    }

    private void BeginTutorialFlow()
    {
        IsPowerUpTutorial = true;
        TutorialManager.IsTrainingMode = true;

        SetActive(startPanel, false);

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        revealRoutine = StartCoroutine(RevealHoleRoutine());

        tutorialSpawner?.ActivateAll();
        SetEnemiesSpeed(enemyNormalSpeed);
        currentStep = TutorialStep.WaitingForPowerUpTap;
        SetPowerUpButtonInteractable(true);

        if (debugLog)
            Debug.Log("[PowerUpTutorialManager] ✅ Tutorial siap — WaitingForPowerUpTap.");
    }

    private void OnPowerUpTapped()
    {
        if (debugLog)
            Debug.Log($"[PowerUpTutorialManager] Button DIKLIK. step={currentStep}");

        if (currentStep == TutorialStep.Success || currentStep == TutorialStep.Fail)
            return;

        fingerTap?.Hide();
        overlay?.HideAll();

        DisableRaycastOnTutorialUI();

        TriggerPowerUpEffect();
        ShowEnemyHint();

        SetGestureEnabled(true);

        currentStep = TutorialStep.WaitingOutcome;

        if (debugLog)
            Debug.Log($"[PowerUpTutorialManager] ✅ Power-up tapped. gesture.enabled={gestureDrawer?.enabled}");
    }

    private void ShowEnemyHint()
    {
        if (hintManager != null && tutorialSpawner != null)
            hintManager.ShowCircleHighlight(tutorialSpawner.SpawnedEnemies);

        if (requiresAksaraPath && hintManager != null && tutorialAksara != null)
            hintManager.ShowPath(tutorialAksara);

        StartBlinking();
    }
    #endregion

    #region Power-Up Event Handler
    private void HandlePowerUpEnded()
    {
        if (!IsPowerUpTutorial) return;
        if (currentStep != TutorialStep.WaitingOutcome) return;

        hintManager?.HideCircleHighlight();
    }
    #endregion

    #region Outcome
    private void OnAllTutorialEnemiesKilled()
    {
        if (currentStep == TutorialStep.Success || currentStep == TutorialStep.Fail) return;

        if (currentStep != TutorialStep.WaitingOutcome)
        {
            HandleTutorialFail();
            return;
        }

        HandleTutorialSuccess();
    }

    private void OnAllTutorialEnemiesCrashed()
    {
        if (currentStep == TutorialStep.Success || currentStep == TutorialStep.Fail) return;
        HandleTutorialFail();
    }

    private void HandleTutorialSuccess()
    {
        currentStep = TutorialStep.Success;
        StopBlinking();
        hintManager?.HideAll();

        SetPowerUpButtonInteractable(true);

        overlay?.HideAll();
        fingerTap?.Hide();

        IsPowerUpTutorial = false;
        TutorialManager.IsTrainingMode = false;
        PowerManager.ResetAllPowerUpsToFullGlobal();

        if (debugLog)
            Debug.Log("[PowerUpTutorialManager] ✅ Tutorial sukses.");

        StartGameNormally();
    }

    private void HandleTutorialFail()
    {
        currentStep = TutorialStep.Fail;
        StopBlinking();
        hintManager?.HideAll();

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        overlay?.HideAll();
        fingerTap?.Hide();

        if (debugLog)
            Debug.LogWarning("[PowerUpTutorialManager] ❌ Tutorial gagal.");

        SetActive(startPanel, true);
    }
    #endregion

    #region Button Handlers
    private void HookButtons()
    {
        if (powerUpButton != null)
        {
            powerUpButton.onClick.AddListener(OnPowerUpTapped);
            if (debugLog) Debug.Log("[PowerUpTutorialManager] ✅ Listener di-attach.");
        }
        else
        {
            Debug.LogError("[PowerUpTutorialManager] ❌ powerUpButton NULL!");
        }

        if (yaButton != null) yaButton.onClick.AddListener(OnYaClicked);
        if (ulangButton != null) ulangButton.onClick.AddListener(OnUlangClicked);
    }

    private void OnYaClicked()
    {
        IsPowerUpTutorial = false;
        TutorialManager.IsTrainingMode = false;

        SetGestureEnabled(true);
        SetPowerUpButtonInteractable(true);

        overlay?.HideAll();
        fingerTap?.Hide();
        SetActive(startPanel, false);

        StartGameNormally();
    }

    private void OnUlangClicked()
    {
        overlay?.HideAll();
        fingerTap?.Hide();
        SetActive(startPanel, false);

        PowerManager.ResetAllPowerUpsToFullGlobal();
        RestartTutorial();
    }
    #endregion

    #region Control Helpers
    private void SetGestureEnabled(bool enabled)
    {
        if (gestureDrawer == null) return;

        if (!enabled)
            gestureDrawer.ResetGestureInput();

        gestureDrawer.enabled = enabled;

        if (debugLog)
            Debug.Log($"[PowerUpTutorialManager] GestureDrawer.enabled = {enabled}");
    }

    private void SetPowerUpButtonInteractable(bool interactable)
    {
        if (powerUpButton == null) return;
        powerUpButton.interactable = interactable;

        if (debugLog)
            Debug.Log($"[PowerUpTutorialManager] PowerUpButton.interactable = {interactable}");
    }
    #endregion

    #region Enemy Management
    private void SpawnTutorialEnemies()
    {
        if (tutorialSpawner == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] tutorialSpawner belum di-assign.");
            return;
        }

        Vector3 center = GetSpawnCenter();
        tutorialSpawner.Spawn(
            count: tutorialEnemyCount,
            center: center,
            radius: spawnRadius,
            enemyData: tutorialEnemyData,
            aksara: tutorialAksara,
            onAllKilled: OnAllTutorialEnemiesKilled,
            onAllCrashed: OnAllTutorialEnemiesCrashed);
    }

    private void ClearAndRespawnEnemies()
    {
        tutorialSpawner?.Clear();
        SpawnTutorialEnemies();
    }

    private Vector3 GetSpawnCenter()
    {
        if (!spawnNearCameraTarget) return manualSpawnCenter;

        if (CameraIntroManager.Instance != null &&
            CameraIntroManager.Instance.targetKanan != null)
            return CameraIntroManager.Instance.targetKanan.position;

        return manualSpawnCenter;
    }

    private void SetEnemiesSpeed(float speed)
    {
        tutorialSpawner?.SetSpeed(speed);
    }
    #endregion

    #region Blink Effect
    private void StartBlinking()
    {
        StopCoroutineSafe(ref blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void StopBlinking()
    {
        StopCoroutineSafe(ref blinkRoutine);

        if (tutorialSpawner == null) return;

        foreach (var sr in tutorialSpawner.GetAllRenderers())
            if (sr != null) sr.enabled = true;
    }

    private IEnumerator BlinkRoutine()
    {
        var renderers = tutorialSpawner != null
            ? tutorialSpawner.GetAllRenderers()
            : new List<SpriteRenderer>();

        while (true)
        {
            foreach (var sr in renderers)
                if (sr != null) sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(blinkInterval);
        }
    }
    #endregion

    #region Intro / Reveal
    private IEnumerator WaitForIntroThenStart()
    {
        if (CameraIntroManager.Instance != null)
        {
            float elapsed = 0f;
            while (!CameraIntroManager.GameStarted && elapsed < maxWaitForIntro)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fallbackIntroDelay);
        }

        BeginTutorialFlow();
    }

    private IEnumerator RevealHoleRoutine()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        overlay?.ShowCover();

        if (coverDuration > 0f)
            yield return new WaitForSecondsRealtime(coverDuration);

        if (overlay != null)
            yield return StartCoroutine(overlay.FadeOutCoverRoutine());

        if (overlay != null && powerUpButton != null)
        {
            RectTransform btnRect = powerUpButton.GetComponent<RectTransform>();
            overlay.ShowOverlay(btnRect);
        }

        // Pastikan raycast di UI tutorial tetap mati saat overlay tampil
        DisableRaycastOnTutorialUI();

        fingerTap?.Show();

        revealRoutine = null;
    }
    #endregion

    #region External References
    private void TriggerPowerUpEffect()
    {
        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);
        foreach (var pm in managers)
        {
            pm.UsePowerUp(powerUpType);
            break;
        }
    }

    private void StartGameNormally()
    {
        GameProgressManager.MarkTutorialCompleted();

        if (isBossLevelTutorial && bossSpawner != null)
        {
            if (debugLog)
                Debug.Log("[PowerUpTutorialManager] Tutorial selesai → spawn boss.");

            bossSpawner.SpawnBoss();
            return;
        }

        ResumeWaveSpawner(startSequenceIfIdle: true);

        if (debugLog)
            Debug.Log("[PowerUpTutorialManager] Gameplay normal dimulai.");
    }
    #endregion

    #region UI Helpers
    private void HideAllInitially()
    {
        overlay?.HideAll();
        fingerTap?.Hide();
        SetActive(startPanel, false);
        hintManager?.HideAll();
    }

    /// <summary>
    /// Matikan raycastTarget di seluruh UI tutorial (overlay, fingerTap, hintManager)
    /// supaya tap tembus ke Button power-up di bawahnya.
    /// Pakai Graphic (bukan cuma Image) supaya Text / RawImage juga kena.
    /// </summary>
    private void DisableRaycastOnTutorialUI()
    {
        DisableRaycast(overlay != null ? overlay.gameObject : null);
        DisableRaycast(fingerTap != null ? fingerTap.gameObject : null);
        DisableRaycast(hintManager != null ? hintManager.gameObject : null);
    }

    private static void DisableRaycast(GameObject go)
    {
        if (go == null) return;

        foreach (var g in go.GetComponentsInChildren<Graphic>(true))
            if (g != null) g.raycastTarget = false;
    }
    #endregion

    #region Helpers
    private bool ShouldSkipTutorial()
    {
        return GameProgressManager.IsTutorialCompleted();
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private void StopCoroutineSafe(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
    #endregion

    #region Debug Helpers
    private void HandleDebugResetTutorial()
    {
#if UNITY_EDITOR
        if (!resetTutorialOnPlay) return;

        GameProgressManager.ResetTutorial();
        GameProgressManager.ClearGameState();

        Debug.Log("[PowerUpTutorialManager] 🔄 Tutorial & GameState di-reset.");

        if (autoUncheckReset)
            resetTutorialOnPlay = false;
#else
        if (resetTutorialOnPlay)
            Debug.LogWarning("[PowerUpTutorialManager] 'Reset Tutorial On Play' hanya berfungsi di Editor.");
#endif
    }
    #endregion
}