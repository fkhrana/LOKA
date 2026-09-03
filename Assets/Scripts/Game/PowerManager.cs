using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    [SerializeField] private Image powerUpImage;
    [SerializeField] private float freezeDuration = 5f;

    [Header("Warna saat masih terkunci")]
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color consumedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private const string UnlockedKey = "PowerUp_Freeze_Unlocked";
    private const string ConsumedKey = "PowerUp_Freeze_Consumed";
    private bool isFrozen;

    private void Start()
    {
        RefreshVisual();
    }

    public void SetLocked()
    {
        PlayerPrefs.SetInt(UnlockedKey, 0);
        PlayerPrefs.SetInt(ConsumedKey, 0);
        PlayerPrefs.Save();
        RefreshVisual();
    }

    public void SetUnlocked()
    {
        UnlockPowerUp();
        RefreshVisual();
    }

    public static void UnlockPowerUp()
    {
        PlayerPrefs.SetInt(UnlockedKey, 1);
        PlayerPrefs.SetInt(ConsumedKey, 0);
        PlayerPrefs.Save();
    }

    public void UsePowerUp()
    {
        if (!IsAvailable() || isFrozen)
            return;

        PlayerPrefs.SetInt(ConsumedKey, 1);
        PlayerPrefs.Save();
        RefreshVisual();

        StartCoroutine(FreezeEnemies());
    }

    private bool IsAvailable()
    {
        return PlayerPrefs.GetInt(UnlockedKey, 0) == 1
            && PlayerPrefs.GetInt(ConsumedKey, 0) == 0;
    }

    private void RefreshVisual()
    {
        if (powerUpImage == null)
            return;

        if (IsAvailable())
            powerUpImage.color = unlockedColor;
        else if (PlayerPrefs.GetInt(UnlockedKey, 0) == 1)
            powerUpImage.color = consumedColor;
        else
            powerUpImage.color = lockedColor;
    }

    private IEnumerator FreezeEnemies()
    {
        isFrozen = true;
        EnemyMovementBehavior.SetAllMovementPaused(true);

        yield return new WaitForSeconds(freezeDuration);

        EnemyMovementBehavior.SetAllMovementPaused(false);
        isFrozen = false;
    }
}