using System.Collections;
using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    public static CollectionPanel Instance { get; private set; }

    [Header("Collection Panels")]
    [SerializeField] private GameObject collectionPanel;
    [SerializeField] private GameObject collectionPanel2;

    [Header("HUD Elements to Hide")]
    [Tooltip("Tombol pause yang disembunyikan saat collection terbuka.")]
    [SerializeField] private GameObject pauseButton;

    // ✅ Hide guided tutorial panel juga
    [SerializeField] private GameObject guidedTutorialPanelToHide;

    // ✅ BARU: FinishPanel yang disembunyikan sementara
    [SerializeField] private GameObject finishPanelToHide;

    // ✅ Tutorial Hint Manager untuk hide circle
    [SerializeField] private TutorialHintManager tutorialHintManager;

    [Header("Collect Animation Target")]
    [SerializeField] private Transform collectBookTarget;
    [SerializeField] private GameObject collectionBookVfx;
    [SerializeField] private float collectionBookVfxLifetime = 2f;
    [SerializeField] private Vector3 collectionBookVfxOffset = new Vector3(0f, -0.3f, 0f);

    public static Transform CollectBookTarget
    {
        get
        {
            if (Instance == null) return null;
            if (Instance.collectBookTarget != null) return Instance.collectBookTarget;
            return Instance.collectionPanel != null ? Instance.collectionPanel.transform : null;
        }
    }

    public static void PlayCollectionBookVfx()
    {
        if (Instance == null || Instance.collectionBookVfx == null) return;

        Transform target = CollectBookTarget;
        if (target == null) return;

        Vector3 targetWorldPosition = GetWorldTargetPosition(target) + Instance.collectionBookVfxOffset;
        GameObject vfx = Instantiate(
            Instance.collectionBookVfx,
            targetWorldPosition,
            Instance.collectionBookVfx.transform.rotation);

        ParticleSystem[] particles = vfx.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particle in particles)
            particle.Play(true);

        Instance.StartCoroutine(PlayCollectionBookVfxRoutine(vfx));
    }

    private static Vector3 GetWorldTargetPosition(Transform target)
    {
        RectTransform targetRect = target as RectTransform;
        Camera worldCamera = Camera.main;

        if (targetRect == null || worldCamera == null)
            return target.position;

        Canvas canvas = targetRect.GetComponentInParent<Canvas>();
        Camera canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            canvasCamera,
            targetRect.position);

        float cameraDistance = Mathf.Abs(worldCamera.transform.position.z);

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, cameraDistance));
        worldPosition.z = 0f;

        return worldPosition;
    }

    private static IEnumerator PlayCollectionBookVfxRoutine(GameObject vfx)
    {
        Vector3 initialScale = vfx.transform.localScale;
        float time = 0f;
        const float growDuration = 0.12f;

        while (time < growDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / growDuration);
            float rhythm = 1f - Mathf.Pow(1f - progress, 3f);
            vfx.transform.localScale = Vector3.Lerp(initialScale, initialScale * 1.3f, rhythm);
            yield return null;
        }

        time = 0f;
        const float shrinkDuration = 0.25f;

        while (time < shrinkDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / shrinkDuration);
            float rhythm = 1f - Mathf.Pow(1f - progress, 2f);
            vfx.transform.localScale = Vector3.Lerp(initialScale * 1.3f, initialScale, rhythm);
            yield return null;
        }

        vfx.transform.localScale = initialScale;
        Destroy(vfx, Instance.collectionBookVfxLifetime);
    }

    private bool isOpen = false;
    private bool wasGuidedTutorialPanelActive = false;

    // ✅ State penyimpanan FinishPanel
    private bool wasFinishPanelActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================
    // HUD HELPERS
    // =========================

    private void HidePauseButton()
    {
        if (pauseButton != null)
            pauseButton.SetActive(false);

        if (guidedTutorialPanelToHide != null)
        {
            wasGuidedTutorialPanelActive = guidedTutorialPanelToHide.activeSelf;
            guidedTutorialPanelToHide.SetActive(false);
        }

        // ✅ Hide circle highlight
        if (tutorialHintManager == null)
            tutorialHintManager = FindFirstObjectByType<TutorialHintManager>();

        if (tutorialHintManager != null)
            tutorialHintManager.HideCircleTemporarily();

        // ✅ Hide FinishPanel
        HideFinishPanelForCollection();
    }

    private void ShowPauseButton()
    {
        if (pauseButton != null)
            pauseButton.SetActive(true);

        if (guidedTutorialPanelToHide != null)
            guidedTutorialPanelToHide.SetActive(wasGuidedTutorialPanelActive);

        // ✅ Restore circle highlight
        if (tutorialHintManager != null)
            tutorialHintManager.RestoreCircle();

        // ✅ Restore FinishPanel
        RestoreFinishPanelAfterCollection();
    }

    // ✅ FINISH PANEL: hide saat collection buka
    private void HideFinishPanelForCollection()
    {
        if (finishPanelToHide == null) return;

        wasFinishPanelActive = finishPanelToHide.activeSelf;

        if (wasFinishPanelActive)
        {
            finishPanelToHide.SetActive(false);
            Debug.Log("[CollectionPanel] FinishPanel disembunyikan sementara.");
        }
    }

    // ✅ FINISH PANEL: restore saat collection tutup
    private void RestoreFinishPanelAfterCollection()
    {
        if (finishPanelToHide == null) return;

        if (wasFinishPanelActive)
        {
            finishPanelToHide.SetActive(true);
            Debug.Log("[CollectionPanel] FinishPanel dikembalikan.");
        }

        wasFinishPanelActive = false;
    }

    // =========================
    // COLLECTION 1
    // =========================

    public void ToggleCollection()
    {
        if (isOpen)
            CloseCollection();
        else
            OpenCollection();
    }

    public void OpenCollection()
    {
        if (isOpen) return;

        isOpen = true;
        HidePauseButton();
        collectionPanel?.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log("[Collection 1] Dibuka");
    }

    public void CloseCollection()
    {
        if (!isOpen) return;

        isOpen = false;

        var effect = collectionPanel?.GetComponent<EffectPanel>();

        if (effect != null)
        {
            effect.CloseDialog(() =>
            {
                collectionPanel?.SetActive(false);
                Time.timeScale = 1f;
                ShowPauseButton();
                Debug.Log("[Collection 1] Ditutup dengan efek");
            });
        }
        else
        {
            collectionPanel?.SetActive(false);
            Time.timeScale = 1f;
            ShowPauseButton();
            Debug.Log("[Collection 1] Ditutup langsung");
        }
    }

    // =========================
    // COLLECTION 2
    // =========================

    public void ToggleCollection2()
    {
        if (collectionPanel2 == null) return;

        if (collectionPanel2.activeSelf)
            CloseCollection2();
        else
            OpenCollection2();
    }

    public void OpenCollection2()
    {
        if (collectionPanel2 == null) return;

        HidePauseButton();
        collectionPanel2.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log("[Collection 2] Dibuka");
    }

    public void CloseCollection2()
    {
        if (collectionPanel2 == null) return;

        var effect = collectionPanel2.GetComponent<EffectPanel>();

        if (effect != null)
        {
            effect.CloseDialog(() =>
            {
                collectionPanel2.SetActive(false);
                Time.timeScale = 1f;
                ShowPauseButton();
                Debug.Log("[Collection 2] Ditutup dengan efek");
            });
        }
        else
        {
            collectionPanel2.SetActive(false);
            Time.timeScale = 1f;
            ShowPauseButton();
            Debug.Log("[Collection 2] Ditutup langsung");
        }
    }
}