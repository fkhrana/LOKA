using UnityEngine;
using System.Collections;

public class ChestReward : MonoBehaviour
{
    [Header("Chest")]
    [SerializeField] private RectTransform chestTransform;
    [SerializeField] private float shakeAngle = 8f;
    [SerializeField] private float shakeSpeed = 25f;

    [Header("Reward")]
    [SerializeField] private RectTransform powerUp;
    [SerializeField] private float powerUpMoveDuration = 2f;
    [SerializeField] private float powerUpMoveHeight = 300f;
    [SerializeField] private float powerUpRotateAngle = 12f;
    [SerializeField] private float powerUpRotateSpeed = 5f;

    [Header("SFX")]
    [SerializeField] private AudioClip chestOpenSFX;

    [Header("Panels")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject winPanel;

    private bool isOpened = false;
    private Quaternion originalRotation;
    private Vector2 powerUpOriginalPosition;

    private void Start()
    {
        if (chestTransform != null)
            originalRotation = chestTransform.localRotation;

        if (powerUp != null)
        {
            powerUpOriginalPosition = powerUp.anchoredPosition;
            powerUp.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isOpened || chestTransform == null)
            return;

        float angle =
            Mathf.Sin(Time.unscaledTime * shakeSpeed) * shakeAngle;

        chestTransform.localRotation =
            originalRotation *
            Quaternion.Euler(0f, 0f, angle);
    }

    public void OpenChest()
    {
        if (isOpened)
            return;

        isOpened = true;

        if (chestTransform != null)
            chestTransform.localRotation = originalRotation;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(chestOpenSFX);

        StartCoroutine(OpenChestEffect());
    }

    private IEnumerator OpenChestEffect()
    {
        yield return new WaitForSecondsRealtime(0.15f);

        if (powerUp == null)
            yield break;

        powerUp.gameObject.SetActive(true);

        powerUp.anchoredPosition = powerUpOriginalPosition;
        powerUp.localRotation = Quaternion.identity;

        Vector2 targetPosition =
            powerUpOriginalPosition +
            new Vector2(0f, powerUpMoveHeight);

        float elapsed = 0f;

        while (elapsed < powerUpMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / powerUpMoveDuration
            );

            t = 1f - Mathf.Pow(1f - t, 3f);

            powerUp.anchoredPosition =
                Vector2.Lerp(
                    powerUpOriginalPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        powerUp.anchoredPosition = targetPosition;

        StartCoroutine(PowerUpRotateLoop());
    }

    private IEnumerator PowerUpRotateLoop()
    {
        while (isOpened && powerUp != null)
        {
            float angle =
                Mathf.Sin(
                    Time.unscaledTime * powerUpRotateSpeed
                ) * powerUpRotateAngle;

            powerUp.localRotation =
                Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }
    }

    public void GoFinalResult()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(true);
    }
}