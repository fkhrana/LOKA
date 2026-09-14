using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using EasyTransition;

public class TutorialManager : MonoBehaviour
{
    public static bool IsTrainingMode { get; private set; } = false;

    public enum SpawnSide { Left, Right }

    // ============================================================
    // ENEMY
    // ============================================================
    [Header("Enemy")]
    [SerializeField] private EnemyGestureCommand enemyPrefab;
    [SerializeField] private Transform worldParent;
    [SerializeField] private Vector3 enemySpawnPos = new Vector3(0f, 1f, 0f);

    [SerializeField, Min(1)] private int enemyCount = 2;

    [SerializeField] private SpawnSide spawnSide = SpawnSide.Right;

    [Range(0f, 1f)]
    [SerializeField] private float enemySpawnAnchorY = 0.5f;

    [SerializeField] private float enemySpawnSpacingY = 0.18f;

    [SerializeField] private float offscreenPadding = 0.15f;

    [Tooltip("Musuh berhenti kalau viewport X-nya sudah melewati nilai ini. " +
             "Semakin kecil = musuh makin masuk ke layar sebelum freeze.")]
    [SerializeField] private float enemyStopViewportX = 0.5f;

    [Header("Enemy Approach")]
    [SerializeField] private bool useEnemyApproach = true;
    [SerializeField] private float enemyStopDistanceToPlayer = 3f;
    [SerializeField] private float approachTimeoutSeconds = 10f;

    [Header("Enemy Data Pool")]
    [SerializeField] private EnemyData[] tutorialEnemyDataPool;
    [SerializeField] private EnemyData tutorialEnemyDataFallback;

    // ============================================================
    // TUTORIAL SAFETY
    // ============================================================
    [Header("Tutorial Safety")]
    [SerializeField] private bool disableColliderDuringApproach = true;
    [SerializeField] private bool fallbackToCanvasCenterIfEnemyLost = true;
    [SerializeField] private bool forceDefeatEnemyAfterGesture = true;
    [SerializeField] private float postGestureDefeatDelay = 0.2f;

    // ============================================================
    // TIMING
    // ============================================================
    [Header("Timing")]
    [SerializeField] private float delayBeforeDotsAndHand = 0.5f;
    [Tooltip("Jeda setelah musuh terakhir mati sebelum cari fragment (kasih waktu drop animation).")]
    [SerializeField] private float delayAfterEnemyDefeated = 1.2f;
    [SerializeField] private float delayBetweenEnemies = 0.4f;
    [Tooltip("Jeda setelah player tap fragment sebelum cek fragment berikutnya.")]
    [SerializeField] private float delayAfterFragmentTap = 1.5f;

    [Header("Timeouts (detik)")]
    [SerializeField] private float gestureDrawTimeout = 60f;
    [SerializeField] private float enemyDestroyTimeout = 10f;
    [SerializeField] private float fragmentTapTimeout = 30f;
    [SerializeField] private float collectionClickTimeout = 30f;
    [SerializeField] private float collectionOpenTimeout = 3f;
    [SerializeField] private float collectionCloseTimeout = 60f;

    // ============================================================
    // AKSARA
    // ============================================================
    [Header("Aksara Pool (butuh minimal 2 aksara berbeda untuk 2 musuh)")]
    [SerializeField] private AksaraData[] tutorialAksaraPool;

    // ============================================================
    // UI CANVAS
    // ============================================================
    [Header("UI Canvas")]
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private RectTransform tutorialCanvasRect;
    [SerializeField] private RectTransform tutorialContainer;

    [Header("UI Sprites")]
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private Sprite handSprite;

    [Header("UI Sizes")]
    [SerializeField] private float dotSizePx = 14f;
    [SerializeField] private float handSizePx = 0f;
    [SerializeField] private Color dotColor = new Color(1f, 0.9f, 0.2f);

    [Header("Dots Offset (dari musuh aktif)")]
    [Tooltip("X negatif = dots muncul di kiri musuh. -200 = dekat musuh, -500 = jauh.")]
    [SerializeField] private Vector2 dotsContainerOffset = new Vector2(-200f, 0f);

    // ============================================================
    // PATH
    // ============================================================
    [Header("Path Settings")]
    [SerializeField] private float pathPixelScale = 0f;
    [SerializeField] private int dotCount = 32;
    [Range(0.1f, 0.9f)]
    [SerializeField] private float pathMaxCanvasRatio = 0.4f;

    [Header("Hand Animation")]
    [SerializeField] private float handPathSpeedPx = 220f;
    [SerializeField] private float handLoopPause = 0.5f;
    [SerializeField] private float handTapAmplitudePx = 25f;
    [SerializeField] private float handTapSpeed = 4f;

    // ============================================================
    // REFERENCES
    // ============================================================
    [Header("UI References")]
    [SerializeField] private Button collectionButton;
    [SerializeField] private RectTransform collectionButtonRect;
    [SerializeField] private GameObject collectionPanelRoot;

