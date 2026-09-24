using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner khusus musuh tutorial.
/// Deteksi nabrak pakai 2 metode:
/// 1. Timing damage — kalau player baru saja take damage, musuh yang hilang = NABRAK
/// 2. Distance fallback — cek jarak terakhir musuh ke player
/// 
/// Hasil:
/// - Semua kill → SUCCESS
/// - Semua crash → GAGAL
/// - Mix (sebagian kill, sebagian crash) → GAGAL (panel muncul)
/// </summary>
public class TutorialEnemySpawner : MonoBehaviour
{
    #region Inspector Fields
    [Header("Prefab & Parent")]
    [SerializeField] private EnemyGestureCommand enemyPrefab;
    [SerializeField] private Transform enemyParent;

    [Header("Spawn Mode")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool useSpawnArea = false;
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(4f, 4f);

    [Header("Isolation")]
    [SerializeField] private string tutorialTag = "TutorialEnemy";
    [SerializeField] private string tutorialLayer = "TutorialEnemy";

    [Header("Anti-Nabrak Filter")]
    [Tooltip("Kalau musuh hilang saat jaraknya ke player kurang dari ini, dianggap NABRAK. " +
             "Naikkan kalau musuh nabrak tidak terdeteksi.")]
    [SerializeField, Min(0.1f)] private float crashDetectionRadius = 5f;

    [Tooltip("Window waktu (detik) setelah player take damage. " +
             "Kalau musuh hilang dalam window ini, dianggap NABRAK.")]
    [SerializeField, Min(0.1f)] private float crashDamageWindow = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool debugSpawn = false;
    [SerializeField] private bool debugDeathTrack = true;
    #endregion

    #region Runtime State
    private readonly List<EnemyGestureCommand> spawnedEnemies = new List<EnemyGestureCommand>();
    private readonly Dictionary<int, EnemyGestureCommand> enemiesById = new Dictionary<int, EnemyGestureCommand>();
    private readonly Dictionary<int, Vector3> lastKnownPositions = new Dictionary<int, Vector3>();
    private readonly HashSet<int> countedEnemyIds = new HashSet<int>();

    private Transform playerTransform;
    private PlayerHealth trackedPlayerHealth;
    private float lastPlayerDamageTime = -999f;

    private Coroutine deathTrackerRoutine;
    private int expectedKillCount = 0;
    private int validKillCount = 0;
    private int totalCrashCount = 0;

    private Action onAllKilledCallback;
    private Action onAllCrashedCallback;

    // === GUARD ===
    private bool hasFiredCallback = false;
    private bool isSpawning = false;
    #endregion

    #region Public Properties
    public IReadOnlyList<EnemyGestureCommand> SpawnedEnemies => spawnedEnemies;
    public int AliveCount => spawnedEnemies.Count;
    public int ValidKillCount => validKillCount;
    public int TotalCrashCount => totalCrashCount;
    public int ExpectedKillCount => expectedKillCount;
    #endregion

    #region Unity Lifecycle
    private void OnDisable()
    {
        UnsubscribePlayerHealth();
        StopDeathTracker();
    }
    #endregion

    #region Public API
    public List<EnemyGestureCommand> Spawn(
        int count,
        Vector3 center,
        float radius,
        EnemyData enemyData,
        AksaraData aksara,
        Action onAllKilled = null,
        Action onAllCrashed = null)
    {
        if (isSpawning)
        {
            Debug.LogWarning("[TutorialEnemySpawner] Spawn() sedang berjalan — abaikan panggilan ganda.");
            return spawnedEnemies;
        }

        isSpawning = true;

        spawnedEnemies.Clear();
        enemiesById.Clear();
        lastKnownPositions.Clear();
        countedEnemyIds.Clear();

        validKillCount = 0;
        totalCrashCount = 0;
        expectedKillCount = count;
        hasFiredCallback = false;

        onAllKilledCallback = onAllKilled;
        onAllCrashedCallback = onAllCrashed;

        if (enemyPrefab == null)
        {
            Debug.LogWarning("[TutorialEnemySpawner] enemyPrefab belum di-assign.");
            isSpawning = false;
            return spawnedEnemies;
        }

        if (count <= 0)
        {
            Debug.LogWarning($"[TutorialEnemySpawner] count={count} tidak valid. Skip spawn.");
            isSpawning = false;
            return spawnedEnemies;
        }

        ResolvePlayerReference();
        SubscribePlayerHealth();

        if (playerTransform == null)
            Debug.LogWarning("[TutorialEnemySpawner] Player tidak ditemukan — " +
                             "deteksi crash hanya pakai jarak (mungkin kurang akurat).");

        Transform parent = enemyParent != null ? enemyParent : transform;

        for (int i = 0; i < count; i++)
        {
            EnemyGestureCommand enemy = SpawnSingleEnemy(i, count, center, radius, parent, enemyData, aksara);

            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);

                int id = enemy.gameObject.GetInstanceID();
                enemiesById[id] = enemy;
                lastKnownPositions[id] = enemy.transform.position;
            }
            else
            {
                Debug.LogWarning($"[TutorialEnemySpawner] Gagal spawn musuh ke-{i}.");
            }
        }

