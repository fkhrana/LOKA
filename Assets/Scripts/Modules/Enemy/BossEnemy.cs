using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovementBehavior))]
public class BossEnemy : MonoBehaviour
{
    public static bool HasActiveBoss { get; private set; }

    public static bool HasActiveBossWithMoreStrokes(int strokeCount)
    {
        if (!HasActiveBoss || GestureRecognizer.Instance == null)
            return false;

        BossEnemy boss = FindAnyObjectByType<BossEnemy>();
        if (boss == null)
            return false;

        for (int i = 0; i < boss.currentAksara.Length; i++)
        {
            if (!boss.solvedAksara[i] && boss.currentAksara[i] != null &&
                GestureRecognizer.Instance.HasTemplateWithMoreStrokes(
                    boss.currentAksara[i].GestureShape,
                    strokeCount))
            {
                return true;
            }
        }

        return false;
    }

    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private EnemyMovementBehavior movementBehavior;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer[] aksaraIconRenderers = new SpriteRenderer[3];
    [SerializeField] private List<AksaraData> aksaraPool = new List<AksaraData>();
    [SerializeField, Min(0.1f)] private float introMoveDuration = 1.5f;
    [SerializeField, Min(0.1f)] private float stateTwoMoveSpeed = 0.45f;
    [SerializeField, Min(1)] private int aksaraPerState = 3;

    [Header("State 1 Standard Enemies")]
    [SerializeField] private EnemyGestureCommand standardEnemyPrefab;
    [SerializeField] private EnemyData standardEnemyData;
    [SerializeField] private List<AksaraData> standardAksaraPool = new List<AksaraData>();
    [SerializeField] private Vector2 standardEnemySpawnOffsetLeft = new Vector2(-1.5f, 0f);
    [SerializeField] private Vector2 standardEnemySpawnOffsetRight = new Vector2(1.5f, 0f);
    [SerializeField, Min(0f)] private float initialStandardEnemySpawnDelay = 0.75f;
    [SerializeField, Min(0f)] private float standardEnemyRespawnDelay = 3f;

    [Header("Boss SFX")]
    [SerializeField] private bool useDefeatSFX = true;
    [SerializeField] private string defeatSFXName = "EnemyDefeat";
    [SerializeField, Range(0f, 1f)] private float defeatSFXVolume = 1f;
    [SerializeField] private bool useAksaraSFX = true;
    [SerializeField] private AksaraSoundLibrary aksaraSoundLibrary;
    [SerializeField, Range(0f, 2f)] private float aksaraSFXVolume = 1.5f;

