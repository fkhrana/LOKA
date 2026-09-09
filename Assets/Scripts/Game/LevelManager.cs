using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level UI")]
    [SerializeField] private GameObject levelCardPrefab;

    [Header("Carousel")]
    [SerializeField] private CarouselSnap carouselSnap;
    [SerializeField] private Button replayButton;

    [Header("Level Icons")]
    [SerializeField] private Sprite[] lockedLevelIcons;
    [SerializeField] private Sprite[] unlockedLevelIcons;

    [Header("SFX")]
    [SerializeField] private AudioClip failureSound;
    [SerializeField] private AudioClip successSound;

    [Header("Gameplay")]
    [SerializeField] private string[] gameplaySceneNames;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private Transform contentParent;

    private int totalLevels;

    private const string UNLOCKED_KEY = "LevelUnlocked_";
    private const string COMPLETED_KEY = "LevelCompleted_";
    private const string CURRENT_KEY = "CurrentLevelIndex";

    private CanvasGroup replayCanvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        totalLevels = Mathf.Max(
            lockedLevelIcons != null
                ? lockedLevelIcons.Length
                : 0,

            unlockedLevelIcons != null
                ? unlockedLevelIcons.Length
                : 0,

            gameplaySceneNames != null
                ? gameplaySceneNames.Length
                : 0
        );

        if (totalLevels <= 0)
        {
            Debug.LogWarning(
                "LevelManager: Tidak ada level."
            );
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SetupMainMenu();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (scene.name != mainMenuSceneName)
            return;

        StartCoroutine(
            SetupMainMenuDelayed()
        );
    }

    private IEnumerator SetupMainMenuDelayed()
    {
        yield return null;

        SetupMainMenu();
    }

    private void SetupMainMenu()
    {
        if (SceneManager.GetActiveScene().name != mainMenuSceneName)
            return;

        FindMainMenuReferences();

        if (carouselSnap == null)
        {
            Debug.LogError(
                "LevelManager: CarouselSnap tidak ditemukan di MainMenu."
            );

            return;
        }

        contentParent =
            carouselSnap.GetContentParent();

        if (contentParent == null)
        {
            Debug.LogError(
                "LevelManager: Content dari CarouselSnap tidak ditemukan. " +
                "Pastikan field 'Content' pada CarouselSnap sudah di-assign di Inspector."
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

        GenerateLevels();

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            contentParent as RectTransform
        );

        carouselSnap.Refresh();

        SetupReplayButton();

        UpdateReplayButton();
    }

    private void FindMainMenuReferences()
    {
        carouselSnap = null;
        replayButton = null;
        contentParent = null;
        replayCanvasGroup = null;

        CarouselSnap[] carousels =
            FindObjectsByType<CarouselSnap>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (CarouselSnap carousel in carousels)
        {
            if (carousel == null)
                continue;

            carouselSnap = carousel;
            break;
        }

        if (carouselSnap == null)
        {
            Debug.LogWarning(
                "LevelManager: Tidak ada CarouselSnap ditemukan di scene (termasuk yang inactive)."
            );
        }

        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (button.gameObject.name
                .ToLower()
                .Contains("replay"))
            {
                replayButton = button;
                break;
            }
        }
    }

    private void SetupReplayButton()
    {
        if (replayButton == null)
        {
            Debug.LogWarning(
                "LevelManager: Replay Button tidak ditemukan."
            );

            return;
        }

        replayCanvasGroup =
            replayButton.GetComponent<CanvasGroup>();

        if (replayCanvasGroup == null)
        {
            replayCanvasGroup =
                replayButton.gameObject.AddComponent<CanvasGroup>();
        }

        replayCanvasGroup.alpha = 1f;

        replayButton.onClick.RemoveAllListeners();

        replayButton.onClick.AddListener(
            ReplayWithShake
        );
    }

    private void GenerateLevels()
    {
        if (contentParent == null ||
            levelCardPrefab == null)
            return;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < totalLevels; i++)
        {
            GameObject card =
                Instantiate(
                    levelCardPrefab,
                    contentParent
                );

            LevelUI levelUI =
                card.GetComponent<LevelUI>();

            if (levelUI != null)
            {
                levelUI.Setup(
                    i,
                    IsUnlocked(i),
                    GetIcon(i)
                );
            }
        }
    }

    private string GetKey(
        string prefix,
        int index
    )
    {
        return prefix + index;
    }

    public bool IsUnlocked(int index)
    {
        if (index < 0 ||
            index >= totalLevels)
            return false;

        return PlayerPrefs.GetInt(
            GetKey(
                UNLOCKED_KEY,
                index
            ),
            0
        ) == 1;
    }

    public bool IsCompleted(int index)
    {
        if (index < 0 ||
            index >= totalLevels)
            return false;

        return PlayerPrefs.GetInt(
            GetKey(
                COMPLETED_KEY,
                index
            ),
            0
        ) == 1;
    }

    private void UnlockLevel(int index)
    {
        if (index < 0 ||
            index >= totalLevels)
            return;

        PlayerPrefs.SetInt(
            GetKey(
                UNLOCKED_KEY,
                index
            ),
            1
        );
    }

    private Sprite GetIcon(int index)
    {
        if (IsUnlocked(index))
        {
            if (unlockedLevelIcons != null &&
                index < unlockedLevelIcons.Length)
            {
                return unlockedLevelIcons[index];
            }
        }
        else
        {
            if (lockedLevelIcons != null &&
                index < lockedLevelIcons.Length)
            {
                return lockedLevelIcons[index];
            }
        }

        return null;
    }

    public void CompleteLevel(int index)
    {
        if (index < 0 ||
            index >= totalLevels)
            return;

        PlayerPrefs.SetInt(
            GetKey(
                COMPLETED_KEY,
                index
            ),
            1
        );

        UnlockLevel(index);

        if (index + 1 < totalLevels)
        {
            UnlockLevel(index + 1);
        }

        PlayerPrefs.Save();

        Debug.Log(
            "Level " +
            (index + 1) +
            " selesai. Level berikutnya dibuka."
        );

        RefreshUI();
    }

    public void SetCurrentLevel(int index)
    {
        if (index < 0 ||
            index >= totalLevels ||
            !IsUnlocked(index))
            return;

        PlayerPrefs.SetInt(
            CURRENT_KEY,
            index
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[LevelManager] Current Level: " +
            (index + 1)
        );
    }

    public int GetCurrentLevelIndex()
    {
        if (totalLevels <= 0)
            return 0;

        return Mathf.Clamp(
            PlayerPrefs.GetInt(
                CURRENT_KEY,
                0
            ),
            0,
            totalLevels - 1
        );
    }

    public void ReplaySelectedLevel()
    {
        int index =
            carouselSnap != null
                ? carouselSnap.GetCurrentIndex()
                : GetCurrentLevelIndex();

        if (!IsUnlocked(index))
        {
            PlayFailureSound();

            Debug.Log(
                "Level " +
                (index + 1) +
                " terkunci."
            );

            return;
        }

        PlaySuccessSound();

        SetCurrentLevel(index);

        GameProgressManager.ClearGameState();

        LoadGameplayScene(index);
    }

    public void ReplayWithShake()
    {
        int index =
            carouselSnap != null
                ? carouselSnap.GetCurrentIndex()
                : GetCurrentLevelIndex();

        if (!IsUnlocked(index))
        {
            PlayFailureSound();

            Debug.Log(
                "Level " +
                (index + 1) +
                " terkunci."
            );

            return;
        }

        PlaySuccessSound();

        Transform card =
            GetCardTransform(index);

        if (card != null)
        {
            ShakeEffect shake =
                card.GetComponent<ShakeEffect>();

            if (shake != null)
                shake.PlayShake();
        }

        StartCoroutine(
            DelayedReplay(index)
        );
    }

    private IEnumerator DelayedReplay(
        int index
    )
    {
        yield return new WaitForSecondsRealtime(
            0.5f
        );

        SetCurrentLevel(index);

        GameProgressManager.ClearGameState();

        LoadGameplayScene(index);
    }

    private bool LoadGameplayScene(int index)
    {
        if (gameplaySceneNames == null ||
            index < 0 ||
            index >= gameplaySceneNames.Length)
        {
            Debug.LogError(
                "LevelManager: Scene untuk Level " +
                (index + 1) +
                " belum diatur."
            );

            return false;
        }

        string sceneName =
            gameplaySceneNames[index];

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError(
                "LevelManager: Nama scene Level " +
                (index + 1) +
                " kosong."
            );

            return false;
        }

        Debug.Log(
            "[LevelManager] Loading Level " +
            (index + 1) +
            ": " +
            sceneName
        );

        SceneManager.LoadScene(
            sceneName
        );

        return true;
    }

    private Transform GetCardTransform(
        int index
    )
    {
        if (contentParent == null)
            return null;

        for (int i = 0;
             i < contentParent.childCount;
             i++)
        {
            Transform child =
                contentParent.GetChild(i);

            LevelUI levelUI =
                child.GetComponent<LevelUI>();

            if (levelUI != null &&
                levelUI.GetLevelIndex() == index)
            {
                return child;
            }
        }

        return null;
    }

    public void UpdateReplayButton()
    {
        if (replayButton == null)
            return;

        int index =
            carouselSnap != null
                ? carouselSnap.GetCurrentIndex()
                : GetCurrentLevelIndex();

        bool unlocked =
            IsUnlocked(index);

        replayButton.interactable =
            unlocked;

        if (replayCanvasGroup != null)
            replayCanvasGroup.alpha = 1f;
    }

    public void RefreshUI()
    {
        if (contentParent == null)
        {
            SetupMainMenu();
            return;
        }

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
                IsUnlocked(i),
                GetIcon(i)
            );
        }

        if (carouselSnap != null)
            carouselSnap.Refresh();

        UpdateReplayButton();
    }

    public void ResetProgress()
    {
        for (int i = 0;
             i < totalLevels;
             i++)
        {
            PlayerPrefs.DeleteKey(
                GetKey(
                    UNLOCKED_KEY,
                    i
                )
            );

            PlayerPrefs.DeleteKey(
                GetKey(
                    COMPLETED_KEY,
                    i
                )
            );
        }

        PlayerPrefs.DeleteKey(
            CURRENT_KEY
        );

        GameProgressManager.ResetProgress();

        PlayerPrefs.Save();

        SetupMainMenu();

        Debug.Log(
            "[LevelManager] Semua progress level berhasil di-reset."
        );
    }

    private void PlayFailureSound()
    {
        if (failureSound != null)
        {
            AudioManager.Instance?.PlayUISFX(
                failureSound
            );
        }
        else
        {
            AudioManager.Instance?.PlayUISFX(
                "Failure"
            );
        }
    }

    private void PlaySuccessSound()
    {
        if (successSound != null)
        {
            AudioManager.Instance?.PlayUISFX(
                successSound
            );
        }
        else
        {
            AudioManager.Instance?.PlayUISFX(
                "ButtonHover"
            );
        }
    }
}