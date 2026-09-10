using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    public enum PowerUpType
    {
        Freeze,
        Combo
    }

    [SerializeField] private Image powerUpImage;
    [SerializeField] private PowerUpType powerUpType = PowerUpType.Freeze;
    [SerializeField] private float freezeDuration = 5f;
    [SerializeField] private float comboRadius = 2.5f;
    [SerializeField] private bool unlockOnStartForTesting;

    [Header("SFX")]
    [SerializeField] private AudioClip freezeSFX;
    [SerializeField] private AudioClip comboSFX;
    [SerializeField] private AudioClip endPowerUpSFX;

    [Header("Warna saat masih terkunci")]
    [SerializeField] private Color lockedColor =
        new Color(0.3f, 0.3f, 0.3f, 1f);

    [SerializeField] private Color unlockedColor =
        Color.white;

    [SerializeField] private Color consumedColor =
        new Color(0.3f, 0.3f, 0.3f, 1f);

    private const string UnlockedKey =
        "PowerUp_Freeze_Unlocked";

    private const string ConsumedKey =
        "PowerUp_Freeze_Consumed";

    private const string ComboUnlockedKey =
        "PowerUp_Combo_Unlocked";

    private const string ComboConsumedKey =
        "PowerUp_Combo_Consumed";

    private bool isFrozen;
    private static bool isComboActive;

    public static bool IsComboActive =>
        isComboActive;

    public static float ActiveComboRadius
    {
        get;
        private set;
    }

    private void Start()
    {
        if (unlockOnStartForTesting)
            UnlockCurrentPowerUp();

        RefreshVisual();
    }

    public void SetLocked()
    {
        PlayerPrefs.SetInt(
            GetUnlockedKey(),
            0
        );

        PlayerPrefs.SetInt(
            GetConsumedKey(),
            0
        );

        PlayerPrefs.Save();

        RefreshVisual();
    }

    public void SetUnlocked()
    {
        UnlockCurrentPowerUp();
        RefreshVisual();
    }

    public static void UnlockPowerUp()
    {
        PlayerPrefs.SetInt(
            UnlockedKey,
            1
        );

        PlayerPrefs.SetInt(
            ConsumedKey,
            0
        );

        PlayerPrefs.Save();
    }

    public void UsePowerUp()
    {
        if (!IsAvailable() || isFrozen)
            return;

        PlayerPrefs.SetInt(
            GetConsumedKey(),
            1
        );

        PlayerPrefs.Save();

        RefreshVisual();

        // SFX Power Up
        if (powerUpType == PowerUpType.Freeze)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(
                    freezeSFX
                );
        }
        else if (powerUpType == PowerUpType.Combo)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(
                    comboSFX
                );
        }

        if (powerUpType == PowerUpType.Combo)
        {
            isComboActive = true;
            ActiveComboRadius = comboRadius;
        }
        else
        {
            StartCoroutine(
                FreezeEnemies()
            );
        }
    }

    public static void EndComboPowerUp()
    {
        isComboActive = false;
        ActiveComboRadius = 0f;

        PlayEndPowerUpSFX();
    }

    private static void PlayEndPowerUpSFX()
    {
        if (AudioManager.Instance == null)
            return;

        PowerManager[] managers =
            FindObjectsOfType<PowerManager>();

        foreach (PowerManager manager in managers)
        {
            if (manager.endPowerUpSFX != null)
            {
                AudioManager.Instance.PlaySFX(
                    manager.endPowerUpSFX
                );

                break;
            }
        }
    }

    private bool IsAvailable()
    {
        return PlayerPrefs.GetInt(
            GetUnlockedKey(),
            0
        ) == 1
        &&
        PlayerPrefs.GetInt(
            GetConsumedKey(),
            0
        ) == 0;
    }

    private void UnlockCurrentPowerUp()
    {
        PlayerPrefs.SetInt(
            GetUnlockedKey(),
            1
        );

        PlayerPrefs.SetInt(
            GetConsumedKey(),
            0
        );

        PlayerPrefs.Save();
    }

    private string GetUnlockedKey()
    {
        return powerUpType == PowerUpType.Combo
            ? ComboUnlockedKey
            : UnlockedKey;
    }

    private string GetConsumedKey()
    {
        return powerUpType == PowerUpType.Combo
            ? ComboConsumedKey
            : ConsumedKey;
    }

    private void RefreshVisual()
    {
        if (powerUpImage == null)
            return;

        if (IsAvailable())
        {
            powerUpImage.color =
                unlockedColor;
        }
        else if (
            PlayerPrefs.GetInt(
                GetUnlockedKey(),
                0
            ) == 1
        )
        {
            powerUpImage.color =
                consumedColor;
        }
        else
        {
            powerUpImage.color =
                lockedColor;
        }
    }

    private IEnumerator FreezeEnemies()
    {
        isFrozen = true;

        EnemyMovementBehavior.SetAllMovementPaused(
            true
        );

        yield return new WaitForSeconds(
            freezeDuration
        );

        EnemyMovementBehavior.SetAllMovementPaused(
            false
        );

        // SFX saat Power Up selesai
        PlayEndPowerUpSFX();

        isFrozen = false;
    }
}