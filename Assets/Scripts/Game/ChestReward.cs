using UnityEngine;

public class ChestReward : MonoBehaviour
{
[Header("Chest")]
[SerializeField] private Animator chestAnimator;
[SerializeField] private RectTransform chestTransform;

[Header("Shake")]
[SerializeField] private float shakeAngle = 8f;
[SerializeField] private float shakeSpeed = 25f;

[Header("Reward")]
[SerializeField] private GameObject powerUp;
[SerializeField] private Animator powerUpAnimator;

[Header("SFX")]
[SerializeField] private AudioClip chestOpenSFX;

[Header("Panels")]
[SerializeField] private GameObject rewardPanel;
[SerializeField] private GameObject winPanel;

private bool isOpened = false;
private Quaternion originalRotation;

private void Start()
{
    if (chestAnimator != null)
        chestAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

    if (powerUpAnimator != null)
        powerUpAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

    if (powerUp != null)
        powerUp.SetActive(false);

    if (chestTransform != null)
        originalRotation = chestTransform.localRotation;
}

private void Update()
{
    if (isOpened || chestTransform == null)
        return;

    float angle = Mathf.Sin(Time.unscaledTime * shakeSpeed) * shakeAngle;

    chestTransform.localRotation =
        originalRotation * Quaternion.Euler(0f, 0f, angle);
}

public void OpenChest()
{
    if (isOpened)
        return;

    isOpened = true;

    if (chestTransform != null)
        chestTransform.localRotation = originalRotation;

    // SFX chest dibuka
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySFX(chestOpenSFX);

    // Chest membuka
    if (chestAnimator != null)
        chestAnimator.SetTrigger("Open");

    // PowerUp langsung muncul setelah chest diklik
    if (powerUp != null)
        powerUp.SetActive(true);

    if (powerUpAnimator != null)
        powerUpAnimator.Play("PowerUpAppear", 0, 0f);
}

public void GoFinalResult()
{
    if (rewardPanel != null)
        rewardPanel.SetActive(false);

    if (winPanel != null)
        winPanel.SetActive(true);
}
}
