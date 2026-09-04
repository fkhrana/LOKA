using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinAksaraStatusUI : MonoBehaviour
{
    [System.Serializable]
    public class StatusSlot
    {
        public AksaraData aksaraData;
        public Image image;
        public Sprite collectedSprite;
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

        if (CollectedAksaraManager.Instance == null)
            return;

        foreach (StatusSlot slot in slots)
        {
            if (slot == null || slot.image == null || slot.aksaraData == null)
                continue;

            bool collected =
                CollectedAksaraManager.Instance.IsCollected(slot.aksaraData);

            if (collected)
            {
                if (slot.collectedSprite != null)
                    slot.image.sprite = slot.collectedSprite;

                slot.image.color = Color.white;
            }
        }
    }
}