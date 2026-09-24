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
        WaitingForScreenTap,
        WaitingForPowerUpTap,
        WaitingOutcome,
        Success,
        Fail
    }

    private TutorialStep currentStep = TutorialStep.Idle;
    #endregion

    #region Inspector Fields
    [Header("Auto-Generate Overlay")]
    [SerializeField] private bool autoGenerateOverlay = true;
    [SerializeField] private float overlayPadding = 60f;
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.86f);
    [SerializeField] private float coverDuration = 1.2f;
    [SerializeField] private float coverFadeOutDuration = 0.4f;

    [Header("Finger Tap")]
    [SerializeField] private GameObject fingerTapIcon;
    [SerializeField] private float fingerOffsetY = 80f;
    [SerializeField] private float fingerTapAmplitude = 25f;
    [SerializeField] private float fingerTapSpeed = 4f;

    [Header("Power Up")]
    [SerializeField] private Button powerUpButton;
    [SerializeField] private PowerManager.PowerUpType powerUpType = PowerManager.PowerUpType.Freeze;
    [Tooltip("Centang kalau power-up ini butuh pemain menggambar aksara.")]
    [SerializeField] private bool requiresAksaraPath = true;

    [Header("Gesture Drawer")]
    [SerializeField] private GestureDrawer gestureDrawer;

    [Header("Hint Manager")]
    [SerializeField] private TutorialHintManager hintManager;

    [Header("Enemy Spawner")]
    [SerializeField] private TutorialEnemySpawner tutorialSpawner;
    [SerializeField, Min(1)] private int tutorialEnemyCount = 2;
    [SerializeField, Min(0.1f)] private float enemySlowSpeed = 0.5f;
    [SerializeField, Min(0.1f)] private float enemyNormalSpeed = 2f;
    [SerializeField] private bool spawnNearCameraTarget = true;
    [SerializeField] private Vector3 manualSpawnCenter = Vector3.zero;
    [SerializeField, Min(0.1f)] private float spawnRadius = 1.5f;

    [Header("Enemy Data")]
    [SerializeField] private AksaraData tutorialAksara;
    [SerializeField] private EnemyData tutorialEnemyData;

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

    [Header("Debug")]
    [SerializeField] private bool resetTutorialOnPlay = false;
    [SerializeField] private bool autoUncheckReset = true;
    [SerializeField] private bool debugLog = true;
    #endregion

    #region Runtime State
    private GameObject overlayPanel;
    private GameObject overlayCover;
    private Coroutine revealRoutine;
    private Coroutine blinkRoutine;
    private Coroutine fingerTapRoutine;
    private Vector2 fingerBasePos;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        HandleDebugResetTutorial();

        if (ShouldSkipTutorial())
        {
            Debug.Log("[PowerUpTutorialManager] Tutorial sudah selesai — skip.");
            gameObject.SetActive(false);
            return;
        }

        if (gestureDrawer == null)
            gestureDrawer = FindFirstObjectByType<GestureDrawer>();

        HookButtons();
        if (autoGenerateOverlay) GenerateOverlay();
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
        SetEnemiesSpeed(enemySlowSpeed);
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
        if (currentStep != TutorialStep.WaitingForScreenTap) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (overlayCover != null && overlayCover.activeSelf) return;

        OnScreenTapped();
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

        IsPowerUpTutorial = true;
        TutorialManager.IsTrainingMode = true;
        currentStep = TutorialStep.WaitingForScreenTap;

        SetActive(overlayPanel, false);
        SetActive(overlayCover, true);
        SetActive(fingerTapIcon, false);
        SetActive(startPanel, false);
        hintManager?.HideAll();

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        ClearAndRespawnEnemies();
        SetEnemiesSpeed(enemySlowSpeed);

        SetupFingerTapPosition();
        revealRoutine = StartCoroutine(RevealHoleRoutine());
    }

    private void BeginTutorialFlow()
    {
        IsPowerUpTutorial = true;
        TutorialManager.IsTrainingMode = true;
        currentStep = TutorialStep.WaitingForScreenTap;

        SetActive(overlayPanel, false);
        SetActive(overlayCover, true);
        SetActive(fingerTapIcon, false);

        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        SetupFingerTapPosition();
        revealRoutine = StartCoroutine(RevealHoleRoutine());
    }

    private void OnScreenTapped()
    {
        tutorialSpawner?.ActivateAll();
        SetEnemiesSpeed(enemyNormalSpeed);
        currentStep = TutorialStep.WaitingForPowerUpTap;

        SetPowerUpButtonInteractable(true);

        if (debugLog)
            Debug.Log("[PowerUpTutorialManager] Screen tapped → WaitingForPowerUpTap " +
                      "(overlay ON, button ON, gesture OFF)");
    }

    private void OnPowerUpTapped()
    {
        if (currentStep != TutorialStep.WaitingForPowerUpTap) return;

        StopFingerTap();
        SetActive(fingerTapIcon, false);

        SetActive(overlayPanel, false);
        SetActive(overlayCover, false);

        TriggerPowerUpEffect();
        ShowEnemyHint();

        SetGestureEnabled(true);

        currentStep = TutorialStep.WaitingOutcome;

        if (debugLog)
            Debug.Log("[PowerUpTutorialManager] Power-up tapped → WaitingOutcome " +
                      "(overlay OFF, gesture ON)");
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

        Debug.Log("[PowerUpTutorialManager] Power-up ended → hide circle highlight.");
        hintManager?.HideCircleHighlight();
    }
    #endregion

    #region Outcome
    private void OnAllTutorialEnemiesKilled()
    {
        if (debugLog)
            Debug.Log($"[PowerUpTutorialManager] OnAllTutorialEnemiesKilled (step={currentStep})");

        if (currentStep == TutorialStep.Success || currentStep == TutorialStep.Fail)
            return;

        if (currentStep != TutorialStep.WaitingOutcome)
        {
            Debug.LogWarning(
                $"[PowerUpTutorialManager] Callback kill ter-trigger di step {currentStep} " +
                "→ musuh pasti nabrak player. Paksa GAGAL."
            );
            HandleTutorialFail();
            return;
        }

        HandleTutorialSuccess();
    }

    private void OnAllTutorialEnemiesCrashed()
    {
        if (debugLog)
            Debug.Log($"[PowerUpTutorialManager] OnAllTutorialEnemiesCrashed (step={currentStep})");

        if (currentStep == TutorialStep.Success || currentStep == TutorialStep.Fail)
            return;

        HandleTutorialFail();
    }

    private void HandleTutorialSuccess()
    {
        currentStep = TutorialStep.Success;
        StopBlinking();
        hintManager?.HideAll();

        // Gesture tetap ON — gameplay butuh

        SetPowerUpButtonInteractable(true);

        SetActive(overlayPanel, false);
        SetActive(overlayCover, false);
        SetActive(fingerTapIcon, false);

        IsPowerUpTutorial = false;
        TutorialManager.IsTrainingMode = false;
        PowerManager.ResetAllPowerUpsToFullGlobal();

        Debug.Log("[PowerUpTutorialManager] ✅ Tutorial sukses → langsung gameplay (gesture tetap ON).");
        StartGameNormally();
    }

    private void HandleTutorialFail()
    {
        currentStep = TutorialStep.Fail;
        StopBlinking();
        hintManager?.HideAll();

        // Fail → gesture OFF (belum masuk gameplay)
        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);

        SetActive(overlayPanel, false);
        SetActive(overlayCover, false);
        SetActive(fingerTapIcon, false);

        Debug.LogWarning("[PowerUpTutorialManager] ❌ Tutorial gagal → panel MULAI MAIN? muncul.");
        SetActive(startPanel, true);
    }
    #endregion

    #region Button Handlers
    private void HookButtons()
    {
        if (powerUpButton != null) powerUpButton.onClick.AddListener(OnPowerUpTapped);
        if (yaButton != null) yaButton.onClick.AddListener(OnYaClicked);
        if (ulangButton != null) ulangButton.onClick.AddListener(OnUlangClicked);
    }

    /// <summary>
    /// Player klik YA di panel MULAI MAIN? → masuk gameplay.
    /// Gesture HARUS ON di sini biar player bisa main.
    /// </summary>
    private void OnYaClicked()
    {
        Debug.Log("[PowerUpTutorialManager] Player klik YA → mulai gameplay.");

        IsPowerUpTutorial = false;
        TutorialManager.IsTrainingMode = false;

        // === ENABLE GESTURE — WAJIB untuk gameplay ===
        SetGestureEnabled(true);

        SetPowerUpButtonInteractable(true);

        SetActive(overlayPanel, false);
        SetActive(overlayCover, false);
        SetActive(fingerTapIcon, false);
        SetActive(startPanel, false);

        StartGameNormally();
    }

    /// <summary>
    /// Player klik ULANG → restart tutorial dari awal.
    /// Gesture OFF karena tutorial restart.
    /// </summary>
    private void OnUlangClicked()
    {
        Debug.Log("[PowerUpTutorialManager] Player klik ULANG → restart tutorial.");

        SetActive(overlayPanel, false);
        SetActive(overlayCover, false);
        SetActive(fingerTapIcon, false);
        SetActive(startPanel, false);

        PowerManager.ResetAllPowerUpsToFullGlobal();
        RestartTutorial();
    }
    #endregion

    #region Control Helpers
    private void SetGestureEnabled(bool enabled)
    {
        if (gestureDrawer == null) return;
        if (gestureDrawer.gameObject == null) return;

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

    #region Finger Tap Animation
    private void SetupFingerTapPosition()
    {
        if (fingerTapIcon == null || powerUpButton == null) return;

        RectTransform fingerRect = fingerTapIcon.GetComponent<RectTransform>();
        RectTransform buttonRect = powerUpButton.GetComponent<RectTransform>();
        if (fingerRect == null || buttonRect == null) return;

        Canvas buttonCanvas = buttonRect.GetComponentInParent<Canvas>();
        Canvas fingerCanvas = fingerRect.GetComponentInParent<Canvas>();

        Camera buttonCam = GetCanvasCamera(buttonCanvas);
        Camera fingerCam = GetCanvasCamera(fingerCanvas);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(fingerRect.parent as RectTransform);

        Vector3 buttonWorldCenter = buttonRect.TransformPoint(buttonRect.rect.center);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(buttonCam, buttonWorldCenter);
        RectTransform fingerParent = fingerRect.parent as RectTransform ?? fingerRect;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            fingerParent, screenPos, fingerCam, out Vector2 localPos);

        fingerBasePos = localPos + new Vector2(0f, fingerOffsetY);
        fingerRect.anchoredPosition = fingerBasePos;
    }

    private void StartFingerTap()
    {
        if (fingerTapIcon == null) return;

        StopCoroutineSafe(ref fingerTapRoutine);
        fingerTapRoutine = StartCoroutine(FingerTapRoutine());
    }

    private void StopFingerTap()
    {
        StopCoroutineSafe(ref fingerTapRoutine);

        if (fingerTapIcon != null)
        {
            RectTransform rt = fingerTapIcon.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = fingerBasePos;
        }
    }

    private IEnumerator FingerTapRoutine()
    {
        RectTransform fingerRect = fingerTapIcon.GetComponent<RectTransform>();
        if (fingerRect == null) yield break;

        while (true)
        {
            float sinValue = Mathf.Sin(Time.unscaledTime * fingerTapSpeed);
            float offset = Mathf.Abs(sinValue) * fingerTapAmplitude;
            fingerRect.anchoredPosition = fingerBasePos - new Vector2(0f, offset);
            yield return null;
        }
    }
    #endregion

    #region Overlay / Intro
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
            Debug.LogWarning($"[PowerUpTutorialManager] CameraIntroManager tidak ditemukan — fallback {fallbackIntroDelay}s.");
            yield return new WaitForSeconds(fallbackIntroDelay);
        }

        BeginTutorialFlow();
    }

    private IEnumerator RevealHoleRoutine()
    {
        yield return new WaitForSecondsRealtime(coverDuration);

        if (overlayCover != null)
        {
            CanvasGroup cg = overlayCover.GetComponent<CanvasGroup>();
            if (cg == null) cg = overlayCover.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            float t = 0f;
            while (t < coverFadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = 1f - Mathf.Clamp01(t / coverFadeOutDuration);
                yield return null;
            }
        }

        SetActive(overlayCover, false);
        SetActive(overlayPanel, true);

        SetupFingerTapPosition();
        SetActive(fingerTapIcon, true);
        StartFingerTap();

        revealRoutine = null;
    }
    #endregion

    #region Overlay Generation
    private void GenerateOverlay()
    {
        if (powerUpButton == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] powerUpButton belum di-assign — overlay tidak di-generate.");
            return;
        }

        Canvas canvas = FindCanvasWithName("Canvas_Tutorial");
        if (canvas == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] Canvas_Tutorial tidak ditemukan.");
            return;
        }

        GameObject panelRoot = CreateEmptyRect("OverlayPanel", canvas.transform);
        GameObject blackTop = CreateBlackImage("Black_Top", panelRoot.transform);
        GameObject blackBottom = CreateBlackImage("Black_Bottom", panelRoot.transform);
        GameObject blackLeft = CreateBlackImage("Black_Left", panelRoot.transform);
        GameObject blackRight = CreateBlackImage("Black_Right", panelRoot.transform);
        GameObject cover = CreateBlackImage("Black_Cover", panelRoot.transform);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        RectTransform buttonRect = powerUpButton.GetComponent<RectTransform>();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        CalculateHoleBounds(panelRect, buttonRect,
            out float holeLeft, out float holeRight,
            out float holeTop, out float holeBottom);

        float canvasW = panelRect.rect.width;
        float canvasH = panelRect.rect.height;
        float cLeft = -canvasW * 0.5f, cRight = canvasW * 0.5f;
        float cBottom = -canvasH * 0.5f, cTop = canvasH * 0.5f;

        SetupStretchHorizontal(blackBottom.GetComponent<RectTransform>(), 0f, 0f, holeBottom - cBottom);
        SetupStretchHorizontal(blackTop.GetComponent<RectTransform>(), 1f, 1f, cTop - holeTop);
        SetupSidePanel(blackLeft.GetComponent<RectTransform>(), 0f, 0f, holeLeft - cLeft, holeTop, holeBottom);
        SetupSidePanel(blackRight.GetComponent<RectTransform>(), 1f, 1f, cRight - holeRight, holeTop, holeBottom);
        SetupCoverFullScreen(cover.GetComponent<RectTransform>());

        overlayPanel = panelRoot;
        overlayCover = cover;

        panelRoot.SetActive(false);
        cover.SetActive(false);
    }

    private void CalculateHoleBounds(
        RectTransform panelRect, RectTransform buttonRect,
        out float holeLeft, out float holeRight,
        out float holeTop, out float holeBottom)
    {
        Vector3[] corners = new Vector3[4];
        buttonRect.GetWorldCorners(corners);

        Vector2 bl = WorldToLocal(panelRect, corners[0]);
        Vector2 tr = WorldToLocal(panelRect, corners[2]);

        holeLeft = bl.x - overlayPadding;
        holeRight = tr.x + overlayPadding;
        holeBottom = bl.y - overlayPadding;
        holeTop = tr.y + overlayPadding;
    }

    private static void SetupCoverFullScreen(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetupStretchHorizontal(RectTransform rt, float anchorY, float pivotY, float height)
    {
        if (rt == null) return;
        rt.anchorMin = new Vector2(0f, anchorY);
        rt.anchorMax = new Vector2(1f, anchorY);
        rt.pivot = new Vector2(0.5f, pivotY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, height);
        rt.anchoredPosition = Vector2.zero;
    }

    private static void SetupSidePanel(RectTransform rt, float anchorX, float pivotX,
        float width, float top, float bottom)
    {
        if (rt == null) return;
        float centerY = (top + bottom) * 0.5f;
        rt.anchorMin = new Vector2(anchorX, 0.5f);
        rt.anchorMax = new Vector2(anchorX, 0.5f);
        rt.pivot = new Vector2(pivotX, 0.5f);
        rt.sizeDelta = new Vector2(width, top - bottom);
        rt.anchoredPosition = new Vector2(0f, centerY);
    }

    private static GameObject CreateEmptyRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go;
    }

    private GameObject CreateBlackImage(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        Image img = go.GetComponent<Image>();
        img.color = overlayColor;
        img.raycastTarget = false;

        return go;
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
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        EnemyWaveSpawner spawner = FindFirstObjectByType<EnemyWaveSpawner>();
        if (spawner != null)
        {
            spawner.StartWaveSequence();
            Debug.Log("[PowerUpTutorialManager] Gameplay normal dimulai.");
        }
        else
        {
            Debug.LogWarning("[PowerUpTutorialManager] EnemyWaveSpawner tidak ditemukan di scene.");
        }
    }
    #endregion

    #region UI Helpers
    private void HideAllInitially()
    {
        SetActive(overlayPanel, false);
        SetActive(overlayCover, false);
        SetActive(fingerTapIcon, false);
        SetActive(startPanel, false);
        hintManager?.HideAll();
    }

    private void DisableRaycastOnTutorialUI()
    {
        DisableRaycast(overlayPanel);
        DisableRaycast(fingerTapIcon);
        DisableRaycast(overlayCover);

        if (hintManager != null)
            DisableRaycast(hintManager.gameObject);
    }

    private static void DisableRaycast(GameObject go)
    {
        if (go == null) return;

        foreach (var img in go.GetComponentsInChildren<Image>(true))
            if (img != null) img.raycastTarget = false;
    }

    private static Canvas FindCanvasWithName(string name)
    {
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (c != null && c.gameObject.name == name) return c;

        return null;
    }

    private static Camera GetCanvasCamera(Canvas canvas)
    {
        if (canvas == null) return null;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return canvas.worldCamera;
    }

    private Vector2 WorldToLocal(RectTransform parent, Vector3 world)
    {
        Canvas canvas = parent.GetComponentInParent<Canvas>();
        Camera cam = GetCanvasCamera(canvas);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, cam, out Vector2 local);
        return local;
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
        PlayerPrefs.DeleteKey("TutorialCompleted");
        GameProgressManager.ClearGameState();
        PlayerPrefs.Save();

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