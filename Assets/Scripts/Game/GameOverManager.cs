using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Nama Scene")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "MainGameplay(Drawing)"; 

    // Tombol "Home"
    public void GoToHome()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    // Tombol "Again" (main ulang)
    public void PlayAgain()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    // Tombol "Tutorial"
    public void OpenTutorial()
    {
        PlayerPrefs.SetInt("OpenTutorial", 1);
        SceneManager.LoadScene(mainMenuScene);
    }
}