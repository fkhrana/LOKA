using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public EnemyData enemyData;
    public AksaraData aksaraData;
    [Range(0f, 1f)] public float weight = 1f;
}

[System.Serializable]
public class EnemyWaveGroup
{
    public string groupName = "Group";
    public EnemyData enemyData;
    public AksaraData aksaraData;
    [Min(1)] public int enemyCount = 5;
}

[System.Serializable]
public class EnemyWaveDefinition
{
    public string waveName = "Wave";
    [Min(0f)] public float delayBeforeSpawn = 0f;
    public List<EnemyWaveGroup> groups = new List<EnemyWaveGroup>();
}

public class EnemyWaveSpawner : MonoBehaviour
{
    [SerializeField] private EnemyGestureCommand enemyPrefab;
    [SerializeField] private BossEnemy bossPrefab;
    [SerializeField] private bool spawnBossOnStart = false;
    [SerializeField] private bool bossOnlyMode = false;
    [Header("Boss Spawn")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform bossStopPoint;
    [SerializeField] private GameObject bossGameplayHudCanvas;
    [SerializeField] private CameraShake bossCameraShakeEffect;
    [SerializeField] private Transform bossProgressStarTarget;
    [SerializeField] private List<AksaraData> bossAksaraPool = new List<AksaraData>();
    [SerializeField, Min(0)] private int enemyCount = 0;
    [SerializeField] private bool spawnOnStart = true;
    [Header("Debug")]
    [SerializeField] private bool startFromWave2OnStart = false;
    [SerializeField] private GameObject waveTransitionBanner;
    [SerializeField, Min(0.1f)] private float waveTransitionBannerDuration = 1.5f;

    [Header("Wave Info SFX")]
    [SerializeField] private bool useWaveInfoSFX = true;
    [SerializeField] private string waveInfoSFXName = "WaveInfo";

    [Range(0f, 1f)]
    [SerializeField] private float waveInfoSFXVolume = 1f;

    [SerializeField] private bool useSpawnArea = false;
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(4f, 4f);
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform spawnedParent;
    [SerializeField] private EnemySpawnEntry[] spawnEntries;
    [SerializeField] private List<EnemyWaveDefinition> waves = new List<EnemyWaveDefinition>();
    [SerializeField, Min(0.1f)] private float delayBetweenWaves = 2f;
    [SerializeField, Min(1)] private int initialConcurrentEnemies = 3;
    [SerializeField, Min(0.1f)] private float staggerSpawnInterval = 0.6f;
    [SerializeField] private GestureShape[] availableAksaraGestures =
        { GestureShape.Na, GestureShape.Ka };

    [SerializeField, Min(1)] private int requiredCorrectGestures = 1;
    [SerializeField, Min(0.1f)] private float minSpawnDistance = 1f;
    [SerializeField, Min(0.1f)] private float maxSpawnDistance = 3f;

    [Header("Fail-safe")]
    [Tooltip("Buffer tambahan (detik) di atas estimasi durasi intro CameraIntroManager, sebelum GameStarted dipaksa true. " +
             "Dalam kondisi normal fail-safe ini tidak akan pernah terpakai; hanya jaga-jaga kalau CameraIntroManager error/hilang.")]
    [SerializeField, Min(1f)] private float gameStartedFailSafeBuffer = 5f;

    private readonly List<EnemyGestureCommand> spawnedEnemies =
        new List<EnemyGestureCommand>();

    private readonly List<EnemyGestureCommand> currentWaveEnemies =
        new List<EnemyGestureCommand>();

    private Coroutine waveSequenceCoroutine;
    private bool bossProgressInitialized;

    private int currentWaveIndex = -1;

    public IReadOnlyList<EnemyGestureCommand> SpawnedEnemies =>
        spawnedEnemies;

    public int CurrentWaveIndex =>
        currentWaveIndex;

    private void OnValidate()
    {
        if (maxSpawnDistance < minSpawnDistance)
            maxSpawnDistance = minSpawnDistance;
    }

    private void Start()
    {
        StartCoroutine(FailSafeGameStarted());

        if (startFromWave2OnStart)
        {
            StartFromWave(1);
            return;
        }

        if (bossOnlyMode && spawnBossOnStart)
        {
            SpawnBoss();
            return;
        }

        string savedState =
            GameProgressManager.GetGameState();

        if (savedState == "Puzzle" ||
            savedState == "Reward")
        {
            Debug.Log(
                "[EnemyWaveSpawner] Resume " +
                savedState +
                " → wave tidak dijalankan."
            );

            return;
        }

        if (savedState == "Gameplay")
        {
            Debug.Log(
                "[EnemyWaveSpawner] State Gameplay → mulai ulang dari wave 1."
            );

            if (spawnOnStart)
                StartWaveSequence();

            return;
        }

        if (spawnOnStart)
            StartWaveSequence();

        if (spawnBossOnStart)
            SpawnBoss();
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("EnemyWaveSpawner: bossPrefab belum di-assign.");
            return;
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogWarning("EnemyWaveSpawner: bossSpawnPoint belum di-assign.");
            return;
        }

        Vector3 spawnPosition = bossSpawnPoint.position;
        Transform parent = spawnedParent != null ? spawnedParent : transform;
        BossEnemy boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity, parent);
        boss.ConfigureStopPosition(bossStopPoint != null ? bossStopPoint.position : spawnPosition);
        boss.ConfigureDefeatPresentation(
            bossGameplayHudCanvas,
            bossCameraShakeEffect
        );

