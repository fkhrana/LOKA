using UnityEngine;

public static class PermanentCollectionManager
{
    private const string KEY_PREFIX = "PermanentCollected_";

    public static void SaveCollected(AksaraData data)
    {
        if (data == null) return;
        string key = KEY_PREFIX + data.GestureShape.ToString();
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public static bool IsCollected(AksaraData data)
    {
        if (data == null) return false;
        string key = KEY_PREFIX + data.GestureShape.ToString();
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}