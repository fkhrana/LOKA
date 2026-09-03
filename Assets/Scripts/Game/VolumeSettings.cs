using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("SFX")]
    [SerializeField] private AudioClip sliderTickSound; // Drag suara "tic" di sini

    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;

        if (bgmSlider != null)
        {
            // Pastikan range slider aman: minValue TIDAK BOLEH 0
            bgmSlider.wholeNumbers = false;
            bgmSlider.minValue = 0.0001f;
            bgmSlider.maxValue = 1f;

            // Set posisi slider sesuai volume saat ini TANPA memicu event dulu
            bgmSlider.SetValueWithoutNotify(AudioManager.Instance.GetCurrentBGMVolume());

            bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.wholeNumbers = false;
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;

            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetCurrentSFXVolume());

            sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
    }

    private void OnDisable()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }

    private void OnBGMChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        PlaySliderTick();
    }

    private void OnSFXChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        PlaySliderTick();
    }

    private void PlaySliderTick()
    {
        if (sliderTickSound == null) return;
        AudioManager.Instance?.PlayUISFX(sliderTickSound);
    }
}