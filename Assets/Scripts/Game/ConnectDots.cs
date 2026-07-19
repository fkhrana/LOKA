using UnityEngine;
using System.Collections.Generic;

// Wadah untuk satu bentuk (kumpulan posisi titik)
[System.Serializable]
public class ShapeData
{
    public string shapeName; // opsional, cuma buat penanda di Inspector
    public List<Vector2> dotPositions;
}

public class ConnectDots : MonoBehaviour
{
    [Header("Setup")]
    public GameObject dotPrefab;
    public List<ShapeData> shapes; // daftar semua bentuk, urut dari bentuk pertama

    [Header("Garis")]
    public LineRenderer lineRenderer;

    [Header("Jeda sebelum lanjut ke bentuk berikutnya (detik)")]
    public float delayBeforeNextShape = 1f;

    [Header("Drag Settings")]
    public float dotTriggerRadius = 0.5f; // seberapa dekat jari harus ke titik biar dianggap "kena"

    private List<GameObject> spawnedDots = new List<GameObject>();
    private int nextDotToClick = 1; // dimulai dari titik nomor 1
    private int currentShapeIndex = 0;
    private bool isDragging = false;
    private List<Vector3> confirmedPoints = new List<Vector3>(); // titik-titik yang sudah pasti tersambung

    void Start()
    {
        LoadShape(currentShapeIndex);
    }

    void LoadShape(int index)
    {
        // Bersihkan dot lama dan garis lama
        foreach (var dot in spawnedDots) Destroy(dot);
        spawnedDots.Clear();
        lineRenderer.positionCount = 0;
        confirmedPoints.Clear();
        nextDotToClick = 1;
        isDragging = false;

        if (index >= shapes.Count)
        {
            Debug.Log("SEMUA BENTUK SELESAI! Game tamat.");
            // TODO: tampilkan UI "Game Selesai" di sini
            return;
        }

        SpawnDots(shapes[index].dotPositions);
    }

    void SpawnDots(List<Vector2> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, positions[i], Quaternion.identity, transform);
            DotController dc = dot.GetComponent<DotController>();
            dc.Setup(i + 1); // nomor mulai dari 1
            spawnedDots.Add(dot);
        }
    }

    void Update()
    {
        Vector2 inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(inputPos);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag(inputPos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopDrag();
        }
    }

    // Mulai drag hanya kalau jari nempel pas di titik nomor 1
    void TryStartDrag(Vector2 pos)
    {
        DotController dot = FindDotNear(pos);
        if (dot != null && dot.dotNumber == nextDotToClick)
        {
            isDragging = true;
            ConfirmDot(dot);
        }
    }

    // Selama drag, cek apakah jari sudah nyampe titik berikutnya
    void ContinueDrag(Vector2 pos)
    {
        DotController dot = FindDotNear(pos);
        if (dot != null && dot.dotNumber == nextDotToClick)
        {
            ConfirmDot(dot);
        }

        // Update garis "sementara" biar mengikuti posisi jari real-time
        lineRenderer.positionCount = confirmedPoints.Count + 1;
        for (int i = 0; i < confirmedPoints.Count; i++)
            lineRenderer.SetPosition(i, confirmedPoints[i]);
        lineRenderer.SetPosition(confirmedPoints.Count, pos);
    }

    void StopDrag()
    {
        isDragging = false;
        // Balikin garis ke titik-titik yang sudah confirmed saja (buang ekor yang ikut jari)
        lineRenderer.positionCount = confirmedPoints.Count;
        for (int i = 0; i < confirmedPoints.Count; i++)
            lineRenderer.SetPosition(i, confirmedPoints[i]);
    }

    // Cari dot yang posisinya dekat dengan pos (dalam radius dotTriggerRadius)
    DotController FindDotNear(Vector2 pos)
    {
        foreach (var dot in spawnedDots)
        {
            if (dot == null) continue;
            float dist = Vector2.Distance(pos, dot.transform.position);
            if (dist <= dotTriggerRadius)
                return dot.GetComponent<DotController>();
        }
        return null;
    }

    // Titik resmi tersambung ke garis
    void ConfirmDot(DotController dc)
    {
        confirmedPoints.Add(dc.transform.position);
        nextDotToClick++;

        if (nextDotToClick > spawnedDots.Count)
        {
            isDragging = false;
            Debug.Log("Bentuk selesai! Lanjut ke bentuk berikutnya...");
            currentShapeIndex++;
            Invoke(nameof(LoadNextShape), delayBeforeNextShape);
        }
    }

    void LoadNextShape()
    {
        LoadShape(currentShapeIndex);
    }
}
