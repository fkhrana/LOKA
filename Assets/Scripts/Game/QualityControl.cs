using UnityEngine;
using TMPro;

public class QualityDropdownController : MonoBehaviour
{
    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    [Header("SFX")]
    [SerializeField] private AudioClip dropdownTickSound; // Drag suara "klik dropdown" di sini

    private void Start()
    {
        if (graphicsDropdown == null) return;

        // Muat nilai tersimpan, fallback ke kualitas aktif
        int savedLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        savedLevel = Mathf.Clamp(savedLevel, 0, QualitySettings.names.Length - 1);
        graphicsDropdown.SetValueWithoutNotify(savedLevel);
        graphicsDropdown.RefreshShownValue();
        QualitySettings.SetQualityLevel(savedLevel);

        graphicsDropdown.onValueChanged.AddListener(ChangeQuality);
    }

    private void ChangeQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();

        // 🔥 Putar suara dropdown
        PlayDropdownTick();
    }

    private void PlayDropdownTick()
    {
        if (dropdownTickSound == null) return;
        AudioManager.Instance?.PlayUISFX(dropdownTickSound);
    }
}