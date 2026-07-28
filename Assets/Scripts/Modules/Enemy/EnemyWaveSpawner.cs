using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private GestureCategory gestureCategory = GestureCategory.Shapes;
    [SerializeField] private GestureShape[] availableShapeGestures = { GestureShape.Circle, GestureShape.Square };
    [SerializeField] private GestureShape[] availableAksaraGestures = { GestureShape.Na, GestureShape.Ka };
    [SerializeField, Min(1)] private int requiredCorrectGestures = 1;
    [SerializeField, Min(0.1f)] private float minSpawnDistance = 1f;
    [SerializeField, Min(0.1f)] private float maxSpawnDistance = 3f;

    private readonly List<EnemyGestureCommand> spawnedEnemies = new List<EnemyGestureCommand>();

    public IReadOnlyList<EnemyGestureCommand> SpawnedEnemies => spawnedEnemies;

    private void OnValidate()
    {
        if (maxSpawnDistance < minSpawnDistance)
            maxSpawnDistance = minSpawnDistance;
    }

    private void Start()
    {
        if (spawnOnStart)
            SpawnWave();
    }

    public void SpawnWave()
    {
        ClearSpawnedEnemies();

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyWaveSpawner: enemyPrefab belum di-assign.");
            return;
        }

        var usedPositions = new List<Vector3>();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i, usedPositions);
            Transform parent = spawnedParent != null ? spawnedParent : transform;

            EnemyGestureCommand enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, parent);
            enemy.SetAutoIssueOnStart(false);
            enemy.ConfigureChallenge(GetGestureForIndex(i), requiredCorrectGestures);
            enemy.IssueCommand();
            spawnedEnemies.Add(enemy);
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
    }

    private Vector3 GetSpawnPosition(int index, List<Vector3> usedPositions)
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
            position = GetCircularSpawnPosition(index);
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

    private Vector3 GetCircularSpawnPosition(int index)
    {
        float angle = (Mathf.PI * 2f * index) / Mathf.Max(1, enemyCount);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Abs(Mathf.Sin(angle))) * 2f;
        return transform.position + offset;
    }

    private GestureShape GetGestureForIndex(int index)
    {
        var candidates = GetAvailableGesturesByCategory();
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
