using System.Collections;
using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    public static CollectionPanel Instance { get; private set; }

    [Header("Collection Panels")]
    [SerializeField] private GameObject collectionPanel;
    [SerializeField] private GameObject collectionPanel2;

    [Header("Collect Animation Target")]
    [SerializeField] private Transform collectBookTarget;
    [SerializeField] private GameObject collectionBookVfx;
    [SerializeField] private float collectionBookVfxLifetime = 2f;
    [SerializeField] private Vector3 collectionBookVfxOffset = new Vector3(0f, -0.3f, 0f);

    public static Transform CollectBookTarget
    {
        get
        {
            if (Instance == null)
                return null;

            if (Instance.collectBookTarget != null)
                return Instance.collectBookTarget;

            return Instance.collectionPanel != null
                ? Instance.collectionPanel.transform
                : null;
        }
    }

    public static void PlayCollectionBookVfx()
    {
        if (Instance == null || Instance.collectionBookVfx == null)
            return;

        Transform target = CollectBookTarget;
        if (target == null)
            return;

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

        float cameraDistance = Mathf.Abs(
            worldCamera.transform.position.z);

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
            vfx.transform.localScale = Vector3.Lerp(
                initialScale,
                initialScale * 1.3f,
                rhythm);
            yield return null;
        }

        time = 0f;
        const float shrinkDuration = 0.25f;

        while (time < shrinkDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / shrinkDuration);
            float rhythm = 1f - Mathf.Pow(1f - progress, 2f);
            vfx.transform.localScale = Vector3.Lerp(
                initialScale * 1.3f,
                initialScale,
                rhythm);
            yield return null;
        }

        vfx.transform.localScale = initialScale;
        Destroy(vfx, Instance.collectionBookVfxLifetime);
    }

    private bool isOpen = false;

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
        collectionPanel?.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log("[Collection 1] Dibuka");
    }

    public void CloseCollection()
    {
        if (!isOpen) return;

        isOpen = false;

        // Cek apakah collectionPanel punya EffectPanel
        var effect = collectionPanel?.GetComponent<EffectPanel>();

        if (effect != null)
        {
            // Tutup dengan efek
            effect.CloseDialog(() =>
            {
                collectionPanel?.SetActive(false);
                Time.timeScale = 1f;

                Debug.Log("[Collection 1] Ditutup dengan efek");
            });
        }
        else
        {
            // Langsung ilang tanpa efek (fallback)
            collectionPanel?.SetActive(false);
            Time.timeScale = 1f;

            Debug.Log("[Collection 1] Ditutup langsung (tidak ada EffectPanel)");
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

        collectionPanel2.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log("[Collection 2] Dibuka");
    }

    public void CloseCollection2()
    {
        if (collectionPanel2 == null) return;

        // Cek apakah collectionPanel2 punya EffectPanel
        var effect = collectionPanel2.GetComponent<EffectPanel>();

        if (effect != null)
        {
            // Tutup dengan efek
            effect.CloseDialog(() =>
            {
                collectionPanel2.SetActive(false);
                Time.timeScale = 1f;

                Debug.Log("[Collection 2] Ditutup dengan efek");
            });
        }
        else
        {
            // Langsung ilang tanpa efek (fallback)
            collectionPanel2.SetActive(false);
            Time.timeScale = 1f;

            Debug.Log("[Collection 2] Ditutup langsung (tidak ada EffectPanel)");
        }
    }
}