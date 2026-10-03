using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string correctTargetTag;

    public bool IsDragging { get; private set; }

    private Vector3 startPosition;
    private Transform startParent;

    // ✅ Simpan posisi & parent ASLI
    private Vector3 originalPosition;
    private Transform originalParent;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // ✅ Simpan posisi & parent asli
        originalPosition = transform.position;
        originalParent = transform.parent;
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

    // ✅ Return ke posisi ASLI + re-enable raycast
    public void ReturnToOriginal()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // ✅ FIX: Re-enable raycastTarget
        Image img = GetComponent<Image>();
        if (img != null)
            img.raycastTarget = true;

        Debug.Log($"[DragItem] Return ke original + raycast enabled: {gameObject.name}");
    }
}