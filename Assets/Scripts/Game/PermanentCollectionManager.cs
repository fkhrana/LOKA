using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PermanentCollectionManager
{
    private const string KEY_PREFIX = "PermanentCollected_";
    private const string LEVEL_KEY_PREFIX = "LevelCollectedAksara_L";
    private const string CURRENT_LEVEL_KEY = "CurrentLevelIndex";

    // Koleksi in-memory khusus training, tidak disave ke PlayerPrefs.
    private static readonly HashSet<string> trainingCollected = new HashSet<string>();

    // Simpan aksara yang didapat ke memory (training) atau PlayerPrefs (normal).
    public static void SaveCollected(AksaraData data)
    {
        if (data == null) return;

        string key = KEY_PREFIX + data.GestureShape.ToString();

        // Training mode → simpan ke memory saja.
        if (TutorialManager.IsTrainingMode)
        {
            trainingCollected.Add(key);
            Debug.Log($"[Training] Koleksi disimpan ke memory: {data.AksaraName} (total: {trainingCollected.Count})");
            return;
        }

        // Normal mode → simpan ke PlayerPrefs.
        PlayerPrefs.SetInt(key, 1);

        int levelIndex = ResolveAssignedLevelIndex(data);
        if (levelIndex >= 0)
            SaveCollectedForLevel(levelIndex, data);
        else
            SaveCollectedForCurrentLevel(data);

        PlayerPrefs.Save();
    }

    // Cek apakah aksara sudah terkoleksi.
    public static bool IsCollected(AksaraData data)
    {
        if (data == null) return false;

        string key = KEY_PREFIX + data.GestureShape.ToString();

        // Training mode → cek memory.
        if (TutorialManager.IsTrainingMode)
            return trainingCollected.Contains(key);

        // Normal mode → cek PlayerPrefs.
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static void SaveCollectedForCurrentLevel(AksaraData data)
    {
        if (data == null) return;

        int levelIndex = Mathf.Max(0, PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0));
        SaveCollectedForLevel(levelIndex, data);
    }

    private static int ResolveAssignedLevelIndex(AksaraData data)
    {
        if (LevelManager.Instance != null)
        {
            int mappedLevel = LevelManager.Instance.GetLevelIndexForAksara(data);
            if (mappedLevel >= 0)
                return mappedLevel;
        }

        return Mathf.Max(0, PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0));
    }

    public static void SaveCollectedForLevel(int levelIndex, AksaraData data)
    {
        if (data == null) return;

        var shapes = LoadCollectedShapesForLevel(levelIndex);
        shapes.Add(data.GestureShape);
        SaveCollectedShapesForLevel(levelIndex, shapes);
    }

    public static void SaveCollectedForLevel(int levelIndex, IEnumerable<AksaraData> dataList)
    {
        if (dataList == null) return;

        var shapes = LoadCollectedShapesForLevel(levelIndex);

        foreach (AksaraData data in dataList)
        {
            if (data != null)
                shapes.Add(data.GestureShape);
        }

        SaveCollectedShapesForLevel(levelIndex, shapes);
    }

    public static bool IsCollectedInLevel(int levelIndex, AksaraData data)
    {
        if (data == null) return false;
        return LoadCollectedShapesForLevel(levelIndex).Contains(data.GestureShape);
    }

    public static List<AksaraData> GetCollectedAksaraForLevel(int levelIndex, IEnumerable<AksaraData> sourceList)
    {
        if (sourceList == null)
            return new List<AksaraData>();

        var shapes = LoadCollectedShapesForLevel(levelIndex);
        return sourceList
            .Where(x => x != null && shapes.Contains(x.GestureShape))
            .ToList();
    }

    public static void ClearLevelCollected(int levelIndex)
    {
        PlayerPrefs.DeleteKey(GetLevelKey(levelIndex));
        PlayerPrefs.Save();
    }

    public static void ClearAllLevelCollected(int totalLevels)
    {
        for (int i = 0; i < totalLevels; i++)
            ClearLevelCollected(i);
    }

    // Reset semua koleksi aksara.
    public static void ResetAksara(AksaraData[] allAksara)
    {
        trainingCollected.Clear();

        if (allAksara != null)
        {
            foreach (AksaraData data in allAksara)
            {
                if (data == null) continue;
                string key = KEY_PREFIX + data.GestureShape.ToString();
                PlayerPrefs.DeleteKey(key);
            }
        }

        for (int i = 0; i <= 3; i++)
            PlayerPrefs.DeleteKey(LEVEL_KEY_PREFIX + i);

        PlayerPrefs.Save();
        Debug.Log("[PermanentCollectionManager] Data aksara di-reset.");
    }

    // Clear memory training, dipanggil saat keluar training scene.
    public static void ClearTrainingCollected()
    {
        int count = trainingCollected.Count;
        trainingCollected.Clear();
        Debug.Log($"[Training] Clear {count} aksara dari memory training.");
    }

    private static string GetLevelKey(int levelIndex)
    {
        return LEVEL_KEY_PREFIX + levelIndex;
    }

    private static HashSet<GestureShape> LoadCollectedShapesForLevel(int levelIndex)
    {
        string value = PlayerPrefs.GetString(GetLevelKey(levelIndex), string.Empty);
        var result = new HashSet<GestureShape>();

        if (string.IsNullOrEmpty(value))
            return result;

        foreach (string token in value.Split('|'))
        {
            if (string.IsNullOrWhiteSpace(token))
                continue;

            if (Enum.TryParse(token, true, out GestureShape shape))
                result.Add(shape);
        }

        return result;
    }

    private static void SaveCollectedShapesForLevel(int levelIndex, HashSet<GestureShape> shapes)
    {
        string value = string.Join("|", shapes
            .OrderBy(shape => (int)shape)
            .Select(shape => shape.ToString()));

        PlayerPrefs.SetString(GetLevelKey(levelIndex), value);
        PlayerPrefs.Save();
    }
}