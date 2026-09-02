    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class DropZone : MonoBehaviour, IDropHandler
    {
    public string zoneTag; // isi "NA" atau "DA"
    [HideInInspector] public bool isFilled = false;

    [Header("SFX")]
    [SerializeField] private AudioClip correctSFX;
    [SerializeField] private AudioClip wrongSFX;

    // === TAMBAHAN UNTUK SHAKE ===
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 15f;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
            originalAnchoredPos = rectTransform.anchoredPosition;
    }

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

            var img = draggedItem.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = false;

            AudioManager.Instance.PlaySFX(correctSFX);

            PuzzleManager.Instance.CheckPuzzleComplete();

            Debug.Log("Benar! Item masuk ke " + zoneTag);
        }
        else
        {
            // SALAH
            draggedItem.ReturnToStart();

            AudioManager.Instance.PlaySFX(wrongSFX);

            // trigger shake
            if (rectTransform != null)
            {
                if (shakeRoutine != null) StopCoroutine(shakeRoutine);
                shakeRoutine = StartCoroutine(ShakePanel());
            }
            // ================================

            Debug.Log("Salah, kembali ke posisi awal");
        }
    }

    // coroutine shake
    private IEnumerator ShakePanel()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            rectTransform.anchoredPosition = originalAnchoredPos + new Vector2(offsetX, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = originalAnchoredPos;
    }

    }
