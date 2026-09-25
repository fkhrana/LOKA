using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tutorial power-up khusus boss level.
/// Flow: tunggu intro → overlay di tombol power-up → player tap
///       → trigger power-up → spawn musuh tutorial → kill semua
///       → SUKSES = spawn boss
///       → GAGAL  = panel MULAI MAIN? (YA / ULANG)
/// TIDAK menyentuh LevelProgressManager / wave spawner.
/// </summary>
public class BossLevelPowerUpTutorial : MonoBehaviour
{
    [Header("Reusable Components")]
    [SerializeField] private TutorialOverlay overlay;
    [SerializeField] private TutorialFingerTap fingerTap;

    [Header("Cover")]
    [SerializeField] private float coverDuration = 1.2f;

    [Header("Power Up")]
    [SerializeField] private Button powerUpButton;
    [SerializeField] private PowerManager.PowerUpType powerUpType = PowerManager.PowerUpType.Freeze;
    [SerializeField] private bool triggerPowerUp = true;
    [SerializeField] private bool requiresAksaraPath = true;

    [Header("References")]
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private TutorialHintManager hintManager;
    [SerializeField] private TutorialEnemySpawner tutorialSpawner;
    [SerializeField] private AksaraData tutorialAksara;
    [SerializeField] private EnemyData tutorialEnemyData;

    [Header("Tutorial Enemies")]
    [SerializeField, Min(1)] private int enemyCount = 3;
    [SerializeField, Min(0.1f)] private float enemySpeed = 2f;
    [SerializeField] private bool spawnNearCameraTarget = true;
    [SerializeField] private Vector3 manualSpawnCenter = Vector3.zero;
    [SerializeField, Min(0.1f)] private float spawnRadius = 1.5f;

    [Header("Blink Effect")]
    [SerializeField, Min(0.05f)] private float blinkInterval = 0.25f;

    [Header("Boss")]
    [SerializeField] private EnemyWaveSpawner bossSpawner;

    [Header("Outcome Panel")]
    [SerializeField] private GameObject failPanel;
    [SerializeField] private Button yaButton;
    [SerializeField] private Button ulangButton;

    [Header("Intro Wait")]
    [SerializeField] private bool waitForCameraIntro = true;
    [SerializeField, Min(0f)] private float fallbackIntroDelay = 10f;

    [Header("Skip Tutorial")]
    [Tooltip("Kalau player sudah pernah tutorial → langsung spawn boss.")]
    [SerializeField] private bool skipIfAlreadyDone = true;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    // ---- Runtime ----
    private bool isFinished;
    private bool waitingForTap;
    private Coroutine blinkRoutine;

    // =====================================================
    // UNITY LIFECYCLE
    // =====================================================
    private void Awake()
    {
        if (overlay == null) overlay = GetComponentInChildren<TutorialOverlay>(true);
        if (fingerTap == null) fingerTap = GetComponentInChildren<TutorialFingerTap>(true);

        if (gestureDrawer == null)
            gestureDrawer = FindFirstObjectByType<GestureDrawer>();

        if (tutorialSpawner == null)
            tutorialSpawner = FindFirstObjectByType<TutorialEnemySpawner>();

        if (failPanel != null) failPanel.SetActive(false);

        if (yaButton != null) yaButton.onClick.AddListener(OnYaClicked);
        if (ulangButton != null) ulangButton.onClick.AddListener(OnUlangClicked);

        overlay?.Build();

        // Pastikan raycast di UI tutorial mati, supaya tap tombol power-up tembus.
        DisableRaycastOnTutorialUI();
    }

    private void OnEnable()
    {
        if (powerUpButton != null)
            powerUpButton.onClick.AddListener(OnPowerUpTapped);
    }

    private void OnDisable()
    {
        if (powerUpButton != null)
            powerUpButton.onClick.RemoveListener(OnPowerUpTapped);

        StopBlink();
        fingerTap?.Hide();
    }

