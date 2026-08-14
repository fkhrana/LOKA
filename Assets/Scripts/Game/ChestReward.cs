using UnityEngine;
using UnityEngine.SceneManagement;

public class ChestReward : MonoBehaviour
{
    [Header("Chest")]
    [SerializeField] private Animator chestAnimator;

    [Header("Reward")]
    [SerializeField] private GameObject powerUp;
    [SerializeField] private Animator powerUpAnimator;

    private bool isOpened = false;

    private void Start()
    {
        powerUp.SetActive(false);
    }

    public void OpenChest()
    {
        if (isOpened)
            return;

        isOpened = true;

        // Chest bergerak
        chestAnimator.SetTrigger("Open");

        // Aktifkan PowerUp
        powerUp.SetActive(true);
        powerUpAnimator.Play("PowerUp_Appear");
    }

    public void GoFinalResult()
    {
        SceneManager.LoadScene("FinalResult");
    }
}