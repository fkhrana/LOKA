using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level UI")]
    [SerializeField] private GameObject levelCardPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Carousel")]
    [SerializeField] private CarouselSnap carouselSnap;
    [SerializeField] private UnityEngine.UI.Button replayButton;

    [Header("Level Icons")]
    [SerializeField] private Sprite[] lockedLevelIcons;
    [SerializeField] private Sprite[] unlockedLevelIcons;

    [Header("SFX")]
    [SerializeField] private AudioClip failureSound;
    [SerializeField] private AudioClip successSound;

    [Header("Gameplay")]
    [SerializeField] private string gameplaySceneName = "MainGameplay(Drawing)";

    private int totalLevels;
    private const string UNLOCKED_KEY = "LevelUnlocked_";
    private const string COMPLETED_KEY = "LevelCompleted_";
    private const string CURRENT_KEY = "CurrentLevelIndex";
    private CanvasGroup replayCanvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        totalLevels = Mathf.Max(lockedLevelIcons?.Length ?? 0, unlockedLevelIcons?.Length ?? 0);
        if (totalLevels <= 0) Debug.LogWarning("LevelManager: Tidak ada level.");

        UnlockLevel(0);

        if (replayButton != null)
        {
            replayCanvasGroup = replayButton.GetComponent<CanvasGroup>();
            if (replayCanvasGroup == null)
                replayCanvasGroup = replayButton.gameObject.AddComponent<CanvasGroup>();
            replayCanvasGroup.alpha = 1f;

            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(ReplayWithShake);
        }
    }

    private void Start()
    {
        GenerateLevels();
        if (carouselSnap != null) carouselSnap.Refresh();
        UpdateReplayButton();
    }

    private void GenerateLevels()
    {
        if (contentParent == null || levelCardPrefab == null) return;
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        for (int i = 0; i < totalLevels; i++)
        {
            var card = Instantiate(levelCardPrefab, contentParent);
            var levelUI = card.GetComponent<LevelUI>();
            if (levelUI != null)
                levelUI.Setup(i, IsUnlocked(i), GetIcon(i));
        }
    }

    private string GetKey(string prefix, int index) => prefix + index;
    public bool IsUnlocked(int index) => index == 0 || PlayerPrefs.GetInt(GetKey(UNLOCKED_KEY, index), 0) == 1;
    public bool IsCompleted(int index) => PlayerPrefs.GetInt(GetKey(COMPLETED_KEY, index), 0) == 1;

    private void UnlockLevel(int index)
    {
        if (index < 0 || index >= totalLevels) return;
        PlayerPrefs.SetInt(GetKey(UNLOCKED_KEY, index), 1);
    }

    private Sprite GetIcon(int index)
    {
        if (IsUnlocked(index) && index < unlockedLevelIcons.Length)
            return unlockedLevelIcons[index];
        if (!IsUnlocked(index) && index < lockedLevelIcons.Length)
            return lockedLevelIcons[index];
        return null;
    }

    public void CompleteLevel(int index)
    {
        if (index < 0 || index >= totalLevels) return;
        PlayerPrefs.SetInt(GetKey(COMPLETED_KEY, index), 1);
        UnlockLevel(index);
        if (index + 1 < totalLevels) UnlockLevel(index + 1);
        PlayerPrefs.Save();
        RefreshUI();
    }

    public void SetCurrentLevel(int index)
    {
        if (index < 0 || index >= totalLevels || !IsUnlocked(index)) return;
        PlayerPrefs.SetInt(CURRENT_KEY, index);
        PlayerPrefs.Save();
    }

    public int GetCurrentLevelIndex() => Mathf.Clamp(PlayerPrefs.GetInt(CURRENT_KEY, 0), 0, totalLevels - 1);

    public void ReplaySelectedLevel()
    {
        int index = carouselSnap?.GetCurrentIndex() ?? GetCurrentLevelIndex();

        if (!IsUnlocked(index))
        {
            if (failureSound != null)
                AudioManager.Instance?.PlayUISFX(failureSound);
            else
                AudioManager.Instance?.PlayUISFX("ButtonClick");
            Debug.Log("Level " + (index + 1) + " terkunci.");
            return;
        }

        if (successSound != null)
            AudioManager.Instance?.PlayUISFX(successSound);
        else
            AudioManager.Instance?.PlayUISFX("ButtonClick");

        SetCurrentLevel(index);
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ReplayWithShake()
    {
        int index = carouselSnap?.GetCurrentIndex() ?? GetCurrentLevelIndex();

        if (!IsUnlocked(index))
        {
            if (failureSound != null)
                AudioManager.Instance?.PlayUISFX(failureSound);
            else
                AudioManager.Instance?.PlayUISFX("ButtonClick");
            Debug.Log("Level " + (index + 1) + " terkunci.");
            return;
        }

        if (successSound != null)
            AudioManager.Instance?.PlayUISFX(successSound);
        else
            AudioManager.Instance?.PlayUISFX("ButtonClick");

        // 🔥 Shake card yang dipilih
        Transform card = GetCardTransform(index);
        if (card != null)
        {
            ShakeEffect shake = card.GetComponent<ShakeEffect>();
            if (shake != null)
                shake.PlayShake();
            else
                Debug.LogWarning("LevelManager: Card tidak memiliki ShakeEffect!");
        }

        StartCoroutine(DelayedReplay());
    }

    private Transform GetCardTransform(int index)
    {
        for (int i = 0; i < contentParent.childCount; i++)
        {
            var child = contentParent.GetChild(i);
            var levelUI = child.GetComponent<LevelUI>();
            if (levelUI != null && levelUI.GetLevelIndex() == index)
                return child;
        }
        return null;
    }

    private IEnumerator DelayedReplay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        int index = carouselSnap?.GetCurrentIndex() ?? GetCurrentLevelIndex();
        SetCurrentLevel(index);
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void UpdateReplayButton()
    {
        if (replayButton == null) return;
        int index = carouselSnap?.GetCurrentIndex() ?? 0;
        bool unlocked = IsUnlocked(index);
        replayButton.interactable = unlocked;
        if (replayCanvasGroup != null) replayCanvasGroup.alpha = 1f;
    }

    public void RefreshUI()
    {
        var cards = contentParent.GetComponentsInChildren<LevelUI>(true);
        for (int i = 0; i < cards.Length && i < totalLevels; i++)
            cards[i].Setup(i, IsUnlocked(i), GetIcon(i));
        UpdateReplayButton();
    }

    public void ResetProgress()
    {
        for (int i = 0; i < totalLevels; i++)
        {
            PlayerPrefs.DeleteKey(GetKey(UNLOCKED_KEY, i));
            PlayerPrefs.DeleteKey(GetKey(COMPLETED_KEY, i));
        }
        PlayerPrefs.DeleteKey(CURRENT_KEY);
        PlayerPrefs.Save();
        GenerateLevels();
        if (carouselSnap != null) carouselSnap.Refresh();
        UpdateReplayButton();
    }
}