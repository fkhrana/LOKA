using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    public enum PowerUpType
    {
        Freeze,
        Combo,
        Boost
    }

    [System.Serializable]
    public class PowerUpSlot
    {
        public Image powerUpImage;
        public Button powerUpButton;
        public PowerUpType powerUpType = PowerUpType.Freeze;

        [Header("Sprite per State")]
        public Sprite lockedSprite;
        public Sprite unlockedSprite;
        public Sprite usedSprite;

        [Header("Freeze")]
        public float freezeDuration = 5f;

        [Header("Combo")]
        public float comboRadius = 2.5f;

        [Header("Knockback")]
        public float knockbackForce = 12f;
        public float knockbackRadius = 3.5f;

        public bool unlockOnStartForTesting;

        [Header("SFX")]
        public AudioClip freezeSFX;
        public AudioClip comboSFX;
        public AudioClip knockbackSFX;
    }

    [Header("Power Up Slots (Gong=Boost, Angklung=Combo, Kacapi=Freeze)")]
    [SerializeField] private List<PowerUpSlot> slots = new List<PowerUpSlot>();

    [Header("SFX")]
    [SerializeField] private AudioClip endPowerUpSFX;

    [Header("Warna per State")]
    [SerializeField] private Color lockedColor   = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color consumedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Tooltip("Warna tint saat power-up sedang aktif")]
    [SerializeField] private Color activeColor   = new Color(1f, 0.9f, 0.4f, 1f);

    [Header("Visual Transition")]
    [SerializeField] private float transitionDuration = 0.3f;

    [Header("Hide During Intro")]
    [Tooltip("Sembunyikan bar saat intro belum selesai")]
    [SerializeField] private bool hideBarDuringIntro = true;
    [SerializeField] private float barFadeDuration = 0.3f;

    [Header("Knockback Range Indicator")]
    [SerializeField] private Color rangeIndicatorColor = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private float rangeIndicatorWidth = 0.08f;
    [SerializeField] private float rangeIndicatorDuration = 0.6f;
    [SerializeField] private int rangeIndicatorSegments = 48;

    private bool isFrozen;
    private static bool isComboActive;

    private CanvasGroup barCanvasGroup;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    public static bool IsComboActive => isComboActive;
    public static float ActiveComboRadius { get; private set; }

    private void Awake()
    {
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
        }

        RefreshButtonsInteractable();
    }

    private void Update()
    {
        bool canClick = CameraIntroManager.GameStarted;

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
        bool canClick = CameraIntroManager.GameStarted;

        foreach (var slot in slots)
        {
            if (slot.powerUpButton == null) continue;

            if (slot.powerUpButton.interactable != canClick)
                slot.powerUpButton.interactable = canClick;
        }
    }

    public void SetLocked(PowerUpType type)
    {
        PlayerPrefs.SetInt(GetUnlockedKey(type), 0);
        PlayerPrefs.SetInt(GetConsumedKey(type), 0);
        PlayerPrefs.Save();
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

    public static void UnlockPowerUp(PowerUpType type = PowerUpType.Freeze)
    {
        PlayerPrefs.SetInt(GetUnlockedKeyStatic(type), 1);
        PlayerPrefs.SetInt(GetConsumedKeyStatic(type), 0);
        PlayerPrefs.Save();
    }

    public void UsePowerUp(PowerUpType type)
    {
        if (!CameraIntroManager.GameStarted)
            return;

        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        if (slot == null) return;

        if (!IsAvailable(type) || isFrozen)
            return;

        PlayerPrefs.SetInt(GetConsumedKey(type), 1);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            if (type == PowerUpType.Freeze && slot.freezeSFX != null)
                AudioManager.Instance.PlaySFX(slot.freezeSFX);
            else if (type == PowerUpType.Combo && slot.comboSFX != null)
                AudioManager.Instance.PlaySFX(slot.comboSFX);
            else if (type == PowerUpType.Boost && slot.knockbackSFX != null)
                AudioManager.Instance.PlaySFX(slot.knockbackSFX);
        }

        if (type == PowerUpType.Freeze)
        {
            StartCoroutine(FreezeRoutine(slot));
        }
        else if (type == PowerUpType.Boost)
        {
            StartCoroutine(KnockbackRoutine(slot));
        }
        else if (type == PowerUpType.Combo)
        {
            isComboActive = true;
            ActiveComboRadius = slot.comboRadius;
            SetActiveVisual(slot);
        }
    }

    public static void EndComboPowerUp()
    {
        isComboActive = false;
        ActiveComboRadius = 0f;
        PlayEndPowerUpSFX();
    }

    private IEnumerator FreezeRoutine(PowerUpSlot slot)
    {
        isFrozen = true;
        SetActiveVisual(slot);

        EnemyMovementBehavior.SetAllMovementPaused(true);
        yield return new WaitForSeconds(slot.freezeDuration);
        EnemyMovementBehavior.SetAllMovementPaused(false);

        PlayEndPowerUpSFX();
        isFrozen = false;

        RefreshVisual(slot, animate: true);
    }

    private IEnumerator KnockbackRoutine(PowerUpSlot slot)
    {
        SetActiveVisual(slot);

        StartCoroutine(ShowRangeIndicator(transform.position, slot.knockbackRadius));

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slot.knockbackRadius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 direction = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            rb.AddForce(direction * slot.knockbackForce, ForceMode2D.Impulse);
        }

        PlayEndPowerUpSFX();

        yield return new WaitForSeconds(0.5f);

        RefreshVisual(slot, animate: true);
    }

    private void SetActiveVisual(PowerUpSlot slot)
    {
        if (slot.powerUpImage == null) return;
        StartCoroutine(FadeTransition(slot, null, activeColor));
    }

    private void RefreshVisualByType(PowerUpType type, bool animate)
    {
        PowerUpSlot slot = slots.Find(s => s.powerUpType == type);
        if (slot != null)
            RefreshVisual(slot, animate);
    }

    private void RefreshVisual(PowerUpSlot slot, bool animate)
    {
        if (slot.powerUpImage == null)
            return;

        Sprite targetSprite = null;
        Color targetColor;

        if (IsAvailable(slot.powerUpType))
        {
            targetSprite = slot.unlockedSprite;
            targetColor = unlockedColor;
        }
        else if (PlayerPrefs.GetInt(GetUnlockedKey(slot.powerUpType), 0) == 1)
        {
            targetSprite = slot.usedSprite;
            targetColor = consumedColor;
        }
        else
        {
            targetSprite = slot.lockedSprite;
            targetColor = lockedColor;
        }

        if (animate && transitionDuration > 0f)
            StartCoroutine(FadeTransition(slot, targetSprite, targetColor));
        else
            ApplyInstant(slot, targetSprite, targetColor);
    }

    private void ApplyInstant(PowerUpSlot slot, Sprite sprite, Color color)
    {
        if (sprite != null)
            slot.powerUpImage.sprite = sprite;

        slot.powerUpImage.color = color;
    }

    private IEnumerator FadeTransition(PowerUpSlot slot, Sprite targetSprite, Color targetColor)
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

        if (targetSprite != null)
            img.sprite = targetSprite;

        t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / half);
            img.color = new Color(targetColor.r, targetColor.g, targetColor.b, Mathf.Lerp(0f, targetColor.a, p));
            yield return null;
        }

        img.color = targetColor;
    }

    private bool IsAvailable(PowerUpType type)
    {
        return PlayerPrefs.GetInt(GetUnlockedKey(type), 0) == 1
            && PlayerPrefs.GetInt(GetConsumedKey(type), 0) == 0;
    }

    private void UnlockCurrentPowerUp(PowerUpType type)
    {
        PlayerPrefs.SetInt(GetUnlockedKey(type), 1);
        PlayerPrefs.SetInt(GetConsumedKey(type), 0);
        PlayerPrefs.Save();
    }

    private string GetUnlockedKey(PowerUpType type) => $"PowerUp_{type}_Unlocked";
    private string GetConsumedKey(PowerUpType type) => $"PowerUp_{type}_Consumed";

    private static string GetUnlockedKeyStatic(PowerUpType type) => $"PowerUp_{type}_Unlocked";
    private static string GetConsumedKeyStatic(PowerUpType type) => $"PowerUp_{type}_Consumed";

    private static void PlayEndPowerUpSFX()
    {
        if (AudioManager.Instance == null)
            return;

        PowerManager[] managers = FindObjectsOfType<PowerManager>();

        foreach (PowerManager manager in managers)
        {
            if (manager.endPowerUpSFX != null)
            {
                AudioManager.Instance.PlaySFX(manager.endPowerUpSFX);
                break;
            }
        }
    }

    private IEnumerator ShowRangeIndicator(Vector3 center, float radius)
    {
        GameObject indicatorObj = new GameObject("KnockbackRangeIndicator");

        LineRenderer line = indicatorObj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = rangeIndicatorSegments;
        line.startWidth = rangeIndicatorWidth;
        line.endWidth = rangeIndicatorWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = rangeIndicatorColor;
        line.endColor = rangeIndicatorColor;

        for (int i = 0; i < rangeIndicatorSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / rangeIndicatorSegments;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
            line.SetPosition(i, pos);
        }

        indicatorObj.transform.position = center;

        float t = 0f;
        Color baseColor = rangeIndicatorColor;

        while (t < rangeIndicatorDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / rangeIndicatorDuration);
            Color fadeColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            line.startColor = fadeColor;
            line.endColor = fadeColor;
            yield return null;
        }

        Destroy(indicatorObj);
    }
}