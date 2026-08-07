using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string correctTargetTag; // isi "NA", "DA", atau "NONE" kalau pengecoh

    private Vector3 startPosition;
    private Transform startParent;
    private CanvasGroup canvasGroup;
    private Transform dragRoot;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startParent = transform.parent;

        var parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            dragRoot = parentCanvas.transform;
            transform.SetParent(dragRoot, true);
        }
        transform.SetAsLastSibling();

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // biar raycast tembus ke slot di bawahnya
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == startParent)
        {
            ReturnToStart();
        }
    }

    public void ReturnToStart()
    {
        transform.position = startPosition;
        transform.SetParent(startParent);
    }
}