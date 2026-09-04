using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string correctTargetTag;

    public bool IsDragging { get; private set; }

    private Vector3 startPosition;
    private Transform startParent;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;

        startPosition = transform.position;
        startParent = transform.parent;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        EffectHover hover = GetComponent<EffectHover>();

        if (hover != null)
            hover.StopHoverEffect();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == startParent)
            ReturnToStart();
    }

    public void ReturnToStart()
    {
        transform.position = startPosition;
        transform.SetParent(startParent);
    }
}