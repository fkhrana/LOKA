using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoResetProgress : MonoBehaviour
{
    [Header("Development Reset")]
    [SerializeField] private bool resetOnStop = false;

    [Header("Aksara Collection")]
    [SerializeField] private AksaraCarouselUI aksaraCarouselUI;

#if UNITY_EDITOR
    private bool resetThisPlaySession = false;

    private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

    // Handle event saat Play Mode berubah.
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Saat mulai Play.
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            resetThisPlaySession = resetOnStop;

            if (resetThisPlaySession)
            {
                ResetGameData();
                Debug.Log("🆕 NEW PLAYER MODE AKTIF!");
            }
        }

        // Saat klik Stop.
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (resetThisPlaySession)
            {
                ResetGameData();
                Debug.Log("🔄 DATA TESTING DI-RESET SETELAH STOP!");
                resetThisPlaySession = false;
            }
        }
    }
#endif

    private void Awake()
    {
        // Reset dikontrol oleh Play Mode.
    }

    // Hapus semua data progress + koleksi.
    private void ResetGameData()
    {
        if (aksaraCarouselUI != null)
            PermanentCollectionManager.ResetAksara(aksaraCarouselUI.AllAksaraData.ToArray());

        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + i);
            PlayerPrefs.DeleteKey("LevelCompleted_" + i);
        }

        foreach (string shape in Enum.GetNames(typeof(GestureShape)))
            PlayerPrefs.DeleteKey("PermanentCollected_" + shape);

        for (int i = 0; i <= 3; i++)
            PlayerPrefs.DeleteKey("LevelCollectedAksara_L" + i);

        // Review final book / level summary data.
        for (int i = 1; i <= 3; i++)
        {
            PlayerPrefs.DeleteKey("FinalBook_Reward_L" + i);
            PlayerPrefs.DeleteKey("FinalBook_Reward_L" + i + "_Name");
            PlayerPrefs.DeleteKey("FinalBook_RewardDesc_L" + i);
            PlayerPrefs.DeleteKey("FinalBook_Aksara_L" + i);
        }

        PlayerPrefs.DeleteKey("CurrentLevelIndex");

        GameProgressManager.ResetProgress();
        CameraIntroManager.ResetIntroFlag();

        PlayerPrefs.Save();
    }

    // Reset manual dari tombol / context menu.
    public void ResetAgain()
    {
        ResetGameData();
        Debug.Log("🔄 DATA GAME BERHASIL DI-RESET MANUAL!");
    }
}