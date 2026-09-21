using UnityEngine;

public class BooksFinalTester : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private int levelIndex = 1;

    [Header("Reward")]
    [SerializeField] private Sprite rewardIcon;
    [SerializeField] private Sprite rewardNameIcon;
    [SerializeField] [TextArea(2, 4)] private string rewardDescription = "Reward Level 1";

    [Header("Aksara Icons")]
    [SerializeField] private Sprite[] aksaraIcons = new Sprite[5];

    [Header("Target UI")]
    [SerializeField] private BooksFinal booksFinal;

    [ContextMenu("Save Test Data")]
    public void SaveTestData()
    {
        BooksFinal.SaveReviewData(
            levelIndex,
            rewardIcon,
            rewardNameIcon,
            rewardDescription,
            aksaraIcons
        );

        if (booksFinal != null)
        {
            booksFinal.gameObject.SetActive(false);
            booksFinal.gameObject.SetActive(true);
        }

        Debug.Log($"[BooksFinalTester] Data level {levelIndex} disimpan.");
    }
}
