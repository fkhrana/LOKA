using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpTutorialManager : MonoBehaviour
{
    public static bool IsPowerUpTutorial { get; private set; }

    private enum TutorialStep
    {
        Idle,
        WaitingForScreenTap,
        WaitingForPowerUpTap,
        ShowingHint,
        WaitingOutcome,
        Success,
        Fail
    }

    [Header("Overlay")]
    [SerializeField] private GameObject overlayPanel;
    [SerializeField] private GameObject overlayCover;
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

    [Header("Enemy Spawner")]
    [SerializeField] private TutorialEnemySpawner tutorialSpawner;
    [SerializeField] private int tutorialEnemyCount = 2;
    [SerializeField] private float enemySlowSpeed = 0.5f;
    [SerializeField] private float enemyNormalSpeed = 2f;
    [SerializeField] private bool spawnNearCameraTarget = true;
    [SerializeField] private Vector3 manualSpawnCenter = Vector3.zero;
    [SerializeField] private float spawnRadius = 1.5f;

    [Header("Enemy Data")]
    [SerializeField] private AksaraData tutorialAksara;
    [SerializeField] private EnemyData tutorialEnemyData;

    [Header("Hint Dots")]
    [SerializeField] private RectTransform hintContainer;
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private Color dotColor = new Color(1f, 0.9f, 0.2f);
    [SerializeField] private float dotSize = 18f;
    [SerializeField] private float displaySize = 220f;
    [SerializeField] private float dotSpacing = 0.1f;

    [Header("Hint Dots Animation")]
    [SerializeField] private float dotRevealDuration = 0.18f;
    [SerializeField] private float dotStagger = 0.06f;
    [SerializeField] private float holdAfterComplete = 0.9f;
    [SerializeField] private float restartDelay = 0.3f;
    [SerializeField] private bool loopDots = true;
    [SerializeField] private bool pulseAfterReveal = true;
    [SerializeField] private float pulseSpeed = 3.5f;
    [SerializeField] private float pulseAmount = 0.18f;

    [Header("Enemy Blink")]
    [SerializeField] private float blinkInterval = 0.25f;

    [Header("Outcome Panel")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button yaButton;
    [SerializeField] private Button ulangButton;

    [Header("Timing")]
    [SerializeField] private float tutorialTimeLimit = 30f;

    [Header("Intro Wait")]
    [SerializeField] private bool waitForCameraIntro = true;
    [SerializeField] private float maxWaitForIntro = 30f;
    [SerializeField] private float fallbackIntroDelay = 10f;

    [Header("Debug")]
    [Tooltip("Centang untuk paksa tutorial muncul terus — buat testing. HILANGKAN sebelum build.")]
    [SerializeField] private bool forceShowForTesting = false;

    private TutorialStep currentStep = TutorialStep.Idle;
    private int enemiesAliveCount;

    private Coroutine failTimerRoutine;
    private Coroutine revealRoutine;
    private Coroutine blinkRoutine;
    private Coroutine fingerTapRoutine;
    private Coroutine dotsAnimRoutine;

    private readonly List<Image> spawnedDots = new List<Image>();
    private Sprite cachedDotSprite;
    private Vector2 fingerBasePos;

    private void Awake()
    {
        if (ShouldSkipTutorial())
        {
            Debug.Log("[PowerUpTutorialManager] Skip — tutorial sudah pernah selesai.");
            gameObject.SetActive(false);
            return;
        }

        HookButtons();
        if (autoGenerateOverlay)
            GenerateOverlay();
        DisableRaycastOnTutorialUI();
    }

    private void Start()
    {
        SpawnTutorialEnemies();
        SetEnemiesSpeed(enemySlowSpeed);

        HideAllInitially();

        if (waitForCameraIntro)
            StartCoroutine(WaitForIntroThenStart());
        else
            OnIntroComplete();
    }

    private void Update()
    {
        HandleScreenTap();
        HandleDotsPulse();
    }

    private void OnDestroy()
    {
        StopAllTutorialCoroutines();
    }

    private bool ShouldSkipTutorial()
    {
        if (forceShowForTesting) return false;
        return GameProgressManager.IsTutorialCompleted();
    }

    private void HookButtons()
    {
        if (powerUpButton != null) powerUpButton.onClick.AddListener(OnPowerUpTapped);
        if (yaButton != null) yaButton.onClick.AddListener(OnYaClicked);
        if (ulangButton != null) ulangButton.onClick.AddListener(OnUlangClicked);
    }

    private void HideAllInitially()
    {
        SetActiveSafe(overlayPanel, false);
        SetActiveSafe(overlayCover, false);
        SetActiveSafe(fingerTapIcon, false);
        SetActiveSafe(startPanel, false);
        HideHintDots();
    }

    private void StopAllTutorialCoroutines()
    {
        StopCoroutineSafe(ref failTimerRoutine);
        StopCoroutineSafe(ref revealRoutine);
        StopCoroutineSafe(ref blinkRoutine);
        StopCoroutineSafe(ref fingerTapRoutine);
        StopCoroutineSafe(ref dotsAnimRoutine);
    }

    private void HandleScreenTap()
    {
        if (currentStep != TutorialStep.WaitingForScreenTap) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (overlayCover != null && overlayCover.activeSelf) return;

        OnScreenTapped();
    }

    private void HandleDotsPulse()
    {
        if (!pulseAfterReveal || spawnedDots.Count == 0) return;

        float s = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;

        foreach (var dot in spawnedDots)
        {
            if (dot == null) continue;
            if (dot.rectTransform.localScale.x < 0.05f) continue;
            dot.rectTransform.localScale = Vector3.one * s;
        }
    }

    private IEnumerator WaitForIntroThenStart()
    {
        bool hasIntro = CameraIntroManager.Instance != null;

        if (hasIntro)
        {
            float elapsed = 0f;
            while (!CameraIntroManager.GameStarted && elapsed < maxWaitForIntro)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!CameraIntroManager.GameStarted)
                Debug.LogWarning("[PowerUpTutorialManager] Timeout nunggu intro.");
        }
        else
        {
            Debug.LogWarning(
                "[PowerUpTutorialManager] CameraIntroManager tidak ditemukan. " +
                $"Pakai fallback delay {fallbackIntroDelay} detik."
            );
            yield return new WaitForSeconds(fallbackIntroDelay);
        }

        OnIntroComplete();
    }

    private void OnIntroComplete()
    {
        IsPowerUpTutorial = true;
        currentStep = TutorialStep.WaitingForScreenTap;

        SetActiveSafe(overlayPanel, true);
        SetActiveSafe(overlayCover, true);
        SetActiveSafe(fingerTapIcon, false);

        SetupFingerTapPosition();

        StopCoroutineSafe(ref revealRoutine);
        revealRoutine = StartCoroutine(RevealHoleRoutine());
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

        SetActiveSafe(overlayCover, false);

        SetupFingerTapPosition();
        SetActiveSafe(fingerTapIcon, true);
        StartFingerTap();

        revealRoutine = null;
    }

    private void SetupFingerTapPosition()
    {
        if (fingerTapIcon == null || powerUpButton == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] fingerTapIcon / powerUpButton belum di-assign.");
            return;
        }

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

    public void StartTutorial()
    {
        IsPowerUpTutorial = true;
        currentStep = TutorialStep.WaitingForScreenTap;

        SetActiveSafe(overlayPanel, true);
        SetActiveSafe(overlayCover, true);
        SetActiveSafe(fingerTapIcon, false);
        SetActiveSafe(startPanel, false);
        HideHintDots();

        ClearTutorialEnemies();
        SpawnTutorialEnemies();
        SetEnemiesSpeed(enemySlowSpeed);

        SetupFingerTapPosition();

        StopCoroutineSafe(ref revealRoutine);
        revealRoutine = StartCoroutine(RevealHoleRoutine());
    }

    private void OnScreenTapped()
    {
        SetActiveSafe(overlayPanel, false);

        if (tutorialSpawner != null)
            tutorialSpawner.ActivateAll();

        SetEnemiesSpeed(enemyNormalSpeed);
        currentStep = TutorialStep.WaitingForPowerUpTap;
    }

    private void OnPowerUpTapped()
    {
        if (currentStep != TutorialStep.WaitingForPowerUpTap) return;

        StopFingerTap();
        SetActiveSafe(fingerTapIcon, false);

        TriggerPowerUp();
        ShowEnemyHint();

        StopCoroutineSafe(ref failTimerRoutine);
        failTimerRoutine = StartCoroutine(FailTimerRoutine());
    }

    private void TriggerPowerUp()
    {
        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);
        foreach (var pm in managers)
        {
            pm.UsePowerUp(powerUpType);
            break;
        }
    }

    private IEnumerator FailTimerRoutine()
    {
        yield return new WaitForSeconds(tutorialTimeLimit);
        NotifyTutorialFailed();
    }

    private void ShowEnemyHint()
    {
        ShowHintDots();
        StartBlinking();
        currentStep = TutorialStep.WaitingOutcome;
    }

    private void ShowHintDots()
    {
        if (hintContainer == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] hintContainer belum di-assign.");
            return;
        }

        if (tutorialAksara == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] tutorialAksara belum di-assign.");
            return;
        }

        GestureShape shape = tutorialAksara.GestureShape;
        List<Vector2> path = GetHardcodedPath(shape);

        if (path == null || path.Count < 2)
        {
            Debug.LogWarning($"[PowerUpTutorialManager] Path kosong untuk {shape}.");
            return;
        }

        ClearDots();

        List<Vector2> points = SamplePath(path, dotSpacing);
        float scale = displaySize / 1.2f;

        foreach (Vector2 p in points)
        {
            Image dot = CreateDot();
            RectTransform rt = dot.rectTransform;
            rt.anchoredPosition = p * scale;
            rt.sizeDelta = new Vector2(dotSize, dotSize);
            dot.color = dotColor;
            rt.localScale = Vector3.zero;
            spawnedDots.Add(dot);
        }

        StopCoroutineSafe(ref dotsAnimRoutine);
        dotsAnimRoutine = StartCoroutine(AnimateDotsRoutine());

        Debug.Log($"[PowerUpTutorialManager] Hint dots → {shape} ({spawnedDots.Count} dots)");
    }

    private void HideHintDots()
    {
        StopCoroutineSafe(ref dotsAnimRoutine);
        ClearDots();
    }

    private void ClearDots()
    {
        foreach (var d in spawnedDots)
            if (d != null) Destroy(d.gameObject);
        spawnedDots.Clear();
    }

    private Image CreateDot()
    {
        GameObject go = new GameObject("Dot", typeof(Image));
        go.transform.SetParent(hintContainer, false);

        Image img = go.GetComponent<Image>();
        img.sprite = GetDotSprite();
        img.color = dotColor;
        img.raycastTarget = false;
        img.preserveAspect = true;

        return img;
    }

    private IEnumerator AnimateDotsRoutine()
    {
        while (true)
        {
            foreach (var d in spawnedDots)
                if (d != null) d.rectTransform.localScale = Vector3.zero;

            yield return null;

            for (int i = 0; i < spawnedDots.Count; i++)
            {
                Image d = spawnedDots[i];
                if (d == null) continue;

                StartCoroutine(PopInDot(d.rectTransform));
                yield return new WaitForSecondsRealtime(dotStagger);
            }

            yield return new WaitForSecondsRealtime(dotRevealDuration + holdAfterComplete);

            if (!loopDots) yield break;

            yield return new WaitForSecondsRealtime(restartDelay);
        }
    }

    private IEnumerator PopInDot(RectTransform rt)
    {
        if (rt == null) yield break;

        float t = 0f;
        while (t < dotRevealDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dotRevealDuration);
            rt.localScale = Vector3.one * EaseOutBack(p);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private void StartBlinking()
    {
        StopCoroutineSafe(ref blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void StopBlinking()
    {
        StopCoroutineSafe(ref blinkRoutine);

        if (tutorialSpawner != null)
        {
            foreach (var sr in tutorialSpawner.GetAllRenderers())
                if (sr != null) sr.enabled = true;
        }
    }

    private IEnumerator BlinkRoutine()
    {
        List<SpriteRenderer> renderers = tutorialSpawner != null
            ? tutorialSpawner.GetAllRenderers()
            : new List<SpriteRenderer>();

        while (true)
        {
            foreach (var sr in renderers)
                if (sr != null) sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void OnAnyTutorialEnemyDied()
    {
        enemiesAliveCount--;
        if (enemiesAliveCount <= 0) OnTutorialSuccess();
    }

    public void NotifyTutorialFailed()
    {
        if (currentStep != TutorialStep.WaitingOutcome) return;

        StopCoroutineSafe(ref failTimerRoutine);

        currentStep = TutorialStep.Fail;
        StopBlinking();
        HideHintDots();
        SetActiveSafe(startPanel, true);
    }

    private void OnTutorialSuccess()
    {
        StopCoroutineSafe(ref failTimerRoutine);

        currentStep = TutorialStep.Success;
        StopBlinking();
        HideHintDots();

        IsPowerUpTutorial = false;
        ResetPowerUpUIToFull();
        StartGameNormally();
    }

    private void OnYaClicked()
    {
        IsPowerUpTutorial = false;
        SetActiveSafe(startPanel, false);
        StartGameNormally();
    }

    private void OnUlangClicked()
    {
        StopCoroutineSafe(ref failTimerRoutine);
        SetActiveSafe(startPanel, false);
        ResetPowerUpUIToFull();
        StartTutorial();
    }

    private void SpawnTutorialEnemies()
    {
        if (tutorialSpawner == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] tutorialSpawner belum di-assign.");
            return;
        }

        Vector3 center = GetSpawnCenter();

        List<EnemyGestureCommand> spawned = tutorialSpawner.Spawn(
            tutorialEnemyCount,
            center,
            spawnRadius,
            tutorialEnemyData,
            tutorialAksara,
            OnAnyTutorialEnemyDied
        );

        enemiesAliveCount = spawned.Count;
    }

    private Vector3 GetSpawnCenter()
    {
        if (!spawnNearCameraTarget) return manualSpawnCenter;

        if (CameraIntroManager.Instance != null &&
            CameraIntroManager.Instance.targetKanan != null)
        {
            return CameraIntroManager.Instance.targetKanan.position;
        }

        return manualSpawnCenter;
    }

    private void ClearTutorialEnemies()
    {
        if (tutorialSpawner != null)
            tutorialSpawner.Clear();

        enemiesAliveCount = 0;
    }

    private void SetEnemiesSpeed(float speed)
    {
        if (tutorialSpawner != null)
            tutorialSpawner.SetSpeed(speed);
    }

    private void GenerateOverlay()
    {
        if (powerUpButton == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] powerUpButton belum di-assign — overlay auto-generate dilewati.");
            return;
        }

        Canvas canvas = FindCanvasWithName("Canvas_Tutorial");
        if (canvas == null)
        {
            Debug.LogWarning("[PowerUpTutorialManager] Canvas_Tutorial tidak ditemukan.");
            return;
        }

        GameObject panelRoot = CreateEmptyRect("OverlayPanel", canvas.transform);

        GameObject blackTop    = CreateBlackImage("Black_Top",    panelRoot.transform);
        GameObject blackBottom = CreateBlackImage("Black_Bottom", panelRoot.transform);
        GameObject blackLeft   = CreateBlackImage("Black_Left",   panelRoot.transform);
        GameObject blackRight  = CreateBlackImage("Black_Right",  panelRoot.transform);
        GameObject cover       = CreateBlackImage("Black_Cover",  panelRoot.transform);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        RectTransform buttonRect = powerUpButton.GetComponent<RectTransform>();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        CalculateHoleBounds(panelRect, buttonRect,
            out float holeLeft, out float holeRight,
            out float holeTop, out float holeBottom);

        float canvasW = panelRect.rect.width;
        float canvasH = panelRect.rect.height;
        float cLeft   = -canvasW * 0.5f;
        float cRight  =  canvasW * 0.5f;
        float cBottom = -canvasH * 0.5f;
        float cTop    =  canvasH * 0.5f;

        SetupStretchHorizontal(blackBottom.GetComponent<RectTransform>(), 0f, 0f, holeBottom - cBottom);
        SetupStretchHorizontal(blackTop.GetComponent<RectTransform>(), 1f, 1f, cTop - holeTop);
        SetupSidePanel(blackLeft.GetComponent<RectTransform>(), 0f, 0f, holeLeft - cLeft, holeTop, holeBottom);
        SetupSidePanel(blackRight.GetComponent<RectTransform>(), 1f, 1f, cRight - holeRight, holeTop, holeBottom);

        SetupCoverFullScreen(cover.GetComponent<RectTransform>());

        overlayPanel = panelRoot;
        overlayCover = cover;

        panelRoot.SetActive(false);
        cover.SetActive(false);

        Debug.Log("[PowerUpTutorialManager] Overlay auto-generated.");
    }

    private void CalculateHoleBounds(RectTransform panelRect, RectTransform buttonRect,
        out float holeLeft, out float holeRight, out float holeTop, out float holeBottom)
    {
        Vector3[] corners = new Vector3[4];
        buttonRect.GetWorldCorners(corners);

        Vector2 bl = WorldToLocal(panelRect, corners[0]);
        Vector2 tr = WorldToLocal(panelRect, corners[2]);

        holeLeft   = bl.x - overlayPadding;
        holeRight  = tr.x + overlayPadding;
        holeBottom = bl.y - overlayPadding;
        holeTop    = tr.y + overlayPadding;
    }

    private void SetupCoverFullScreen(RectTransform rt)
    {
        if (rt == null) return;

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void SetupStretchHorizontal(RectTransform rt, float anchorY, float pivotY, float height)
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

    private void SetupSidePanel(RectTransform rt, float anchorX, float pivotX,
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

    private GameObject CreateEmptyRect(string name, Transform parent)
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

    private void ResetPowerUpUIToFull()
    {
        PowerManager.ResetAllPowerUpsToFullGlobal();
    }

    private void StartGameNormally()
    {
        GameProgressManager.MarkTutorialCompleted();

        // Set juga key lama biar SaveCurrentProgress & PauseOverlay nggak skip.
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        StartWaveSequenceIfAvailable();
    }

    private void StartWaveSequenceIfAvailable()
    {
        EnemyWaveSpawner spawner = FindFirstObjectByType<EnemyWaveSpawner>();
        if (spawner == null)
        {
            Debug.Log("[PowerUpTutorialManager] EnemyWaveSpawner tidak ditemukan di scene.");
            return;
        }

        spawner.StartWaveSequence();
        Debug.Log("[PowerUpTutorialManager] Gameplay normal dimulai.");
    }

    private List<Vector2> GetHardcodedPath(GestureShape shape)
    {
        switch (shape)
        {
            case GestureShape.Na:
                return new List<Vector2>
                {
                    new Vector2(-0.6f, 0f),
                    new Vector2( 0.6f, 0f),
                };

            case GestureShape.Ka:
                return new List<Vector2>
                {
                    new Vector2(-0.5f,  0.5f),
                    new Vector2( 0.5f,  0.5f),
                    new Vector2( 0.5f, -0.5f),
                    new Vector2(-0.5f, -0.5f),
                };

            case GestureShape.Wa:
                return new List<Vector2>
                {
                    new Vector2(-0.6f, -0.6f),
                    new Vector2(-0.4f,  0.6f),
                    new Vector2( 0f,    0f),
                    new Vector2( 0.4f,  0.6f),
                    new Vector2( 0.6f, -0.6f),
                };

            case GestureShape.La:
                return new List<Vector2>
                {
                    new Vector2(-0.5f,  0.6f),
                    new Vector2(-0.5f, -0.6f),
                    new Vector2( 0.6f, -0.6f),
                };

            case GestureShape.Da:
                return new List<Vector2>
                {
                    new Vector2(-0.5f,  0.5f),
                    new Vector2( 0.5f,  0.5f),
                    new Vector2( 0.5f, -0.5f),
                    new Vector2(-0.5f, -0.5f),
                };

            default:
                return null;
        }
    }

    private List<Vector2> SamplePath(List<Vector2> path, float spacing)
    {
        var result = new List<Vector2>();

        float totalLen = 0f;
        for (int i = 1; i < path.Count; i++)
            totalLen += Vector2.Distance(path[i - 1], path[i]);

        if (totalLen < 0.0001f)
        {
            result.Add(path[0]);
            return result;
        }

        int dotCount = Mathf.Max(2, Mathf.FloorToInt(totalLen / spacing) + 1);
        float actualSpacing = totalLen / (dotCount - 1);

        result.Add(path[0]);

        float targetDist = actualSpacing;
        float walked = 0f;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 a = path[i - 1];
            Vector2 b = path[i];
            float segLen = Vector2.Distance(a, b);

            while (targetDist <= walked + segLen)
            {
                float t = (targetDist - walked) / segLen;
                result.Add(Vector2.Lerp(a, b, t));
                targetDist += actualSpacing;
            }

            walked += segLen;
        }

        return result;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p = t - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }

    private void DisableRaycastOnTutorialUI()
    {
        DisableRaycast(overlayPanel);
        DisableRaycast(fingerTapIcon);
        DisableRaycast(overlayCover);
        if (hintContainer != null) DisableRaycast(hintContainer.gameObject);
    }

    private void DisableRaycast(GameObject go)
    {
        if (go == null) return;
        foreach (var img in go.GetComponentsInChildren<Image>(true))
            if (img != null) img.raycastTarget = false;
    }

    private Canvas FindCanvasWithName(string name)
    {
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c != null && c.gameObject.name == name) return c;
        }
        return null;
    }

    private Camera GetCanvasCamera(Canvas canvas)
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

    private Sprite GetDotSprite()
    {
        if (cachedDotSprite != null) return cachedDotSprite;

        cachedDotSprite = dotSprite != null ? dotSprite : CreateCircleSprite(64);
        return cachedDotSprite;
    }

    private Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        float radius = size * 0.5f;
        float inner = radius - 1.5f;
        var center = new Vector2(radius, radius);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center.x;
                float dy = y + 0.5f - center.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha;
                if (dist <= inner) alpha = 1f;
                else if (dist >= radius) alpha = 0f;
                else alpha = 1f - ((dist - inner) / (radius - inner));

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void StopCoroutineSafe(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}