using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsPanelManager : MonoBehaviour
{
    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    [Header("Audio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private AudioMixer mainAudioMixer;

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat("MusicV", musicSlider.value);
    }

    public void ChangeSfxVolume()
    {
        mainAudioMixer.SetFloat("SFXV", sfxSlider.value);
    }
}