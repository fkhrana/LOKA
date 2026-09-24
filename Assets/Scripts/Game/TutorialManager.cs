using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using EasyTransition;

public class TutorialManager : MonoBehaviour
{
    public static bool IsTrainingMode { get; set; } = false;
    public enum SpawnSide { Left, Right }

    [Header("Player")]
    [Tooltip("Drag GameObject player ke sini. Kalau kosong, akan dicari otomatis.")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float waitForPlayerTimeout = 5f;

    [Header("Enemy")]
    [SerializeField] private EnemyGestureCommand enemyPrefab;
    [SerializeField] private Transform worldParent;
    [SerializeField] private Vector3 enemySpawnPos = new Vector3(0f, 1f, 0f);
    [SerializeField, Min(1)] private int enemyCount = 2;
    [SerializeField] private SpawnSide spawnSide = SpawnSide.Right;
    [Range(0f, 1f)] [SerializeField] private float enemySpawnAnchorY = 0.5f;
    [SerializeField] private float enemySpawnSpacingY = 0.25f;
    [SerializeField] private float enemySpawnSpacingX = 0.05f;
    [SerializeField] private float offscreenPadding = 0.15f;
    [SerializeField] private float enemyStopViewportX = 0.5f;

    [Header("Enemy Approach")]
    [SerializeField] private bool useEnemyApproach = true;
    [SerializeField] private float enemyStopDistanceToPlayer = 3f;
    [SerializeField] private float approachTimeoutSeconds = 10f;

    [Header("Enemy Separation (anti tumpang tindih)")]
    [SerializeField] private float enemyMinSeparationWorld = 0f;
    [SerializeField] private float enemySeparationPadding = 0.5f;
    [SerializeField, Min(0)] private int separationIterations = 8;
    [SerializeField] private bool separateDuringApproach = true;
    [Range(0.05f, 1f)] [SerializeField] private float approachSeparationStrength = 0.5f;

    [Header("Enemy Data Pool")]
    [SerializeField] private EnemyData[] tutorialEnemyDataPool;
    [SerializeField] private EnemyData tutorialEnemyDataFallback;

    [Header("Tutorial Safety")]
    [SerializeField] private bool disableColliderDuringApproach = true;
    [SerializeField] private bool forceDefeatEnemyAfterGesture = true;
    [SerializeField] private float postGestureDefeatDelay = 0.2f;

    [Header("Timing")]
    [SerializeField] private float delayBeforeDotsAndHand = 0.5f;
    [SerializeField] private float delayAfterEnemyDefeated = 1.2f;
    [SerializeField] private float delayBetweenEnemies = 0.4f;
    [SerializeField] private float delayAfterFragmentTap = 1.5f;

    [Header("SFX Timing")]
    [Tooltip("Jeda antara enemy drop dan SFX hooray (detik). Biar nggak tabrakan dengan SFX correct.")]
    [Range(0f, 1f)]
    [SerializeField] private float hoorayDelayAfterDrop = 0.15f;

    [Header("Timeouts (detik)")]
    [SerializeField] private float gestureDrawTimeout = 60f;
    [SerializeField] private float enemyDestroyTimeout = 10f;
    [SerializeField] private float fragmentTapTimeout = 30f;
    [SerializeField] private float collectionClickTimeout = 30f;
    [SerializeField] private float collectionOpenTimeout = 3f;
    [SerializeField] private float collectionCloseTimeout = 60f;

    [Header("Aksara Pool")]
    [SerializeField] private AksaraData[] tutorialAksaraPool;

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

    [Header("Dots Offset (dari player)")]
    [SerializeField] private Vector2 dotsContainerOffset = new Vector2(0f, 120f);

    [Header("Path Settings")]
    [SerializeField] private float pathPixelScale = 0f;
    [SerializeField] private int dotCount = 32;
    [Range(0.1f, 0.9f)] [SerializeField] private float pathMaxCanvasRatio = 0.4f;

    [Header("Hand Animation")]
    [SerializeField] private float handPathSpeedPx = 220f;
    [SerializeField] private float handLoopPause = 0.5f;
    [SerializeField] private float handTapAmplitudePx = 25f;
    [SerializeField] private float handTapSpeed = 4f;

    [Header("UI References")]
    [SerializeField] private Button collectionButton;
    [SerializeField] private RectTransform collectionButtonRect;
    [SerializeField] private GameObject collectionPanelRoot;

    [Header("Follow Banner")]
    [Tooltip("Satu GameObject banner (background + TMP_Text). Teks diganti via array di bawah.")]
    [SerializeField] private GameObject followAksaraBanner;

    [Tooltip("Referensi ke komponen TMP_Text di dalam banner.")]
    [SerializeField] private TMP_Text followBannerText;

    [Tooltip("Teks banner untuk tiap dots. Index 0 = dots musuh pertama, 1 = dots musuh kedua, dst.")]
    [SerializeField] private string[] dotsBannerTexts =
    {
        "Yuk ikutin aksaranya!",
        "Sekarang aksara berikutnya!"
    };

    [Tooltip("Teks banner untuk tiap fragment drop. Index 0 = drop pertama, 1 = drop kedua, dst.")]
    [SerializeField] private string[] fragmentBannerTexts =
    {
        "Tap fragment yang muncul!",
        "Satu lagi, tap fragmentnya!"
    };

    [Header("Finish Panel")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private Button mainButton;
    [SerializeField] private Button latihanButton;

    [Header("Scene")]
    [SerializeField] private string gameplayScene = "MainGameplay(Drawing)";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0f;

    [Tooltip("Durasi fade out BGM sebelum pindah scene.")]
    [SerializeField] private float bgmFadeOutDuration = 0.8f;

    [Header("SFX")]
    [SerializeField] private AudioClip aksaraDropSFX;
    [SerializeField] private AudioClip hoorayAksara1SFX;
    [SerializeField] private AudioClip hoorayAksara2SFX;
    [SerializeField] private AudioClip wrongClickSFX;
    [Tooltip("SFX saat gesture berhasil (correct).")]
    [SerializeField] private AudioClip correctSFX;

    [Header("Hand Tap SFX")]
    [Tooltip("SFX tap saat hand nunjukin path aksara (dots). Main tiap loop selesai.")]
    [SerializeField] private AudioClip tapDotsSFX;

    [Tooltip("SFX tap saat hand nunjukin fragment aksara. Main tiap hand turun.")]
    [SerializeField] private AudioClip tapFragmentSFX;

    [Tooltip("SFX tap saat hand nunjukin tombol book/collection. Main tiap hand turun.")]
    [SerializeField] private AudioClip tapBookSFX;

    [Range(0f, 2f)]
    [SerializeField] private float tapSFXVolume = 0.6f;

    [Range(0f, 2f)]
    [Tooltip("Volume khusus untuk tap dots (path aksara). Bisa beda dari tap fragment/book.")]
    [SerializeField] private float tapDotsSfxVolume = 1f;

    [Header("Shake Effect (salah klik fragment)")]
    [SerializeField] private bool enableShakeOnWrongClick = true;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    private GestureDrawer gestureDrawer;
    private EnemyData currentEnemyData;
    private GestureShape currentTarget;
    private EnemyGestureCommand activeTargetEnemy;

    private readonly List<EnemyGestureCommand> currentEnemies = new List<EnemyGestureCommand>();
    private readonly List<AksaraData> currentAksaraList = new List<AksaraData>();
    private readonly List<float> cachedHalfExtents = new List<float>();
    private readonly List<GameObject> currentDots = new List<GameObject>();

    private readonly List<AksaraData> aksaraBag = new List<AksaraData>();
    private readonly List<EnemyData> enemyDataBag = new List<EnemyData>();

    private GameObject currentItem;
    private Image currentHand;

    private Coroutine handAnimCoroutine;
    private Coroutine tutorialCoroutine;
    private Coroutine shakeCoroutine;

    private bool waitingForDraw;
    private bool waitingForItemClick;
    private bool waitingForCollectionClick;

    private Sprite cachedDotSprite;
    private bool bannerActive;

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
            return Mathf.Min(CanvasRect.height * 0.25f, maxScale);
        }
    }

