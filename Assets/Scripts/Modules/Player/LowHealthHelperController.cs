using UnityEngine;

public class LowHealthHelperController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private LowHealthHelper helperPrefab;
    [SerializeField] private RectTransform helperParent;
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private ParticleSystem healVfx;

    [Header("Activation")]
    [SerializeField] private bool activateWithLoveGesture = true;
    [SerializeField] private bool allowClickActivation;

    [Header("Rules")]
    [Tooltip("Jumlah bar health yang ditampilkan saat health penuh.")]
    [SerializeField] private int healthUnits = 5;

    [Tooltip("Helper muncul jika sisa bar health lebih kecil dari nilai ini.")]
    [SerializeField] private int healthThreshold = 5;

    [Tooltip("Jumlah bar health yang dikembalikan helper.")]
    [SerializeField] private int healUnits = 1;

    [Min(0)]
    [SerializeField] private int maxUsesPerScene = 1;

    [Min(0f)]
    [SerializeField] private float spawnCooldown = 0.5f;

    [Header("Placement")]
    [SerializeField] private float topMargin = 80f;

    [Header("SFX")]
    [SerializeField] private AudioClip helperAppearSFX;
    [SerializeField] private AudioClip helperClickSFX;

    private LowHealthHelper activeHelper;
    private int usesThisScene;
    private int lastHealth = -1;
    private float nextAllowedSpawnTime;

    private void Awake()
    {
        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (healVfx == null && playerHealth != null)
            healVfx = playerHealth.GetComponentInChildren<ParticleSystem>(true);

        if (healVfx != null)
        {
            ParticleSystem.MainModule main = healVfx.main;
            main.playOnAwake = false;
            healVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (playerHealth == null)
            Debug.LogWarning(
                "LowHealthHelperController: Player Health wajib di-assign di Inspector.",
                this
            );

        if (helperParent == null)
            Debug.LogWarning(
                "LowHealthHelperController: Helper Parent wajib di-assign di Inspector.",
                this
            );
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning(
                "LowHealthHelperController: PlayerHealth belum diisi.",
                this
            );
            return;
        }

        if (helperPrefab == null)
        {
            Debug.LogWarning(
                "LowHealthHelperController: Helper Prefab belum diisi.",
                this
            );
            return;
        }

        if (helperParent == null)
        {
            Debug.LogWarning(
                "LowHealthHelperController: Helper Parent belum diisi dan Canvas tidak ditemukan.",
                this
            );
            return;
        }

        lastHealth = playerHealth.CurrentHealth;
        TrySpawnForCurrentHealth(false);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged += HandleHealthChanged;

        if (gestureDrawer != null)
            gestureDrawer.GestureRecognized += HandleGestureRecognized;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= HandleHealthChanged;

        if (gestureDrawer != null)
            gestureDrawer.GestureRecognized -= HandleGestureRecognized;
    }

    public bool AllowClickActivation => allowClickActivation;

    // =================================================================
    // METHOD BARU: Dipakai oleh FirstHitTutorialManager
    // =================================================================
    /// <summary>
    /// Paksa spawn helper untuk keperluan tutorial (tanpa cek HP threshold).
    /// </summary>
    public bool TrySpawnHelperForTutorial()
    {
        if (playerHealth == null || helperPrefab == null || helperParent == null)
        {
            Debug.LogWarning("[LowHealthHelperController] Referensi belum lengkap untuk spawn tutorial.");
            return false;
        }

        if (playerHealth.IsDead || playerHealth.CurrentHealth <= 0)
        {
            Debug.Log("[LowHealthHelperController] Player dead — skip spawn tutorial.");
            return false;
        }

        if (activeHelper != null)
        {
            Debug.Log("[LowHealthHelperController] Helper sudah aktif — skip spawn tutorial.");
            return false;
        }

        LowHealthHelper helper = Instantiate(helperPrefab, helperParent, false);
        activeHelper = helper;

        if (AudioManager.Instance != null && helperAppearSFX != null)
            AudioManager.Instance.PlaySFX(helperAppearSFX);

        RectTransform helperRect = helper.transform as RectTransform;
        if (helperRect != null)
        {
            helperRect.anchorMin = new Vector2(0.5f, 1f);
            helperRect.anchorMax = new Vector2(0.5f, 1f);
            helperRect.pivot = new Vector2(0.5f, 1f);
            helperRect.anchoredPosition = new Vector2(0f, -topMargin);
        }

        helper.Initialize(this);

        Debug.Log("[LowHealthHelperController] Helper di-spawn untuk tutorial.");
        return true;
    }
    // =================================================================

    private void HandleGestureRecognized(
        System.Collections.Generic.List<System.Collections.Generic.List<Vector2>> strokes,
        GestureRecognitionResult result
    )
    {
        if (
            !activateWithLoveGesture ||
            !result.IsRecognized ||
            result.DetectedShape != GestureShape.Love ||
            activeHelper == null
        )
            return;

        activeHelper.Activate();
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        bool tookDamage = lastHealth >= 0 && currentHealth < lastHealth;
        lastHealth = currentHealth;

        if (tookDamage)
            TrySpawnForCurrentHealth(true);
    }

    private void TrySpawnForCurrentHealth(bool fromDamage)
    {
        if (playerHealth == null || helperPrefab == null || helperParent == null)
            return;

        if (playerHealth.IsDead || playerHealth.CurrentHealth <= 0)
            return;

        float healthPerUnit = (float)playerHealth.MaxHealth / Mathf.Max(1, healthUnits);
        float currentHealthUnits = playerHealth.CurrentHealth / healthPerUnit;

        if (currentHealthUnits >= healthThreshold)
            return;

        if (usesThisScene >= maxUsesPerScene || activeHelper != null)
            return;

        if (fromDamage && Time.unscaledTime < nextAllowedSpawnTime)
            return;

        nextAllowedSpawnTime = Time.unscaledTime + spawnCooldown;
        usesThisScene++;

        LowHealthHelper helper = Instantiate(helperPrefab, helperParent, false);
        activeHelper = helper;

        if (AudioManager.Instance != null && helperAppearSFX != null)
            AudioManager.Instance.PlaySFX(helperAppearSFX);

        RectTransform helperRect = helper.transform as RectTransform;
        if (helperRect != null)
        {
            helperRect.anchorMin = new Vector2(0.5f, 1f);
            helperRect.anchorMax = new Vector2(0.5f, 1f);
            helperRect.pivot = new Vector2(0.5f, 1f);
            helperRect.anchoredPosition = new Vector2(0f, -topMargin);
        }

        helper.Initialize(this);
    }

    public void ConsumeHelper(LowHealthHelper helper)
    {
        if (helper == null || helper != activeHelper || playerHealth == null)
            return;

        activeHelper = null;

        if (AudioManager.Instance != null && helperClickSFX != null)
            AudioManager.Instance.PlaySFX(helperClickSFX);

        int healthPerUnit = Mathf.Max(
            1,
            Mathf.RoundToInt((float)playerHealth.MaxHealth / Mathf.Max(1, healthUnits))
        );

        playerHealth.Heal(healthPerUnit * healUnits);
        PlayHealVfx();
        helper.FadeOutAndDestroy();
    }

    private void PlayHealVfx()
    {
        if (healVfx == null)
            return;

        healVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        healVfx.Play(true);
    }

    public void NotifyHelperDestroyed(LowHealthHelper helper)
    {
        if (helper == activeHelper)
            activeHelper = null;
    }
}