using UnityEngine;

public class RewardButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClip[] rewardSFX;

    public void PlayRewardSFX()
    {
        if (rewardSFX == null || rewardSFX.Length == 0) return;
        if (AudioManager.Instance == null) return;

        AudioClip sfx = rewardSFX[Random.Range(0, rewardSFX.Length)];
        AudioManager.Instance.PlaySFX(sfx);
    }
}