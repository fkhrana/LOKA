using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelWinAksaraStatusUI : MonoBehaviour
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

    [Header("Target Level")]
    [SerializeField] private int targetLevelIndex = -1;

    [Header("All Aksara Pool")]
    [SerializeField] private List<AksaraData> allAksaraData = new List<AksaraData>();

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

    private int ResolveTargetLevelIndex()
    {
        if (targetLevelIndex >= 0)
            return targetLevelIndex;

        if (LevelManager.Instance != null)
            return LevelManager.Instance.GetCurrentLevelIndex();

        return 0;
    }

    private List<AksaraData> ResolveAksaraPool()
    {
        if (allAksaraData != null && allAksaraData.Count > 0)
            return allAksaraData;

        if (LevelManager.Instance == null)
            return new List<AksaraData>();

        int levelIndex = ResolveTargetLevelIndex();
        return LevelManager.Instance.GetAksaraListForLevel(levelIndex) ?? new List<AksaraData>();
    }

    public void RefreshStatus()
    {
        if (slots == null || slots.Count == 0)
            return;

        int resolvedLevelIndex = ResolveTargetLevelIndex();
        List<AksaraData> resolvedPool = ResolveAksaraPool();

        var collectedThisLevel = PermanentCollectionManager.GetCollectedAksaraForLevel(
            resolvedLevelIndex,
            resolvedPool
        );

        var collectedSet = new HashSet<AksaraData>(collectedThisLevel);

        foreach (StatusSlot slot in slots)
        {
            if (slot == null || slot.image == null)
                continue;

            bool collected = slot.aksaraData != null && collectedSet.Contains(slot.aksaraData);

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
