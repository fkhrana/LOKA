using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossLevelPowerUpTutorial : MonoBehaviour
{
    #region Static
    public static bool IsBossLevelTutorial { get; private set; }
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
    [Tooltip("GameObject overlay manual (Image full screen). Cukup di-on/off, " +
             "tanpa lubang, tanpa auto-build. Button power-up render di atasnya.")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private TutorialFingerTap fingerTap;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float fingerAppearDelay = 0.3f;

    [Header("Wait For Enemies Approach")]
    [SerializeField] private bool waitUntilEnemiesApproach = true;
    [SerializeField, Min(0.5f)] private float approachTravelDistance = 3f;
    [SerializeField, Min(1f)] private float maxWaitForEnemiesApproach = 15f;

    [Header("Power Up")]
    [SerializeField] private Button powerUpButton;
    [Tooltip("Default Shield untuk boss level, tapi bisa diganti via Inspector.")]
    [SerializeField] private PowerManager.PowerUpType powerUpType = PowerManager.PowerUpType.Shield;
    [SerializeField] private bool requiresAksaraPath = true;

    [Header("Feedback Visual Tombol")]
    [SerializeField] private Color buttonNormalColor = Color.white;
    [SerializeField] private Color buttonPressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField, Min(0f)] private float buttonPressedFeedbackDuration = 0.15f;

    [Header("Dodge UI")]
    [SerializeField] private GameObject dodgeUI;
    [SerializeField, Min(0f)] private float dodgeFadeInDuration = 0.25f;
    [Tooltip("Minimum hold time (detik). Kalau Shield lebih lama, Dodge UI ikut lebih lama.")]
    [SerializeField, Min(0f)] private float dodgeHoldDuration = 1.2f;
    [SerializeField, Min(0f)] private float dodgeFadeOutDuration = 0.4f;
    [SerializeField, Min(0f)] private float dodgePulseAmplitude = 0.15f;
    [SerializeField, Min(0f)] private float dodgePulseSpeed = 4f;

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

    [Header("Boss Spawn")]
    [SerializeField] private EnemyWaveSpawner bossSpawner;

    [Header("Debug")]
    [SerializeField] private bool resetTutorialOnPlay = false;
    [SerializeField] private bool autoUncheckReset = true;
    [SerializeField] private bool debugLog = true;
    #endregion

    #region Runtime State
    private Coroutine blinkRoutine;
    private Coroutine revealRoutine;
    private Coroutine dodgeRoutine;

    private Vector3 approachReferencePosition;
    private bool approachReferenceValid = false;

    private CanvasGroup dodgeCanvasGroup;
    private RectTransform dodgeRect;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        HandleDebugResetTutorial();

        if (ShouldSkipTutorial())
        {
            if (debugLog)
                Debug.Log("[BossLevelPowerUpTutorial] Tutorial sudah selesai — skip & spawn boss.");

            StartCoroutine(SpawnBossDelayed());
            gameObject.SetActive(false);
            return;
        }

        if (gestureDrawer == null)
            gestureDrawer = FindFirstObjectByType<GestureDrawer>();

        CacheDodgeUI();

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

    private void OnDestroy()
    {
        StopAllCoroutines();
        PowerManager.OnAnyPowerUpEnded -= HandlePowerUpEnded;
    }
    #endregion

    #region Tutorial Flow
    public void RestartTutorial()
    {
        StopAllCoroutines();

        IsBossLevelTutorial = true;
        TutorialManager.IsTrainingMode = true;

        ResetButtonVisual();
        SetOverlayActive(false);
        fingerTap?.Hide();
        SetActive(startPanel, false);
        hintManager?.HideAll();
        HideDodgeUI();

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        ClearAndRespawnEnemies();
        SetEnemiesSpeed(enemyNormalSpeed);

        tutorialSpawner?.ActivateAll();
        currentStep = TutorialStep.WaitingForPowerUpTap;
        SetPowerUpButtonInteractable(true);

        revealRoutine = StartCoroutine(RevealRoutine());

        if (debugLog)
            Debug.Log("[BossLevelPowerUpTutorial] 🔄 Restart — WaitingForPowerUpTap.");
    }

    private void BeginTutorialFlow()
    {
        IsBossLevelTutorial = true;
        TutorialManager.IsTrainingMode = true;

        SetActive(startPanel, false);

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        tutorialSpawner?.ActivateAll();
        SetEnemiesSpeed(enemyNormalSpeed);
        currentStep = TutorialStep.WaitingForPowerUpTap;
        SetPowerUpButtonInteractable(true);

        revealRoutine = StartCoroutine(RevealRoutine());

        if (debugLog)
            Debug.Log("[BossLevelPowerUpTutorial] ✅ Tutorial siap — WaitingForPowerUpTap.");
    }

    private void OnPowerUpTapped()
    {
        if (currentStep != TutorialStep.WaitingForPowerUpTap)
        {
            if (debugLog)
                Debug.Log($"[BossLevelPowerUpTutorial] Tap diabaikan. step={currentStep}");
            return;
        }

        if (debugLog)
            Debug.Log($"[BossLevelPowerUpTutorial] Button DIKLIK. step={currentStep}");

        currentStep = TutorialStep.WaitingOutcome;

        SetPowerUpButtonInteractable(false);
        PlayButtonPressedFeedback();

        fingerTap?.Hide();

        if (buttonPressedFeedbackDuration > 0f)
            StartCoroutine(FinishPowerUpTapRoutine());
        else
            FinishPowerUpTap();
    }

    private IEnumerator FinishPowerUpTapRoutine()
    {
        yield return new WaitForSecondsRealtime(buttonPressedFeedbackDuration);
        FinishPowerUpTap();
    }

    private void FinishPowerUpTap()
    {
        SetOverlayActive(false);
        DisableRaycastOnTutorialUI();

        TriggerPowerUpEffect();
        PlayDodgeUI();
        ShowEnemyHint();

        SetGestureEnabled(true);

        if (debugLog)
            Debug.Log($"[BossLevelPowerUpTutorial] ✅ Power-up tapped ({powerUpType}). " +
                      $"gesture.enabled={gestureDrawer?.enabled}");
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
        if (!IsBossLevelTutorial) return;
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

        SetOverlayActive(false);
        fingerTap?.Hide();
        HideDodgeUI();

        ResetButtonVisual();

        IsBossLevelTutorial = false;
        TutorialManager.IsTrainingMode = false;

        // Konsisten dengan Script 1: reset power-up supaya penuh saat boss fight
        PowerManager.ResetAllPowerUpsToFullGlobal();

        if (debugLog)
            Debug.Log("[BossLevelPowerUpTutorial] ✅ Tutorial sukses — spawn boss.");

        bossSpawner?.SpawnBoss();
    }

    private void HandleTutorialFail()
    {
        currentStep = TutorialStep.Fail;
        StopBlinking();
        hintManager?.HideAll();

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        SetOverlayActive(false);
        fingerTap?.Hide();
        HideDodgeUI();

        ResetButtonVisual();

        if (debugLog)
            Debug.LogWarning("[BossLevelPowerUpTutorial] ❌ Tutorial gagal.");

        SetActive(startPanel, true);
    }
    #endregion

    #region Button Handlers
    private void HookButtons()
    {
        if (powerUpButton != null)
        {
            powerUpButton.onClick.AddListener(OnPowerUpTapped);
            if (debugLog) Debug.Log("[BossLevelPowerUpTutorial] ✅ Listener di-attach.");
        }
        else
        {
            Debug.LogError("[BossLevelPowerUpTutorial] ❌ powerUpButton NULL!");
        }

        if (yaButton != null) yaButton.onClick.AddListener(OnYaClicked);
        if (ulangButton != null) ulangButton.onClick.AddListener(OnUlangClicked);
    }

    private void OnYaClicked()
    {
        IsBossLevelTutorial = false;
        TutorialManager.IsTrainingMode = false;

        SetGestureEnabled(true);
        SetPowerUpButtonInteractable(true);

        SetOverlayActive(false);
        fingerTap?.Hide();
        HideDodgeUI();
        SetActive(startPanel, false);

        PowerManager.ResetAllPowerUpsToFullGlobal();

        bossSpawner?.SpawnBoss();
    }

    private void OnUlangClicked()
    {
        SetOverlayActive(false);
        fingerTap?.Hide();
        HideDodgeUI();
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
            Debug.Log($"[BossLevelPowerUpTutorial] GestureDrawer.enabled = {enabled}");
    }

    private void SetPowerUpButtonInteractable(bool interactable)
    {
        if (powerUpButton == null) return;
        powerUpButton.interactable = interactable;

        if (debugLog)
            Debug.Log($"[BossLevelPowerUpTutorial] PowerUpButton.interactable = {interactable}");
    }

    private void SetOverlayActive(bool active)
    {
        if (overlay != null)
            overlay.SetActive(active);
    }
    #endregion

    #region Button Visual Feedback
    private void PlayButtonPressedFeedback()
    {
        if (powerUpButton == null) return;

        ColorBlock cb = powerUpButton.colors;
        cb.normalColor = buttonPressedColor;
        cb.selectedColor = buttonPressedColor;
        cb.highlightedColor = buttonPressedColor;
        cb.disabledColor = buttonPressedColor;
        powerUpButton.colors = cb;
    }

    private void ResetButtonVisual()
    {
        if (powerUpButton == null) return;

        ColorBlock cb = powerUpButton.colors;
        cb.normalColor = buttonNormalColor;
        cb.selectedColor = buttonNormalColor;
        cb.highlightedColor = buttonNormalColor;
        cb.disabledColor = buttonNormalColor;
        powerUpButton.colors = cb;
    }
    #endregion

    #region Dodge UI
    private void CacheDodgeUI()
    {
        if (dodgeUI == null) return;

        dodgeRect = dodgeUI.GetComponent<RectTransform>();

        dodgeCanvasGroup = dodgeUI.GetComponent<CanvasGroup>();
        if (dodgeCanvasGroup == null)
            dodgeCanvasGroup = dodgeUI.AddComponent<CanvasGroup>();

        dodgeCanvasGroup.alpha = 0f;
        dodgeCanvasGroup.blocksRaycasts = false;
        dodgeCanvasGroup.interactable = false;

        dodgeUI.SetActive(false);
    }

    private void PlayDodgeUI()
    {
        if (dodgeUI == null) return;

        if (dodgeRoutine != null)
            StopCoroutine(dodgeRoutine);

        dodgeRoutine = StartCoroutine(DodgeUIRoutine());
    }

    private void HideDodgeUI()
    {
        if (dodgeRoutine != null)
        {
            StopCoroutine(dodgeRoutine);
            dodgeRoutine = null;
        }

        if (dodgeUI == null) return;

        if (dodgeCanvasGroup != null) dodgeCanvasGroup.alpha = 0f;
        if (dodgeRect != null) dodgeRect.localScale = Vector3.one;

        dodgeUI.SetActive(false);
    }

    /// <summary>
    /// Dodge UI sekarang sinkron dengan durasi Shield:
    /// Fase 1 (Fade In)  → dodgeFadeInDuration
    /// Fase 2 (Pulse)    → selama PowerManager.IsShieldActive == true
    ///                     dengan minimum dodgeHoldDuration
    /// Fase 3 (Fade Out) → dodgeFadeOutDuration
    /// </summary>
    private IEnumerator DodgeUIRoutine()
    {
        dodgeUI.SetActive(true);

        if (dodgeRect == null)
            dodgeRect = dodgeUI.GetComponent<RectTransform>();

        dodgeRect.localScale = Vector3.one;

        // ============================================================
        // FASE 1: FADE IN
        // ============================================================
        float t = 0f;
        while (t < dodgeFadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(0.01f, dodgeFadeInDuration));
            if (dodgeCanvasGroup != null) dodgeCanvasGroup.alpha = p;
            float s = Mathf.Lerp(0.6f, 1f, p);
            dodgeRect.localScale = Vector3.one * s;
            yield return null;
        }

        if (dodgeCanvasGroup != null) dodgeCanvasGroup.alpha = 1f;
        dodgeRect.localScale = Vector3.one;

        // ============================================================
        // FASE 2: PULSE + HOLD — selama Shield masih aktif
        // Minimal hold = dodgeHoldDuration (safety kalau Shield mati cepat)
        // ============================================================
        float pulseTime = 0f;
        float elapsed = 0f;

        while (true)
        {
            bool shieldActive = PowerManager.IsShieldActive;
            bool minHoldReached = elapsed >= dodgeHoldDuration;

            // Keluar kalau Shield sudah mati DAN minimal hold tercapai
            if (!shieldActive && minHoldReached)
                break;

            elapsed += Time.unscaledDeltaTime;
            pulseTime += Time.unscaledDeltaTime;

            float pulse = 1f + Mathf.Sin(pulseTime * dodgePulseSpeed) * dodgePulseAmplitude;
            dodgeRect.localScale = Vector3.one * pulse;
            yield return null;
        }

        // ============================================================
        // FASE 3: FADE OUT
        // ============================================================
        t = 0f;
        Vector3 startScale = dodgeRect.localScale;
        while (t < dodgeFadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(0.01f, dodgeFadeOutDuration));
            if (dodgeCanvasGroup != null) dodgeCanvasGroup.alpha = 1f - p;
            dodgeRect.localScale = Vector3.Lerp(startScale, Vector3.one * 0.4f, p);
            yield return null;
        }

        if (dodgeCanvasGroup != null) dodgeCanvasGroup.alpha = 0f;
        dodgeRect.localScale = Vector3.one;
        dodgeUI.SetActive(false);
        dodgeRoutine = null;
    }
    #endregion

    #region Enemy Management
    private void SpawnTutorialEnemies()
    {
        if (tutorialSpawner == null)
        {
            Debug.LogWarning("[BossLevelPowerUpTutorial] tutorialSpawner belum di-assign.");
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

        CacheApproachReference(center);
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

    private void CacheApproachReference(Vector3 spawnCenter)
    {
        approachReferencePosition = spawnCenter;
        approachReferenceValid = true;

        if (debugLog)
            Debug.Log($"[BossLevelPowerUpTutorial] Approach reference = {spawnCenter}");
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

    private IEnumerator RevealRoutine()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        ResetButtonVisual();

        if (waitUntilEnemiesApproach)
            yield return StartCoroutine(WaitForEnemiesApproachRoutine());

        SetOverlayActive(true);

        if (fingerAppearDelay > 0f)
            yield return new WaitForSecondsRealtime(fingerAppearDelay);

        DisableRaycastOnTutorialUI();
        fingerTap?.Show();

        revealRoutine = null;
    }

    private IEnumerator WaitForEnemiesApproachRoutine()
    {
        if (tutorialSpawner == null) yield break;
        if (!approachReferenceValid) yield break;

        float elapsed = 0f;

        while (elapsed < maxWaitForEnemiesApproach)
        {
            if (HasAnyEnemyTraveledFarEnough())
            {
                if (debugLog)
                    Debug.Log($"[BossLevelPowerUpTutorial] ✅ Musuh sudah jalan " +
                              $"{approachTravelDistance} unit dari spawn.");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (debugLog)
            Debug.LogWarning($"[BossLevelPowerUpTutorial] ⏱ Timeout nunggu musuh " +
                             $"({maxWaitForEnemiesApproach}s). Lanjut paksa.");
    }

    private bool HasAnyEnemyTraveledFarEnough()
    {
        var enemies = tutorialSpawner.SpawnedEnemies;
        if (enemies == null || enemies.Count == 0) return false;

        float threshold = approachTravelDistance * approachTravelDistance;

        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e == null) continue;

            float dx = approachReferencePosition.x - e.transform.position.x;
            if (dx >= approachTravelDistance) return true;

            float distSq = (e.transform.position - approachReferencePosition).sqrMagnitude;
            if (distSq >= threshold) return true;
        }

        return false;
    }
    #endregion

    #region Boss Spawn
    private IEnumerator SpawnBossDelayed()
    {
        yield return null;
        bossSpawner?.SpawnBoss();
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
    #endregion

    #region UI Helpers
    private void HideAllInitially()
    {
        SetOverlayActive(false);
        fingerTap?.Hide();
        SetActive(startPanel, false);
        hintManager?.HideAll();
        HideDodgeUI();
    }

    private void DisableRaycastOnTutorialUI()
    {
        DisableRaycast(overlay);
        DisableRaycast(fingerTap != null ? fingerTap.gameObject : null);
        DisableRaycast(hintManager != null ? hintManager.gameObject : null);
        DisableRaycast(dodgeUI);
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

        Debug.Log("[BossLevelPowerUpTutorial] 🔄 Tutorial & GameState di-reset.");

        if (autoUncheckReset)
            resetTutorialOnPlay = false;
#else
        if (resetTutorialOnPlay)
            Debug.LogWarning("[BossLevelPowerUpTutorial] 'Reset Tutorial On Play' hanya berfungsi di Editor.");
#endif
    }
    #endregion
}