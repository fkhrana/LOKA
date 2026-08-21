using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level UI")]
    [SerializeField] private GameObject levelCardPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Carousel")]
    [SerializeField] private CarouselSnap carouselSnap;
    [SerializeField] private UnityEngine.UI.Button replayButton;

    [Header("Locked Level Icons")]
    [SerializeField] private Sprite[] lockedLevelIcons;

    [Header("Unlocked Level Icons")]
    [SerializeField] private Sprite[] unlockedLevelIcons;

    [Header("Gameplay")]
    [SerializeField] private string gameplaySceneName = "MainGameplay(Drawing)";

    private int totalLevels;

    private const string LEVEL_UNLOCKED_KEY = "Level_";
    private const string LEVEL_COMPLETED_KEY = "LevelCompleted_";
    private const string CURRENT_LEVEL_KEY = "CurrentLevelIndex";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CalculateTotalLevels();
        InitializeLevelOne();
    }

    private void Start()
    {
        GenerateLevels();

        Canvas.ForceUpdateCanvases();

        if (carouselSnap != null)
            carouselSnap.Refresh();

        UpdateReplayButton();
    }

    // Menghitung jumlah level berdasarkan jumlah icon.
    private void CalculateTotalLevels()
    {
        int lockedCount = lockedLevelIcons != null
            ? lockedLevelIcons.Length
            : 0;

        int unlockedCount = unlockedLevelIcons != null
            ? unlockedLevelIcons.Length
            : 0;

        totalLevels = Mathf.Max(lockedCount, unlockedCount);

        if (totalLevels <= 0)
        {
            Debug.LogWarning("LevelManager: Tidak ada level icon.");
        }
    }

    // Level 1 selalu terbuka.
    private void InitializeLevelOne()
    {
        if (totalLevels <= 0)
            return;

        PlayerPrefs.SetInt(GetUnlockedKey(0), 1);
        PlayerPrefs.Save();
    }

    // Membuat key PlayerPrefs untuk status unlock.
    private string GetUnlockedKey(int index)
    {
        return LEVEL_UNLOCKED_KEY + index;
    }

    // Membuat key PlayerPrefs untuk status selesai.
    private string GetCompletedKey(int index)
    {
        return LEVEL_COMPLETED_KEY + index;
    }

    // Mengecek apakah level sudah terbuka.
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= totalLevels)
            return false;

        if (levelIndex == 0)
            return true;

        return PlayerPrefs.GetInt(
            GetUnlockedKey(levelIndex),
            0
        ) == 1;
    }

    // Mengecek apakah level sudah selesai.
    public bool IsLevelCompleted(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= totalLevels)
            return false;

        return PlayerPrefs.GetInt(
            GetCompletedKey(levelIndex),
            0
        ) == 1;
    }

    // Mengambil icon sesuai status level.
    public Sprite GetLevelIcon(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= totalLevels)
            return null;

        if (IsLevelUnlocked(levelIndex))
        {
            if (unlockedLevelIcons != null &&
                levelIndex < unlockedLevelIcons.Length)
            {
                return unlockedLevelIcons[levelIndex];
            }
        }
        else
        {
            if (lockedLevelIcons != null &&
                levelIndex < lockedLevelIcons.Length)
            {
                return lockedLevelIcons[levelIndex];
            }
        }

        return null;
    }

    // Membuat semua card level.
    private void GenerateLevels()
    {
        if (contentParent == null)
        {
            Debug.LogWarning(
                "LevelManager: Content Parent belum diisi."
            );
            return;
        }

        if (levelCardPrefab == null)
        {
            Debug.LogError(
                "LevelManager: Level Card Prefab belum diisi."
            );
            return;
        }

        ClearLevelCards();

        for (int i = 0; i < totalLevels; i++)
        {
            GameObject card = Instantiate(
                levelCardPrefab,
                contentParent
            );

            LevelUI levelUI = card.GetComponent<LevelUI>();

            if (levelUI == null)
            {
                Debug.LogError(
                    "Level Card Prefab tidak memiliki LevelUI."
                );
                continue;
            }

            levelUI.Setup(
                i,
                IsLevelUnlocked(i),
                GetLevelIcon(i)
            );
        }
    }

    // Menghapus card level lama.
    private void ClearLevelCards()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(
                contentParent.GetChild(i).gameObject
            );
        }
    }

    // Menyimpan level yang sedang dimainkan.
    public void SetCurrentLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= totalLevels)
        {
            Debug.LogWarning(
                "LevelManager: Level index tidak valid: " +
                levelIndex
            );
            return;
        }

        if (!IsLevelUnlocked(levelIndex))
        {
            Debug.LogWarning(
                "Level " +
                (levelIndex + 1) +
                " masih locked."
            );
            return;
        }

        PlayerPrefs.SetInt(
            CURRENT_LEVEL_KEY,
            levelIndex
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Current Level = " +
            (levelIndex + 1)
        );
    }

    // Mengambil index level yang sedang dimainkan.
    public int GetCurrentLevelIndex()
    {
        int index = PlayerPrefs.GetInt(
            CURRENT_LEVEL_KEY,
            0
        );

        if (index < 0 || index >= totalLevels)
            return 0;

        return index;
    }

    // Menyelesaikan level yang sedang dimainkan.
    public void CompleteCurrentLevel()
    {
        CompleteLevel(
            GetCurrentLevelIndex()
        );
    }

    // Menandai level selesai dan membuka level berikutnya.
    public void CompleteLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= totalLevels)
        {
            Debug.LogWarning(
                "LevelManager: Level index tidak valid."
            );
            return;
        }

        bool alreadyCompleted =
            IsLevelCompleted(levelIndex);

        PlayerPrefs.SetInt(
            GetCompletedKey(levelIndex),
            1
        );

        PlayerPrefs.SetInt(
            GetUnlockedKey(levelIndex),
            1
        );

        int nextLevel = levelIndex + 1;

        if (nextLevel < totalLevels)
        {
            PlayerPrefs.SetInt(
                GetUnlockedKey(nextLevel),
                1
            );

            if (!alreadyCompleted)
            {
                Debug.Log(
                    "Level " +
                    (levelIndex + 1) +
                    " selesai!"
                );

                Debug.Log(
                    "Level " +
                    (nextLevel + 1) +
                    " berhasil di-unlock!"
                );
            }
        }
        else
        {
            Debug.Log("Level terakhir selesai!");
        }

        PlayerPrefs.Save();
        RefreshUI();
    }

    // Replay level yang sedang dipilih carousel.
    public void ReplaySelectedLevel()
    {
        if (carouselSnap == null)
        {
            Debug.LogError(
                "LevelManager: CarouselSnap belum diisi."
            );
            return;
        }

        int selectedIndex =
            carouselSnap.GetCurrentIndex();

        ReplayLevel(selectedIndex);
    }

    // Memulai kembali level tertentu.
    public void ReplayLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= totalLevels)
        {
            Debug.LogWarning(
                "LevelManager: Level tidak valid."
            );
            return;
        }

        if (!IsLevelUnlocked(levelIndex))
        {
            Debug.Log(
                "Level " +
                (levelIndex + 1) +
                " masih terkunci."
            );
            return;
        }

        SetCurrentLevel(levelIndex);

        Debug.Log(
            "Replay Level " +
            (levelIndex + 1)
        );

        SceneManager.LoadScene(
            gameplaySceneName
        );
    }

    // Mengatur tombol replay berdasarkan level yang sedang dipilih.
    public void UpdateReplayButton()
    {
        if (replayButton == null)
            return;

        if (carouselSnap == null)
        {
            replayButton.interactable = false;
            return;
        }

        int selectedIndex =
            carouselSnap.GetCurrentIndex();

        replayButton.interactable =
            IsLevelUnlocked(selectedIndex);
    }

    // Memperbarui tampilan semua card.
    public void RefreshUI()
    {
        if (contentParent == null)
            return;

        LevelUI[] cards =
            contentParent.GetComponentsInChildren<LevelUI>(
                true
            );

        for (int i = 0;
             i < cards.Length && i < totalLevels;
             i++)
        {
            cards[i].Setup(
                i,
                IsLevelUnlocked(i),
                GetLevelIcon(i)
            );
        }

        UpdateReplayButton();
    }

    // Mereset semua progress level.
    public void ResetProgress()
    {
        for (int i = 0; i < totalLevels; i++)
        {
            PlayerPrefs.DeleteKey(
                GetUnlockedKey(i)
            );

            PlayerPrefs.DeleteKey(
                GetCompletedKey(i)
            );
        }

        PlayerPrefs.DeleteKey(
            CURRENT_LEVEL_KEY
        );

        PlayerPrefs.Save();

        InitializeLevelOne();
        GenerateLevels();

        if (carouselSnap != null)
            carouselSnap.Refresh();

        UpdateReplayButton();

        Debug.Log(
            "Progress level berhasil di-reset."
        );
    }
}