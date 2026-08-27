using UnityEngine;
using TMPro;

public class QualityDropdownController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown graphicsDropdown;

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
}
}