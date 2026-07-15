using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Leveling : MonoBehaviour
{
    public void BackMain()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenLevel1()
    {
        SceneManager.LoadScene("Scratch");
    }

    public void OpenLevel2()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void OpenLevel3()
    {
        SceneManager.LoadScene("Win");
    }

    public GameObject scrolbar;
    float scroll_pos = 0;
    float[] pos;

    void Start()
    {
        
    }

    void Update()
    {
        pos = new float[transform.childCount];

        if (pos.Length <= 1 || scrolbar == null)
        {
            return;
        }

        float distance = 1f / (pos.Length - 1f);

        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        if (Input.GetMouseButton(0))
        {
            scroll_pos = scrolbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2f) &&
                    scroll_pos > pos[i] - (distance / 2f))
                {
                    scrolbar.GetComponent<Scrollbar>().value =
                        Mathf.Lerp(
                            scrolbar.GetComponent<Scrollbar>().value,
                            pos[i],
                            0.1f
                        );
                }
            }
        }
    }
}