    private float EffectiveHandSize => handSizePx > 0f ? handSizePx : CanvasRect.height * 0.12f;
    private float EffectiveDotSize => dotSizePx > 0f ? dotSizePx : CanvasRect.height * 0.015f;

    private void Awake()
    {
        IsTrainingMode = true;
        CameraIntroManager.GameStarted = true;
        ResolveReferences();
    }

    private void Start()
    {
        if (finishPanel != null) finishPanel.SetActive(false);

        HideBanner();

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

        HideBanner();

        if (tutorialCoroutine != null) StopCoroutine(tutorialCoroutine);
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);

        if (gestureDrawer != null) gestureDrawer.GestureRecognized -= OnGestureRecognized;
        if (mainButton != null) mainButton.onClick.RemoveListener(OnClickMain);
        if (latihanButton != null) latihanButton.onClick.RemoveListener(OnClickLatihan);
        if (collectionButton != null) collectionButton.onClick.RemoveListener(OnCollectionClicked);

        PermanentCollectionManager.ClearTrainingCollected();

        for (int i = 0; i < currentEnemies.Count; i++)
            if (currentEnemies[i] != null) Destroy(currentEnemies[i].gameObject);
        currentEnemies.Clear();
        cachedHalfExtents.Clear();

        if (currentItem != null) Destroy(currentItem);

