using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public string zoneTag; // isi "NA" atau "DA"
    [HideInInspector] public bool isFilled = false;

    public void OnDrop(PointerEventData eventData)
    {
        DragItem draggedItem = eventData.pointerDrag.GetComponent<DragItem>();
        if (draggedItem == null) return;

        if (draggedItem.correctTargetTag == zoneTag)
        {
            // BENAR
            draggedItem.transform.SetParent(transform);
            draggedItem.transform.localPosition = Vector3.zero;

            isFilled = true;

            // Kunci biar item ini nggak bisa didrag-drag lagi setelah benar
            var img = draggedItem.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = false;

            PuzzleManager.Instance.CheckPuzzleComplete();

            Debug.Log("Benar! Item masuk ke " + zoneTag);
        }
        else
        {
            // SALAH
            draggedItem.ReturnToStart();
            Debug.Log("Salah, kembali ke posisi awal");
        }
    }
}