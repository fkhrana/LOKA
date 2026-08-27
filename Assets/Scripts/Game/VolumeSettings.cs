using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

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
    }

    private void OnSFXChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }
}