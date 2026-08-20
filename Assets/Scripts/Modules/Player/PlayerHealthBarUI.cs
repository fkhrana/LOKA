using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthImage;
    [SerializeField] private Sprite[] healthSprites;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform fillTransform;
    [SerializeField] private TMP_Text valueText;

    private Vector3 initialFillScale = Vector3.one;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (healthImage == null)
            healthImage = fillImage;

        if (fillTransform == null && fillImage != null)
            fillTransform = fillImage.rectTransform;

        if (fillTransform != null)
            initialFillScale = fillTransform.localScale;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged += UpdateBar;

        SyncNow();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= UpdateBar;
    }

    public void SyncNow()
    {
        if (playerHealth == null)
            return;

        UpdateBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void UpdateBar(int current, int max)
    {
        float normalized = max > 0 ? (float)current / max : 0f;

        if (healthImage != null && healthSprites != null && healthSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(
                Mathf.FloorToInt((1f - normalized) * healthSprites.Length),
                0,
                healthSprites.Length - 1);

            if (healthSprites[spriteIndex] != null)
                healthImage.sprite = healthSprites[spriteIndex];
        }

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = normalized;
        }

        if (fillImage != null && (healthSprites == null || healthSprites.Length == 0))
        {
            if (fillImage.type == Image.Type.Filled)
            {
                fillImage.fillAmount = normalized;
            }
            else if (fillTransform != null)
            {
                fillTransform.localScale = new Vector3(initialFillScale.x * normalized, initialFillScale.y, initialFillScale.z);
            }
        }

        if (valueText != null)
            valueText.text = $"{current}/{max}";
    }
}