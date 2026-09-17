using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelAksaraStatusUI : MonoBehaviour
{
    [Header("Slot Container")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite collectedSprite;
    [SerializeField] private Sprite uncollectedSprite;

    [Header("Colors")]
    [Tooltip("Warna tint saat aksara sudah dikoleksi.")]
    [SerializeField] private Color collectedColor = Color.white;

    [Tooltip("Warna tint saat aksara belum dikoleksi. Putih/abu transparan biar bentuk aksara tetap keliatan.")]
    [SerializeField] private Color uncollectedColor = new Color(0.6f, 0.6f, 0.6f, 0.36f);

    [Header("Options")]
    [SerializeField] private bool refreshOnEnable = true;

    private readonly List<Image> spawnedSlots = new List<Image>();
    private List<AksaraData> currentAksaraList;

    private void OnEnable()
    {
        if (refreshOnEnable)
            RefreshStatus();
    }

    public void Setup(List<AksaraData> aksaraList)
    {
        currentAksaraList = aksaraList;
        RebuildSlots();
        RefreshStatus();
    }

    private void RebuildSlots()
    {
        if (slotContainer == null || slotPrefab == null)
            return;

        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        spawnedSlots.Clear();

        if (currentAksaraList == null)
            return;

        foreach (AksaraData data in currentAksaraList)
        {
            if (data == null) continue;

            GameObject slotGO = Instantiate(slotPrefab, slotContainer);
            Image img = slotGO.GetComponent<Image>();

            if (img == null)
                img = slotGO.GetComponentInChildren<Image>(true);

            if (img != null)
                spawnedSlots.Add(img);
        }
    }

    public void RefreshStatus()
    {
        if (spawnedSlots.Count == 0 || currentAksaraList == null)
            return;

        bool managerExists = CollectedAksaraManager.Instance != null;

        for (int i = 0; i < spawnedSlots.Count && i < currentAksaraList.Count; i++)
        {
            Image img = spawnedSlots[i];
            AksaraData data = currentAksaraList[i];

            if (img == null) continue;

            bool collected = managerExists
                && data != null
                && CollectedAksaraManager.Instance.IsCollected(data);

            Sprite fallbackSprite = data != null ? data.IconSprite : null;

            Sprite targetSprite = collected
                ? (collectedSprite != null ? collectedSprite : fallbackSprite)
                : (uncollectedSprite != null ? uncollectedSprite : fallbackSprite);

            if (targetSprite != null)
                img.sprite = targetSprite;

            img.color = collected ? collectedColor : uncollectedColor;
        }
    }
}