        ClearDots();
        ClearHand();
        Time.timeScale = 1f;
    }

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
        return idx >= 0 && idx < currentAksaraList.Count ? currentAksaraList[idx] : null;
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

    private void ResolveReferences()
    {
        if (tutorialCanvas == null)
            tutorialCanvas = FindCanvasWithName("latihan", "tutorial");

        if (tutorialCanvasRect == null && tutorialCanvas != null)
            tutorialCanvasRect = tutorialCanvas.GetComponent<RectTransform>();

        if (tutorialContainer == null && tutorialCanvasRect != null)
            tutorialContainer = tutorialCanvasRect;

        if (collectionButton == null)
            collectionButton = FindCollectionButton();

        if (collectionButton != null && collectionButtonRect == null)
            collectionButtonRect = collectionButton.GetComponent<RectTransform>();

        if (collectionPanelRoot == null)
            collectionPanelRoot = FindGameObjectByName("koleksipanel", "collectionpanel");

        // Auto-cari TMP_Text di banner kalau belum di-assign
        if (followBannerText == null && followAksaraBanner != null)
            followBannerText = followAksaraBanner.GetComponentInChildren<TMP_Text>(true);
    }

    private Canvas FindCanvasWithName(params string[] keywords)
    {
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
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
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
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
        foreach (var go in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go == null) continue;
            string n = go.name.ToLower();
            foreach (var k in keywords)
                if (n.Contains(k)) return go;
        }
        return null;
    }

    private IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (!ValidateSetup()) { Debug.LogError("[Tutorial] Setup tidak lengkap."); yield break; }
        if (!PickAksaraAndEnemy()) yield break;

        yield return WaitForPlayerRoutine();

        yield return SpawnEnemiesRoutine();
        yield return ApproachAndFreezeEnemiesRoutine();

        int enemyIndex = 0;

        while (HasAnyAliveEnemy())
        {
            EnemyGestureCommand active = GetFirstAliveEnemy();
            if (active == null) break;

            AksaraData activeAksara = GetAksaraFor(active);
            if (activeAksara == null) { Debug.LogError($"[Tutorial] Aksara musuh #{enemyIndex} tidak ada."); break; }

            activeTargetEnemy = active;
            currentTarget = activeAksara.GestureShape;

            ActivateOnlyTargetChallenge(active, activeAksara);

            yield return new WaitForSeconds(delayBeforeDotsAndHand);

            // Tampilkan banner dengan teks sesuai index dots
            ShowBannerByIndex(dotsBannerTexts, enemyIndex);

            SpawnDottedPath();
            SpawnHandOnPath();
            PlayHandAlongPath();

            if (gestureDrawer != null) gestureDrawer.enabled = true;

            waitingForDraw = true;
            yield return WaitUntilOrTimeout(() => !waitingForDraw, gestureDrawTimeout, $"Gesture musuh #{enemyIndex}");
            waitingForDraw = false;

            HideBanner();

            PlayCorrectSFX();

            ClearHand();
            ClearDots();

            if (gestureDrawer != null) gestureDrawer.enabled = false;

            yield return new WaitForSeconds(postGestureDefeatDelay);

            if (active != null && forceDefeatEnemyAfterGesture)
            {
                Enemy comp = active.GetComponent<Enemy>();
                if (comp != null)
                {
                    active.StopCommandMode();
                    comp.OnDefeated();
                    comp.StartDefeatBlink();

                    yield return new WaitForSeconds(hoorayDelayAfterDrop);

                    PlayHoorayForEnemyIndex(enemyIndex);

                    float remaining = Mathf.Max(0.1f, comp.DefeatBlinkDuration) - hoorayDelayAfterDrop;
                    if (remaining > 0f)
                        yield return new WaitForSeconds(remaining);
                }
            }

            yield return WaitUntilOrTimeout(() => active == null, enemyDestroyTimeout, $"Musuh #{enemyIndex} destroy");

            if (active != null) Destroy(active.gameObject);

            activeTargetEnemy = null;
            enemyIndex++;

            if (HasAnyAliveEnemy())
                yield return new WaitForSeconds(delayBetweenEnemies);
        }

        yield return new WaitForSeconds(delayAfterEnemyDefeated);

        int fragmentsCollected = 0;
        int maxFragments = Mathf.Max(enemyCount, 1);
        float phaseStart = Time.time;
        float maxPhaseTime = maxFragments * (fragmentTapTimeout + delayAfterFragmentTap + 2f);

        while (fragmentsCollected < maxFragments && (Time.time - phaseStart) < maxPhaseTime)
        {
            GameObject frag = FindActiveFragment();
            if (frag == null) break;

            currentItem = frag;
            PlayAksaraDropSFX();

            // Tampilkan banner fragment sesuai index drop
            ShowBannerByIndex(fragmentBannerTexts, fragmentsCollected);

            yield return TapFragmentRoutine();

            HideBanner();

            fragmentsCollected++;

            float waitStart = Time.time;
            while (currentItem != null && (Time.time - waitStart) < 3f)
                yield return null;

            yield return new WaitForSeconds(delayAfterFragmentTap);
        }

        yield return TapBookButtonRoutine();
        yield return WaitCollectionPanelCycle();

        ShowFinishPanel();
        tutorialCoroutine = null;
    }

    private IEnumerator WaitForPlayerRoutine()
    {
        if (FindPlayerTransform() != null)
            yield break;

        float elapsed = 0f;
        while (FindPlayerTransform() == null && elapsed < waitForPlayerTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (FindPlayerTransform() == null)
            Debug.LogWarning($"[Tutorial] Player tidak ditemukan setelah {waitForPlayerTimeout:F1}s — dots akan pakai fallback musuh.");
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
            Debug.LogWarning($"[Tutorial] Timeout: {label} ({timeout:F1}s)");
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
                else { Debug.LogError("[Tutorial] AksaraData pool kosong!"); return false; }
            }
            currentAksaraList.Add(a);
        }

        currentEnemyData = PickEnemyDataFromBag();
        if (currentEnemyData == null) { Debug.LogError("[Tutorial] EnemyData pool kosong!"); return false; }

        return true;
    }

    private IEnumerator TapFragmentRoutine()
    {
        if (currentItem == null) yield break;

        SpawnHandAtWorld(currentItem.transform.position, tapFragmentSFX);

        waitingForItemClick = true;
        yield return WaitUntilOrTimeout(() => !waitingForItemClick, fragmentTapTimeout, "Fragment tap");
        waitingForItemClick = false;

        ClearHand();
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator TapBookButtonRoutine()
    {
        ClearHand();
        if (collectionButtonRect == null) yield break;

        SpawnHandAtUI(collectionButtonRect, tapBookSFX);

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
            yield return new WaitForSecondsRealtime(3f);
            yield break;
        }

        yield return WaitUntilOrTimeout(() => collectionPanelRoot.activeInHierarchy, collectionOpenTimeout, "Panel open");

        if (!collectionPanelRoot.activeInHierarchy)
        {
            ClearHand();
            ClearDots();
            yield break;
        }

        yield return WaitUntilOrTimeout(() => !collectionPanelRoot.activeInHierarchy, collectionCloseTimeout, "Panel close");

        yield return new WaitForSecondsRealtime(0.3f);
        ClearHand();
        ClearDots();
    }

    private void ShowFinishPanel()
    {
        if (collectionButton != null) collectionButton.gameObject.SetActive(false);
        if (finishPanel != null) finishPanel.SetActive(true);
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
            collectionPanelRoot.SetActive(true);
    }

    private bool ValidateSetup()
    {
        bool ok = true;

        if (enemyPrefab == null) { Debug.LogError("[Tutorial] enemyPrefab kosong!"); ok = false; }
        if (tutorialCanvas == null) { Debug.LogError("[Tutorial] tutorialCanvas kosong!"); ok = false; }
        if (tutorialCanvasRect == null) { Debug.LogError("[Tutorial] tutorialCanvasRect kosong!"); ok = false; }
        if (tutorialContainer == null) { Debug.LogError("[Tutorial] tutorialContainer kosong!"); ok = false; }

        int poolSize = tutorialEnemyDataPool?.Length ?? 0;
        if (poolSize == 0 && tutorialEnemyDataFallback == null)
        {
            Debug.LogError("[Tutorial] EnemyData pool + fallback kosong!");
            ok = false;
        }

        return ok;
    }

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

    private IEnumerator SpawnEnemiesRoutine()
    {
        currentEnemies.Clear();
        cachedHalfExtents.Clear();

        Transform parent = worldParent != null ? worldParent : transform;
        int count = Mathf.Max(1, enemyCount);
        float xDir = spawnSide == SpawnSide.Left ? 1f : -1f;

        for (int i = 0; i < count; i++)
        {
            float centeredIndex = i - (count - 1) * 0.5f;
            float vpYOffset = centeredIndex * enemySpawnSpacingY;
            float vpXOffset = centeredIndex * enemySpawnSpacingX * xDir;

            Vector3 spawnPos = GetEnemyWorldSpawnPosition(vpXOffset, vpYOffset);
            EnemyGestureCommand enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, parent);
            if (enemy == null) continue;

            enemy.name = $"TutorialEnemy_{i}";

            Vector3 ep = enemy.transform.position;
            ep.z = enemySpawnPos.z + i * 0.01f;
            enemy.transform.position = ep;

            enemy.transform.localScale = Vector3.one;
            enemy.SetAutoIssueOnStart(false);

            if (disableColliderDuringApproach)
                foreach (var c in enemy.GetComponentsInChildren<Collider2D>(true))
                    c.enabled = false;

            currentEnemies.Add(enemy);
        }

        if (currentEnemies.Count == 0) yield break;

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
        }

        yield return null;

        for (int i = 0; i < currentEnemies.Count; i++)
            cachedHalfExtents.Add(GetEnemyHalfExtent(currentEnemies[i]));
    }

    private IEnumerator ApproachAndFreezeEnemiesRoutine()
    {
        if (!HasAnyAliveEnemy()) yield break;

        if (!useEnemyApproach)
        {
            FreezeAllEnemies();
            yield break;
        }

        Transform targetPlayer = FindPlayerTransform();
        Camera cam = Camera.main;

        float startTime = Time.time;
        bool frozen = false;

        while (Time.time - startTime < approachTimeoutSeconds)
        {
            if (!HasAnyAliveEnemy()) yield break;

            if (separateDuringApproach)
                SeparateEnemies(approachSeparationStrength, 1, syncAfter: false);

            Vector3 center = GetEnemiesCenterWorld();

            if (targetPlayer != null && enemyStopDistanceToPlayer > 0f)
            {
                if (Vector2.Distance(center, targetPlayer.position) <= enemyStopDistanceToPlayer)
                {
                    FreezeAllEnemies();
                    frozen = true;
                    break;
                }
            }

            if (cam != null)
            {
                float extremeX = spawnSide == SpawnSide.Left ? float.MinValue : float.MaxValue;
                bool hasValid = false;

                for (int i = 0; i < currentEnemies.Count; i++)
                {
                    if (currentEnemies[i] == null) continue;

                    Vector3 vp = cam.WorldToViewportPoint(currentEnemies[i].transform.position);
                    if (vp.z < 0f) continue;

                    if (spawnSide == SpawnSide.Left)
                        extremeX = Mathf.Max(extremeX, vp.x);
                    else
                        extremeX = Mathf.Min(extremeX, vp.x);

                    hasValid = true;
                }

                if (hasValid)
                {
                    bool reachedStop = spawnSide == SpawnSide.Left
                        ? extremeX >= enemyStopViewportX
                        : extremeX <= enemyStopViewportX;

                    if (reachedStop)
                    {
                        FreezeAllEnemies();
                        frozen = true;
                        break;
                    }
                }
            }

            yield return null;
        }

        if (!frozen && HasAnyAliveEnemy())
            FreezeAllEnemies();
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

        SeparateEnemies(1f, separationIterations, syncAfter: true);
    }

    private void SeparateEnemies(float strength, int iterations, bool syncAfter)
    {
        if (currentEnemies.Count < 2) return;
        if (iterations <= 0 && strength <= 0f) return;

        int m = currentEnemies.Count;
        float str = Mathf.Clamp01(strength);
        int iterCount = Mathf.Max(1, iterations);

        for (int iter = 0; iter < iterCount; iter++)
        {
            bool anyMoved = false;

            for (int a = 0; a < m; a++)
            {
                var ea = currentEnemies[a];
                if (ea == null) continue;

                for (int b = a + 1; b < m; b++)
                {
                    var eb = currentEnemies[b];
                    if (eb == null) continue;

                    float sep = enemyMinSeparationWorld > 0f
                        ? enemyMinSeparationWorld
                        : GetCachedHalfExtent(ea, a) + GetCachedHalfExtent(eb, b) + enemySeparationPadding;

                    Vector3 pa = ea.transform.position;
                    Vector3 pb = eb.transform.position;

                    Vector2 diff = new Vector2(pa.x - pb.x, pa.y - pb.y);
                    float d = diff.magnitude;

                    if (d >= sep) continue;

                    if (d < 0.0001f)
                    {
                        float ang = (a * 137.5f + b * 53.1f) * Mathf.Deg2Rad;
                        diff = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                        d = 0.0001f;
                    }

                    Vector2 dir = diff / d;
                    float push = (sep - d) * 0.5f * str;

                    Vector3 newA = pa + (Vector3)(dir * push);
                    Vector3 newB = pb - (Vector3)(dir * push);
                    newA.z = pa.z;
                    newB.z = pb.z;

                    Rigidbody2D rbA = ea.GetComponent<Rigidbody2D>();
                    Rigidbody2D rbB = eb.GetComponent<Rigidbody2D>();

                    if (rbA != null)
                        rbA.position = newA;
                    else
                        ea.transform.position = newA;

                    if (rbB != null)
                        rbB.position = newB;
                    else
                        eb.transform.position = newB;

                    anyMoved = true;
                }
            }

            if (!anyMoved) break;
        }

        if (!syncAfter) return;

        for (int i = 0; i < m; i++)
            if (currentEnemies[i] != null) currentEnemies[i].SyncSpawnPosition();
    }

    private float GetCachedHalfExtent(EnemyGestureCommand enemy, int indexHint)
    {
        if (indexHint >= 0 && indexHint < cachedHalfExtents.Count && cachedHalfExtents[indexHint] > 0.01f)
            return cachedHalfExtents[indexHint];
        return GetEnemyHalfExtent(enemy);
    }

    private float GetEnemyHalfExtent(EnemyGestureCommand enemy)
    {
        if (enemy == null) return 1f;

        float maxExtent = 0f;

        foreach (var r in enemy.GetComponentsInChildren<Renderer>())
        {
            if (r == null || !r.enabled) continue;
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            maxExtent = Mathf.Max(maxExtent, r.bounds.extents.x, r.bounds.extents.y);
        }

        if (maxExtent < 0.01f)
        {
            foreach (var c in enemy.GetComponentsInChildren<Collider2D>())
                if (c != null)
                    maxExtent = Mathf.Max(maxExtent, c.bounds.extents.x, c.bounds.extents.y);
        }

        return maxExtent > 0.01f ? maxExtent : 1f;
    }

    private void ActivateOnlyTargetChallenge(EnemyGestureCommand target, AksaraData targetAksara)
    {
        if (target == null || targetAksara == null) return;

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            var enemy = currentEnemies[i];
            if (enemy == null) continue;

            var movement = enemy.GetComponent<EnemyMovementBehavior>();

            if (enemy == target)
                enemy.ConfigureChallenge(targetAksara.GestureShape, 1);
            else
                enemy.StopCommandMode();

            if (movement != null)
            {
                movement.SetMovementPaused(true);
                movement.SetActive(false);
            }

            enemy.SyncSpawnPosition();
        }
    }

    private Transform FindPlayerTransform()
    {
        if (playerTransform != null)
            return playerTransform;

        var ph = FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
        {
            playerTransform = ph.transform;
            return playerTransform;
        }

        GameObject byTag = null;
        try { byTag = GameObject.FindGameObjectWithTag("Player"); }
        catch { }

        if (byTag != null)
        {
            playerTransform = byTag.transform;
            return playerTransform;
        }

        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (t == null) continue;
            string n = t.name.ToLower();
            if (n == "player" || n.Contains("player"))
            {
                playerTransform = t;
                return playerTransform;
            }
        }

        return null;
    }

    private Vector3 GetEnemyWorldSpawnPosition(float viewportXOffset, float viewportYOffset)
    {
        Camera cam = Camera.main;
        if (cam == null) return enemySpawnPos;

        float anchorX = (spawnSide == SpawnSide.Left ? -offscreenPadding : 1f + offscreenPadding) + viewportXOffset;
        float anchorY = Mathf.Clamp01(enemySpawnAnchorY + viewportYOffset);

        float z = Mathf.Abs(cam.transform.position.z - enemySpawnPos.z);
        if (z < 0.1f) z = 10f;

        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(anchorX, anchorY, z));
        worldPos.z = enemySpawnPos.z;
        return worldPos;
    }

    private GameObject FindActiveFragment()
    {
        foreach (var f in FindObjectsByType<AksaraFragmentItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (f != null && f.gameObject.activeInHierarchy) return f.gameObject;
        return null;
    }

    private Vector2 GetDotsCenterUI()
    {
        Transform pt = FindPlayerTransform();
        if (pt != null)
            return WorldToContainerLocal(pt.position) + dotsContainerOffset;

        return new Vector2(0f, CanvasRect.height * 0.05f) + dotsContainerOffset;
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

    private void SpawnHandAtUI(RectTransform targetUI, AudioClip tapSFX, bool withTapping = true)
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
            handAnimCoroutine = StartCoroutine(HandTapRoutine(localPos, tapSFX));
        }
    }

    private void SpawnHandAtWorld(Vector3 worldPos, AudioClip tapSFX, bool withTapping = true)
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
            handAnimCoroutine = StartCoroutine(HandTapRoutine(localPos, tapSFX));
        }
    }

    private void PlayHandAlongPath()
    {
        if (currentHand == null) return;
        if (handAnimCoroutine != null) StopCoroutine(handAnimCoroutine);
        handAnimCoroutine = StartCoroutine(HandPathRoutine(tapDotsSFX));
    }

    private IEnumerator HandPathRoutine(AudioClip tapSFX)
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

            PlayHandTapSFX(tapSFX, tapDotsSfxVolume);

            if (currentHand != null)
                currentHand.rectTransform.anchoredPosition = center + path[0] * scale;

            yield return new WaitForSecondsRealtime(handLoopPause);
        }
    }

    private IEnumerator HandTapRoutine(Vector2 basePos, AudioClip tapSFX)
    {
        float lastTapTime = 0f;
        const float TAP_COOLDOWN = 0.35f;

        while (true)
        {
            float sinValue = Mathf.Sin(Time.unscaledTime * handTapSpeed);
            float offset = Mathf.Abs(sinValue) * handTapAmplitudePx;

            if (currentHand != null)
                currentHand.rectTransform.anchoredPosition = basePos - new Vector2(0f, offset);

            if (Mathf.Abs(sinValue) > 0.95f && Time.unscaledTime - lastTapTime > TAP_COOLDOWN)
            {
                PlayHandTapSFX(tapSFX, tapSFXVolume);
                lastTapTime = Time.unscaledTime;
            }

            yield return null;
        }
    }

    private void PlayHandTapSFX(AudioClip clip, float volume)
    {
        if (clip == null) return;
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip, volume);
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

    public void SetTutorialVisualsVisible(bool visible)
    {
        if (currentHand != null)
            currentHand.gameObject.SetActive(visible);

        for (int i = 0; i < currentDots.Count; i++)
            if (currentDots[i] != null)
                currentDots[i].SetActive(visible);

        if (bannerActive && followAksaraBanner != null)
            followAksaraBanner.SetActive(visible);
    }

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
        if (c == null || c.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return c.worldCamera;
    }

    private void OnGestureRecognized(List<List<Vector2>> strokes, GestureRecognitionResult result)
    {
        if (!waitingForDraw) return;
        if (!result.IsRecognized) return;
        if (result.DetectedShape != currentTarget) return;

        waitingForDraw = false;
    }

    private void OnCollectionClicked()
    {
        if (!waitingForCollectionClick) return;
        waitingForCollectionClick = false;
    }

    private void Update()
    {
        if (!waitingForItemClick) return;

        if (currentItem == null) { waitingForItemClick = false; return; }
        if (!Input.GetMouseButtonDown(0)) return;

        if (Time.timeScale <= 0f) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        if (Vector2.Distance(wp, currentItem.transform.position) <= 1f)
        {
            currentItem.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
            waitingForItemClick = false;
        }
        else
        {
            TriggerWrongClickFeedback();
        }
    }

    // === Follow Banner helpers (text-based) ===

    // Tampilkan banner dengan teks sesuai index. Fallback ke teks default kalau array kosong.
    private void ShowBannerByIndex(string[] texts, int index)
    {
        if (followAksaraBanner == null) return;

        if (followBannerText != null && texts != null && index >= 0 && index < texts.Length)
            followBannerText.text = texts[index];

        followAksaraBanner.SetActive(true);
        bannerActive = true;
    }

    private void HideBanner()
    {
        if (followAksaraBanner != null)
            followAksaraBanner.SetActive(false);

        bannerActive = false;
    }

    // === End Follow Banner helpers ===

    private void PlayAksaraDropSFX()
    {
        if (AudioManager.Instance == null || aksaraDropSFX == null) return;
        AudioManager.Instance.PlaySFX(aksaraDropSFX);
    }

    private void PlayCorrectSFX()
    {
        if (AudioManager.Instance == null || correctSFX == null) return;
        Debug.Log($"[SFX] Correct @ {Time.time:F2}s");
        AudioManager.Instance.PlaySFX(correctSFX);
    }

    private void PlayHoorayForEnemyIndex(int enemyIndex)
    {
        if (AudioManager.Instance == null) return;
        AudioClip clip = enemyIndex == 0 ? hoorayAksara1SFX : hoorayAksara2SFX;
        if (clip == null) return;
        Debug.Log($"[SFX] Hooray #{enemyIndex} @ {Time.time:F2}s");
        AudioManager.Instance.PlaySFX(clip);
    }

    private void TriggerWrongClickFeedback()
    {
        if (wrongClickSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(wrongClickSFX);

        if (!enableShakeOnWrongClick) return;

        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCameraRoutine());
    }

    private IEnumerator ShakeCameraRoutine()
    {
        Camera cam = Camera.main;
        if (cam == null) { shakeCoroutine = null; yield break; }

        Transform t = cam.transform;
        Vector3 original = t.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float damper = 1f - (elapsed / shakeDuration);
            float ang = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * shakeMagnitude * damper;

            t.localPosition = original + new Vector3(offset.x, offset.y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        t.localPosition = original;
        shakeCoroutine = null;
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

    private void OnClickMain()
    {
        if (finishPanel != null) finishPanel.SetActive(false);
        if (tutorialCanvas != null) tutorialCanvas.gameObject.SetActive(false);

        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        if (LevelManager.Instance != null)
            LevelManager.Instance.UnlockFirstLevel();

        string returnScene = PlayerPrefs.GetString("ReturnSceneAfterTutorial", "");
        string targetScene;

        if (!string.IsNullOrEmpty(returnScene))
        {
            targetScene = returnScene;

            PlayerPrefs.DeleteKey("ReturnSceneAfterTutorial");
            PlayerPrefs.Save();

            Debug.Log($"[Tutorial] Selesai → kembali ke scene asal: {targetScene}");
        }
        else
        {
            targetScene = gameplayScene;

            GameProgressManager.ClearGameState();

            Debug.Log($"[Tutorial] Selesai → lanjut ke gameplay default: {targetScene}");
        }

        GameProgressManager.SaveLastScene(SceneManager.GetActiveScene().name);

        Time.timeScale = 1f;

        StartCoroutine(FadeAndLoadScene(targetScene));
    }

    private void OnClickLatihan()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeAndLoadScene(SceneManager.GetActiveScene().name));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (AudioManager.Instance != null)
            yield return AudioManager.Instance.FadeOutBGMAndWait(bgmFadeOutDuration);

        TransitionManager tm = TransitionManager.Instance();

        if (tm != null && transitionSettings != null)
            tm.Transition(sceneName, transitionSettings, loadDelay);
        else
            SceneManager.LoadScene(sceneName);
    }
}