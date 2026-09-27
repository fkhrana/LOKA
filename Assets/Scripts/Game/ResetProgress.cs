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

    [Header("Level Count")]
    [Tooltip("Total level termasuk boss. Dipakai untuk reset progress.")]
    [SerializeField, Min(1)] private int totalLevels = 4;

#if UNITY_EDITOR
    private bool resetThisPlaySession = false;

    private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            resetThisPlaySession = resetOnStop;

            if (resetThisPlaySession)
            {
                ResetGameData();
                Debug.Log("🆕 NEW PLAYER MODE AKTIF!");
            }
        }

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

    private void ResetGameData()
    {
        if (aksaraCarouselUI != null)
            PermanentCollectionManager.ResetAksara(aksaraCarouselUI.AllAksaraData.ToArray());

        // FIX: pakai totalLevels — include index boss
        for (int i = 0; i < totalLevels; i++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + i);
            PlayerPrefs.DeleteKey("LevelCompleted_" + i);
        }

        foreach (string shape in Enum.GetNames(typeof(GestureShape)))
            PlayerPrefs.DeleteKey("PermanentCollected_" + shape);

        for (int i = 0; i <= totalLevels; i++)
            PlayerPrefs.DeleteKey("LevelCollectedAksara_L" + i);

        // Tutorial per-level
        for (int i = 0; i < totalLevels; i++)
            PlayerPrefs.DeleteKey("TutorialPowerUpDone_L" + i);

        // Boss tutorial
        for (int i = 0; i < totalLevels; i++)
            PlayerPrefs.DeleteKey("BossTutorialDone_L" + i);

        // Review final book / level summary data
        for (int i = 1; i <= totalLevels; i++)
        {
            PlayerPrefs.DeleteKey("FinalBook_Reward_L" + i);
            PlayerPrefs.DeleteKey("FinalBook_Reward_L" + i + "_Name");
            PlayerPrefs.DeleteKey("FinalBook_RewardDesc_L" + i);
            PlayerPrefs.DeleteKey("FinalBook_Aksara_L" + i);
        }

        PlayerPrefs.DeleteKey("CurrentLevelIndex");
        PlayerPrefs.DeleteKey("CutsceneCompleted");
        PlayerPrefs.DeleteKey("LastSceneName");

        GameProgressManager.ResetProgress();
        CameraIntroManager.ResetIntroFlag();

        PlayerPrefs.Save();

        Debug.Log("[AutoResetProgress] ✅ Reset selesai. " +
                  $"totalLevels={totalLevels}, bossIndex={totalLevels - 1}.");
    }

    public void ResetAgain()
    {
        ResetGameData();
        Debug.Log("🔄 DATA GAME BERHASIL DI-RESET MANUAL!");
    }
}