    [Header("Finish Panel")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private Button mainButton;
    [SerializeField] private Button latihanButton;

    [Header("Scene")]
    [SerializeField] private string gameplayScene = "MainGameplay(Drawing)";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    // ============================================================
    // PRIVATE STATE
    // ============================================================
    private GestureDrawer gestureDrawer;
    private EnemyData currentEnemyData;
    private GestureShape currentTarget;

    private readonly List<EnemyGestureCommand> currentEnemies = new List<EnemyGestureCommand>();
    private readonly List<AksaraData> currentAksaraList = new List<AksaraData>();
    private EnemyGestureCommand activeTargetEnemy;

    private GameObject currentItem;
    private Image currentHand;

    private readonly List<GameObject> currentDots = new List<GameObject>();

    private Coroutine handAnimCoroutine;
    private Coroutine tutorialCoroutine;

    private bool waitingForDraw;
    private bool waitingForItemClick;
    private bool waitingForCollectionClick;

    private Sprite cachedDotSprite;

    private readonly List<AksaraData> aksaraBag = new List<AksaraData>();
    private readonly List<EnemyData> enemyDataBag = new List<EnemyData>();

    private Vector2? dotsCenterOverride;

    private Rect CanvasRect => tutorialCanvasRect != null
        ? tutorialCanvasRect.rect
        : new Rect(0, 0, Screen.width, Screen.height);

    private float EffectivePathScale
    {
        get
        {
            if (pathPixelScale > 0f) return pathPixelScale;

            float safeDim = Mathf.Min(CanvasRect.width, CanvasRect.height);
            float maxScale = (safeDim * pathMaxCanvasRatio) / 1.2f;
            float naturalScale = CanvasRect.height * 0.25f;

            return Mathf.Min(naturalScale, maxScale);
        }
    }

    private float EffectiveHandSize => handSizePx > 0f ? handSizePx : CanvasRect.height * 0.12f;
    private float EffectiveDotSize => dotSizePx > 0f ? dotSizePx : CanvasRect.height * 0.015f;

    // ============================================================
    // LIFECYCLE
    // ============================================================
    private void Awake()
    {
        IsTrainingMode = true;
        CameraIntroManager.GameStarted = true;
        ResolveReferences();
    }

    private void Start()
    {
        if (finishPanel != null) finishPanel.SetActive(false);
        if (mainButton != null) mainButton.onClick.AddListener(OnClickMain);
        if (latihanButton != null) latihanButton.onClick.AddListener(OnClickLatihan);
        if (collectionButton != null) collectionButton.onClick.AddListener(OnCollectionClicked);

        gestureDrawer = FindAnyObjectByType<GestureDrawer>();
        if (gestureDrawer != null)
        {
            gestureDrawer.GestureRecognized += OnGestureRecognized;
            gestureDrawer.enabled = false;
        }
        else
        {
            Debug.LogWarning("[Tutorial] GestureDrawer tidak ditemukan!");
        }

        tutorialCoroutine = StartCoroutine(TutorialSequence());
    }

    private void OnDestroy()
    {
        IsTrainingMode = false;

        if (tutorialCoroutine != null) StopCoroutine(tutorialCoroutine);

        if (gestureDrawer != null)
            gestureDrawer.GestureRecognized -= OnGestureRecognized;
        if (mainButton != null) mainButton.onClick.RemoveListener(OnClickMain);
        if (latihanButton != null) latihanButton.onClick.RemoveListener(OnClickLatihan);
        if (collectionButton != null) collectionButton.onClick.RemoveListener(OnCollectionClicked);

        PermanentCollectionManager.ClearTrainingCollected();

        for (int i = 0; i < currentEnemies.Count; i++)
            if (currentEnemies[i] != null) Destroy(currentEnemies[i].gameObject);
        currentEnemies.Clear();

        if (currentItem != null) Destroy(currentItem);

        ClearDots();
        ClearHand();

        Time.timeScale = 1f;
    }

    // ============================================================
    // ENEMY HELPERS
    // ============================================================
    private bool HasAnyAliveEnemy()
    {
        for (int i = 0; i < currentEnemies.Count; i++)
            if (currentEnemies[i] != null) return true;
        return false;
    }

    private EnemyGestureCommand GetFirstAliveEnemy()
    {
        for (int i = 0; i < currentEnemies.Count; i++)
            if (currentEnemies[i] != null) return currentEnemies[i];
        return null;
    }

    private int GetEnemyIndex(EnemyGestureCommand enemy)
    {
        for (int i = 0; i < currentEnemies.Count; i++)
            if (currentEnemies[i] == enemy) return i;
        return -1;
    }

    private AksaraData GetAksaraFor(EnemyGestureCommand enemy)
    {
        int idx = GetEnemyIndex(enemy);
        if (idx >= 0 && idx < currentAksaraList.Count)
            return currentAksaraList[idx];
        return null;
    }

    private Vector3 GetEnemiesCenterWorld()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            if (currentEnemies[i] == null) continue;
            sum += currentEnemies[i].transform.position;
            count++;
        }

