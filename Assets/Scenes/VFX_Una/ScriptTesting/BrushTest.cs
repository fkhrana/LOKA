using UnityEngine;

public class TesKuas : MonoBehaviour
{
    TrailRenderer trail;

    void Start()
    {
        // Mencari komponen Trail Renderer secara otomatis
        trail = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        // Saat kita baru mengeklik layar
        if (Input.GetMouseButtonDown(0) && trail != null)
        {
            trail.Clear(); // Menghapus jejak lama agar garisnya tidak melompat
        }

        // Selama kita menahan klik dan menggeser mouse
        if (Input.GetMouseButton(0)) 
        {
            Vector3 posisiMouse = Input.mousePosition;
            posisiMouse.z = 10f; // Jarak virtual dari kamera
            
            // Ubah posisi VFX mengikuti kursor mouse
            transform.position = Camera.main.ScreenToWorldPoint(posisiMouse);
        }
    }
}