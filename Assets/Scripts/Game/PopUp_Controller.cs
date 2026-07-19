using UnityEngine;

public class PopupController : MonoBehaviour
{
    public GameObject popup;

    public void OpenPopup()
    {
        popup.SetActive(true);
    }

    public void ClosePopup()
    {
        popup.SetActive(false);
    }
}