    private void Start()
    {
        StartCoroutine(RunRoutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (yaButton != null) yaButton.onClick.RemoveListener(OnYaClicked);
        if (ulangButton != null) ulangButton.onClick.RemoveListener(OnUlangClicked);
    }

    // =====================================================
    // MAIN FLOW
    // =====================================================
    private IEnumerator RunRoutine()
    {
        // Skip kalau sudah pernah tutorial — cek DULU sebelum nunggu intro,
        // biar EnemyWaveSpawner tahu boss harus di-spawn oleh kita.
        if (skipIfAlreadyDone && GameProgressManager.IsTutorialCompleted())
        {
            if (debugLog)
                Debug.Log("[BossTutorial] Sudah pernah tutorial → skip, langsung boss.");

            SpawnBoss();
            yield break;
        }

        // Tunggu intro selesai (panning + countdown)
        yield return StartCoroutine(WaitForIntroRoutine());

        // Tampilkan tutorial
        yield return StartCoroutine(ShowTutorialRoutine());
    }

    private IEnumerator WaitForIntroRoutine()
    {
        if (!waitForCameraIntro) yield break;

        if (CameraIntroManager.Instance == null)
        {
            if (debugLog)
                Debug.Log($"[BossTutorial] CameraIntroManager tidak ada → tunggu {fallbackIntroDelay}s.");

            yield return new WaitForSeconds(fallbackIntroDelay);
            yield break;
        }

        if (debugLog)
            Debug.Log("[BossTutorial] Menunggu intro selesai...");

        while (!CameraIntroManager.GameStarted)
            yield return null;

        if (debugLog)
            Debug.Log("[BossTutorial] Intro selesai.");
    }

    private IEnumerator ShowTutorialRoutine()
    {
        isFinished = false;
        waitingForTap = false;

        // Reset UI
        SetGestureEnabled(false);
        SetPowerUpButtonInteractable(false);
        if (failPanel != null) failPanel.SetActive(false);
        hintManager?.HideAll();

        // 1. Cover hitam penuh
        overlay?.ShowCover();

        if (coverDuration > 0f)
            yield return new WaitForSecondsRealtime(coverDuration);

        // 2. Fade cover
        if (overlay != null)
            yield return StartCoroutine(overlay.FadeOutCoverRoutine());

        // 3. Tampilkan overlay dengan lubang di tombol power-up
        if (overlay != null && powerUpButton != null)
        {
            RectTransform btnRect = powerUpButton.GetComponent<RectTransform>();
            overlay.ShowOverlay(btnRect);
        }

        // Pastikan raycast tetap mati
        DisableRaycastOnTutorialUI();

        // 4. Jari + aktifkan tombol
        fingerTap?.Show();
        SetPowerUpButtonInteractable(true);
        waitingForTap = true;

        if (debugLog)
            Debug.Log("[BossTutorial] Menunggu tap power-up...");
    }

    // =====================================================
    // TAP HANDLER
    // =====================================================
    private void OnPowerUpTapped()
    {
        if (isFinished || !waitingForTap) return;

        waitingForTap = false;

        if (debugLog)
            Debug.Log("[BossTutorial] Player tap → trigger power-up.");

        fingerTap?.Hide();
        overlay?.HideAll();

        SetPowerUpButtonInteractable(false);

        // Trigger power-up
        if (triggerPowerUp)
            TriggerPowerUpEffect();

        // Aktifkan gesture
        SetGestureEnabled(true);

        // Spawn musuh tutorial
        SpawnTutorialEnemies();
    }

    private void TriggerPowerUpEffect()
    {
        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);
        foreach (var pm in managers)
        {
            pm.UsePowerUp(powerUpType);
            break;
        }
    }