        return count > 0 ? sum / count : Vector3.zero;
    }

    // ============================================================
    // REFERENCE RESOLVER
    // ============================================================
    private void ResolveReferences()
    {
        if (tutorialCanvas == null)
            tutorialCanvas = FindCanvasWithName("latihan", "tutorial");

        if (tutorialCanvasRect == null && tutorialCanvas != null)
            tutorialCanvasRect = tutorialCanvas.GetComponent<RectTransform>();

        if (tutorialContainer == null && tutorialCanvasRect != null)
        {
            tutorialContainer = tutorialCanvasRect;
            Debug.LogWarning("[Tutorial] tutorialContainer fallback ke Canvas RectTransform.");
        }

        if (collectionButton == null)
            collectionButton = FindCollectionButton();

        if (collectionButton != null && collectionButtonRect == null)
            collectionButtonRect = collectionButton.GetComponent<RectTransform>();

        if (collectionPanelRoot == null)
            collectionPanelRoot = FindGameObjectByName("koleksipanel", "collectionpanel");
    }

    private Canvas FindCanvasWithName(params string[] keywords)
    {
        Canvas[] all = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var c in all)
        {
            if (c == null) continue;
            string n = c.gameObject.name.ToLower();
            foreach (var k in keywords)
                if (n.Contains(k)) return c;
        }

        return null;
    }

    private Button FindCollectionButton()
    {
        Button[] all = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var b in all)
        {
            if (b == null) continue;
            string n = b.gameObject.name.ToLower();

            if (n.Contains("back")) continue;
            if (n.Contains("book") || n.Contains("collection") || n.Contains("koleksi"))
                return b;
        }

        return null;
    }

    private GameObject FindGameObjectByName(params string[] keywords)
    {
        GameObject[] all = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var go in all)
        {
            if (go == null) continue;
            string n = go.name.ToLower();
            foreach (var k in keywords)
                if (n.Contains(k)) return go;
        }

        return null;
    }

    // ============================================================
    // MAIN SEQUENCE
    // ============================================================
    private IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (!ValidateSetup())
        {
            Debug.LogError("[Tutorial] Setup tidak lengkap. Hentikan tutorial.");
            yield break;
        }

        if (!PickAksaraAndEnemy()) yield break;

        // --- Phase 1A: Spawn enemies ---
        yield return SpawnEnemiesRoutine();

        // --- Phase 1B: Approach + freeze ---
        yield return ApproachAndFreezeEnemiesRoutine();

        // --- Phase 1C: Loop per musuh ---
        int enemyIndex = 0;

        while (HasAnyAliveEnemy())
        {
            EnemyGestureCommand active = GetFirstAliveEnemy();
            if (active == null) break;

            AksaraData activeAksara = GetAksaraFor(active);
            if (activeAksara == null)
            {
                Debug.LogError($"[Tutorial] Aksara untuk musuh #{enemyIndex} tidak ditemukan.");
                break;
            }

            activeTargetEnemy = active;
            currentTarget = activeAksara.GestureShape;

            Debug.Log($"[Tutorial] === Musuh #{enemyIndex} aktif. Target: {currentTarget} ===");

            dotsCenterOverride = null;

            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(active.transform.position);
                bool offScreen = vp.z < 0f || vp.x < 0.05f || vp.x > 0.95f || vp.y < 0.05f || vp.y > 0.95f;

                if (offScreen && fallbackToCanvasCenterIfEnemyLost)
                {
                    Debug.LogWarning($"[Tutorial] Musuh aktif di luar viewport ({vp}). Fallback ke tengah canvas.");
                    dotsCenterOverride = new Vector2(0f, CanvasRect.height * 0.05f) + dotsContainerOffset;
                }
            }

            yield return new WaitForSeconds(delayBeforeDotsAndHand);

            SpawnDottedPath();
            SpawnHandOnPath();
            PlayHandAlongPath();

            if (gestureDrawer != null)
            {
                gestureDrawer.enabled = true;
                Debug.Log($"[Tutorial] GestureDrawer aktif untuk musuh #{enemyIndex}.");
            }

            waitingForDraw = true;
            yield return WaitUntilOrTimeout(
                () => !waitingForDraw,
                gestureDrawTimeout,
                $"Gesture draw musuh #{enemyIndex}");
            waitingForDraw = false;

            ClearHand();
            ClearDots();
            dotsCenterOverride = null;

            if (gestureDrawer != null) gestureDrawer.enabled = false;

            yield return new WaitForSeconds(postGestureDefeatDelay);

            if (active != null && forceDefeatEnemyAfterGesture)
            {
                Debug.Log($"[Tutorial] Force defeat musuh #{enemyIndex}.");

                Enemy comp = active.GetComponent<Enemy>();
                if (comp != null)
                {
                    active.StopCommandMode();
                    comp.OnDefeated();
                    comp.StartDefeatBlink();

                    float blinkDur = Mathf.Max(0.1f, comp.DefeatBlinkDuration);
                    yield return new WaitForSeconds(blinkDur);
                }
            }

            yield return WaitUntilOrTimeout(
                () => active == null,
                enemyDestroyTimeout,
                $"Musuh #{enemyIndex} destroy");

            if (active != null)
            {
                Debug.LogWarning($"[Tutorial] Force destroy musuh #{enemyIndex}.");
                Destroy(active.gameObject);
            }

            activeTargetEnemy = null;
            enemyIndex++;

            if (HasAnyAliveEnemy())
                yield return new WaitForSeconds(delayBetweenEnemies);
        }

        // ============================================================
        // Phase 4: Kumpulkan SEMUA fragment
        // ============================================================
        yield return new WaitForSeconds(delayAfterEnemyDefeated);

        int fragmentsCollected = 0;
        int maxFragments = Mathf.Max(enemyCount, 1);
        float phaseStart = Time.time;
        float maxPhaseTime = maxFragments * (fragmentTapTimeout + delayAfterFragmentTap + 2f);

        while (fragmentsCollected < maxFragments &&
               (Time.time - phaseStart) < maxPhaseTime)
        {
            GameObject frag = FindActiveFragment();
            if (frag == null) break;

            currentItem = frag;
            Debug.Log($"[Tutorial] Fragment #{fragmentsCollected + 1} muncul: {frag.name}");

            yield return TapFragmentRoutine();
            fragmentsCollected++;

            // Tunggu fragment hilang (terbang ke book)
            float waitStart = Time.time;
            while (currentItem != null && (Time.time - waitStart) < 3f)
                yield return null;

            yield return new WaitForSeconds(delayAfterFragmentTap);
        }

        if (fragmentsCollected == 0)
            Debug.LogWarning("[Tutorial] Tidak ada fragment yang dikumpulkan.");
        else
            Debug.Log($"[Tutorial] {fragmentsCollected} fragment dikumpulkan.");

        // --- Phase 5: Tap Book Button + tunggu panel ---
        yield return TapBookButtonRoutine();
        yield return WaitCollectionPanelCycle();

        // --- Phase 6: Finish ---
        ShowFinishPanel();
        tutorialCoroutine = null;
    }

    private IEnumerator WaitUntilOrTimeout(Func<bool> condition, float timeout, string label)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!condition())
            Debug.LogWarning($"[Tutorial] Timeout menunggu: {label} ({timeout:F1}s)");
    }

    private bool PickAksaraAndEnemy()
    {
        currentAksaraList.Clear();

        int count = Mathf.Max(1, enemyCount);

        while (aksaraBag.Count < count) RefillAksaraBag();

        for (int i = 0; i < count; i++)
        {
            AksaraData a = PickAksaraFromBag();

            if (a == null)
            {
                if (currentAksaraList.Count > 0)
                    a = currentAksaraList[UnityEngine.Random.Range(0, currentAksaraList.Count)];
                else
                {
                    Debug.LogError("[Tutorial] AksaraData pool kosong!");
                    return false;
                }
            }

            currentAksaraList.Add(a);
        }

        currentEnemyData = PickEnemyDataFromBag();
        if (currentEnemyData == null)
        {
            Debug.LogError("[Tutorial] EnemyData pool kosong!");
            return false;
        }

        string aksaraStr = "";
        for (int i = 0; i < currentAksaraList.Count; i++)
            aksaraStr += $"{currentAksaraList[i].GestureShape} ";

        Debug.Log($"[Tutorial] Picked {currentAksaraList.Count} aksara: {aksaraStr}");
        return true;
    }

    private IEnumerator TapFragmentRoutine()
    {
        if (currentItem == null) yield break;

        SpawnHandAtWorld(currentItem.transform.position);

        waitingForItemClick = true;
        yield return WaitUntilOrTimeout(() => !waitingForItemClick, fragmentTapTimeout, "Fragment tap");
        waitingForItemClick = false;

        ClearHand();
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator TapBookButtonRoutine()
    {
        ClearHand();

        if (collectionButtonRect == null)
        {
            Debug.LogError("[Tutorial] collectionButtonRect null!");
            yield break;
        }

        SpawnHandAtUI(collectionButtonRect);

        waitingForCollectionClick = true;
        yield return WaitUntilOrTimeout(() => !waitingForCollectionClick, collectionClickTimeout, "Book button click");
        waitingForCollectionClick = false;

        ClearHand();
        yield return new WaitForSeconds(0.3f);

        OpenCollectionPanel();
    }

    private IEnumerator WaitCollectionPanelCycle()
    {
        if (collectionPanelRoot == null)
        {
            Debug.LogWarning("[Tutorial] collectionPanelRoot null → fallback delay 3s.");
            yield return new WaitForSecondsRealtime(3f);
            yield break;
        }

        yield return WaitUntilOrTimeout(
            () => collectionPanelRoot.activeInHierarchy,
            collectionOpenTimeout,
            "Collection panel open");

        if (!collectionPanelRoot.activeInHierarchy)
        {
            Debug.LogWarning("[Tutorial] Panel koleksi tidak buka. Lanjut ke finish.");
            ClearHand();
            ClearDots();
            yield break;
        }

        yield return WaitUntilOrTimeout(
            () => !collectionPanelRoot.activeInHierarchy,
            collectionCloseTimeout,
            "Collection panel close");

        yield return new WaitForSecondsRealtime(0.3f);
        ClearHand();
        ClearDots();
    }

    private void ShowFinishPanel()
    {
        Debug.Log("[Tutorial] Menampilkan finish panel.");

        if (collectionButton != null)
            collectionButton.gameObject.SetActive(false);

        if (finishPanel != null)
            finishPanel.SetActive(true);

        Time.timeScale = 1f;
    }

    private void OpenCollectionPanel()
    {
        if (CollectionPanel.Instance != null)
        {
            CollectionPanel.Instance.OpenCollection();
            return;
        }

        if (collectionPanelRoot != null)
        {
            collectionPanelRoot.SetActive(true);
            Debug.LogWarning("[Tutorial] CollectionPanel.Instance null → force SetActive.");
        }
    }

    private bool ValidateSetup()
    {
        bool ok = true;

        if (enemyPrefab == null) { Debug.LogError("[Tutorial] enemyPrefab kosong!"); ok = false; }

        int poolSize = tutorialEnemyDataPool != null ? tutorialEnemyDataPool.Length : 0;
        if (poolSize == 0 && tutorialEnemyDataFallback == null)
        {
            Debug.LogError("[Tutorial] EnemyData pool + fallback kosong!");
            ok = false;
        }

        if (tutorialCanvas == null) { Debug.LogError("[Tutorial] tutorialCanvas kosong!"); ok = false; }
        if (tutorialCanvasRect == null) { Debug.LogError("[Tutorial] tutorialCanvasRect kosong!"); ok = false; }
        if (tutorialContainer == null) { Debug.LogError("[Tutorial] tutorialContainer kosong!"); ok = false; }
        if (handSprite == null) Debug.LogWarning("[Tutorial] handSprite kosong.");
        if (gestureDrawer == null) Debug.LogWarning("[Tutorial] gestureDrawer null.");

        int aksaraPoolSize = tutorialAksaraPool != null ? tutorialAksaraPool.Length : 0;
        if (aksaraPoolSize < Mathf.Max(1, enemyCount))
        {
            Debug.LogWarning($"[Tutorial] Aksara pool hanya {aksaraPoolSize} sedangkan enemyCount {enemyCount}. " +
                             "Aksara akan di-duplikasi → fragment kedua mungkin tidak drop.");
        }

        return ok;
    }

    // ============================================================
    // SHUFFLE BAG
    // ============================================================
    private AksaraData PickAksaraFromBag()
    {
        if (aksaraBag.Count == 0) RefillAksaraBag();
        if (aksaraBag.Count == 0) return null;

        AksaraData picked = aksaraBag[0];
        aksaraBag.RemoveAt(0);
        return picked;
    }

    private void RefillAksaraBag()
    {
        aksaraBag.Clear();
        if (tutorialAksaraPool == null) return;

        foreach (var a in tutorialAksaraPool)
        {
            if (a == null) continue;
            if (!TutorialLetterPaths.IsSupported(a.GestureShape)) continue;
            aksaraBag.Add(a);
        }

        Shuffle(aksaraBag);
    }

    private EnemyData PickEnemyDataFromBag()
    {
        if (enemyDataBag.Count == 0) RefillEnemyDataBag();
        if (enemyDataBag.Count == 0) return tutorialEnemyDataFallback;

        EnemyData picked = enemyDataBag[0];
        enemyDataBag.RemoveAt(0);
        return picked;
    }

    private void RefillEnemyDataBag()
    {
        enemyDataBag.Clear();

        if (tutorialEnemyDataPool != null)
            foreach (var d in tutorialEnemyDataPool)
                if (d != null) enemyDataBag.Add(d);

        if (enemyDataBag.Count == 0 && tutorialEnemyDataFallback != null)
            enemyDataBag.Add(tutorialEnemyDataFallback);

        Shuffle(enemyDataBag);
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ============================================================
    // SPAWN ENEMIES
    // ============================================================
    private IEnumerator SpawnEnemiesRoutine()
    {
        currentEnemies.Clear();

        Transform parent = worldParent != null ? worldParent : transform;
        int count = Mathf.Max(1, enemyCount);

        for (int i = 0; i < count; i++)
        {
            float viewportOffset = (i - (count - 1) * 0.5f) * enemySpawnSpacingY;
            Vector3 spawnPos = GetEnemyWorldSpawnPosition(viewportOffset);

            EnemyGestureCommand enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, parent);
            if (enemy == null)
            {
                Debug.LogError($"[Tutorial] Spawn enemy #{i} gagal!");
                continue;
            }

            enemy.name = $"TutorialEnemy_{i}";
            enemy.transform.localScale = Vector3.one;
            enemy.SetAutoIssueOnStart(false);

            var capturedEnemy = enemy;
            var guard = enemy.gameObject.AddComponent<TutorialEnemyGuard>();
            guard.OnDestroyed += () =>
            {
                string name = capturedEnemy != null ? capturedEnemy.name : "(destroyed)";
                Debug.LogWarning($"[Tutorial] {name} destroy pada t={Time.time:F2}s (frame={Time.frameCount})");
            };

            if (disableColliderDuringApproach)
            {
                var cols = enemy.GetComponentsInChildren<Collider2D>(true);
                foreach (var c in cols) c.enabled = false;
            }

            currentEnemies.Add(enemy);
        }

        if (currentEnemies.Count == 0)
        {
            Debug.LogError("[Tutorial] Tidak ada enemy yang berhasil spawn.");
            yield break;
        }

        yield return null;
        yield return null;

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            var enemy = currentEnemies[i];
            if (enemy == null) continue;

            AksaraData aksara = (i < currentAksaraList.Count) ? currentAksaraList[i] : currentAksaraList[0];

            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
                enemyComponent.Configure(currentEnemyData, aksara);
            else
                enemy.ConfigureChallenge(aksara.GestureShape, 1);

            enemy.SyncSpawnPosition();

            var movement = enemy.GetComponent<EnemyMovementBehavior>();
            if (movement != null && useEnemyApproach)
            {
                movement.SetMovementPaused(false);
                movement.SetActive(true);
            }

            TMP_Text label = enemy.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = aksara.GestureShape.ToString();

            Debug.Log($"[Tutorial] Enemy #{i} configured. Aksara: {aksara.GestureShape}");
        }

        yield return null;

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            var enemy = currentEnemies[i];
            if (enemy == null) continue;

            var movement = enemy.GetComponent<EnemyMovementBehavior>();
            if (movement != null && useEnemyApproach)
            {
                movement.SetMovementPaused(false);
                movement.SetActive(true);
            }
        }

        Debug.Log($"[Tutorial] {currentEnemies.Count} enemy siap.");
    }

    // ============================================================
    // APPROACH + FREEZE (ALL)
    // ============================================================
    private IEnumerator ApproachAndFreezeEnemiesRoutine()
    {
        if (!HasAnyAliveEnemy()) yield break;

        if (!useEnemyApproach)
        {
            FreezeAllEnemies();
            yield break;
        }

        Transform playerTransform = FindPlayerTransform();
        Camera cam = Camera.main;

        Debug.Log($"[Tutorial] Approach start. side={spawnSide}, " +
                  $"targetDist={enemyStopDistanceToPlayer}, targetVPx={enemyStopViewportX}");

        float startTime = Time.time;
        float nextLog = startTime + 1f;
        bool frozen = false;

        while (Time.time - startTime < approachTimeoutSeconds)
        {
            if (!HasAnyAliveEnemy())
            {
                Debug.LogError($"[Tutorial] Semua enemy HILANG di tengah approach setelah {Time.time - startTime:F2}s.");
                yield break;
            }

            Vector3 center = GetEnemiesCenterWorld();

            if (Time.time >= nextLog)
            {
                float d = playerTransform != null ? Vector2.Distance(center, playerTransform.position) : -1f;
                Vector3 vp = cam != null ? cam.WorldToViewportPoint(center) : Vector3.zero;
                Debug.Log($"[Tutorial] t={Time.time - startTime:F1}s center={center} dist={d:F2} vpX={vp.x:F2}");
                nextLog += 1f;
            }

            if (playerTransform != null && enemyStopDistanceToPlayer > 0f)
            {
                float dist = Vector2.Distance(center, playerTransform.position);
                if (dist <= enemyStopDistanceToPlayer)
                {
                    Debug.Log($"[Tutorial] Center dekat player (dist={dist:F2}). Freeze.");
                    FreezeAllEnemies();
                    frozen = true;
                    break;
                }
            }

            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(center);

                bool reachedStop = spawnSide == SpawnSide.Left
                    ? vp.x >= enemyStopViewportX
                    : vp.x <= enemyStopViewportX;

                if (reachedStop)
                {
                    Debug.Log($"[Tutorial] Center viewport X={vp.x:F2} sudah sampai target " +
                              $"{enemyStopViewportX:F2}. Freeze.");
                    FreezeAllEnemies();
                    frozen = true;
                    break;
                }
            }

            yield return null;
        }

        if (!frozen && HasAnyAliveEnemy())
        {
            Debug.LogWarning("[Tutorial] Approach timeout. Freeze paksa.");
            FreezeAllEnemies();
        }
    }

    private void FreezeAllEnemies()
    {
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            var enemy = currentEnemies[i];
            if (enemy == null) continue;

            var movement = enemy.GetComponent<EnemyMovementBehavior>();
            if (movement != null)
            {
                movement.SetMovementPaused(true);
                movement.SetActive(false);
            }

            enemy.SyncSpawnPosition();
        }

        Debug.Log("[Tutorial] Semua enemy FROZEN.");
    }

    private Transform FindPlayerTransform()
    {
        var ph = FindAnyObjectByType<PlayerHealth>();
        return ph != null ? ph.transform : null;
    }

    private Vector3 GetEnemyWorldSpawnPosition(float viewportYOffset)
    {
        Camera cam = Camera.main;
        if (cam == null) return enemySpawnPos;

        float anchorX = spawnSide == SpawnSide.Left
            ? -offscreenPadding
            : 1f + offscreenPadding;

        float anchorY = Mathf.Clamp01(enemySpawnAnchorY + viewportYOffset);

        float z = Mathf.Abs(cam.transform.position.z - enemySpawnPos.z);
        if (z < 0.1f) z = 10f;

        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(anchorX, anchorY, z));
        worldPos.z = enemySpawnPos.z;
        return worldPos;
    }

    // ============================================================
    // FRAGMENT FINDER
    // ============================================================
    private GameObject FindActiveFragment()
    {
        AksaraFragmentItem[] all = FindObjectsByType<AksaraFragmentItem>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var f in all)
        {
            if (f != null && f.gameObject.activeInHierarchy)
                return f.gameObject;
        }

        return null;
    }

    // ============================================================
    // DOTTED PATH
    // ============================================================
    private Vector2 GetDotsCenterUI()
    {
        if (dotsCenterOverride.HasValue) return dotsCenterOverride.Value;

        if (activeTargetEnemy != null)
            return WorldToContainerLocal(activeTargetEnemy.transform.position) + dotsContainerOffset;

        if (!HasAnyAliveEnemy()) return Vector2.zero;
        return WorldToContainerLocal(GetEnemiesCenterWorld()) + dotsContainerOffset;
    }

    private void SpawnDottedPath()
    {
        if (tutorialContainer == null) return;
        ClearDots();

        List<Vector2> path = TutorialLetterPaths.GetPath(currentTarget);
        if (path == null || path.Count < 2) return;

        float totalLen = GetPathLength(path);
        if (totalLen <= 0f) return;

        Vector2 center = GetDotsCenterUI();
        float scale = EffectivePathScale;

        int dotTotal = Mathf.Max(dotCount, 8);
        float step = totalLen / dotTotal;

        int segIdx = 0;
        float travelled = 0f;
        Vector2 segStart = path[0];

        for (int i = 0; i <= dotTotal; i++)
        {
            float targetDist = i * step;

            while (segIdx < path.Count - 1)
            {
                Vector2 segEnd = path[segIdx + 1];
                float segLen = Vector2.Distance(segStart, segEnd);

                if (travelled + segLen >= targetDist)
                {
                    float t = (targetDist - travelled) / segLen;
                    Vector2 local = Vector2.Lerp(segStart, segEnd, t);
                    CreateDot(center + local * scale);
                    break;
                }

                travelled += segLen;
                segStart = segEnd;
                segIdx++;
            }
        }
    }

    private void CreateDot(Vector2 anchoredPos)
    {
        var dotGO = new GameObject("TutorialDot", typeof(RectTransform), typeof(Image));
        dotGO.transform.SetParent(tutorialContainer, false);
        dotGO.transform.SetAsLastSibling();

        var img = dotGO.GetComponent<Image>();
        img.sprite = GetDotSprite();
        img.color = dotColor;
        img.raycastTarget = false;

        var rt = dotGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.one * EffectiveDotSize;
        rt.anchoredPosition = anchoredPos;

        currentDots.Add(dotGO);
    }

    private float GetPathLength(List<Vector2> path)
    {
        float len = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            len += Vector2.Distance(path[i], path[i + 1]);
        return len;
    }

    // ============================================================
    // HAND
    // ============================================================
    private void SpawnHandOnPath()
    {
        if (tutorialContainer == null || handSprite == null) return;
        currentHand = CreateHandImage(tutorialContainer);
    }

    private Image CreateHandImage(RectTransform parent)
    {
        var handGO = new GameObject("TutorialHand", typeof(RectTransform), typeof(Image));
        handGO.transform.SetParent(parent, false);
        handGO.transform.SetAsLastSibling();

        var img = handGO.GetComponent<Image>();
        img.sprite = handSprite;
        img.raycastTarget = false;
        img.preserveAspect = true;

        var rt = handGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.one * EffectiveHandSize;

        return img;
    }

    private void SpawnHandAtUI(RectTransform targetUI, bool withTapping = true)
    {
        ClearHand();
        if (targetUI == null || handSprite == null || tutorialContainer == null) return;

        currentHand = CreateHandImage(tutorialContainer);

        Vector3 worldCenter = targetUI.TransformPoint(targetUI.rect.center);
        Camera srcCam = GetCanvasCameraFor(targetUI);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(srcCam, worldCenter);

        Camera dstCam = GetCanvasCameraFor(tutorialContainer);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tutorialContainer, screenPos, dstCam, out Vector2 localPos);

        currentHand.rectTransform.anchoredPosition = localPos;

        if (withTapping)
        {
            if (handAnimCoroutine != null) StopCoroutine(handAnimCoroutine);
            handAnimCoroutine = StartCoroutine(HandTapRoutine(localPos));
        }
    }

    private void SpawnHandAtWorld(Vector3 worldPos, bool withTapping = true)
    {
        ClearHand();
        if (handSprite == null || tutorialContainer == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        currentHand = CreateHandImage(tutorialContainer);

        Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
        Camera dstCam = GetCanvasCameraFor(tutorialContainer);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tutorialContainer, screenPos, dstCam, out Vector2 localPos);

        currentHand.rectTransform.anchoredPosition = localPos;

        if (withTapping)
        {
            if (handAnimCoroutine != null) StopCoroutine(handAnimCoroutine);
            handAnimCoroutine = StartCoroutine(HandTapRoutine(localPos));
        }
    }

    private void PlayHandAlongPath()
    {
        if (currentHand == null) return;
        if (handAnimCoroutine != null) StopCoroutine(handAnimCoroutine);
        handAnimCoroutine = StartCoroutine(HandPathRoutine());
    }

    private IEnumerator HandPathRoutine()
    {
        List<Vector2> path = TutorialLetterPaths.GetPath(currentTarget);
        if (path == null || path.Count < 2) yield break;

        Vector2 center = GetDotsCenterUI();
        float scale = EffectivePathScale;

        while (true)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 a = path[i];
                Vector2 b = path[i + 1];
                float duration = (Vector2.Distance(a, b) * scale) / Mathf.Max(1f, handPathSpeedPx);
                float t = 0f;

                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    Vector2 local = Vector2.Lerp(a, b, Mathf.Clamp01(t / duration));

                    if (currentHand != null)
                        currentHand.rectTransform.anchoredPosition = center + local * scale;

                    yield return null;
                }
            }

            if (currentHand != null)
                currentHand.rectTransform.anchoredPosition = center + path[0] * scale;

            yield return new WaitForSecondsRealtime(handLoopPause);
        }
    }

    private IEnumerator HandTapRoutine(Vector2 basePos)
    {
        while (true)
        {
            float offset = Mathf.Abs(Mathf.Sin(Time.unscaledTime * handTapSpeed)) * handTapAmplitudePx;

            if (currentHand != null)
                currentHand.rectTransform.anchoredPosition = basePos - new Vector2(0f, offset);

            yield return null;
        }
    }

    private void ClearHand()
    {
        if (handAnimCoroutine != null)
        {
            StopCoroutine(handAnimCoroutine);
            handAnimCoroutine = null;
        }

        if (currentHand != null)
        {
            Destroy(currentHand.gameObject);
            currentHand = null;
        }
    }

    private void ClearDots()
    {
        foreach (var d in currentDots)
            if (d != null) Destroy(d);

        currentDots.Clear();
    }

    // ============================================================
    // COORDINATE HELPERS
    // ============================================================
    private Vector2 WorldToContainerLocal(Vector3 worldPos)
    {
        Camera cam = Camera.main;
        if (cam == null || tutorialContainer == null) return Vector2.zero;

        Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
        Camera dstCam = GetCanvasCameraFor(tutorialContainer);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tutorialContainer, screenPos, dstCam, out Vector2 localPos);

        return localPos;
    }

    private static Camera GetCanvasCameraFor(RectTransform rect)
    {
        if (rect == null) return null;

        Canvas c = rect.GetComponentInParent<Canvas>();
        if (c == null) return null;
        if (c.renderMode == RenderMode.ScreenSpaceOverlay) return null;

        return c.worldCamera;
    }

    // ============================================================
    // GESTURE
    // ============================================================
    private void OnGestureRecognized(List<List<Vector2>> strokes, GestureRecognitionResult result)
    {
        if (!waitingForDraw) return;
        if (!result.IsRecognized) return;

        if (result.DetectedShape != currentTarget)
        {
            Debug.Log($"[Tutorial] Gesture {result.DetectedShape} bukan target saat ini ({currentTarget}). Abaikan.");
            return;
        }

        Debug.Log($"[Tutorial] Berhasil gambar {currentTarget}!");
        waitingForDraw = false;
    }

    // ============================================================
    // COLLECTION CLICK
    // ============================================================
    private void OnCollectionClicked()
    {
        if (!waitingForCollectionClick) return;
        waitingForCollectionClick = false;
    }

    // ============================================================
    // INPUT (fragment tap)
    // ============================================================
    private void Update()
    {
        if (!waitingForItemClick) return;

        if (currentItem == null)
        {
            waitingForItemClick = false;
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        if (Vector2.Distance(wp, currentItem.transform.position) <= 1f)
        {
            currentItem.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
            waitingForItemClick = false;
        }
    }

    // ============================================================
    // SPRITE CACHE
    // ============================================================
    private Sprite GetDotSprite()
    {
        if (cachedDotSprite != null) return cachedDotSprite;

        cachedDotSprite = dotSprite != null
            ? dotSprite
            : CreateCircleSprite(64);

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

    // ============================================================
    // FINISH BUTTONS
    // ============================================================
    private void OnClickMain()
{
    if (finishPanel != null) finishPanel.SetActive(false);
    if (tutorialCanvas != null) tutorialCanvas.gameObject.SetActive(false);

    // Tandai tutorial sudah selesai
    PlayerPrefs.SetInt("TutorialCompleted", 1);
    PlayerPrefs.Save();

    // === Unlock Level 1 setelah tutorial selesai ===
    // Ini yang bikin player bisa masuk gameplay lewat Main Menu.
    if (LevelManager.Instance != null)
        LevelManager.Instance.UnlockFirstLevel();

    // TIDAK set "HasSeenFirstGameplayIntro" — biar intro panning
    // tetap jalan saat pertama kali masuk gameplay.

    Time.timeScale = 1f;

    TransitionManager tm = TransitionManager.Instance();

    if (tm != null && transitionSettings != null)
    {
        Debug.Log("[Tutorial] Transition ke gameplay pakai TransitionSettings.");
        tm.Transition(gameplayScene, transitionSettings, loadDelay);
    }
    else
    {
        Debug.LogWarning("[Tutorial] TransitionManager / TransitionSettings null. Load langsung.");
        SceneManager.LoadScene(gameplayScene);
    }
}

    private void OnClickLatihan()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

// ============================================================
// HELPER — diagnostik waktu destroy enemy
// ============================================================
internal class TutorialEnemyGuard : MonoBehaviour
{
    public Action OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}