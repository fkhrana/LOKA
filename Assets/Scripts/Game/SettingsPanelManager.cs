using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsPanelManager : MonoBehaviour
{
    public TMP_Dropdown graphicsDropdown;

    public Slider masterv;
    public Slider musicv;
    public Slider sfxv;

    public AudioMixer mainAudioMixer;

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat("MasterV", masterv.value);
    }

    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat("MusicV", musicv.value);
    }

    public void ChangeSfxVolume()
    {
        mainAudioMixer.SetFloat("SFXV", sfxv.value);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}