        if (bossOnlyMode)
        {
            LevelProgressManager.Instance?.SetLevelBarTarget(bossProgressStarTarget);
            InitializeBossProgress(boss.ProgressUnits);
        }

        boss.ConfigureAksaraPool(bossAksaraPool);
        boss.SyncSpawnPosition();
        boss.BeginBossFight();
    }

    private void InitializeBossProgress(int progressUnits)
    {
        if (bossProgressInitialized)
            return;

        LevelProgressManager.Instance?.Initialize(progressUnits, null);
        bossProgressInitialized = true;
    }

    private IEnumerator FailSafeGameStarted()
    {
        // Hitung estimasi durasi intro terpanjang (skenario fresh start dengan panning) secara dinamis
        // dari CameraIntroManager.Instance, supaya tidak salah tembak kalau nilai jeda diubah di Inspector.
        float estimatedIntroDuration = 12f; // fallback kalau CameraIntroManager tidak ditemukan

        if (CameraIntroManager.Instance != null)
        {
            var intro = CameraIntroManager.Instance;

            float panningDuration =
                intro.jedaAwal +
                (intro.durasiPan * 2f) +
                intro.jedaLihatMusuh;

            float countdownDuration = 4f;

            estimatedIntroDuration = panningDuration + countdownDuration;
        }

        float timeout = estimatedIntroDuration + gameStartedFailSafeBuffer;

        Debug.Log($"[EnemyWaveSpawner] Fail-safe GameStarted aktif dalam {timeout:F1} detik jika belum true.");

        yield return new WaitForSeconds(timeout);

        if (!CameraIntroManager.GameStarted)
        {
            if (CameraIntroManager.Instance == null)
            {
                Debug.LogWarning(
                    "[EnemyWaveSpawner] CameraIntroManager.Instance tidak ditemukan setelah " +
                    timeout +
                    " detik — GameStarted dipaksa true agar musuh tidak macet permanen."
                );
            }
            else
            {
                Debug.LogWarning(
                    "[EnemyWaveSpawner] GameStarted masih false setelah " +
                    timeout +
                    " detik meskipun CameraIntroManager ada — kemungkinan intro macet/error. " +
                    "GameStarted dipaksa true sebagai upaya terakhir."
                );
            }

            CameraIntroManager.GameStarted = true;
        }
    }

    public void StartWaveSequence()
    {
        StartFromWave(0);
    }

    public void StartFromWave(int waveIndex)
    {
        StopWaveSequence();

        ClearSpawnedEnemies();

        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                "EnemyWaveSpawner: enemyPrefab belum di-assign."
            );

            return;
        }

        if (waves == null ||
            waves.Count == 0)
        {
            LevelProgressManager.Instance?.Initialize(
                enemyCount,
                null
            );

            currentWaveIndex = 0;

            GameProgressManager.SaveWaveIndex(
                currentWaveIndex
            );

            SpawnWave();

            return;
        }

        int totalLevelEnemies = 0;

        var milestones =
            new List<int>();

        for (int w = 0;
             w < waves.Count;
             w++)
        {
            var wave = waves[w];

            if (wave == null ||
                wave.groups == null)
                continue;

            int waveTotal = 0;

            for (int gi = 0;
                 gi < wave.groups.Count;
                 gi++)
            {
                var g =
                    wave.groups[gi];

                if (g != null)
                {
                    waveTotal +=
                        Mathf.Max(
                            0,
                            g.enemyCount
                        );
                }
            }

            totalLevelEnemies +=
                waveTotal;

            milestones.Add(
                totalLevelEnemies
            );
        }

        waveIndex = Mathf.Clamp(waveIndex, 0, waves.Count - 1);

        LevelProgressManager.Instance?.Initialize(
            totalLevelEnemies,
            milestones,
            false,
            CountEnemiesBeforeWave(waveIndex)
        );

        currentWaveIndex = waveIndex - 1;

        currentWaveEnemies.Clear();

        GameProgressManager.SaveWaveIndex(
            waveIndex
        );

        waveSequenceCoroutine =
            StartCoroutine(
                SpawnWaveSequenceRoutine(waveIndex)
            );
    }

    public void ResumeWaveSequence()
    {
        StopWaveSequence();

        ClearSpawnedEnemies();

        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                "EnemyWaveSpawner: enemyPrefab belum di-assign."
            );

            return;
        }

        if (waves == null ||
            waves.Count == 0)
        {
            currentWaveIndex = 0;

            LevelProgressManager.Instance?.Initialize(
                enemyCount,
                null,
                true
            );

            SpawnWave();

            return;
        }

        int totalLevelEnemies = 0;

        var milestones =
            new List<int>();

        for (int w = 0;
             w < waves.Count;
             w++)
        {
            var wave =
                waves[w];

            if (wave == null ||
                wave.groups == null)
                continue;

            int waveTotal = 0;

            for (int gi = 0;
                 gi < wave.groups.Count;
                 gi++)
            {
                var group =
                    wave.groups[gi];

                if (group != null)
                {
                    waveTotal +=
                        Mathf.Max(
                            0,
                            group.enemyCount
                        );
                }
            }

            totalLevelEnemies +=
                waveTotal;

            milestones.Add(
                totalLevelEnemies
            );
        }

        LevelProgressManager.Instance?.Initialize(
            totalLevelEnemies,
            milestones,
            true
        );

        int savedWave =
            GameProgressManager.GetWaveIndex();

        currentWaveIndex =
            Mathf.Clamp(
                savedWave,
                0,
                waves.Count - 1
            );

        Debug.Log(
            "[EnemyWaveSpawner] Resume → Wave " +
            (currentWaveIndex + 1)
        );

        waveSequenceCoroutine =
            StartCoroutine(
                ResumeWaveSequenceRoutine()
            );
    }

    private int CountEnemiesBeforeWave(int waveIndex)
    {
        int completedEnemies = 0;

        for (int i = 0; i < waveIndex && i < waves.Count; i++)
        {
            EnemyWaveDefinition wave = waves[i];

            if (wave == null || wave.groups == null)
                continue;

            for (int groupIndex = 0; groupIndex < wave.groups.Count; groupIndex++)
            {
                EnemyWaveGroup group = wave.groups[groupIndex];

                if (group != null)
                    completedEnemies += Mathf.Max(0, group.enemyCount);
            }
        }

        return completedEnemies;
    }

    public void StopWaveSequence()
    {
        if (waveSequenceCoroutine != null)
        {
            StopCoroutine(
                waveSequenceCoroutine
            );

            waveSequenceCoroutine = null;
        }
    }

    public void SaveCurrentWave()
    {
        if (currentWaveIndex < 0)
            return;

        GameProgressManager.SaveWaveIndex(
            currentWaveIndex
        );

        GameProgressManager.SaveGameState(
            "Gameplay"
        );

        Debug.Log(
            "[EnemyWaveSpawner] Progress gameplay disimpan → Wave " +
            (currentWaveIndex + 1)
        );
    }

    public void SpawnWave()
    {
        ClearSpawnedEnemies();

        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                "EnemyWaveSpawner: enemyPrefab belum di-assign."
            );

            return;
        }

        currentWaveEnemies.Clear();

        LevelProgressManager.Instance?.Initialize(
            enemyCount,
            null
        );

        var usedPositions =
            new List<Vector3>();

        for (int i = 0;
             i < enemyCount;
             i++)
        {
            SpawnEnemyFromLegacyConfig(
                i,
                usedPositions
            );
        }

        if (spawnedEnemies.Count != enemyCount)
        {
            Debug.LogWarning(
                $"EnemyWaveSpawner: expected {enemyCount} enemies but spawned {spawnedEnemies.Count}."
            );
        }
    }

    public void ClearSpawnedEnemies()
    {
        for (int i = 0;
             i < spawnedEnemies.Count;
             i++)
        {
            if (spawnedEnemies[i] != null)
            {
                Destroy(
                    spawnedEnemies[i].gameObject
                );
            }
        }

        spawnedEnemies.Clear();

        currentWaveEnemies.Clear();
    }

    private IEnumerator SpawnWaveSequenceRoutine(int startWaveIndex)
    {
        if (waves == null ||
            waves.Count == 0)
        {
            SpawnWave();

            yield break;
        }

        for (int waveIndex = startWaveIndex;
             waveIndex < waves.Count;
             waveIndex++)
        {
            currentWaveIndex =
                waveIndex;

            GameProgressManager.SaveWaveIndex(
                waveIndex
            );

            LevelProgressManager.Instance?.SetLevelBarTargetForWave(
                waveIndex
            );

            EnemyWaveDefinition wave =
                waves[waveIndex];

            if (wave != null &&
                wave.delayBeforeSpawn > 0f)
            {
                yield return new WaitForSeconds(
                    wave.delayBeforeSpawn
                );
            }

            yield return StartCoroutine(
                SpawnWaveDefinitionRoutine(
                    wave,
                    waveIndex
                )
            );

            if (waveIndex < waves.Count - 1)
            {
                yield return StartCoroutine(
                    WaitForWaveToClearRoutine(
                        delayBetweenWaves
                    )
                );

                yield return StartCoroutine(
                    ShowWaveTransitionBannerRoutine(
                        waveIndex + 2
                    )
                );
            }
            else
            {
                yield return StartCoroutine(
                    WaitForWaveToClearRoutine(
                        delayBetweenWaves
                    )
                );

                if (PuzzleManager.Instance != null &&
                    !PuzzleManager.Instance.IsPuzzleCompleted())
                {
                    yield return StartCoroutine(
                        PuzzleManager.Instance.PlayWaveCompleteSequence()
                    );

                    yield return StartCoroutine(
                        WaitForPuzzleCompletionRoutine()
                    );
                }
            }
        }

        currentWaveIndex =
            waves.Count;

        GameProgressManager.SaveWaveIndex(
            currentWaveIndex
        );
    }

    private IEnumerator ResumeWaveSequenceRoutine()
    {
        for (int waveIndex = currentWaveIndex;
             waveIndex < waves.Count;
             waveIndex++)
        {
            currentWaveIndex =
                waveIndex;

            GameProgressManager.SaveWaveIndex(
                waveIndex
            );

            LevelProgressManager.Instance?.SetLevelBarTargetForWave(
                waveIndex
            );

            EnemyWaveDefinition wave =
                waves[waveIndex];

            Debug.Log(
                "[EnemyWaveSpawner] Resume spawn → " +
                wave.waveName
            );

            if (wave != null &&
                wave.delayBeforeSpawn > 0f)
            {
                yield return new WaitForSeconds(
                    wave.delayBeforeSpawn
                );
            }

            yield return StartCoroutine(
                SpawnWaveDefinitionRoutine(
                    wave,
                    waveIndex
                )
            );

            if (waveIndex < waves.Count - 1)
            {
                yield return StartCoroutine(
                    WaitForWaveToClearRoutine(
                        delayBetweenWaves
                    )
                );

                yield return StartCoroutine(
                    ShowWaveTransitionBannerRoutine(
                        waveIndex + 2
                    )
                );
            }
            else
            {
                yield return StartCoroutine(
                    WaitForWaveToClearRoutine(
                        delayBetweenWaves
                    )
                );

                if (PuzzleManager.Instance != null &&
                    !PuzzleManager.Instance.IsPuzzleCompleted())
                {
                    yield return StartCoroutine(
                        PuzzleManager.Instance.PlayWaveCompleteSequence()
                    );

                    yield return StartCoroutine(
                        WaitForPuzzleCompletionRoutine()
                    );
                }
            }
        }

        currentWaveIndex =
            waves.Count;

        GameProgressManager.SaveWaveIndex(
            currentWaveIndex
        );
    }

    private IEnumerator ShowWaveTransitionBannerRoutine(
        int nextWaveNumber
    )
    {
        if (waveTransitionBanner == null)
            yield break;

        waveTransitionBanner.SetActive(true);

        var canvasGroup =
            waveTransitionBanner.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (useWaveInfoSFX &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                waveInfoSFXName,
                waveInfoSFXVolume
            );
        }

        yield return new WaitForSeconds(
            waveTransitionBannerDuration
        );

        waveTransitionBanner.SetActive(false);
    }

    private IEnumerator SpawnWaveDefinitionRoutine(
        EnemyWaveDefinition wave,
        int waveIndex
    )
    {
        if (wave == null)
            yield break;

        currentWaveEnemies.Clear();

        int totalEnemiesInWave = 0;

        var usedPositions =
            new List<Vector3>();

        if (wave.groups == null ||
            wave.groups.Count == 0)
        {
            Debug.LogWarning(
                $"EnemyWaveSpawner: wave '{wave.waveName}' has no groups configured."
            );

            yield break;
        }

        for (int groupIndex = 0;
             groupIndex < wave.groups.Count;
             groupIndex++)
        {
            EnemyWaveGroup group =
                wave.groups[groupIndex];

            if (group == null ||
                group.enemyCount <= 0)
                continue;

            totalEnemiesInWave +=
                group.enemyCount;
        }

        Debug.Log(
            $"[EnemyWaveSpawner] Spawning {wave.waveName} ({waveIndex + 1}/{waves.Count})"
        );

        int spawnedCount = 0;

        int initialBatchSize =
            Mathf.Min(
                initialConcurrentEnemies,
                totalEnemiesInWave
            );

        var remainingGroupCounts =
            new List<int>();

        for (int groupIndex = 0;
             groupIndex < wave.groups.Count;
             groupIndex++)
        {
            EnemyWaveGroup group =
                wave.groups[groupIndex];

            if (group == null ||
                group.enemyCount <= 0)
            {
                remainingGroupCounts.Add(0);
            }
            else
            {
                remainingGroupCounts.Add(
                    group.enemyCount
                );
            }
        }

        int nextGroupIndex = 0;

        for (int i = 0;
             i < initialBatchSize;
             i++)
        {
            if (spawnedCount >= totalEnemiesInWave)
                break;

            EnemyWaveGroup selectedGroup =
                GetNextMixedGroup(
                    wave,
                    remainingGroupCounts,
                    ref nextGroupIndex
                );

            if (selectedGroup == null)
                break;

            SpawnEnemy(
                selectedGroup.enemyData,
                selectedGroup.aksaraData,
                spawnedCount,
                totalEnemiesInWave,
                usedPositions
            );

            spawnedCount++;
        }

        while (spawnedCount < totalEnemiesInWave)
        {
            RefreshCurrentWaveEnemies();

            if (currentWaveEnemies.Count < initialBatchSize)
            {
                EnemyWaveGroup selectedGroup =
                    GetNextMixedGroup(
                        wave,
                        remainingGroupCounts,
                        ref nextGroupIndex
                    );

                if (selectedGroup == null)
                    break;

                SpawnEnemy(
                    selectedGroup.enemyData,
                    selectedGroup.aksaraData,
                    spawnedCount,
                    totalEnemiesInWave,
                    usedPositions
                );

                spawnedCount++;

                if (staggerSpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(
                        staggerSpawnInterval
                    );
                }
            }
            else
            {
                yield return new WaitForSeconds(
                    0.2f
                );
            }
        }

        Debug.Log(
            $"[EnemyWaveSpawner] {wave.waveName} spawned {spawnedCount} enemies."
        );
    }

    private EnemyWaveGroup GetNextMixedGroup(
        EnemyWaveDefinition wave,
        List<int> remainingGroupCounts,
        ref int nextGroupIndex
    )
    {
        if (wave == null ||
            wave.groups == null ||
            wave.groups.Count == 0 ||
            remainingGroupCounts == null)
        {
            return null;
        }

        int totalGroups =
            wave.groups.Count;

        for (int scan = 0;
             scan < totalGroups;
             scan++)
        {
            int candidateIndex =
                (nextGroupIndex + scan) %
                totalGroups;

            EnemyWaveGroup group =
                wave.groups[candidateIndex];

            if (group != null &&
                group.enemyCount > 0 &&
                remainingGroupCounts[candidateIndex] > 0)
            {
                nextGroupIndex =
                    (candidateIndex + 1) %
                    totalGroups;

                remainingGroupCounts[candidateIndex]--;

                return group;
            }
        }

        return null;
    }

    private void SpawnEnemyFromLegacyConfig(
        int index,
        List<Vector3> usedPositions
    )
    {
        EnemySpawnEntry selectedEntry =
            GetRandomEntry();

        SpawnEnemy(
            selectedEntry != null
                ? selectedEntry.enemyData
                : null,

            selectedEntry != null
                ? selectedEntry.aksaraData
                : null,

            index,
            enemyCount,
            usedPositions
        );
    }

    private void SpawnEnemy(
        EnemyData enemyData,
        AksaraData aksaraData,
        int spawnIndex,
        int totalEnemiesInWave,
        List<Vector3> usedPositions
    )
    {
        Vector3 spawnPosition =
            GetSpawnPosition(
                spawnIndex,
                totalEnemiesInWave,
                usedPositions
            );

        Transform parent =
            spawnedParent != null
                ? spawnedParent
                : transform;

        EnemyGestureCommand enemy =
            Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity,
                parent
            );

        enemy.SetAutoIssueOnStart(false);

        Enemy enemyComponent =
            enemy.GetComponent<Enemy>();

        if (enemyComponent != null)
        {
            enemyComponent.Configure(
                enemyData,
                aksaraData
            );
        }
        else if (aksaraData != null)
        {
            enemy.ConfigureChallenge(
                aksaraData.GestureShape,
                requiredCorrectGestures
            );
        }
        else
        {
            enemy.ConfigureChallenge(
                GetGestureForIndex(spawnIndex),
                requiredCorrectGestures
            );
        }

        KeepEnemyInsideSpawnArea(enemy);

        enemy.SyncSpawnPosition();

        if (usedPositions.Count > 0)
        {
            usedPositions[
                usedPositions.Count - 1
            ] = enemy.transform.position;
        }

        enemy.IssueCommand();

        spawnedEnemies.Add(enemy);

        currentWaveEnemies.Add(enemy);
    }

    private void KeepEnemyInsideSpawnArea(
        EnemyGestureCommand enemy
    )
    {
        if (!useSpawnArea ||
            enemy == null)
            return;

        Collider2D enemyCollider =
            enemy.GetComponent<Collider2D>();

        if (enemyCollider == null)
        {
            enemyCollider =
                enemy.GetComponentInChildren<Collider2D>(
                    true
                );
        }

        if (enemyCollider == null)
            return;

        Bounds areaBounds =
            new Bounds(
                new Vector3(
                    spawnAreaCenter.x,
                    spawnAreaCenter.y,
                    enemy.transform.position.z
                ),
                new Vector3(
                    spawnAreaSize.x,
                    spawnAreaSize.y,
                    0f
                )
            );

        Bounds enemyBounds =
            enemyCollider.bounds;

        float minX =
            areaBounds.min.x +
            (enemy.transform.position.x -
             enemyBounds.min.x);

        float maxX =
            areaBounds.max.x -
            (enemyBounds.max.x -
             enemy.transform.position.x);

        float minY =
            areaBounds.min.y +
            (enemy.transform.position.y -
             enemyBounds.min.y);

        float maxY =
            areaBounds.max.y -
            (enemyBounds.max.y -
             enemy.transform.position.y);

        Vector3 correctedPosition =
            enemy.transform.position;

        correctedPosition.x =
            minX <= maxX
                ? Mathf.Clamp(
                    correctedPosition.x,
                    minX,
                    maxX
                )
                : areaBounds.center.x;

        correctedPosition.y =
            minY <= maxY
                ? Mathf.Clamp(
                    correctedPosition.y,
                    minY,
                    maxY
                )
                : areaBounds.center.y;

        enemy.transform.position =
            correctedPosition;
    }

    private IEnumerator WaitForPuzzleCompletionRoutine()
    {
        if (PuzzleManager.Instance == null)
            yield break;

        while (!PuzzleManager.Instance.IsPuzzleCompleted())
        {
            yield return null;
        }
    }

    private IEnumerator WaitForWaveToClearRoutine(
        float extraDelay
    )
    {
        if (currentWaveEnemies.Count == 0)
        {
            if (extraDelay > 0f)
            {
                yield return new WaitForSeconds(
                    extraDelay
                );
            }

            yield break;
        }

        while (true)
        {
            for (int i =
                     currentWaveEnemies.Count - 1;
                 i >= 0;
                 i--)
            {
                if (currentWaveEnemies[i] == null)
                {
                    currentWaveEnemies.RemoveAt(i);
                }
            }

            if (currentWaveEnemies.Count == 0)
                break;

            yield return new WaitForSeconds(
                0.2f
            );
        }

        if (extraDelay > 0f)
        {
            yield return new WaitForSeconds(
                extraDelay
            );
        }
    }

    private void RefreshCurrentWaveEnemies()
    {
        for (int i =
                 currentWaveEnemies.Count - 1;
             i >= 0;
             i--)
        {
            if (currentWaveEnemies[i] == null)
            {
                currentWaveEnemies.RemoveAt(i);
            }
        }
    }

    private EnemySpawnEntry GetRandomEntry()
    {
        if (spawnEntries == null ||
            spawnEntries.Length == 0)
            return null;

        float totalWeight = 0f;

        foreach (var entry in spawnEntries)
        {
            if (entry == null ||
                entry.weight <= 0f)
                continue;

            totalWeight +=
                entry.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float roll =
            Random.value *
            totalWeight;

        float currentWeight = 0f;

        foreach (var entry in spawnEntries)
        {
            if (entry == null ||
                entry.weight <= 0f)
                continue;

            currentWeight +=
                entry.weight;

            if (roll <= currentWeight)
                return entry;
        }

        return spawnEntries[
            spawnEntries.Length - 1
        ];
    }

    private Vector3 GetSpawnPosition(
        int index,
        int totalEnemiesInWave,
        List<Vector3> usedPositions
    )
    {
        Vector3 position;

        if (spawnPoints != null &&
            spawnPoints.Length > 0)
        {
            if (index < spawnPoints.Length)
            {
                Transform spawnPoint =
                    spawnPoints[index];

                position =
                    spawnPoint != null
                        ? spawnPoint.position
                        : GetAreaSpawnPosition();
            }
            else
            {
                position =
                    GetAreaSpawnPosition();
            }
        }
        else if (useSpawnArea)
        {
            position =
                GetAreaSpawnPosition(
                    index,
                    totalEnemiesInWave
                );
        }
        else
        {
            position =
                GetCircularSpawnPosition(
                    index,
                    totalEnemiesInWave
                );
        }

        position =
            GetValidSpawnPosition(
                position,
                usedPositions,
                index,
                totalEnemiesInWave
            );

        usedPositions.Add(
            position
        );

        return position;
    }

    private Vector3 GetAreaSpawnPosition(
        int index = 0,
        int totalEnemies = 1
    )
    {
        totalEnemies = Mathf.Max(1, totalEnemies);

        float areaMinX = spawnAreaCenter.x - spawnAreaSize.x / 2f;
        float segmentWidth = spawnAreaSize.x / totalEnemies;
        float segmentMinX = areaMinX + segmentWidth * index;

        float x =
            segmentMinX +
            Random.Range(
                0f,
                segmentWidth
            );

        float y =
            spawnAreaCenter.y +
            Random.Range(
                -spawnAreaSize.y / 2f,
                spawnAreaSize.y / 2f
            );

        return new Vector3(
            x,
            y,
            0f
        );
    }

    private bool IsPositionTooClose(
        Vector3 position,
        List<Vector3> usedPositions
    )
    {
        for (int i = 0;
             i < usedPositions.Count;
             i++)
        {
            if (Vector3.Distance(
                    position,
                    usedPositions[i]
                ) < minSpawnDistance)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPositionTooFar(
        Vector3 position,
        List<Vector3> usedPositions
    )
    {
        if (usedPositions.Count == 0)
            return false;

        float nearestDistance =
            float.MaxValue;

        for (int i = 0;
             i < usedPositions.Count;
             i++)
        {
            float distance =
                Vector3.Distance(
                    position,
                    usedPositions[i]
                );

            nearestDistance =
                Mathf.Min(
                    nearestDistance,
                    distance
                );
        }

        return nearestDistance >
               maxSpawnDistance;
    }

    private Vector3 GetValidSpawnPosition(
        Vector3 position,
        List<Vector3> usedPositions,
        int index,
        int totalEnemiesInWave
    )
    {
        if (usedPositions.Count == 0)
            return position;

        int attempt = 0;

        while (
            IsPositionTooClose(
                position,
                usedPositions
            ) &&
            attempt < 40
        )
        {
            position =
                GetAreaSpawnPosition(
                    index,
                    totalEnemiesInWave
                );

            attempt++;
        }

        for (int separationPass = 0; separationPass < 10; separationPass++)
        {
            bool moved = false;

            for (int i = 0; i < usedPositions.Count; i++)
            {
                Vector3 difference = position - usedPositions[i];
                float distance = difference.magnitude;

                if (distance >= minSpawnDistance)
                    continue;

                Vector3 direction = distance > 0.001f
                    ? difference / distance
                    : Vector3.up;

                position = usedPositions[i] + direction * minSpawnDistance;
                moved = true;
            }

            if (!moved || !IsPositionTooClose(position, usedPositions))
                break;
        }

        return position;
    }

    private Vector3 GetCircularSpawnPosition(
        int index,
        int totalEnemiesInWave
    )
    {
        float angle =
            (Mathf.PI * 2f * index) /
            Mathf.Max(
                1,
                totalEnemiesInWave
            );

        Vector3 offset =
            new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Abs(
                    Mathf.Sin(angle)
                )
            ) * 2f;

        return transform.position +
               offset;
    }

    private GestureShape GetGestureForIndex(
        int index
    )
    {
        GestureShape[] candidates =
            GetAvailableGesturesByCategory();

        if (candidates == null ||
            candidates.Length == 0)
        {
            return GestureShape.Na;
        }

        int selectedIndex =
            Random.Range(
                0,
                candidates.Length
            );

        return candidates[
            selectedIndex
        ];
    }

    private GestureShape[] GetAvailableGesturesByCategory()
    {
        return availableAksaraGestures != null &&
               availableAksaraGestures.Length > 0
            ? availableAksaraGestures
            : new[]
            {
                GestureShape.Na,
                GestureShape.Ka
            };
    }
}