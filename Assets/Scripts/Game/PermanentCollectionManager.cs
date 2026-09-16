using System.Collections.Generic;
using UnityEngine;

public static class PermanentCollectionManager
{
    private const string KEY_PREFIX = "PermanentCollected_";

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

    // Reset semua koleksi aksara.
    public static void ResetAksara(AksaraData[] allAksara)
    {
        trainingCollected.Clear();

        if (allAksara == null) return;

        foreach (AksaraData data in allAksara)
        {
            if (data == null) continue;
            string key = KEY_PREFIX + data.GestureShape.ToString();
            PlayerPrefs.DeleteKey(key);
        }

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
}