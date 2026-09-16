using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("SFX")]
    [SerializeField] private AudioClip sliderTickSound;
    [Range(0f, 1f)] [SerializeField] private float sliderTickVolume = 0.5f;

    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;

        if (bgmSlider != null)
        {
            // Range aman: minValue tidak boleh 0.
            bgmSlider.wholeNumbers = false;
            bgmSlider.minValue = 0.0001f;
            bgmSlider.maxValue = 1f;

            // Set posisi slider tanpa memicu event.
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
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }

    // Slider BGM berubah: update volume + tick.
    private void OnBGMChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        PlaySliderTick();
    }

    // Slider SFX berubah: update volume + tick.
    private void OnSFXChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        PlaySliderTick();
    }

    // Putar SFX tick saat slider bergerak (tanpa ducking BGM).
    private void PlaySliderTick()
    {
        if (sliderTickSound == null) return;
        AudioManager.Instance?.PlaySliderTickSFX(sliderTickSound, sliderTickVolume);
    }
}