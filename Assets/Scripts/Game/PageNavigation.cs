using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultButton : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "MainGameplay(Drawing)";

    // HOME
    public void Home()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    // REPLAY
    public void Replay()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    // NEXT
    public void Next()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex + 1
        );
    }

    // BACK
    public void Back()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex - 1
        );
    }
}