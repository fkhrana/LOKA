using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]
    public GameObject levelCardPrefab;   // Prefab LevelCard
    public Transform contentParent;      // GameObject Content (di dalam ScrollView)
    public Sprite[] levelIcons;          // Icon untuk tiap level (urut sesuai level)

    private List<LevelData> levels = new List<LevelData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadProgress();
        GenerateLevels();
    }

    // Memuat status unlock dari PlayerPrefs
    void LoadProgress()
    {
        int totalLevels = levelIcons.Length;
        for (int i = 0; i < totalLevels; i++)
        {
            bool unlocked = false;
            if (i == 0) unlocked = true; // Level 1 selalu terbuka
            else
            {
                // Cek PlayerPrefs: 1 = unlocked, 0 = locked
                int saved = PlayerPrefs.GetInt("Level_" + i, 0);
                unlocked = (saved == 1);
            }

            Sprite icon = (i < levelIcons.Length) ? levelIcons[i] : null;
            levels.Add(new LevelData(i, unlocked, "Level " + (i + 1), icon));
        }
    }

    // Membuat semua card level di dalam Content
    void GenerateLevels()
    {
        // Hapus card lama (jika ada)
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (LevelData data in levels)
        {
            GameObject card = Instantiate(levelCardPrefab, contentParent);
            LevelUI ui = card.GetComponent<LevelUI>();
            ui.Setup(data);
        }
    }

    // Panggil saat pemain selesai bermain level (untuk unlock level berikutnya)
    public void CompleteLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;

        int nextIndex = index + 1;
        if (nextIndex < levels.Count)
        {
            levels[nextIndex].isUnlocked = true;
            PlayerPrefs.SetInt("Level_" + nextIndex, 1);
            PlayerPrefs.Save();
            RefreshUI();
        }
    }

    // Refresh semua card (misal setelah unlock)
    void RefreshUI()
    {
        LevelUI[] allCards = contentParent.GetComponentsInChildren<LevelUI>();
        for (int i = 0; i < allCards.Length && i < levels.Count; i++)
        {
            allCards[i].Setup(levels[i]);
        }
    }
}