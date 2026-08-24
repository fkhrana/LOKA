using UnityEngine;
using System.Collections;

public class AnimasiDrop : MonoBehaviour
{
    [Header("Pengaturan Pantulan")]
    public float tinggiLompat = 1.5f;     
    public float jarakMenyamping = -1.5f; 
    public float durasiDrop = 0.4f;       
    public GameObject vfxDropPrefab;      

    private Vector3 posisiAwalLompat;
    private GameObject vfxDropAktif; // Variabel baru untuk menyimpan VFX-nya

    void Start()
    {
        posisiAwalLompat = transform.position;
        MulaiLompat();
    }

    public void MulaiLompat()
    {
        StopAllCoroutines(); 
        transform.position = posisiAwalLompat; 
        StartCoroutine(ProsesDrop());
    }

    public IEnumerator ProsesDrop()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) { trail.emitting = false; trail.Clear(); }
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null) { ps.Stop(); ps.Clear(); }

        // --- SOLUSI 1: MUNCULKAN DAN TEMPELKAN VFX KE ITEM ---
        if (vfxDropPrefab != null) 
        {
            // Hapus VFX lama jika masih ada (biar gak dobel-dobel)
            if (vfxDropAktif != null) Destroy(vfxDropAktif);
            
            // Tambahkan kata 'transform' di belakang agar VFX-nya jadi anak & nempel ke item!
            vfxDropAktif = Instantiate(vfxDropPrefab, transform.position, vfxDropPrefab.transform.rotation, transform);
        }

        Vector3 awal = transform.position;
   
        
        // --- SISTEM PENDETEKSI TANAH (RAYCAST) ---
        // 1. Tentukan titik X (menyamping) tempat dia akan jatuh
        Vector3 titikTarget = awal + new Vector3(jarakMenyamping, 0f, 0f);
        
        // 2. Beri posisi cadangan (misal jarak jatuhnya 1 meter) kalau-kalau tidak ada tanah
        Vector3 tanah = titikTarget + new Vector3(0f, -1f, 0f); 

        // 3. Tembakkan laser dari titikTarget lurus ke bawah (Vector3.down) sejauh 100 meter
        RaycastHit tabrakan;
        if (Physics.Raycast(titikTarget, Vector3.down, out tabrakan, 100f))
        {
            // Jika laser menabrak sesuatu, jadikan titik tabrakan itu sebagai lantai mendaratnya!
            tanah = tabrakan.point; 
        }
        // ------------------------------------------

        yield return StartCoroutine(Lompatan(awal, tanah, tinggiLompat, durasiDrop));
        
        Vector3 tanah2 = tanah + new Vector3(jarakMenyamping * 0.2f, 0f, 0f);
        yield return StartCoroutine(Lompatan(tanah, tanah2, tinggiLompat * 0.3f, durasiDrop * 0.5f));
    }

    private IEnumerator Lompatan(Vector3 dari, Vector3 ke, float tinggi, float waktuTotal)
    {
        float waktu = 0f;
        while (waktu < waktuTotal)
        {
            waktu += Time.deltaTime;
            float p = waktu / waktuTotal; 
            float lengkung = Mathf.Sin(p * Mathf.PI) * tinggi;
            transform.position = Vector3.Lerp(dari, ke, p) + new Vector3(0, lengkung, 0);
            yield return null; 
        }
        transform.position = ke;
    }

    // --- FUNGSI BARU UNTUK MEMBUNUH VFX ---
    public void MatikanVfxDrop()
    {
        if (vfxDropAktif != null)
        {
            Destroy(vfxDropAktif);
        }
    }
}