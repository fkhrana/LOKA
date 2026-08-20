using UnityEngine;

public class ChestReward : MonoBehaviour
{
    [Header("Chest")]
    [SerializeField] private Animator chestAnimator;

    [Header("Reward")]
    [SerializeField] private GameObject powerUp;
    [SerializeField] private Animator powerUpAnimator;

    [Header("Panels")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject winPanel;

    private bool isOpened = false;

    private void Start()
    {
        if (chestAnimator != null)
            chestAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (powerUpAnimator != null)
            powerUpAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (powerUp != null)
            powerUp.SetActive(false);
    }

    public void OpenChest()
    {
        if (isOpened)
            return;

        isOpened = true;

        // Chest bergerak
        if (chestAnimator != null)
            chestAnimator.SetTrigger("Open");

        // Aktifkan PowerUp
        if (powerUp != null)
            powerUp.SetActive(true);

        if (powerUpAnimator != null)
            powerUpAnimator.Play("PowerUp_Appear", 0, 0f);
    }

    public void GoFinalResult()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(true);
    }
}