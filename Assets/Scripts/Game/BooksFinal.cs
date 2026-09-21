using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BooksFinal : MonoBehaviour
{
    [Header("Book Pages")]
    [SerializeField] private GameObject rewardPage;
    [SerializeField] private GameObject aksaraPage;

    [Header("Reward Page")]
    [SerializeField] private Image rewardPowerUpIcon;
    [SerializeField] private Image rewardPowerUpNameImage;
    [SerializeField] private TextMeshProUGUI rewardDescriptionText;

    [Header("Aksara Page")]
    [SerializeField] private Image[] aksaraIcons = new Image[5];

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;

    [Header("Review Levels")]
    [SerializeField] private int[] reviewLevelIndexes = { 0, 1, 2, 3 };

    private readonly List<LevelReviewData> reviewData = new();
    private int currentReviewIndex;

    private const string KEY_REWARD_PREFIX = "FinalBook_Reward_L";
    private const string KEY_AKSARA_PREFIX = "FinalBook_Aksara_L";
    private const string KEY_REWARD_DESC_PREFIX = "FinalBook_RewardDesc_L";

    private static readonly Dictionary<int, LevelReviewData> RuntimeReviewCache = new();

    private void Awake()
    {
        BindButtons();
    }

    private void OnEnable()
    {
        LoadReviewData();
        RefreshBook();
    }

    private void BindButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextReview);
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(PreviousReview);
        }

        if (homeButton != null)
            homeButton.onClick.RemoveAllListeners();

        if (replayButton != null)
            replayButton.onClick.RemoveAllListeners();
    }

    public void NextReview()
    {
        if (reviewData.Count == 0)
            return;

        currentReviewIndex = (currentReviewIndex + 1) % reviewData.Count;
        RefreshBook();
    }

    public void PreviousReview()
    {
        if (reviewData.Count == 0)
            return;

        currentReviewIndex = (currentReviewIndex - 1 + reviewData.Count) % reviewData.Count;
        RefreshBook();
    }

    private void RefreshBook()
    {
        if (rewardPage != null)
            rewardPage.SetActive(true);

        if (aksaraPage != null)
            aksaraPage.SetActive(true);

        if (reviewData.Count == 0)
        {
            if (rewardDescriptionText != null)
                rewardDescriptionText.text = "Belum ada reward.";

            SetRewardImages(null, null);
            SetAksaraIcons(null);

            if (nextButton != null)
                nextButton.interactable = false;

            if (previousButton != null)
                previousButton.interactable = false;

            return;
        }

        LevelReviewData currentData = reviewData[currentReviewIndex];

        SetRewardImages(currentData.RewardIcon, currentData.RewardNameIcon);

        if (rewardDescriptionText != null)
            rewardDescriptionText.text = currentData.RewardDescription;

        SetAksaraIcons(currentData.AksaraIcons);

        if (nextButton != null)
            nextButton.interactable = currentReviewIndex < reviewData.Count - 1;

        if (previousButton != null)
            previousButton.interactable = currentReviewIndex > 0;
    }

    private void SetRewardImages(Sprite rewardIcon, Sprite rewardNameIcon)
    {
        if (rewardPowerUpIcon != null)
        {
            rewardPowerUpIcon.sprite = rewardIcon;
            rewardPowerUpIcon.preserveAspect = true;
            rewardPowerUpIcon.SetNativeSize();
        }

        if (rewardPowerUpNameImage != null)
        {
            rewardPowerUpNameImage.sprite = rewardNameIcon;
            rewardPowerUpNameImage.preserveAspect = true;
            rewardPowerUpNameImage.SetNativeSize();
        }
    }

    private void SetAksaraIcons(IReadOnlyList<Sprite> sprites)
    {
        for (int i = 0; i < aksaraIcons.Length; i++)
        {
            if (aksaraIcons[i] == null)
                continue;

            Sprite sprite = sprites != null && i < sprites.Count ? sprites[i] : null;
            aksaraIcons[i].sprite = sprite;
            aksaraIcons[i].preserveAspect = true;

            if (sprite != null)
            {
                aksaraIcons[i].enabled = true;
                aksaraIcons[i].SetNativeSize();
                aksaraIcons[i].color = Color.white;
            }
            else
            {
                aksaraIcons[i].enabled = false;
                aksaraIcons[i].color = Color.clear;
            }
        }
    }

    private void LoadReviewData()
    {
        reviewData.Clear();

        foreach (int levelIndex in reviewLevelIndexes)
        {
            LevelReviewData data = LoadReviewForLevel(levelIndex);
            if (data == null)
                continue;

            reviewData.Add(data);
        }

        if (reviewData.Count == 0)
        {
            reviewData.Add(new LevelReviewData(
                "Review Level",
                null,
                null,
                "Belum ada reward.",
                new List<Sprite>()));
        }

        currentReviewIndex = Mathf.Clamp(currentReviewIndex, 0, reviewData.Count - 1);
    }

    private LevelReviewData LoadReviewForLevel(int levelIndex)
    {
        if (RuntimeReviewCache.TryGetValue(levelIndex, out var cached))
        {
            AppendCollectedAksaraIfNeeded(levelIndex, cached);
            return cached;
        }

        string rewardDescription = PlayerPrefs.GetString(GetRewardDescriptionKey(levelIndex), string.Empty);
        string rewardIconKey = PlayerPrefs.GetString(GetRewardIconKey(levelIndex), string.Empty);
        string rewardNameIconKey = PlayerPrefs.GetString(GetRewardNameIconKey(levelIndex), string.Empty);
        string aksaraCsv = PlayerPrefs.GetString(GetAksaraKey(levelIndex), string.Empty);

        List<Sprite> aksaraSprites = new();

        if (!string.IsNullOrEmpty(aksaraCsv))
        {
            var spriteKeys = aksaraCsv
                .Split('|')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim())
                .Distinct();

            foreach (string spriteKey in spriteKeys)
            {
                Sprite sprite = LoadSpriteByKey(spriteKey);
                if (sprite != null)
                    aksaraSprites.Add(sprite);
            }
        }

        AppendCollectedAksara(levelIndex, aksaraSprites);

        if (string.IsNullOrEmpty(rewardDescription) && string.IsNullOrEmpty(rewardIconKey) &&
            string.IsNullOrEmpty(rewardNameIconKey) && aksaraSprites.Count == 0)
            return null;

        Sprite rewardIcon = LoadSpriteByKey(rewardIconKey);
        Sprite rewardNameIcon = LoadSpriteByKey(rewardNameIconKey);

        return new LevelReviewData(
            $"Level {levelIndex + 1}",
            rewardIcon,
            rewardNameIcon,
            string.IsNullOrEmpty(rewardDescription) ? "Tidak ada reward" : rewardDescription,
            aksaraSprites);
    }

    private static void AppendCollectedAksaraIfNeeded(int levelIndex, LevelReviewData data)
    {
        if (data == null || data.AksaraIcons.Count > 0)
            return;

        AppendCollectedAksara(levelIndex, data.AksaraIcons);
    }

    private static void AppendCollectedAksara(int levelIndex, List<Sprite> targetSprites)
    {
        if (targetSprites == null || LevelManager.Instance == null)
            return;

        List<AksaraData> levelAksara = LevelManager.Instance.GetAksaraListForLevel(levelIndex);
        List<AksaraData> collectedAksara = PermanentCollectionManager.GetCollectedAksaraForLevel(
            levelIndex,
            levelAksara
        );

        foreach (AksaraData aksara in collectedAksara)
        {
            if (aksara != null && aksara.IconSprite != null && !targetSprites.Contains(aksara.IconSprite))
                targetSprites.Add(aksara.IconSprite);
        }
    }

    public static void SaveReviewData(
        int levelIndex,
        Sprite rewardIcon,
        Sprite rewardNameIcon,
        string rewardDescription,
        IEnumerable<Sprite> aksaraSprites)
    {
        if (levelIndex < 0)
            return;

        var aksaraList = ResolveAksaraSpritesForSave(levelIndex, aksaraSprites);

        var data = new LevelReviewData(
            $"Level {levelIndex + 1}",
            rewardIcon,
            rewardNameIcon,
            string.IsNullOrEmpty(rewardDescription) ? "Tidak ada reward" : rewardDescription,
            aksaraList);

        RuntimeReviewCache[levelIndex] = data;

        PlayerPrefs.SetString(GetRewardDescriptionKey(levelIndex), data.RewardDescription);
        PlayerPrefs.SetString(GetRewardIconKey(levelIndex), GetSpriteKey(rewardIcon));
        PlayerPrefs.SetString(GetRewardNameIconKey(levelIndex), GetSpriteKey(rewardNameIcon));

        string aksaraJoined = string.Join("|", aksaraList
            .Where(x => x != null)
            .Select(GetSpriteKey)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct());

        PlayerPrefs.SetString(GetAksaraKey(levelIndex), aksaraJoined);
        PlayerPrefs.Save();
    }

    private static List<Sprite> ResolveAksaraSpritesForSave(int levelIndex, IEnumerable<Sprite> aksaraSprites)
    {
        var merged = new List<Sprite>();

        if (aksaraSprites != null)
        {
            foreach (Sprite sprite in aksaraSprites)
            {
                if (sprite != null && !merged.Contains(sprite))
                    merged.Add(sprite);
            }
        }

        string existingCsv = PlayerPrefs.GetString(GetAksaraKey(levelIndex), string.Empty);
        if (!string.IsNullOrEmpty(existingCsv))
        {
            foreach (string key in existingCsv.Split('|'))
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                Sprite sprite = LoadSpriteByKey(key);
                if (sprite != null && !merged.Contains(sprite))
                    merged.Add(sprite);
            }
        }

        if (LevelManager.Instance != null)
        {
            var levelAksara = LevelManager.Instance.GetAksaraListForLevel(levelIndex);
            if (levelAksara != null)
            {
                foreach (AksaraData aksara in PermanentCollectionManager.GetCollectedAksaraForLevel(levelIndex, levelAksara))
                {
                    if (aksara == null || aksara.IconSprite == null || merged.Contains(aksara.IconSprite))
                        continue;

                    merged.Add(aksara.IconSprite);
                }
            }
        }

        return merged;
    }

    public static void SaveAutomaticPowerUpReward(
        int levelIndex,
        PowerManager.PowerUpType powerUpType,
        Sprite rewardIcon,
        Sprite rewardNameSprite,
        string rewardDescription)
    {
        string resolvedDescription = string.IsNullOrEmpty(rewardDescription)
            ? powerUpType switch
            {
                PowerManager.PowerUpType.Freeze => "Membekukan musuh untuk sementara.",
                PowerManager.PowerUpType.Combo => "Mengalahkan beberapa musuh sekaligus.",
                PowerManager.PowerUpType.Shield => "Melindungi pemain dari serangan musuh.",
                _ => "Reward berhasil didapatkan."
            }
            : rewardDescription;

        SaveReviewData(
            levelIndex,
            rewardIcon,
            rewardNameSprite,
            resolvedDescription,
            null
        );
    }

    public static void AppendCollectedAksara(int levelIndex, Sprite aksaraSprite)
    {
        if (levelIndex < 0 || aksaraSprite == null)
            return;

        string spriteKey = GetSpriteKey(aksaraSprite);
        if (string.IsNullOrEmpty(spriteKey))
            return;

        string existing = PlayerPrefs.GetString(GetAksaraKey(levelIndex), string.Empty);
        List<string> keys = existing
            .Split('|')
            .Where(key => !string.IsNullOrEmpty(key))
            .ToList();

        if (!keys.Contains(spriteKey))
            keys.Add(spriteKey);

        PlayerPrefs.SetString(GetAksaraKey(levelIndex), string.Join("|", keys));

        if (RuntimeReviewCache.TryGetValue(levelIndex, out LevelReviewData cached) &&
            !cached.AksaraIcons.Contains(aksaraSprite))
        {
            cached.AksaraIcons.Add(aksaraSprite);
        }

        PlayerPrefs.Save();
    }

    public static void ClearReviewData(int levelIndex)
    {
        if (RuntimeReviewCache.ContainsKey(levelIndex))
            RuntimeReviewCache.Remove(levelIndex);

        PlayerPrefs.DeleteKey(GetRewardDescriptionKey(levelIndex));
        PlayerPrefs.DeleteKey(GetRewardIconKey(levelIndex));
        PlayerPrefs.DeleteKey(GetRewardNameIconKey(levelIndex));
        PlayerPrefs.DeleteKey(GetAksaraKey(levelIndex));
        PlayerPrefs.Save();
    }

    private static Sprite LoadSpriteByKey(string spriteKey)
    {
        if (string.IsNullOrEmpty(spriteKey))
            return null;

#if UNITY_EDITOR
        string assetPath = AssetDatabase.GUIDToAssetPath(spriteKey);
        if (!string.IsNullOrEmpty(assetPath))
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
                return sprite;
        }
