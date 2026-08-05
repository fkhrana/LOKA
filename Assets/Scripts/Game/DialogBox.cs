using UnityEngine;

public class DialogBox : MonoBehaviour
{
    [Header("Dialog")]

    [SerializeField] private Transform box;

    [Space]

    [SerializeField] private CanvasGroup background;

    private void OnEnable()
    {
        background.alpha = 0;
        background.LeanAlpha(1, 0.5f);

        box.localPosition = new Vector3(0, -Screen.height, 0);

        box.LeanMoveLocalY(0, 0.5f)
            .setEaseOutExpo()
            .setDelay(0.1f);
    }

    public void CloseDialog()
    {
        background.LeanAlpha(0, 0.5f);

        box.LeanMoveLocalY(-Screen.height, 0.5f)
            .setEaseInExpo()
            .setOnComplete(OnComplete);
    }

    private void OnComplete()
    {
        gameObject.SetActive(false);
    }
}