using UnityEngine;
using System.Collections.Generic;

public class ConnectDotsManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject dotPrefab;
    public List<Vector2> dotPositions; // isi posisi titik-titik di Inspector, urut dari titik 1

    [Header("Garis")]
    public LineRenderer lineRenderer;

    private List<GameObject> spawnedDots = new List<GameObject>();
    private int nextDotToClick = 1; // dimulai dari titik nomor 1

    void Start()
    {
        SpawnDots();
        lineRenderer.positionCount = 0;
    }

    void SpawnDots()
    {
        for (int i = 0; i < dotPositions.Count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotPositions[i], Quaternion.identity, transform);
            DotController dc = dot.GetComponent<DotController>();
            dc.Setup(i + 1); // nomor mulai dari 1
            spawnedDots.Add(dot);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckClick();
        }
    }

    void CheckClick()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            DotController dc = hit.collider.GetComponent<DotController>();
            if (dc != null && dc.dotNumber == nextDotToClick)
            {
                // Klik benar! tambahkan titik ini ke garis
                AddPointToLine(dc.transform.position);
                nextDotToClick++;

                if (nextDotToClick > spawnedDots.Count)
                {
                    Debug.Log("SELESAI! Semua titik tersambung.");
                    // TODO: tampilkan UI menang di sini
                }
            }
            // kalau dc.dotNumber != nextDotToClick, artinya salah urutan -> tidak terjadi apa-apa
        }
    }

    void AddPointToLine(Vector3 point)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, point);
    }
}
