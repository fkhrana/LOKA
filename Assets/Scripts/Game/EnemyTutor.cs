using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper yang spawn enemy tutorial dengan EnemyData + AksaraData tetap.
/// Tidak butuh modifikasi EnemyWaveSpawner.
/// </summary>
public class TutorialEnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyGestureCommand enemyPrefab;
    [SerializeField] private Transform enemyParent;

    private readonly List<EnemyGestureCommand> spawned = new List<EnemyGestureCommand>();
    private Coroutine deathTracker;

    public IReadOnlyList<EnemyGestureCommand> SpawnedEnemies => spawned;

    /// <summary>
    /// Spawn `count` enemy di sekitar `center` dalam radius tertentu.
    /// </summary>
    public List<EnemyGestureCommand> Spawn(
        int count,
        Vector3 center,
        float radius,
        EnemyData enemyData,
        AksaraData aksara,
        Action onAnyDied)
    {
        spawned.Clear();

        if (enemyPrefab == null)
        {
            Debug.LogWarning("[TutorialEnemySpawner] enemyPrefab belum di-assign.");
            return spawned;
        }

        Transform parent = enemyParent != null ? enemyParent : transform;

        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)Mathf.Max(1, count)) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            EnemyGestureCommand enemy = Instantiate(
                enemyPrefab, pos, Quaternion.identity, parent);

            if (enemy == null) continue;

            enemy.name = $"TutorialEnemy_{i}";
            enemy.SetAutoIssueOnStart(false);

            // Configure EnemyData + AksaraData.
            Enemy enemyComp = enemy.GetComponent<Enemy>();
            if (enemyComp != null && enemyData != null)
                enemyComp.Configure(enemyData, aksara);
            else if (aksara != null)
                enemy.ConfigureChallenge(aksara.GestureShape, 1);

            // Configure movement.
            EnemyMovementBehavior movement = enemy.GetComponent<EnemyMovementBehavior>();
            if (movement == null)
                movement = enemy.GetComponentInChildren<EnemyMovementBehavior>(true);

            if (movement != null)
            {
                movement.SetSpawnPosition(pos);
                movement.SetActive(false);   // standby dulu
                movement.SetMovementPaused(false);
            }

            enemy.SyncSpawnPosition();

            spawned.Add(enemy);
        }

        if (deathTracker != null) StopCoroutine(deathTracker);
        deathTracker = StartCoroutine(TrackDeaths(onAnyDied));

        return spawned;
    }

    /// <summary>
    /// Aktifkan semua enemy biar mulai gerak approach ke player.
    /// </summary>
    public void ActivateAll()
    {
        foreach (var e in spawned)
        {
            if (e == null) continue;

            var movement = e.GetComponent<EnemyMovementBehavior>();
            if (movement == null)
                movement = e.GetComponentInChildren<EnemyMovementBehavior>(true);

            if (movement != null)
            {
                movement.SetActive(true);
                movement.SetMovementPaused(false);
            }
        }
    }

    /// <summary>
    /// Set speed semua enemy (slow/normal).
    /// </summary>
    public void SetSpeed(float speed)
    {
        foreach (var e in spawned)
        {
            if (e == null) continue;

            var movement = e.GetComponent<EnemyMovementBehavior>();
            if (movement == null)
                movement = e.GetComponentInChildren<EnemyMovementBehavior>(true);

            if (movement != null)
                movement.SetSpeedFromData(speed);
        }
    }

    /// <summary>
    /// Semua SpriteRenderer anak enemy — buat blink.
    /// </summary>
    public List<SpriteRenderer> GetAllRenderers()
    {
        var result = new List<SpriteRenderer>();

        foreach (var e in spawned)
        {
            if (e == null) continue;
            SpriteRenderer sr = e.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) result.Add(sr);
        }

        return result;
    }

    public void Clear()
    {
        foreach (var e in spawned)
            if (e != null) Destroy(e.gameObject);

        spawned.Clear();

        if (deathTracker != null)
        {
            StopCoroutine(deathTracker);
            deathTracker = null;
        }
    }

    private IEnumerator TrackDeaths(Action onAnyDied)
    {
        int lastAlive = spawned.Count;

        while (lastAlive > 0)
        {
            int alive = 0;
            for (int i = 0; i < spawned.Count; i++)
                if (spawned[i] != null) alive++;

            if (alive < lastAlive)
            {
                int died = lastAlive - alive;
                for (int d = 0; d < died; d++)
                    onAnyDied?.Invoke();
                lastAlive = alive;
            }

            if (alive == 0) yield break;
            yield return new WaitForSeconds(0.2f);
        }
    }
}