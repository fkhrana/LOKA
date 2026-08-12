using UnityEngine;
using UnityEngine.UI;

public class CarouselSnap : MonoBehaviour
{
    [Header("ScrollRect")]
    public ScrollRect scrollRect;

    [Header("Navigation Buttons")]
    public Button nextButton;   // Drag NextButton di sini
    public Button prevButton;   // Drag PrevButton di sini

    private RectTransform content;
    private float[] positions;        // Posisi snap untuk tiap card
    private int targetIndex = 0;      // Index card yang sedang aktif
    private bool isSnapping = false;

    void Start()
    {
        // Cari ScrollRect jika belum diisi
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        if (scrollRect == null)
        {
            Debug.LogError("ScrollRect tidak ditemukan! Pasang script ini di GameObject yang punya ScrollRect.");
            return;
        }

        content = scrollRect.content;
        int childCount = content.childCount;

        if (childCount <= 1)
        {
            // Jika cuma 1 card, disable tombol
            if (nextButton != null) nextButton.interactable = false;
            if (prevButton != null) prevButton.interactable = false;
            return;
        }

        // Hitung posisi snap untuk setiap card
        positions = new float[childCount];
        float spacing = 1f / (childCount - 1);
        for (int i = 0; i < childCount; i++)
        {
            positions[i] = spacing * i;
        }

        // Set posisi awal ke card pertama
        targetIndex = 0;
        scrollRect.horizontalNormalizedPosition = 0;
        UpdateButtonInteractable();

        // Pasang listener untuk tombol
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (prevButton != null)
            prevButton.onClick.AddListener(PrevPage);
    }

    void Update()
    {
        if (!isSnapping) return;

        // Smooth snap ke target
        float targetPos = positions[targetIndex];
        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            targetPos,
            Time.deltaTime * 10f
        );

        // Jika sudah hampir sampai, langsung set ke posisi pasti
        if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetPos) < 0.001f)
        {
            scrollRect.horizontalNormalizedPosition = targetPos;
            isSnapping = false;
            UpdateButtonInteractable();
        }
    }

    // Dipanggil saat drag selesai (hubungkan ke event OnEndDrag ScrollRect)
    public void OnEndDrag()
    {
        if (positions == null || positions.Length == 0) return;

        float currentPos = scrollRect.horizontalNormalizedPosition;
        float closestDist = Mathf.Infinity;

        for (int i = 0; i < positions.Length; i++)
        {
            float dist = Mathf.Abs(currentPos - positions[i]);
            if (dist < closestDist)
            {
                closestDist = dist;
                targetIndex = i;
            }
        }

        isSnapping = true;
    }

    // Tombol Next
    public void NextPage()
    {
        if (positions == null || targetIndex >= positions.Length - 1) return;
        targetIndex++;
        isSnapping = true;
    }

    // Tombol Prev
    public void PrevPage()
    {
        if (positions == null || targetIndex <= 0) return;
        targetIndex--;
        isSnapping = true;
    }

    // Aktif/nonaktifkan tombol sesuai posisi
    private void UpdateButtonInteractable()
    {
        if (nextButton != null)
            nextButton.interactable = (targetIndex < positions.Length - 1);

        if (prevButton != null)
            prevButton.interactable = (targetIndex > 0);
    }
}