    // =====================================================
    // TUTORIAL ENEMIES
    // =====================================================
    private void SpawnTutorialEnemies()
    {
        if (tutorialSpawner == null)
        {
            Debug.LogWarning("[BossTutorial] TutorialEnemySpawner tidak ada → skip warm-up.");
            OnTutorialSuccess();
            return;
        }

        Vector3 center = GetSpawnCenter();

        tutorialSpawner.Spawn(
            count: enemyCount,
            center: center,
            radius: spawnRadius,
            enemyData: tutorialEnemyData,
            aksara: tutorialAksara,
            onAllKilled: OnTutorialSuccess,
            onAllCrashed: OnTutorialFail
        );

        tutorialSpawner.ActivateAll();
        tutorialSpawner.SetSpeed(enemySpeed);

        // Hint + blink
        if (hintManager != null)
            hintManager.ShowCircleHighlight(tutorialSpawner.SpawnedEnemies);

        if (requiresAksaraPath && hintManager != null && tutorialAksara != null)
            hintManager.ShowPath(tutorialAksara);

        StartBlink();

        if (debugLog)
            Debug.Log($"[BossTutorial] Spawn {enemyCount} musuh tutorial.");
    }

    private Vector3 GetSpawnCenter()
    {
        if (!spawnNearCameraTarget) return manualSpawnCenter;

        if (CameraIntroManager.Instance != null &&
            CameraIntroManager.Instance.targetKanan != null)
            return CameraIntroManager.Instance.targetKanan.position;

        return manualSpawnCenter;
    }

    // =====================================================
    // OUTCOME
    // =====================================================
    private void OnTutorialSuccess()
    {
        if (isFinished) return;
        isFinished = true;

        if (debugLog)
            Debug.Log("[BossTutorial] ✅ Sukses → spawn boss.");

        StopBlink();
        hintManager?.HideAll();
        SetGestureEnabled(false);

        // Tandai tutorial selesai di sini, baru spawn boss.
        GameProgressManager.MarkTutorialCompleted();

        SpawnBoss();
    }

    private void OnTutorialFail()
    {
        if (isFinished) return;
        isFinished = true;

        if (debugLog)
            Debug.Log("[BossTutorial] ❌ Gagal → tampilkan panel.");

        StopBlink();
        hintManager?.HideAll();
        SetGestureEnabled(false);

        if (failPanel != null)
            failPanel.SetActive(true);
    }

    private void SpawnBoss()
    {
        if (bossSpawner == null)
        {
            Debug.LogError("[BossTutorial] bossSpawner belum di-assign!");
            return;
        }

        // Guard: jangan double spawn kalau EnemyWaveSpawner sudah spawn boss.
        if (EnemyWaveSpawner.IsBossSpawned)
        {
            if (debugLog)
                Debug.Log("[BossTutorial] Boss sudah di-spawn — skip.");
            return;
        }

        bossSpawner.SpawnBoss();

        if (debugLog)
            Debug.Log("[BossTutorial] Boss spawned.");
    }

    // =====================================================
    // PANEL BUTTONS
    // =====================================================
    private void OnYaClicked()
    {
        if (debugLog)
            Debug.Log("[BossTutorial] YA → skip, langsung boss.");

        if (failPanel != null) failPanel.SetActive(false);

        tutorialSpawner?.Clear();

        SpawnBoss();
    }

    private void OnUlangClicked()
    {
        if (debugLog)
            Debug.Log("[BossTutorial] ULANG → restart tutorial.");

        if (failPanel != null) failPanel.SetActive(false);

        tutorialSpawner?.Clear();

        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        // Reset guard boss supaya bisa spawn lagi setelah tutorial diulang.
        EnemyWaveSpawner.ResetBossSpawned();

        yield return null;
        yield return StartCoroutine(ShowTutorialRoutine());
    }

    // =====================================================
    // GESTURE
    // =====================================================
    private void SetGestureEnabled(bool enabled)
    {
        if (gestureDrawer == null) return;
        if (!enabled) gestureDrawer.ResetGestureInput();
        gestureDrawer.enabled = enabled;

        if (debugLog)
            Debug.Log($"[BossTutorial] GestureDrawer.enabled = {enabled}");
    }

    private void SetPowerUpButtonInteractable(bool interactable)
    {
        if (powerUpButton != null)
            powerUpButton.interactable = interactable;
    }

    // =====================================================
    // BLINK
    // =====================================================
    private void StartBlink()
    {
        StopBlink();
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void StopBlink()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

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

    // =====================================================
    // UI HELPERS
    // =====================================================
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
}