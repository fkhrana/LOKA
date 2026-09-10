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

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged +=
            OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -=
            OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(
        PlayModeStateChange state
    )
    {
        // Saat mulai Play
        if (
            state == PlayModeStateChange.EnteredPlayMode
        )
        {
            resetThisPlaySession =
                resetOnStop;

            if (resetThisPlaySession)
            {
                ResetGameData();

                Debug.Log(
                    "🆕 NEW PLAYER MODE AKTIF!"
                );
            }
        }

        // Saat klik Stop
        if (
            state == PlayModeStateChange.EnteredEditMode
        )
        {
            if (resetThisPlaySession)
            {
                ResetGameData();

                Debug.Log(
                    "🔄 DATA TESTING DI-RESET SETELAH STOP!"
                );

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
        // Reset semua koleksi Aksara
        if (aksaraCarouselUI != null)
        {
            PermanentCollectionManager.ResetAksara(
                aksaraCarouselUI.AllAksaraData.ToArray()
            );
        }

        // Reset progress semua level
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.DeleteKey(
                "LevelUnlocked_" + i
            );

            PlayerPrefs.DeleteKey(
                "LevelCompleted_" + i
            );
        }

        // Reset level yang sedang dipilih
        PlayerPrefs.DeleteKey(
            "CurrentLevelIndex"
        );

        // Reset progress gameplay
        GameProgressManager.ResetProgress();

        PlayerPrefs.Save();
    }

    public void ResetAgain()
    {
        ResetGameData();

        Debug.Log(
            "🔄 DATA GAME BERHASIL DI-RESET MANUAL!"
        );
    }
}