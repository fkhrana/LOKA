using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Debug")]
    [SerializeField] private bool debugSpawn = false;
    [SerializeField] private bool debugDeathTrack = true;
    #endregion

    #region Runtime State
    private readonly List<EnemyGestureCommand> spawnedEnemies = new List<EnemyGestureCommand>();
    private readonly Dictionary<int, EnemyGestureCommand> enemiesById = new Dictionary<int, EnemyGestureCommand>();
    private readonly HashSet<int> countedEnemyIds = new HashSet<int>();
    private readonly HashSet<int> enemiesThatDamagedPlayer = new HashSet<int>();
    private readonly Dictionary<int, EnemyMovementBehavior> trackedEnemyMovements = new Dictionary<int, EnemyMovementBehavior>();
    private readonly Dictionary<int, Action> enemyDamageHandlers = new Dictionary<int, Action>();

    private Coroutine deathTrackerRoutine;
    private int expectedKillCount = 0;
    private int validKillCount = 0;
    private int totalCrashCount = 0;

    private Action onAllKilledCallback;
    private Action onAllCrashedCallback;

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
        UnsubscribeEnemyDamageHandlers();
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

        UnsubscribeEnemyDamageHandlers();
        spawnedEnemies.Clear();
        enemiesById.Clear();
        countedEnemyIds.Clear();
        enemiesThatDamagedPlayer.Clear();

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

        Transform parent = enemyParent != null ? enemyParent : transform;

        for (int i = 0; i < count; i++)
        {
            EnemyGestureCommand enemy = SpawnSingleEnemy(i, count, center, radius, parent, enemyData, aksara);

            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);

                int id = enemy.gameObject.GetInstanceID();
                enemiesById[id] = enemy;
                TrackEnemyDamage(id, enemy);
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
        UnsubscribeEnemyDamageHandlers();

        foreach (var enemy in spawnedEnemies)
            if (enemy != null) Destroy(enemy.gameObject);

        spawnedEnemies.Clear();
        enemiesById.Clear();
        countedEnemyIds.Clear();
        enemiesThatDamagedPlayer.Clear();

        hasFiredCallback = true;
        isSpawning = false;
    }
    #endregion

    #region Enemy Contact Tracking
    private void TrackEnemyDamage(int id, EnemyGestureCommand enemy)
    {
        EnemyMovementBehavior movement = enemy.GetComponent<EnemyMovementBehavior>()
            ?? enemy.GetComponentInChildren<EnemyMovementBehavior>(true);
        if (movement == null) return;

        Action handler = () => enemiesThatDamagedPlayer.Add(id);
        movement.PlayerDamagedByContact += handler;
        trackedEnemyMovements[id] = movement;
        enemyDamageHandlers[id] = handler;
    }

    private void UntrackEnemyDamage(int id)
    {
        if (trackedEnemyMovements.TryGetValue(id, out EnemyMovementBehavior movement) &&
            enemyDamageHandlers.TryGetValue(id, out Action handler) &&
            movement != null)
        {
            movement.PlayerDamagedByContact -= handler;
        }

        trackedEnemyMovements.Remove(id);
        enemyDamageHandlers.Remove(id);
    }

    private void UnsubscribeEnemyDamageHandlers()
    {
        foreach (var pair in enemyDamageHandlers)
        {
            if (trackedEnemyMovements.TryGetValue(pair.Key, out EnemyMovementBehavior movement) &&
                movement != null)
            {
                movement.PlayerDamagedByContact -= pair.Value;
            }
        }

        enemyDamageHandlers.Clear();
        trackedEnemyMovements.Clear();
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
        enemy.SetAllowMovementBeforeGameStarted(true);

        // === FIX: musuh tutorial TIDAK lapor progress ke LevelProgressManager ===
        enemy.SetReportProgress(false);

        ConfigureEnemy(enemy, enemyData, aksara);
        enemy.GetComponent<Enemy>()?.SetDropEnabled(false);
        ConfigureMovement(enemy, position);   // ← instance method (diubah dari static)
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

    /// <summary>
    /// Setup movement tutorial enemy.
    /// FIX PENTING: panggil Initialize() supaya playerHealth, collider,
    /// spriteRenderer, dan Rigidbody2D ter-set. Tanpa ini,
    /// HandlePlayerContact() akan bail out karena playerHealth == null
    /// sehingga Shield knockback & damage TIDAK bekerja.
    /// </summary>
    private void ConfigureMovement(EnemyGestureCommand enemy, Vector3 spawnPos)
    {
        var movement = enemy.GetComponent<EnemyMovementBehavior>()
                    ?? enemy.GetComponentInChildren<EnemyMovementBehavior>(true);

        if (movement == null)
        {
            Debug.LogWarning($"[TutorialEnemySpawner] {enemy.name} tidak punya " +
                             "EnemyMovementBehavior — knockback/damage tidak akan jalan.");
            return;
        }

        // === FIX UTAMA: panggil Initialize() ===
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

        Collider2D enemyCollider = enemy.GetComponent<Collider2D>()
                                ?? enemy.GetComponentInChildren<Collider2D>(true);

        SpriteRenderer spriteRenderer = enemy.GetComponentInChildren<SpriteRenderer>(true);

        if (playerHealth == null)
            Debug.LogWarning("[TutorialEnemySpawner] ⚠️ PlayerHealth tidak ditemukan — " +
                             "damage & shield knockback TIDAK akan bekerja!");

        if (enemyCollider == null)
            Debug.LogWarning($"[TutorialEnemySpawner] ⚠️ {enemy.name} tidak punya Collider2D — " +
                             "contact event tidak akan fire!");

        movement.Initialize(playerHealth, enemyCollider, spriteRenderer);

        // Setup setelah Initialize
        movement.SetSpawnPosition(spawnPos);
        movement.SetActive(false);
        movement.SetMovementPaused(false);

        if (debugSpawn)
            Debug.Log($"[TutorialEnemySpawner] {enemy.name} Initialize OK " +
                      $"(playerHealth={(playerHealth != null)}, " +
                      $"collider={(enemyCollider != null)})");
    }

    private void SetEnemyActive(EnemyGestureCommand enemy, bool active)
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
            DetectDestroyedEnemies();

            if (enemiesById.Count == 0)
            {
                HandleAllEnemiesGone();
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
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
            UntrackEnemyDamage(id);
        }
    }

    private void EvaluateEnemyDeath(int id)
    {
        bool isCrash = enemiesThatDamagedPlayer.Contains(id);

        if (debugDeathTrack)
        {
            Debug.Log($"[TutorialEnemySpawner] Enemy #{id} — " +
                      $"damagedPlayer={isCrash} → {(isCrash ? "NABRAK" : "DI-KILL")}");
        }

        if (isCrash)
            totalCrashCount++;
        else
            validKillCount++;
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

        if (validKillCount >= expectedKillCount)
        {
            Debug.Log("[TutorialEnemySpawner] ✅ Semua musuh di-kill player → SUCCESS.");
            onAllKilledCallback?.Invoke();
            return;
        }

        if (totalCrashCount >= expectedKillCount)
        {
            Debug.LogWarning("[TutorialEnemySpawner] ❌ Semua musuh nabrak player → GAGAL.");
            onAllCrashedCallback?.Invoke();
            return;
        }

        Debug.LogWarning($"[TutorialEnemySpawner] ⚠️ MIX (kill={validKillCount}, " +
                         $"crash={totalCrashCount}) → GAGAL, panel MULAI MAIN? muncul.");
        onAllCrashedCallback?.Invoke();
    }
    #endregion
}