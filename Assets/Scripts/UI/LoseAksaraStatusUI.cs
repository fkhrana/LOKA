using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseAksaraStatusUI : MonoBehaviour
{
    [System.Serializable]
    public class StatusSlot
    {
        public AksaraData aksaraData;
        public Image image;
        public Sprite collectedSprite;
        public Sprite uncollectedSprite;
        [Range(0f, 1f)] public float uncollectedAlpha = 0.36f;
    }

    [SerializeField] private List<StatusSlot> slots = new List<StatusSlot>();
    [SerializeField] private bool refreshOnEnable = true;

    private void OnEnable()
    {
        if (refreshOnEnable)
            RefreshStatus();
    }

    private void Start()
    {
        if (refreshOnEnable)
            RefreshStatus();
    }

    public void RefreshStatus()
    {
        if (slots == null || slots.Count == 0)
            return;

        bool managerExists = CollectedAksaraManager.Instance != null;

        foreach (StatusSlot slot in slots)
        {
            if (slot == null || slot.image == null)
                continue;

            bool collected = managerExists && slot.aksaraData != null && CollectedAksaraManager.Instance.IsCollected(slot.aksaraData);

            Sprite targetSprite = collected
                ? (slot.collectedSprite != null ? slot.collectedSprite : slot.image.sprite)
                : (slot.uncollectedSprite != null ? slot.uncollectedSprite : slot.image.sprite);

            if (targetSprite != null)
                slot.image.sprite = targetSprite;

            if (collected)
            {
                slot.image.color = Color.white;
            }
            else
            {
                slot.image.color = new Color(0f, 0f, 0f, slot.uncollectedAlpha);
            }
        }
    }
}
