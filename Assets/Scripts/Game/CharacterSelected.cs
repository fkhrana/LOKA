using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    public GameObject guruminSelected;
    public GameObject basariSelected;

    private int selectedCharacter;

    void Start()
    {
        SelectCharacter(0);
    }

    public void SelectCharacter(int characterIndex)
    {
        selectedCharacter = characterIndex;

        if (characterIndex == 0)
        {
            guruminSelected.SetActive(true);
            basariSelected.SetActive(false);

            Debug.Log("Gurumin selected");
        }
        else if (characterIndex == 1)
        {
            guruminSelected.SetActive(false);
            basariSelected.SetActive(true);

            Debug.Log("Basari selected");
        }
    }
}