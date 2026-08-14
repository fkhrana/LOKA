using UnityEngine;

public class BookFlip : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject hadiahPanel;
    [SerializeField] private GameObject aksaraPanel;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private bool isAksaraOpen = false;

    private void Start()
    {
        // Kondisi awal
        hadiahPanel.SetActive(true);
        aksaraPanel.SetActive(false);
    }

    public void NextPage()
    {
        if (!isAksaraOpen)
        {
            isAksaraOpen = true;

            hadiahPanel.SetActive(true);
            aksaraPanel.SetActive(true);

            animator.SetTrigger("OpenBook");
        }
    }

    public void PreviousPage()
    {
        if (isAksaraOpen)
        {
            isAksaraOpen = false;

            animator.SetTrigger("CloseBook");
        }
    }
}