    [Header("Boss Defeat")]
    [SerializeField, Min(0f)] private float defeatBlinkDuration = 0.6f;
    [SerializeField, Min(0.01f)] private float defeatBlinkInterval = 0.1f;
    [SerializeField, Min(0f)] private float defeatShrinkDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] private float defeatShrinkTargetScale = 0.1f;
    [SerializeField] private string defeatAnimationStateName = "enemyDieBlubub";

    private readonly AksaraData[] currentAksara = new AksaraData[3];
    private readonly bool[] solvedAksara = new bool[3];
    private int state;
    private bool isListening;
    private bool isTransitioning;
    private readonly List<EnemyGestureCommand> standardEnemies = new List<EnemyGestureCommand>();
    private readonly Dictionary<EnemyGestureCommand, int> standardEnemySlots =
        new Dictionary<EnemyGestureCommand, int>();
    private readonly bool[] occupiedStandardSlots = new bool[2];
    private int standardAksaraIndex;
    private Coroutine standardRespawnCoroutine;

    public int ProgressUnits => aksaraPerState * 2;

    public void ConfigureAksaraPool(IEnumerable<AksaraData> availableAksara)
    {
        aksaraPool.Clear();

        if (availableAksara == null)
            return;

        foreach (AksaraData aksara in availableAksara)
        {
            if (aksara != null && !aksaraPool.Contains(aksara))
                aksaraPool.Add(aksara);
        }
    }

    public void SyncSpawnPosition()
    {
        if (movementBehavior != null)
            movementBehavior.SetSpawnPosition(transform.position);
    }

    public void BeginBossFight()
    {
        if (movementBehavior == null)
            movementBehavior = GetComponent<EnemyMovementBehavior>();

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        Collider2D bossCollider = GetComponent<Collider2D>();
        SpriteRenderer bossRenderer = GetComponentInChildren<SpriteRenderer>(true);
        movementBehavior.Initialize(playerHealth, bossCollider, bossRenderer);

        EnemyGestureCommand regularChallenge = GetComponent<EnemyGestureCommand>();
        if (regularChallenge != null)
        {
            regularChallenge.ClearCommand();
            regularChallenge.enabled = false;
        }

        if (aksaraPool.Count < aksaraPerState)
        {
            AksaraData[] loadedAksara = Resources.LoadAll<AksaraData>(string.Empty);
            ConfigureAksaraPool(loadedAksara);
        }

        if (aksaraPool.Count < aksaraPerState)
        {
            Debug.LogWarning("BossEnemy membutuhkan minimal 3 AksaraData unik.");
            return;
        }

        StopAllCoroutines();
        state = 1;
        HasActiveBoss = true;
        EnemyGestureCommand.EnemyDefeatedWithSource -= HandleStandardEnemyDefeated;
        EnemyGestureCommand.EnemyDefeatedWithSource += HandleStandardEnemyDefeated;
        Subscribe();
        SetIcons(ChooseUniqueAksara());
        movementBehavior.SetActive(false);
        StartCoroutine(StopAfterIntro());
        StartCoroutine(SpawnStandardEnemiesAfterIntro());
    }

    private void OnValidate()
    {
        aksaraPerState = 3;
    }

    private void Awake()
    {
        if (movementBehavior == null)
            movementBehavior = GetComponent<EnemyMovementBehavior>();

        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
    }

    private void LateUpdate()
    {
        if (state == 0 || movementBehavior == null)
            return;

        movementBehavior.Tick(CameraIntroManager.GameStarted);
    }

    private void OnDisable()
    {
        Unsubscribe();
        EnemyGestureCommand.EnemyDefeatedWithSource -= HandleStandardEnemyDefeated;
        if (HasActiveBoss)
            HasActiveBoss = false;
    }

    private IEnumerator StopAfterIntro()
    {
        yield return new WaitUntil(() => CameraIntroManager.GameStarted);
        movementBehavior.SetActive(true);
        yield return new WaitForSeconds(introMoveDuration);
        if (movementBehavior != null)
            movementBehavior.SetActive(false);
    }

    private IEnumerator SpawnStandardEnemiesAfterIntro()
    {
        yield return new WaitUntil(() => CameraIntroManager.GameStarted);
        yield return new WaitForSeconds(introMoveDuration);

        for (int i = 0; i < 2; i++)
        {
            SpawnStandardEnemy();

            if (i == 0 && initialStandardEnemySpawnDelay > 0f)
                yield return new WaitForSeconds(initialStandardEnemySpawnDelay);
        }
    }

    private void SpawnStandardEnemy()
    {
        if (state != 1 || isTransitioning)
            return;

        if (standardEnemyPrefab == null)
        {
            Debug.LogWarning("BossEnemy: standardEnemyPrefab belum di-assign.");
            return;
        }

        int slot = FindFreeStandardSlot();
        if (slot < 0)
            return;

        AksaraData standardAksara = ChooseStandardAksara(standardAksaraIndex++);
        if (standardAksara == null)
        {
            Debug.LogWarning("BossEnemy: standardAksaraPool membutuhkan minimal 2 AksaraData.");
            return;
        }

        Vector3 spawnPosition = GetStandardSpawnPosition(slot);
        Transform parent = transform.parent;
        EnemyGestureCommand standardEnemy = Instantiate(
            standardEnemyPrefab,
            spawnPosition,
            Quaternion.identity,
            parent
        );

        standardEnemy.SetAutoIssueOnStart(false);
        standardEnemy.SetCanReceiveChallengeDuringBoss(true);

        Enemy enemyVisual = standardEnemy.GetComponent<Enemy>();
        if (enemyVisual != null && standardEnemyData != null)
        {
            enemyVisual.Configure(standardEnemyData, standardAksara);
        }
        else
        {
            standardEnemy.ConfigureChallenge(standardAksara.GestureShape, 1);
        }

        enemyVisual?.SetDropEnabled(false);

        standardEnemy.SyncSpawnPosition();
        standardEnemy.IssueCommand();
        standardEnemies.Add(standardEnemy);
        standardEnemySlots[standardEnemy] = slot;
        occupiedStandardSlots[slot] = true;
        LevelProgressManager.Instance?.AddTotalProgressUnits(1);

        Debug.Log($"[Boss] Spawn enemy standard di slot {slot + 1}/2 dengan aksara {standardAksara.AksaraName}.");
    }

    private int FindFreeStandardSlot()
    {
        for (int i = 0; i < occupiedStandardSlots.Length; i++)
        {
            if (!occupiedStandardSlots[i])
                return i;
        }

        return -1;
    }

    private void HandleStandardEnemyDefeated(EnemyGestureCommand defeatedEnemy)
    {
        if (!standardEnemySlots.TryGetValue(defeatedEnemy, out int slot))
            return;

        standardEnemySlots.Remove(defeatedEnemy);
        standardEnemies.Remove(defeatedEnemy);
        occupiedStandardSlots[slot] = false;

        if (state == 1 && !isTransitioning && standardEnemies.Count == 0 &&
            standardRespawnCoroutine == null)
        {
            standardRespawnCoroutine = StartCoroutine(RespawnStandardPairAfterDelay());
        }
    }

    private IEnumerator RespawnStandardPairAfterDelay()
    {
        yield return new WaitForSeconds(standardEnemyRespawnDelay);
        standardRespawnCoroutine = null;

        if (state != 1 || isTransitioning)
            yield break;

        SpawnStandardEnemy();

        if (initialStandardEnemySpawnDelay > 0f)
            yield return new WaitForSeconds(initialStandardEnemySpawnDelay);

        if (state == 1 && !isTransitioning)
            SpawnStandardEnemy();
    }

    private AksaraData ChooseStandardAksara(int index)
    {
        List<AksaraData> candidates = new List<AksaraData>();

        foreach (AksaraData aksara in standardAksaraPool)
        {
            if (aksara == null || candidates.Contains(aksara))
                continue;

            bool usedByBoss = false;
            for (int i = 0; i < currentAksara.Length; i++)
            {
                if (currentAksara[i] != null && currentAksara[i].GestureShape == aksara.GestureShape)
                {
                    usedByBoss = true;
                    break;
                }
            }

            if (!usedByBoss)
                candidates.Add(aksara);
        }

        if (candidates.Count < 2)
            candidates = new List<AksaraData>(standardAksaraPool);

        candidates.RemoveAll(aksara => aksara == null);
        if (candidates.Count == 0)
            return null;

        return candidates[index % candidates.Count];
    }

    private Vector3 GetStandardSpawnPosition(int index)
    {
        Vector2 localOffset = index == 0
            ? standardEnemySpawnOffsetLeft
            : standardEnemySpawnOffsetRight;

        return transform.TransformPoint(localOffset);
    }

    private void Subscribe()
    {
        if (gestureDrawer == null || isListening)
            return;

        gestureDrawer.GestureRecognized += HandleGestureRecognized;
        isListening = true;
    }

    private void Unsubscribe()
    {
        if (gestureDrawer == null || !isListening)
            return;

        gestureDrawer.GestureRecognized -= HandleGestureRecognized;
        isListening = false;
    }

    private void HandleGestureRecognized(List<List<Vector2>> strokes, GestureRecognitionResult result)
    {
        if (!HasActiveBoss || state == 0 || isTransitioning)
            return;

        Debug.Log(
            $"[Boss] State {state} meminta: {GetRequestedAksaraLog()} | " +
            $"terdeteksi: {(result.IsRecognized ? result.DetectedShape.ToString() : "Tidak dikenali")}"
        );

        if (!result.IsRecognized || !IsVisibleOnCamera())
            return;

        int matchedIndex = FindUnsolvedAksara(result.DetectedShape);
        if (matchedIndex < 0)
        {
            Debug.Log($"[Boss] Aksara {result.DetectedShape} bukan permintaan aktif atau sudah selesai.");
            return;
        }

        solvedAksara[matchedIndex] = true;
        LevelProgressManager.Instance?.OnEnemyProcessed();
        PlayAksaraSFX(currentAksara[matchedIndex].GestureShape);
        Debug.Log($"[Boss] Aksara {result.DetectedShape} benar. Sisa: {GetRequestedAksaraLog()}");

        if (!AreAllAksaraSolved())
        {
            HideIconAndPlayVfx(matchedIndex, null);
            return;
        }

        isTransitioning = true;
        if (state == 1)
        {
            HideIconAndPlayVfx(matchedIndex, null);
            AdvanceToStateTwo();
            return;
        }

        HideIconAndPlayVfx(matchedIndex, null);
        DefeatBoss();
    }

    private AksaraData[] ChooseUniqueAksara()
    {
        List<AksaraData> shuffled = new List<AksaraData>(aksaraPool);
        AksaraData[] selected = new AksaraData[3];

        for (int i = 0; i < selected.Length; i++)
        {
            int randomIndex = Random.Range(0, shuffled.Count);
            selected[i] = shuffled[randomIndex];
            shuffled.RemoveAt(randomIndex);
        }

        return selected;
    }

    private void SetIcons(AksaraData[] selected)
    {
        for (int i = 0; i < currentAksara.Length; i++)
        {
            currentAksara[i] = selected[i];
            solvedAksara[i] = false;

            if (i < aksaraIconRenderers.Length && aksaraIconRenderers[i] != null)
            {
                aksaraIconRenderers[i].sprite = selected[i].IconSprite;
                aksaraIconRenderers[i].enabled = true;
            }
        }
    }

    private int FindUnsolvedAksara(GestureShape detectedShape)
    {
        for (int i = 0; i < currentAksara.Length; i++)
        {
            if (!solvedAksara[i] && currentAksara[i] != null &&
                currentAksara[i].GestureShape == detectedShape)
                return i;
        }

        return -1;
    }

    private bool AreAllAksaraSolved()
    {
        for (int i = 0; i < aksaraPerState; i++)
        {
            if (!solvedAksara[i])
                return false;
        }

        return true;
    }

    private string GetRequestedAksaraLog()
    {
        List<string> requested = new List<string>();

        for (int i = 0; i < currentAksara.Length; i++)
        {
            if (currentAksara[i] != null && !solvedAksara[i])
                requested.Add(currentAksara[i].AksaraName);
        }

        return requested.Count > 0 ? string.Join(", ", requested) : "Selesai";
    }

    private void HideIconAndPlayVfx(int iconIndex, System.Action onComplete)
    {
        if (iconIndex < 0 || iconIndex >= aksaraIconRenderers.Length)
        {
            onComplete?.Invoke();
            return;
        }

        SpriteRenderer iconRenderer = aksaraIconRenderers[iconIndex];
        if (iconRenderer == null)
        {
            onComplete?.Invoke();
            return;
        }

        Vector3 iconPosition = iconRenderer.transform.position;
        iconRenderer.enabled = false;
        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.PlayNonCollectibleItemVfx(iconPosition, onComplete);
        else
            onComplete?.Invoke();
    }

    private void AdvanceToStateTwo()
    {
        isTransitioning = false;
        state = 2;
        SetIcons(ChooseUniqueAksara());
        movementBehavior.SetSpeedFromData(stateTwoMoveSpeed);
        movementBehavior.SetActive(true);
        Debug.Log("BossEnemy masuk state 2 setelah aksara state 1 berhasil.");
    }

    private void HideAllIcons()
    {
        for (int i = 0; i < aksaraIconRenderers.Length; i++)
        {
            if (aksaraIconRenderers[i] != null)
                aksaraIconRenderers[i].enabled = false;
        }
    }

    private void DefeatBoss()
    {
        state = 0;
        isTransitioning = false;
        HasActiveBoss = false;
        Unsubscribe();
        movementBehavior.SetActive(false);
        HideAllIcons();
        PlayDefeatSFX();

        StartCoroutine(BossDefeatRoutine());
    }

    private void PlayDefeatSFX()
    {
        if (useDefeatSFX && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(defeatSFXName, defeatSFXVolume);
    }

    private void PlayAksaraSFX(GestureShape gestureShape)
    {
        if (!useAksaraSFX || aksaraSoundLibrary == null || AudioManager.Instance == null)
            return;

        AudioClip clip = aksaraSoundLibrary.GetClip(gestureShape);
        if (clip != null)
        {
            float volume = aksaraSoundLibrary.GetVolume(gestureShape);
            AudioManager.Instance.PlayLoudSFX(
                clip,
                Mathf.Clamp(aksaraSFXVolume * volume * 8f, 0f, 10f)
            );
        }
    }

    private IEnumerator BossDefeatRoutine()
    {
        float elapsed = 0f;
        bool isVisible = true;
        float interval = Mathf.Max(0.01f, defeatBlinkInterval);

        while (elapsed < defeatBlinkDuration)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;
            isVisible = !isVisible;

            if (bodyRenderer != null)
                bodyRenderer.enabled = isVisible;
        }

        if (bodyRenderer != null)
            bodyRenderer.enabled = true;

        yield return ShrinkBossRoutine();

        if (bodyRenderer != null)
            bodyRenderer.enabled = false;

        PlayDefeatAnimation();
        yield return new WaitForSeconds(GetDefeatAnimationDuration());
        Destroy(gameObject);
    }

    private IEnumerator ShrinkBossRoutine()
    {
        if (bodyRenderer == null || defeatShrinkDuration <= 0f)
            yield break;

        Transform bodyTransform = bodyRenderer.transform;
        Vector3 initialScale = bodyTransform.localScale;
        Vector3 targetScale = initialScale * defeatShrinkTargetScale;
        float elapsed = 0f;

        while (elapsed < defeatShrinkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / defeatShrinkDuration);
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            bodyTransform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
            yield return null;
        }

        bodyTransform.localScale = targetScale;
    }

    private void PlayDefeatAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        int stateHash = Animator.StringToHash(defeatAnimationStateName);
        if (animator.HasState(0, stateHash))
            animator.Play(stateHash, 0, 0f);
    }

    private float GetDefeatAnimationDuration()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0f;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == defeatAnimationStateName)
                return clips[i].length;
        }

        return 0f;
    }

    private bool IsVisibleOnCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return true;

        Vector3 viewportPoint = camera.WorldToViewportPoint(transform.position);
        return viewportPoint.z > 0f && viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
               viewportPoint.y >= 0f && viewportPoint.y <= 1f;
    }
}