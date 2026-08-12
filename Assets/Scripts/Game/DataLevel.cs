using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelIndex;
    public bool isUnlocked;
    public string levelName;
    public Sprite levelIcon;

    public LevelData(int index, bool unlocked, string name, Sprite icon)
    {
        levelIndex = index;
        isUnlocked = unlocked;
        levelName = name;
        levelIcon = icon;
    }
}