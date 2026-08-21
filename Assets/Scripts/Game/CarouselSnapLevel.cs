using UnityEngine;
using UnityEngine.UI;

public class CarouselSnap : MonoBehaviour
{
    [Header("ScrollRect")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Navigation Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Snap Settings")]
    [SerializeField] private float snapSpeed = 10f;

    private RectTransform content;
    private float[] positions;
    private int targetIndex;
    private bool isSnapping;

    private void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        if (scrollRect == null)
        {
            Debug.LogError("CarouselSnap: ScrollRect tidak ditemukan.");
            enabled = false;
            return;
        }

        content = scrollRect.content;

        if (content == null)
        {
            Debug.LogError("CarouselSnap: Content belum diisi.");
            enabled = false;
            return;
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (prevButton != null)
            prevButton.onClick.AddListener(PrevPage);
    }

    private void Start()
    {
        Invoke(nameof(Refresh), 0.05f);
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);

        if (prevButton != null)
            prevButton.onClick.RemoveListener(PrevPage);
    }

    public void Refresh()
    {
        if (scrollRect == null)
            return;

        content = scrollRect.content;

        if (content == null)
            return;

        int childCount = content.childCount;

        if (childCount <= 1)
        {
            positions = null;
            targetIndex = 0;
            isSnapping = false;

            UpdateButtons();
            NotifyLevelManager();
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        positions = new float[childCount];

        float spacing = 1f / (childCount - 1);

        for (int i = 0; i < childCount; i++)
            positions[i] = spacing * i;

        targetIndex = 0;
        isSnapping = false;

        scrollRect.horizontalNormalizedPosition = 0f;

        UpdateButtons();
        NotifyLevelManager();

        Debug.Log("CarouselSnap: " + childCount + " card ditemukan.");
    }

    private void Update()
    {
        if (!isSnapping || positions == null || positions.Length == 0)
            return;

        float targetPosition = positions[targetIndex];

        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            targetPosition,
            Time.unscaledDeltaTime * snapSpeed
        );

        if (Mathf.Abs(
            scrollRect.horizontalNormalizedPosition - targetPosition
        ) < 0.001f)
        {
            scrollRect.horizontalNormalizedPosition = targetPosition;
            isSnapping = false;
            UpdateButtons();
        }
    }

    public void NextPage()
    {
        if (positions == null || targetIndex >= positions.Length - 1)
            return;

        targetIndex++;
        isSnapping = true;

        UpdateButtons();
        NotifyLevelManager();

        Debug.Log("Carousel → Level " + (targetIndex + 1));
    }

    public void PrevPage()
    {
        if (positions == null || targetIndex <= 0)
            return;

        targetIndex--;
        isSnapping = true;

        UpdateButtons();
        NotifyLevelManager();

        Debug.Log("Carousel → Level " + (targetIndex + 1));
    }

    public int GetCurrentIndex()
    {
        return targetIndex;
    }

    private void UpdateButtons()
    {
        bool hasMultipleCards =
            positions != null &&
            positions.Length > 1;

        if (nextButton != null)
        {
            nextButton.interactable =
                hasMultipleCards &&
                targetIndex < positions.Length - 1;
        }

        if (prevButton != null)
        {
            prevButton.interactable =
                hasMultipleCards &&
                targetIndex > 0;
        }
    }

    private void NotifyLevelManager()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.UpdateReplayButton();
    }

    public void OnEndDrag()
    {
        if (positions == null || positions.Length == 0)
            return;

        float currentPosition =
            scrollRect.horizontalNormalizedPosition;

        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            float distance = Mathf.Abs(
                currentPosition - positions[i]
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        targetIndex = closestIndex;
        isSnapping = true;

        UpdateButtons();
        NotifyLevelManager();

        Debug.Log("Carousel Drag → Level " + (targetIndex + 1));
    }
}