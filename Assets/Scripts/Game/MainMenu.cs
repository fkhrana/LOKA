using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  public void PlayGame()
  {
    SceneManager.LoadScene("ChooseHero");
  }

  public void LevelingGame()
  {
    SceneManager.LoadScene("Leveling");
  }

  public void ExitGame()
  {
     Debug.Log("Keluar dari game");
     Application.Quit();
  }

}
