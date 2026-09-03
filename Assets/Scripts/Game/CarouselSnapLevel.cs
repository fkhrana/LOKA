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
        scrollRect = scrollRect ?? GetComponent<ScrollRect>();
        if (scrollRect == null) { enabled = false; return; }
        content = scrollRect.content;
        if (content == null) { enabled = false; return; }

        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
    }

    private void Start() => Invoke(nameof(Refresh), 0.05f);

    private void OnDestroy()
    {
        if (nextButton) nextButton.onClick.RemoveListener(NextPage);
        if (prevButton) prevButton.onClick.RemoveListener(PrevPage);
    }

    public void Refresh()
    {
        if (scrollRect == null || content == null) return;

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
    }

    private void Update()
    {
        if (!isSnapping || positions == null || positions.Length == 0) return;

        float target = positions[targetIndex];
        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition, target, Time.unscaledDeltaTime * snapSpeed);

        if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - target) < 0.001f)
        {
            scrollRect.horizontalNormalizedPosition = target;
            isSnapping = false;
            UpdateButtons();
        }
    }

    public void NextPage()
    {
        if (positions == null || targetIndex >= positions.Length - 1) return;
        targetIndex++;
        isSnapping = true;
        UpdateButtons();
        NotifyLevelManager();
    }

    public void PrevPage()
    {
        if (positions == null || targetIndex <= 0) return;
        targetIndex--;
        isSnapping = true;
        UpdateButtons();
        NotifyLevelManager();
    }

    public int GetCurrentIndex() => targetIndex;

    private void UpdateButtons()
    {
        bool hasMultiple = positions != null && positions.Length > 1;
        if (nextButton) nextButton.interactable = hasMultiple && targetIndex < positions.Length - 1;
        if (prevButton) prevButton.interactable = hasMultiple && targetIndex > 0;
    }

    private void NotifyLevelManager() => LevelManager.Instance?.UpdateReplayButton();

    public void OnEndDrag()
    {
        if (positions == null || positions.Length == 0) return;

        float currentPos = scrollRect.horizontalNormalizedPosition;
        int closest = 0;
        float closestDist = float.MaxValue;
        for (int i = 0; i < positions.Length; i++)
        {
            float dist = Mathf.Abs(currentPos - positions[i]);
            if (dist < closestDist) { closestDist = dist; closest = i; }
        }

        targetIndex = closest;
        isSnapping = true;
        UpdateButtons();
        NotifyLevelManager();
    }
}