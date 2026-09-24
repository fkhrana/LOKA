using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    public enum PowerUpType
    {
        Freeze,
        Combo,
        Shield
    }

    public static event System.Action OnAnyPowerUpEnded;

    [System.Serializable]
    public class PowerUpSlot
    {
        public Image powerUpImage;
        public Image powerUpProgressImage;
        public Button powerUpButton;
        public PowerUpType powerUpType = PowerUpType.Freeze;
        public Sprite rewardNameSprite;
        [TextArea(2, 4)] public string rewardDescription;

        [Header("Freeze")]
        public float freezeDuration = 5f;

        [Header("Combo")]
        public float comboRadius = 2.5f;
        public float comboDuration = 8f;

        [Header("Shield")]
        public float shieldDuration = 5f;
        public bool shieldKnockbackToSpawn = true;
        public float shieldKnockbackDistance = 5f;

        [Header("Testing")]
        public bool unlockOnStartForTesting;

        [Header("SFX")]
        public AudioClip freezeSFX;
        public AudioClip comboSFX;
        [FormerlySerializedAs("knockbackSFX")]
        public AudioClip shieldSFX;
    }

    [Header("Power Up Slots (Gong=Shield, Angklung=Combo, Kacapi=Freeze)")]
    [SerializeField] private List<PowerUpSlot> slots = new List<PowerUpSlot>();

    [Header("SFX")]
    [SerializeField] private AudioClip endPowerUpSFX;

    [Header("Player Power Up VFX")]
    [SerializeField] private GameObject timeFreezeVfx;
    [SerializeField] private GameObject comboVfx;
    [SerializeField] private GameObject shieldVfx;
    [SerializeField] private bool vfxFollowsPowerUpDuration;

    [SerializeField] private Color activeColor = new Color(1f, 0.9f, 0.4f, 1f);

    [Header("Visual Transition")]
    [SerializeField] private float transitionDuration = 0.3f;

    [Header("Hide During Intro")]
    [SerializeField] private bool hideBarDuringIntro = true;
    [SerializeField] private float barFadeDuration = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private bool isFrozen;
    private static bool isComboActive;
    private static bool isShieldActive;
    private static bool shieldKnockbackToSpawn;
    private static float shieldKnockbackDistance;
    private readonly HashSet<PowerUpType> unlockedPowerUps = new HashSet<PowerUpType>();
    private readonly HashSet<PowerUpType> consumedPowerUps = new HashSet<PowerUpType>();

    private CanvasGroup barCanvasGroup;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    private Coroutine freezeRoutine;
    private Coroutine shieldRoutine;
    private Coroutine comboRoutine;
    private Coroutine freezeProgressRoutine;
    private Coroutine fadeTransitionRoutine;

    private bool isResetting = false;

    public static bool IsComboActive => isComboActive;
    public static bool IsShieldActive => isShieldActive;
    public static bool ShieldKnockbackToSpawn => shieldKnockbackToSpawn;
    public static float ShieldKnockbackDistance => shieldKnockbackDistance;
    public static float ActiveComboRadius { get; private set; }

    private void Awake()
    {
        isShieldActive = false;
        shieldKnockbackToSpawn = false;
        shieldKnockbackDistance = 0f;
        StopVfx(timeFreezeVfx);
        StopVfx(comboVfx);
        StopVfx(shieldVfx);

        if (hideBarDuringIntro)
        {
            barCanvasGroup = GetComponent<CanvasGroup>();
            if (barCanvasGroup == null)
                barCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            barCanvasGroup.alpha = 0f;
            barCanvasGroup.blocksRaycasts = false;
            barCanvasGroup.interactable = false;
        }
    }

    private void Start()
    {
        foreach (var slot in slots)
        {
            if (slot.unlockOnStartForTesting)
                UnlockCurrentPowerUp(slot.powerUpType);

            if (slot.powerUpButton != null)
            {
                PowerUpType capturedType = slot.powerUpType;
                slot.powerUpButton.onClick.AddListener(() => UsePowerUp(capturedType));
            }

            RefreshVisual(slot, animate: false);
            PrepareProgressImage(slot);
            SetProgress(slot, 1f);
        }

        RefreshButtonsInteractable();
    }

    private void Update()
    {
        // === FIX: tutorial juga boleh klik ===
        bool canClick = CameraIntroManager.GameStarted
                     || PowerUpTutorialManager.IsPowerUpTutorial;

        RefreshButtonsInteractable();

        if (hideBarDuringIntro && barCanvasGroup != null)
        {
            targetAlpha = canClick ? 1f : 0f;

            if (!Mathf.Approximately(currentAlpha, targetAlpha))
            {
                currentAlpha = Mathf.MoveTowards(
                    currentAlpha,
                    targetAlpha,
                    Time.unscaledDeltaTime / barFadeDuration
                );

                barCanvasGroup.alpha = currentAlpha;
            }

            barCanvasGroup.blocksRaycasts = canClick;
            barCanvasGroup.interactable = canClick;
        }
    }

    private void RefreshButtonsInteractable()
    {
        // === FIX: tutorial juga boleh klik ===
        bool canClick = CameraIntroManager.GameStarted
                     || PowerUpTutorialManager.IsPowerUpTutorial;

        foreach (var slot in slots)
        {
            if (slot.powerUpButton == null) continue;

            if (slot.powerUpButton.interactable != canClick)
                slot.powerUpButton.interactable = canClick;
        }
    }

    public void SetLocked(PowerUpType type)
    {
        unlockedPowerUps.Remove(type);
        consumedPowerUps.Remove(type);
        RefreshVisualByType(type, animate: false);
    }

    public void SetUnlocked(PowerUpType type)
    {
        UnlockCurrentPowerUp(type);
        RefreshVisualByType(type, animate: true);
    }

    public Transform GetSlotTransform(PowerUpType type)
    {
        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        return slot?.powerUpImage != null ? slot.powerUpImage.transform : null;
    }

    public Sprite GetPowerUpSprite(PowerUpType type)
    {
        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        return slot?.powerUpImage != null ? slot.powerUpImage.sprite : null;
    }

    public Sprite GetPowerUpNameSprite(PowerUpType type)
    {
        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        return slot?.rewardNameSprite;
    }

    public string GetPowerUpRewardDescription(PowerUpType type)
    {
        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        return slot?.rewardDescription;
    }

    public static void UnlockPowerUp(PowerUpType type = PowerUpType.Freeze)
    {
        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);
        foreach (PowerManager manager in managers)
        {
            manager.SetUnlocked(type);
            break;
        }
    }

    public void UsePowerUp(PowerUpType type)
    {
        // === FIX: tutorial juga boleh pakai power-up ===
        if (!CameraIntroManager.GameStarted && !PowerUpTutorialManager.IsPowerUpTutorial)
            return;

        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        if (slot == null) return;

        if (!IsAvailable(type) || isFrozen)
            return;

        if (type == PowerUpType.Combo && isComboActive)
            return;

        if (type != PowerUpType.Combo)
            consumedPowerUps.Add(type);

        if (AudioManager.Instance != null)
        {
            if (type == PowerUpType.Freeze && slot.freezeSFX != null)
                AudioManager.Instance.PlaySFX(slot.freezeSFX);
            else if (type == PowerUpType.Combo && slot.comboSFX != null)
                AudioManager.Instance.PlaySFX(slot.comboSFX);
            else if (type == PowerUpType.Shield && slot.shieldSFX != null)
                AudioManager.Instance.PlaySFX(slot.shieldSFX);
        }

        if (type == PowerUpType.Freeze)
        {
            freezeRoutine = StartCoroutine(FreezeRoutine(slot));
        }
        else if (type == PowerUpType.Shield)
        {
            isShieldActive = true;
            shieldKnockbackToSpawn = slot.shieldKnockbackToSpawn;
            shieldKnockbackDistance = Mathf.Max(0f, slot.shieldKnockbackDistance);
            SetProgress(slot, 1f);
            PlayVfx(shieldVfx);
            SetActiveVisual(slot);
            shieldRoutine = StartCoroutine(ShieldRoutine(slot));
        }
        else if (type == PowerUpType.Combo)
        {
            isComboActive = true;
            ActiveComboRadius = slot.comboRadius;
            SetProgress(slot, 1f);
            PlayVfx(comboVfx);
            SetActiveVisual(slot);
            comboRoutine = StartCoroutine(ComboRoutine(slot));
        }
    }

    public static void EndComboPowerUp()
    {
        if (!isComboActive) return;

        isComboActive = false;
        ActiveComboRadius = 0f;

        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);
        foreach (PowerManager manager in managers)
        {
            if (manager.comboRoutine != null)
            {
                manager.StopCoroutine(manager.comboRoutine);
                manager.comboRoutine = null;
            }

            if (manager.vfxFollowsPowerUpDuration)
                manager.StopVfx(manager.comboVfx);

            PowerUpSlot slot = manager.slots.Find(item => item.powerUpType == PowerUpType.Combo);
            if (slot != null)
            {
                manager.consumedPowerUps.Add(PowerUpType.Combo);
                manager.SetProgress(slot, 0f);
                manager.RefreshVisual(slot, animate: false);
                manager.ResetActiveVisual(slot);
            }
        }

        PlayEndPowerUpSFX();
        OnAnyPowerUpEnded?.Invoke();
    }

    private IEnumerator ComboRoutine(PowerUpSlot slot)
    {
        float duration = Mathf.Max(0f, slot.comboDuration);
        float elapsed = 0f;

        while (elapsed < duration && isComboActive)
        {
            elapsed += Time.deltaTime;
            SetProgress(slot, 1f - (elapsed / Mathf.Max(0.01f, duration)));
            yield return null;
        }

        if (isComboActive)
            EndComboPowerUp();

        comboRoutine = null;
    }

    private IEnumerator FreezeRoutine(PowerUpSlot slot)
    {
        isFrozen = true;
        PlayVfx(timeFreezeVfx);
        SetActiveVisual(slot);

        freezeProgressRoutine = StartCoroutine(UpdateProgressRoutine(slot, slot.freezeDuration));

        EnemyMovementBehavior.SetAllMovementPaused(true);
        yield return new WaitForSeconds(slot.freezeDuration);
        EnemyMovementBehavior.SetAllMovementPaused(false);

        if (vfxFollowsPowerUpDuration)
            StopVfx(timeFreezeVfx);

        PlayEndPowerUpSFX();
        isFrozen = false;

        RefreshVisual(slot, animate: true);
        SetProgress(slot, 0f);
        ResetActiveVisual(slot);

        freezeRoutine = null;
        freezeProgressRoutine = null;

        OnAnyPowerUpEnded?.Invoke();
    }

    private IEnumerator ShieldRoutine(PowerUpSlot slot)
    {
        float duration = Mathf.Max(0f, slot.shieldDuration);
        float elapsed = 0f;

        while (elapsed < duration && isShieldActive)
        {
            elapsed += Time.deltaTime;
            SetProgress(slot, 1f - (elapsed / Mathf.Max(0.01f, duration)));
            yield return null;
        }

        if (isShieldActive)
            EndShieldPowerUp();

        shieldRoutine = null;
    }

    public static void EndShieldPowerUp()
    {
        if (!isShieldActive)
            return;

        isShieldActive = false;
        shieldKnockbackToSpawn = false;
        shieldKnockbackDistance = 0f;

        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);
        foreach (PowerManager manager in managers)
        {
            if (manager.vfxFollowsPowerUpDuration)
                manager.StopVfx(manager.shieldVfx);

            PowerUpSlot slot = manager.slots.Find(item => item.powerUpType == PowerUpType.Shield);
            if (slot != null)
            {
                manager.SetProgress(slot, 0f);
                manager.RefreshVisual(slot, animate: false);
                manager.ResetActiveVisual(slot);
            }
        }

        PlayEndPowerUpSFX();
        OnAnyPowerUpEnded?.Invoke();
    }

    private void PrepareProgressImage(PowerUpSlot slot)
    {
        Image progressImage = GetProgressImage(slot);
        if (progressImage == null)
            return;

        progressImage.type = Image.Type.Filled;
        progressImage.fillMethod = Image.FillMethod.Radial360;
        progressImage.fillOrigin = 2;
        progressImage.fillClockwise = false;
    }

    private Image GetProgressImage(PowerUpSlot slot)
    {
        return slot.powerUpProgressImage != null
            ? slot.powerUpProgressImage
            : slot.powerUpImage;
    }

    private void SetProgress(PowerUpSlot slot, float progress)
    {
        if (isResetting) return;

        Image progressImage = GetProgressImage(slot);
        if (progressImage != null)
            progressImage.fillAmount = Mathf.Clamp01(progress);
    }

    private IEnumerator UpdateProgressRoutine(PowerUpSlot slot, float duration)
    {
        if (duration <= 0f)
        {
            SetProgress(slot, 0f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetProgress(slot, 1f - (elapsed / duration));
            yield return null;
        }

        SetProgress(slot, 0f);
    }

    private void PlayVfx(GameObject vfx)
    {
        if (vfx == null)
            return;

        vfx.SetActive(true);

        float longestDuration = 0f;

        foreach (ParticleSystem particles in vfx.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = vfxFollowsPowerUpDuration;

            if (!vfxFollowsPowerUpDuration)
            {
                longestDuration = Mathf.Max(
                    longestDuration,
                    main.duration + main.startLifetime.constantMax
                );
            }
            particles.Play(true);
        }

        if (!vfxFollowsPowerUpDuration)
            StartCoroutine(StopVfxAfterDuration(vfx, longestDuration));
    }

    private IEnumerator StopVfxAfterDuration(GameObject vfx, float duration)
    {
        yield return new WaitForSeconds(duration);
        StopVfx(vfx);
    }

    private void StopVfx(GameObject vfx)
    {
        if (vfx == null)
            return;

        foreach (ParticleSystem particles in vfx.GetComponentsInChildren<ParticleSystem>(true))
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        vfx.SetActive(false);
    }

    private void SetActiveVisual(PowerUpSlot slot)
    {
        if (slot.powerUpImage == null) return;

        if (fadeTransitionRoutine != null)
        {
            StopCoroutine(fadeTransitionRoutine);
            fadeTransitionRoutine = null;
        }

        fadeTransitionRoutine = StartCoroutine(FadeTransition(slot, activeColor));
    }

    private void ResetActiveVisual(PowerUpSlot slot)
    {
        if (fadeTransitionRoutine != null)
        {
            StopCoroutine(fadeTransitionRoutine);
            fadeTransitionRoutine = null;
        }

        if (slot.powerUpImage != null)
            slot.powerUpImage.color = Color.white;

        if (slot.powerUpProgressImage != null)
            slot.powerUpProgressImage.color = Color.white;
    }

    private void RefreshVisualByType(PowerUpType type, bool animate)
    {
        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        if (slot != null)
            RefreshVisual(slot, animate);
    }

    private void RefreshVisual(PowerUpSlot slot, bool animate)
    {
        bool isUnlocked = unlockedPowerUps.Contains(slot.powerUpType);
        SetButtonVisible(slot, isUnlocked);
    }

    private void SetButtonVisible(PowerUpSlot slot, bool visible)
    {
        if (slot.powerUpButton != null)
            slot.powerUpButton.gameObject.SetActive(visible);
    }

    private IEnumerator FadeTransition(PowerUpSlot slot, Color targetColor)
    {
        Image img = slot.powerUpImage;
        float half = transitionDuration * 0.5f;

        Color startColor = img.color;
        float t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / half);
            img.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, p));
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / half);
            img.color = new Color(targetColor.r, targetColor.g, targetColor.b, Mathf.Lerp(0f, targetColor.a, p));
            yield return null;
        }

        img.color = targetColor;

        if (slot.powerUpProgressImage != null)
            slot.powerUpProgressImage.color = targetColor;

        fadeTransitionRoutine = null;
    }

    private bool IsAvailable(PowerUpType type)
    {
        return unlockedPowerUps.Contains(type)
            && !consumedPowerUps.Contains(type);
    }

    private void UnlockCurrentPowerUp(PowerUpType type)
    {
        unlockedPowerUps.Add(type);
        consumedPowerUps.Remove(type);
    }

    private static void PlayEndPowerUpSFX()
    {
        if (AudioManager.Instance == null)
            return;

        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);

        foreach (PowerManager manager in managers)
        {
            if (manager.endPowerUpSFX != null)
            {
                AudioManager.Instance.PlaySFX(manager.endPowerUpSFX);
                break;
            }
        }
    }

    private void StopAllPowerUpRoutines()
    {
        if (freezeRoutine != null) { StopCoroutine(freezeRoutine); freezeRoutine = null; }
        if (shieldRoutine != null) { StopCoroutine(shieldRoutine); shieldRoutine = null; }
        if (comboRoutine != null) { StopCoroutine(comboRoutine); comboRoutine = null; }
        if (freezeProgressRoutine != null) { StopCoroutine(freezeProgressRoutine); freezeProgressRoutine = null; }
        if (fadeTransitionRoutine != null) { StopCoroutine(fadeTransitionRoutine); fadeTransitionRoutine = null; }
    }

    public void ResetAllPowerUpsToFull()
    {
        if (debugLog)
            Debug.Log($"[PowerManager] ResetAllPowerUpsToFull START pada {gameObject.name}");

        isResetting = true;

        StopAllPowerUpRoutines();
        StopAllCoroutines();

        foreach (var slot in slots)
        {
            consumedPowerUps.Remove(slot.powerUpType);

            Image progressImage = GetProgressImage(slot);
            if (progressImage != null)
                progressImage.fillAmount = 1f;

            RefreshVisual(slot, animate: false);

            if (slot.powerUpImage != null)
                slot.powerUpImage.color = Color.white;
            if (slot.powerUpProgressImage != null)
                slot.powerUpProgressImage.color = Color.white;
        }

        isFrozen = false;
        isComboActive = false;
        isShieldActive = false;
        ActiveComboRadius = 0f;
        shieldKnockbackToSpawn = false;
        shieldKnockbackDistance = 0f;

        EnemyMovementBehavior.SetAllMovementPaused(false);

        StopVfx(timeFreezeVfx);
        StopVfx(comboVfx);
        StopVfx(shieldVfx);

        RefreshButtonsInteractable();

        isResetting = false;

        if (debugLog)
            Debug.Log($"[PowerManager] ResetAllPowerUpsToFull DONE pada {gameObject.name}");
    }

    public static void ResetAllPowerUpsToFullGlobal()
    {
        PowerManager[] managers = FindObjectsByType<PowerManager>(FindObjectsSortMode.None);

        if (managers.Length == 0)
        {
            Debug.LogWarning("[PowerManager] Tidak ada PowerManager di scene saat reset!");
            return;
        }

        Debug.Log($"[PowerManager] Reset {managers.Length} manager.");

        foreach (PowerManager manager in managers)
            manager.ResetAllPowerUpsToFull();
    }
}