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
    [SerializeField, Min(0)] private int enemyCount = 0;
    [SerializeField] private bool spawnOnStart = true;
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
    [SerializeField] private GestureCategory gestureCategory = GestureCategory.Shapes;
    [SerializeField] private GestureShape[] availableShapeGestures = { GestureShape.Circle, GestureShape.Square };
    [SerializeField] private GestureShape[] availableAksaraGestures = { GestureShape.Na, GestureShape.Ka };
    [SerializeField, Min(1)] private int requiredCorrectGestures = 1;
    [SerializeField, Min(0.1f)] private float minSpawnDistance = 1f;
    [SerializeField, Min(0.1f)] private float maxSpawnDistance = 3f;

    private readonly List<EnemyGestureCommand> spawnedEnemies = new List<EnemyGestureCommand>();
    private readonly List<EnemyGestureCommand> currentWaveEnemies = new List<EnemyGestureCommand>();
    private Coroutine waveSequenceCoroutine;
    private int currentWaveIndex = -1;

    public IReadOnlyList<EnemyGestureCommand> SpawnedEnemies => spawnedEnemies;
    public int CurrentWaveIndex => currentWaveIndex;

    private void OnValidate()
    {
        if (maxSpawnDistance < minSpawnDistance)
            maxSpawnDistance = minSpawnDistance;
    }

    private void Start()
    {
        if (spawnOnStart)
            StartWaveSequence();
    }

    public void StartWaveSequence()
    {
        StopWaveSequence();
        ClearSpawnedEnemies();

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyWaveSpawner: enemyPrefab belum di-assign.");
            return;
        }

        // Initialize progress manager with total enemies for the level and wave milestones
        if (waves == null || waves.Count == 0)
        {
            LevelProgressManager.Instance?.Initialize(enemyCount, null);
            SpawnWave();
            return;
        }

        int totalLevelEnemies = 0;
        var milestones = new List<int>();
        for (int w = 0; w < waves.Count; w++)
        {
            var wave = waves[w];
            if (wave == null || wave.groups == null)
                continue;

            int waveTotal = 0;
            for (int gi = 0; gi < wave.groups.Count; gi++)
            {
                var g = wave.groups[gi];
                if (g != null)
                    waveTotal += Mathf.Max(0, g.enemyCount);
            }

            totalLevelEnemies += waveTotal;
            milestones.Add(totalLevelEnemies);
        }

        LevelProgressManager.Instance?.Initialize(totalLevelEnemies, milestones);

        currentWaveIndex = -1;
        currentWaveEnemies.Clear();
        waveSequenceCoroutine = StartCoroutine(SpawnWaveSequenceRoutine());
    }

    public void StopWaveSequence()
    {
        if (waveSequenceCoroutine != null)
        {
            StopCoroutine(waveSequenceCoroutine);
            waveSequenceCoroutine = null;
        }
    }

    public void SpawnWave()
    {
        ClearSpawnedEnemies();

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyWaveSpawner: enemyPrefab belum di-assign.");
            return;
        }

        currentWaveEnemies.Clear();
        // initialize progress for legacy single-wave usage
        LevelProgressManager.Instance?.Initialize(enemyCount, null);
        var usedPositions = new List<Vector3>();

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemyFromLegacyConfig(i, usedPositions);
        }

        if (spawnedEnemies.Count != enemyCount)
            Debug.LogWarning($"EnemyWaveSpawner: expected {enemyCount} enemies but spawned {spawnedEnemies.Count}.");
    }

    public void ClearSpawnedEnemies()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
                Destroy(spawnedEnemies[i].gameObject);
        }

        spawnedEnemies.Clear();
        currentWaveEnemies.Clear();
    }

    private IEnumerator SpawnWaveSequenceRoutine()
    {
        if (waves == null || waves.Count == 0)
        {
            SpawnWave();
            yield break;
        }

        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            currentWaveIndex = waveIndex;
            EnemyWaveDefinition wave = waves[waveIndex];

            if (wave != null && wave.delayBeforeSpawn > 0f)
                yield return new WaitForSeconds(wave.delayBeforeSpawn);

            yield return StartCoroutine(SpawnWaveDefinitionRoutine(wave, waveIndex));

            if (waveIndex < waves.Count - 1)
            {
                yield return StartCoroutine(WaitForWaveToClearRoutine(delayBetweenWaves));

                if (waveIndex == 0 && PuzzleManager.Instance != null)
                {
                    PuzzleManager.Instance.ShowPuzzleOnce();
                    yield return StartCoroutine(WaitForPuzzleCompletionRoutine());
                }
            }
        }

        currentWaveIndex = waves.Count;
    }

    private IEnumerator SpawnWaveDefinitionRoutine(EnemyWaveDefinition wave, int waveIndex)
    {
        if (wave == null)
            yield break;

        currentWaveEnemies.Clear();

        int totalEnemiesInWave = 0;
        var usedPositions = new List<Vector3>();

        if (wave.groups == null || wave.groups.Count == 0)
        {
            Debug.LogWarning($"EnemyWaveSpawner: wave '{wave.waveName}' has no groups configured.");
            yield break;
        }

        for (int groupIndex = 0; groupIndex < wave.groups.Count; groupIndex++)
        {
            EnemyWaveGroup group = wave.groups[groupIndex];
            if (group == null || group.enemyCount <= 0)
                continue;

            totalEnemiesInWave += group.enemyCount;
        }

        Debug.Log($"[EnemyWaveSpawner] Spawning {wave.waveName} ({waveIndex + 1}/{(waves != null ? waves.Count : 1)})");

        int spawnedCount = 0;
        int initialBatchSize = Mathf.Min(initialConcurrentEnemies, totalEnemiesInWave);
        var remainingGroupCounts = new List<int>();
        for (int groupIndex = 0; groupIndex < wave.groups.Count; groupIndex++)
        {
            EnemyWaveGroup group = wave.groups[groupIndex];
            if (group == null || group.enemyCount <= 0)
                remainingGroupCounts.Add(0);
            else
                remainingGroupCounts.Add(group.enemyCount);
        }

        int nextGroupIndex = 0;

        for (int i = 0; i < initialBatchSize; i++)
        {
            if (spawnedCount >= totalEnemiesInWave)
                break;

            EnemyWaveGroup selectedGroup = GetNextMixedGroup(wave, remainingGroupCounts, ref nextGroupIndex);
            if (selectedGroup == null)
                break;

            SpawnEnemy(selectedGroup != null ? selectedGroup.enemyData : null, selectedGroup != null ? selectedGroup.aksaraData : null, spawnedCount, totalEnemiesInWave, usedPositions);
            spawnedCount++;
        }

        while (spawnedCount < totalEnemiesInWave)
        {
            RefreshCurrentWaveEnemies();

            if (currentWaveEnemies.Count < initialBatchSize)
            {
                EnemyWaveGroup selectedGroup = GetNextMixedGroup(wave, remainingGroupCounts, ref nextGroupIndex);
                if (selectedGroup == null)
                    break;

                SpawnEnemy(selectedGroup != null ? selectedGroup.enemyData : null, selectedGroup != null ? selectedGroup.aksaraData : null, spawnedCount, totalEnemiesInWave, usedPositions);
                spawnedCount++;

                if (staggerSpawnInterval > 0f)
                    yield return new WaitForSeconds(staggerSpawnInterval);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        Debug.Log($"[EnemyWaveSpawner] {wave.waveName} spawned {spawnedCount} enemies.");
    }

    private EnemyWaveGroup GetNextMixedGroup(EnemyWaveDefinition wave, List<int> remainingGroupCounts, ref int nextGroupIndex)
    {
        if (wave == null || wave.groups == null || wave.groups.Count == 0 || remainingGroupCounts == null)
            return null;

        int totalGroups = wave.groups.Count;
        for (int scan = 0; scan < totalGroups; scan++)
        {
            int candidateIndex = (nextGroupIndex + scan) % totalGroups;
            EnemyWaveGroup group = wave.groups[candidateIndex];
            if (group != null && group.enemyCount > 0 && remainingGroupCounts[candidateIndex] > 0)
            {
                nextGroupIndex = (candidateIndex + 1) % totalGroups;
                remainingGroupCounts[candidateIndex]--;
                return group;
            }
        }

        return null;
    }

    private void SpawnEnemyFromLegacyConfig(int index, List<Vector3> usedPositions)
    {
        EnemySpawnEntry selectedEntry = GetRandomEntry();
        SpawnEnemy(selectedEntry != null ? selectedEntry.enemyData : null, selectedEntry != null ? selectedEntry.aksaraData : null, index, enemyCount, usedPositions);
    }

    private void SpawnEnemy(EnemyData enemyData, AksaraData aksaraData, int spawnIndex, int totalEnemiesInWave, List<Vector3> usedPositions)
    {
        Vector3 spawnPosition = GetSpawnPosition(spawnIndex, totalEnemiesInWave, usedPositions);
        Transform parent = spawnedParent != null ? spawnedParent : transform;

        EnemyGestureCommand enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, parent);
        enemy.SetAutoIssueOnStart(false);

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Configure(enemyData, aksaraData);
        }
        else if (aksaraData != null)
        {
            enemy.ConfigureChallenge(aksaraData.GestureShape, requiredCorrectGestures);
        }
        else
        {
            enemy.ConfigureChallenge(GetGestureForIndex(spawnIndex), requiredCorrectGestures);
        }

        enemy.IssueCommand();
        spawnedEnemies.Add(enemy);
        currentWaveEnemies.Add(enemy);
        // attach visibility notifier so progress increments only when enemy becomes visible on camera
        if (enemy != null && enemy.gameObject != null)
        {
            enemy.gameObject.AddComponent<EnemyVisibilityNotifier>();
        }
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

    private IEnumerator WaitForWaveToClearRoutine(float extraDelay)
    {
        if (currentWaveEnemies.Count == 0)
        {
            if (extraDelay > 0f)
                yield return new WaitForSeconds(extraDelay);
            yield break;
        }

        while (true)
        {
            for (int i = currentWaveEnemies.Count - 1; i >= 0; i--)
            {
                if (currentWaveEnemies[i] == null)
                    currentWaveEnemies.RemoveAt(i);
            }

            if (currentWaveEnemies.Count == 0)
                break;

            yield return new WaitForSeconds(0.2f);
        }

        if (extraDelay > 0f)
            yield return new WaitForSeconds(extraDelay);
    }

    private void RefreshCurrentWaveEnemies()
    {
        for (int i = currentWaveEnemies.Count - 1; i >= 0; i--)
        {
            if (currentWaveEnemies[i] == null)
                currentWaveEnemies.RemoveAt(i);
        }
    }

    private EnemySpawnEntry GetRandomEntry()
    {
        if (spawnEntries == null || spawnEntries.Length == 0)
            return null;

        float totalWeight = 0f;
        foreach (var entry in spawnEntries)
        {
            if (entry == null || entry.weight <= 0f)
                continue;

            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float roll = Random.value * totalWeight;
        float currentWeight = 0f;

        foreach (var entry in spawnEntries)
        {
            if (entry == null || entry.weight <= 0f)
                continue;

            currentWeight += entry.weight;
            if (roll <= currentWeight)
                return entry;
        }

        return spawnEntries[spawnEntries.Length - 1];
    }

    private Vector3 GetSpawnPosition(int index, int totalEnemiesInWave, List<Vector3> usedPositions)
    {
        Vector3 position;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            if (index < spawnPoints.Length)
            {
                Transform spawnPoint = spawnPoints[index];
                position = spawnPoint != null ? spawnPoint.position : GetAreaSpawnPosition();
            }
            else
            {
                position = GetAreaSpawnPosition();
            }
        }
        else if (useSpawnArea)
        {
            position = GetAreaSpawnPosition();
        }
        else
        {
            position = GetCircularSpawnPosition(index, totalEnemiesInWave);
        }

        position = GetValidSpawnPosition(position, usedPositions);
        usedPositions.Add(position);
        return position;
    }

    private Vector3 GetAreaSpawnPosition()
    {
        float x = spawnAreaCenter.x + Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float y = spawnAreaCenter.y + Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
        return new Vector3(x, y, 0f);
    }

    private bool IsPositionTooClose(Vector3 position, List<Vector3> usedPositions)
    {
        for (int i = 0; i < usedPositions.Count; i++)
        {
            if (Vector3.Distance(position, usedPositions[i]) < minSpawnDistance)
                return true;
        }
        return false;
    }

    private bool IsPositionTooFar(Vector3 position, List<Vector3> usedPositions)
    {
        if (usedPositions.Count == 0)
            return false;

        float nearestDistance = float.MaxValue;
        for (int i = 0; i < usedPositions.Count; i++)
        {
            float distance = Vector3.Distance(position, usedPositions[i]);
            nearestDistance = Mathf.Min(nearestDistance, distance);
        }

        return nearestDistance > maxSpawnDistance;
    }

    private Vector3 GetValidSpawnPosition(Vector3 position, List<Vector3> usedPositions)
    {
        if (usedPositions.Count == 0)
            return position;

        int attempt = 0;
        while (IsPositionTooClose(position, usedPositions) && attempt < 40)
        {
            position = GetAreaSpawnPosition();
            attempt++;
        }

        if (IsPositionTooClose(position, usedPositions))
        {
            Vector2 nudge = Random.insideUnitCircle.normalized * minSpawnDistance;
            position += new Vector3(nudge.x, nudge.y, 0f);
        }

        return position;
    }

    private Vector3 GetCircularSpawnPosition(int index, int totalEnemiesInWave)
    {
        float angle = (Mathf.PI * 2f * index) / Mathf.Max(1, totalEnemiesInWave);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Abs(Mathf.Sin(angle))) * 2f;
        return transform.position + offset;
    }

    private GestureShape GetGestureForIndex(int index)
    {
        GestureShape[] candidates = GetAvailableGesturesByCategory();
        if (candidates == null || candidates.Length == 0)
            return GestureShape.Circle;

        int selectedIndex = Random.Range(0, candidates.Length);
        return candidates[selectedIndex];
    }

    private GestureShape[] GetAvailableGesturesByCategory()
    {
        switch (gestureCategory)
        {
            case GestureCategory.Aksara:
                return availableAksaraGestures != null && availableAksaraGestures.Length > 0
                    ? availableAksaraGestures
                    : new[] { GestureShape.Na, GestureShape.Ka };
            case GestureCategory.Shapes:
                return availableShapeGestures != null && availableShapeGestures.Length > 0
                    ? availableShapeGestures
                    : new[] { GestureShape.Circle, GestureShape.Square };
            default:
                var combined = new List<GestureShape>();
                if (availableShapeGestures != null)
                    combined.AddRange(availableShapeGestures);
                if (availableAksaraGestures != null)
                    combined.AddRange(availableAksaraGestures);
                return combined.Count > 0 ? combined.ToArray() : new[] { GestureShape.Circle, GestureShape.Square, GestureShape.Na, GestureShape.Ka };
        }
    }

    private enum GestureCategory
    {
        Shapes,
        Aksara,
        Mixed
    }
}
