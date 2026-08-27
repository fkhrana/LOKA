using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private string bgmName;

    void Start()
    {
        if (AudioManager.Instance == null) return;
        if (bgmClip != null) AudioManager.Instance.PlayBGM(bgmClip);
        else if (!string.IsNullOrEmpty(bgmName)) AudioManager.Instance.PlayBGM(bgmName);
    }
}