#endif

        Sprite resourceSprite = Resources.Load<Sprite>(spriteKey);
        if (resourceSprite != null)
            return resourceSprite;

        return Resources.FindObjectsOfTypeAll<Sprite>()
            .FirstOrDefault(s => s.name == spriteKey);
    }

    private static string GetSpriteKey(Sprite sprite)
    {
        if (sprite == null)
            return string.Empty;

#if UNITY_EDITOR
        string assetPath = AssetDatabase.GetAssetPath(sprite);
        if (!string.IsNullOrEmpty(assetPath))
            return AssetDatabase.AssetPathToGUID(assetPath);
#endif

        return sprite.name;
    }

    private static string GetRewardDescriptionKey(int levelIndex)
    {
        return KEY_REWARD_DESC_PREFIX + levelIndex;
    }

    private static string GetRewardIconKey(int levelIndex)
    {
        return KEY_REWARD_PREFIX + levelIndex;
    }

    private static string GetRewardNameIconKey(int levelIndex)
    {
        return KEY_REWARD_PREFIX + levelIndex + "_Name";
    }

    private static string GetAksaraKey(int levelIndex)
    {
        return KEY_AKSARA_PREFIX + levelIndex;
    }

    private class LevelReviewData
    {
        public string LevelLabel { get; }
        public Sprite RewardIcon { get; }
        public Sprite RewardNameIcon { get; }
        public string RewardDescription { get; }
        public List<Sprite> AksaraIcons { get; }

        public LevelReviewData(
            string levelLabel,
            Sprite rewardIcon,
            Sprite rewardNameIcon,
            string rewardDescription,
            List<Sprite> aksaraIcons)
        {
            LevelLabel = levelLabel;
            RewardIcon = rewardIcon;
            RewardNameIcon = rewardNameIcon;
            RewardDescription = rewardDescription;
            AksaraIcons = aksaraIcons ?? new List<Sprite>();
        }
    }
}
