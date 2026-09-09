using UnityEngine;

public class AutoResetProgress : MonoBehaviour
{
    [Header("Development Reset")]
    [SerializeField] private bool resetOnStart = false;

    [Header("Aksara Collection")]
    [SerializeField] private AksaraCarouselUI aksaraCarouselUI;

    private void Awake()
    {
        if (!resetOnStart)
            return;

        ResetGameData();
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

        // Reset semua progress LevelManager
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + i);
            PlayerPrefs.DeleteKey("LevelCompleted_" + i);
        }

        // Reset level yang sedang dipilih
        PlayerPrefs.DeleteKey("CurrentLevelIndex");

        // Reset progress gameplay dan cutscene
        GameProgressManager.ResetProgress();

        PlayerPrefs.Save();

        Debug.Log(
            "🔄 DATA GAME BERHASIL DI-RESET UNTUK DEVELOPMENT!"
        );
    }

    public void ResetAgain()
    {
        ResetGameData();

        Debug.Log(
            "🔄 DATA GAME BERHASIL DI-RESET MANUAL!"
        );
    }
}