        if (spawnedEnemies.Count != expectedKillCount)
        {
            Debug.LogWarning($"[TutorialEnemySpawner] Expected {expectedKillCount} musuh, " +
                             $"tapi cuma {spawnedEnemies.Count} yang berhasil spawn. " +
                             "Expected count disesuaikan.");
            expectedKillCount = spawnedEnemies.Count;
        }

        isSpawning = false;

        if (spawnedEnemies.Count == 0)
        {
            Debug.LogWarning("[TutorialEnemySpawner] Tidak ada musuh yang berhasil spawn — skip tracking.");
            return spawnedEnemies;
        }

        RestartDeathTracker();
        return spawnedEnemies;
    }

    public void ActivateAll()
    {
        foreach (var enemy in spawnedEnemies)
            if (enemy != null)
                SetEnemyActive(enemy, true);
    }

    public void SetSpeed(float speed)
    {
        foreach (var enemy in spawnedEnemies)
            if (enemy != null)
                SetEnemySpeed(enemy, speed);
    }

    public List<SpriteRenderer> GetAllRenderers()
    {
        var result = new List<SpriteRenderer>();
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy == null) continue;
            SpriteRenderer sr = enemy.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) result.Add(sr);
        }
        return result;
    }

    public void Clear()
    {
        StopDeathTracker();

        foreach (var enemy in spawnedEnemies)
            if (enemy != null) Destroy(enemy.gameObject);

        spawnedEnemies.Clear();
        enemiesById.Clear();
        lastKnownPositions.Clear();
        countedEnemyIds.Clear();

        hasFiredCallback = true;
        isSpawning = false;
    }
    #endregion

    #region Player Reference
    private void ResolvePlayerReference()
    {
        if (playerTransform == null)
        {
            GameObject p = null;
            try { p = GameObject.FindGameObjectWithTag("Player"); } catch { }

            if (p != null)
                playerTransform = p.transform;
        }

        if (playerTransform == null)
        {
            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
            {
                playerTransform = ph.transform;
                Debug.Log("[TutorialEnemySpawner] Player ditemukan via PlayerHealth fallback.");
            }
        }

        if (trackedPlayerHealth == null && playerTransform != null)
            trackedPlayerHealth = playerTransform.GetComponent<PlayerHealth>();
    }

    private void SubscribePlayerHealth()
    {
        if (trackedPlayerHealth == null) return;
        trackedPlayerHealth.DamageTaken -= OnPlayerDamaged;
        trackedPlayerHealth.DamageTaken += OnPlayerDamaged;
    }

    private void UnsubscribePlayerHealth()
    {
        if (trackedPlayerHealth != null)
            trackedPlayerHealth.DamageTaken -= OnPlayerDamaged;
    }

    private void OnPlayerDamaged(int amount)
    {
        lastPlayerDamageTime = Time.unscaledTime;

        if (debugDeathTrack)
            Debug.Log($"[TutorialEnemySpawner] Player take damage {amount}. " +
                      $"Damage window aktif {crashDamageWindow}s.");
    }
    #endregion

    #region Spawn Implementation
    private EnemyGestureCommand SpawnSingleEnemy(
        int index, int totalCount, Vector3 center, float radius,
        Transform parent, EnemyData enemyData, AksaraData aksara)
    {
        Vector3 position = GetSpawnPosition(index, totalCount, center, radius);

        EnemyGestureCommand enemy = Instantiate(enemyPrefab, position, Quaternion.identity, parent);
        if (enemy == null) return null;

        enemy.name = $"TutorialEnemy_{index}";
        ApplyIsolationTagAndLayer(enemy.gameObject);

        enemy.SetAutoIssueOnStart(false);

        ConfigureEnemy(enemy, enemyData, aksara);
        ConfigureMovement(enemy, position);
        enemy.SyncSpawnPosition();

        enemy.IssueCommand();

        return enemy;
    }

    private Vector3 GetSpawnPosition(int index, int totalCount, Vector3 center, float radius)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[index % spawnPoints.Length];
            if (point != null)
            {
                if (debugSpawn)
                    Debug.Log($"[TutorialEnemySpawner] Spawn #{index} → SpawnPoint");
                return point.position;
            }
        }

        if (useSpawnArea)
        {
            Vector3 pos = GetAreaSpawnPosition(index, totalCount);
            if (debugSpawn)
                Debug.Log($"[TutorialEnemySpawner] Spawn #{index} → SpawnArea = {pos}");
            return pos;
        }

        float angle = (index / (float)Mathf.Max(1, totalCount)) * Mathf.PI * 2f;
        Vector3 circularPos = center + new Vector3(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius,
            0f);

        if (debugSpawn)
            Debug.Log($"[TutorialEnemySpawner] Spawn #{index} → Circular = {circularPos}");

        return circularPos;
    }

    private Vector3 GetAreaSpawnPosition(int index, int totalEnemies)
    {
        totalEnemies = Mathf.Max(1, totalEnemies);

        float areaMinX = spawnAreaCenter.x - spawnAreaSize.x * 0.5f;
        float segmentWidth = spawnAreaSize.x / totalEnemies;
        float segmentMinX = areaMinX + segmentWidth * index;

        float x = segmentMinX + UnityEngine.Random.Range(0f, segmentWidth);
        float y = spawnAreaCenter.y + UnityEngine.Random.Range(
            -spawnAreaSize.y * 0.5f,
            spawnAreaSize.y * 0.5f);

        return new Vector3(x, y, 0f);
    }

    private void ApplyIsolationTagAndLayer(GameObject enemyGO)
    {
        if (!string.IsNullOrEmpty(tutorialTag) && IsTagRegistered(tutorialTag))
            enemyGO.tag = tutorialTag;

        if (!string.IsNullOrEmpty(tutorialLayer))
        {
            int layer = LayerMask.NameToLayer(tutorialLayer);
            if (layer != -1) enemyGO.layer = layer;
        }
    }

    private static bool IsTagRegistered(string tag)
    {
        try { GameObject.FindWithTag(tag); return true; }
        catch { return false; }
    }

    private static void ConfigureEnemy(EnemyGestureCommand enemy, EnemyData enemyData, AksaraData aksara)
    {
        Enemy enemyComponent = enemy.GetComponent<Enemy>();

        if (enemyComponent != null && enemyData != null)
            enemyComponent.Configure(enemyData, aksara);
        else if (aksara != null)
            enemy.ConfigureChallenge(aksara.GestureShape, 1);
    }

    private static void ConfigureMovement(EnemyGestureCommand enemy, Vector3 spawnPos)
    {
        var movement = enemy.GetComponent<EnemyMovementBehavior>()
                    ?? enemy.GetComponentInChildren<EnemyMovementBehavior>(true);
        if (movement == null) return;

        movement.SetSpawnPosition(spawnPos);
        movement.SetActive(false);
        movement.SetMovementPaused(false);
    }

    private static void SetEnemyActive(EnemyGestureCommand enemy, bool active)
    {
        var movement = enemy.GetComponent<EnemyMovementBehavior>()
                    ?? enemy.GetComponentInChildren<EnemyMovementBehavior>(true);
        if (movement == null) return;

        movement.SetActive(active);
        movement.SetMovementPaused(false);
    }

    private static void SetEnemySpeed(EnemyGestureCommand enemy, float speed)
    {
        var movement = enemy.GetComponent<EnemyMovementBehavior>()
                    ?? enemy.GetComponentInChildren<EnemyMovementBehavior>(true);
        if (movement != null) movement.SetSpeedFromData(speed);
    }
    #endregion

    #region Death Tracking
    private void RestartDeathTracker()
    {
        StopDeathTracker();
        if (spawnedEnemies.Count == 0) return;
        deathTrackerRoutine = StartCoroutine(TrackDeathsRoutine());
    }

    private void StopDeathTracker()
    {
        if (deathTrackerRoutine != null)
        {
            StopCoroutine(deathTrackerRoutine);
            deathTrackerRoutine = null;
        }
    }

    private IEnumerator TrackDeathsRoutine()
    {
        while (true)
        {
            UpdateLastKnownPositions();
            DetectDestroyedEnemies();

            if (enemiesById.Count == 0)
            {
                HandleAllEnemiesGone();
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateLastKnownPositions()
    {
        foreach (var kvp in enemiesById)
        {
            EnemyGestureCommand enemy = kvp.Value;
            if (enemy != null)
                lastKnownPositions[kvp.Key] = enemy.transform.position;
        }
    }

    private void DetectDestroyedEnemies()
    {
        List<int> destroyedIds = new List<int>();

        foreach (var kvp in enemiesById)
        {
            int id = kvp.Key;
            EnemyGestureCommand enemy = kvp.Value;

            if (enemy == null)
            {
                destroyedIds.Add(id);

                // Guard: cegah double-count musuh yang sama
                if (!countedEnemyIds.Contains(id))
                {
                    EvaluateEnemyDeath(id);
                    countedEnemyIds.Add(id);
                }
            }
        }

        foreach (int id in destroyedIds)
        {
            enemiesById.Remove(id);
            lastKnownPositions.Remove(id);
        }
    }

    private void EvaluateEnemyDeath(int id)
    {
        Vector3 lastPos = lastKnownPositions.ContainsKey(id)
            ? lastKnownPositions[id]
            : Vector3.zero;

        bool playerJustDamaged =
            (Time.unscaledTime - lastPlayerDamageTime) <= crashDamageWindow;

        bool nearPlayer = IsNearPlayer(lastPos);

        bool isCrash = playerJustDamaged || nearPlayer;

        if (debugDeathTrack)
        {
            float dist = playerTransform != null
                ? Vector3.Distance(lastPos, playerTransform.position)
                : -1f;

            Debug.Log($"[TutorialEnemySpawner] Enemy #{id} — " +
                      $"playerJustDamaged={playerJustDamaged}, " +
                      $"nearPlayer={nearPlayer} (dist={dist:F2}) " +
                      $"→ {(isCrash ? "NABRAK" : "DI-KILL")}");
        }

        if (isCrash)
            totalCrashCount++;
        else
            validKillCount++;
    }

    private bool IsNearPlayer(Vector3 position)
    {
        if (playerTransform == null) return false;

        float distance = Vector3.Distance(
            new Vector3(position.x, position.y, 0f),
            new Vector3(playerTransform.position.x, playerTransform.position.y, 0f));

        return distance < crashDetectionRadius;
    }

    private void HandleAllEnemiesGone()
    {
        if (hasFiredCallback)
        {
            Debug.Log("[TutorialEnemySpawner] Callback sudah pernah fire — skip.");
            return;
        }
        hasFiredCallback = true;

        if (debugDeathTrack)
        {
            Debug.Log($"[TutorialEnemySpawner] Semua musuh hilang. " +
                      $"Valid kill: {validKillCount}/{expectedKillCount}, " +
                      $"Crash: {totalCrashCount}/{expectedKillCount}");
        }

        // === SEMUA DI-KILL → SUCCESS ===
        if (validKillCount >= expectedKillCount)
        {
            Debug.Log("[TutorialEnemySpawner] ✅ Semua musuh di-kill player → SUCCESS.");
            onAllKilledCallback?.Invoke();
            return;
        }

        // === SEMUA NABRAK → GAGAL ===
        if (totalCrashCount >= expectedKillCount)
        {
            Debug.LogWarning("[TutorialEnemySpawner] ❌ Semua musuh nabrak player → GAGAL.");
            onAllCrashedCallback?.Invoke();
            return;
        }

        // === MIX (sebagian kill, sebagian crash) → GAGAL ===
        Debug.LogWarning($"[TutorialEnemySpawner] ⚠️ MIX (kill={validKillCount}, " +
                         $"crash={totalCrashCount}) → GAGAL, panel MULAI MAIN? muncul.");
        onAllCrashedCallback?.Invoke();
    }
    #endregion
}