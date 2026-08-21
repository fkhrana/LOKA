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

    [Header("Colors")]
    [SerializeField] private Color unlockedColor =
        new Color(1f, 0.84f, 0f);

    [SerializeField] private Color lockedColor =
        new Color(0.5f, 0.5f, 0.5f);

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName =
        "MainGameplay(Drawing)";

    private int levelIndex;
    private bool isUnlocked;

    public void Setup(int index, bool unlocked, Sprite icon)
    {
        levelIndex = index;
        isUnlocked = unlocked;

        UpdateIcon(icon);
        UpdateVisual();
    }

    private void UpdateIcon(Sprite icon)
    {
        if (levelIconImage != null)
            levelIconImage.sprite = icon;
    }

    private void UpdateVisual()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color =
                isUnlocked
                    ? unlockedColor
                    : lockedColor;
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
            Debug.Log(
                "Level " +
                (levelIndex + 1) +
                " masih terkunci."
            );

            return;
        }

        PlayLevel();
    }

    private void PlayLevel()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError(
                "LevelUI: LevelManager tidak ditemukan."
            );

            return;
        }

        LevelManager.Instance.SetCurrentLevel(levelIndex);

        Debug.Log(
            "Memulai Level " +
            (levelIndex + 1)
        );

        SceneManager.LoadScene(gameplaySceneName);
    }
}