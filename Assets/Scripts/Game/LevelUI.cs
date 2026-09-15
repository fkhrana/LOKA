using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image levelIconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject lockIcon;

    [Header("Colors (Fallback)")]
    [Tooltip("Warna fallback kalau tidak ada background sprite untuk level ini.")]
    [SerializeField] private Color unlockedColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip lockedSound;

    private int levelIndex;
    private bool isUnlocked;

    public int GetLevelIndex()
    {
        return levelIndex;
    }

    public void Setup(
        int index,
        bool unlocked,
        Sprite icon,
        Sprite backgroundSprite
    )
    {
        levelIndex = index;
        isUnlocked = unlocked;

        if (levelIconImage != null)
            levelIconImage.sprite = icon;

        if (backgroundImage != null)
        {
            if (backgroundSprite != null)
            {
                // Sprite khusus untuk level ini — pakai warna putih
                // supaya tidak di-tint oleh warna locked/unlocked.
                backgroundImage.sprite = backgroundSprite;
                backgroundImage.color = Color.white;
            }
            else
            {
                // Fallback: tint warna seperti perilaku lama.
                backgroundImage.sprite = null;
                backgroundImage.color =
                    isUnlocked ? unlockedColor : lockedColor;
            }
        }

        if (lockOverlay != null)
            lockOverlay.SetActive(!isUnlocked);

        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUnlocked)
        {
            PlaySfxOrFallback(lockedSound);
            Debug.Log("Level " + (levelIndex + 1) + " masih terkunci.");
            return;
        }

        PlaySfxOrFallback(clickSound);

        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelUI: LevelManager tidak ditemukan.");
            return;
        }

        LevelManager.Instance.SetCurrentLevel(levelIndex);

        string sceneName = LevelManager.Instance.GetSceneNameForLevel(levelIndex);

<<<<<<< HEAD
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"LevelUI: Scene name untuk level {levelIndex + 1} kosong.");
            return;
        }

        Debug.Log($"Memulai Level {levelIndex + 1} → {sceneName}");

        SceneManager.LoadScene(sceneName);
=======
        GameProgressManager.MarkStartedFromMainMenu();
        SceneManager.LoadScene(
            gameplaySceneName
        );
>>>>>>> parent of 20fa826 (Revert "Merge branch 'main' of https://github.com/fkhrana/LOKA")
    }

    private void PlaySfxOrFallback(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.Instance?.PlayUISFX(clip);
        }
        else
        {
            AudioManager.Instance?.PlayUISFX("ButtonClick");
        }
    }
}