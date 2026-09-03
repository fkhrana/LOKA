using UnityEngine;
using System.Collections;

public class KotakPuzzle : MonoBehaviour
{
    [Header("VFX")]
    [Header("Hubungkan ke Manager")]
    public GameManagerPuzzle gameManager; 
    public ParticleSystem vfxLedakanBintang; 

    [Header("Status Kotak")]
    public bool sudahTerisi = false; // Penanda kotak sudah diisi item yang benar

    private Vector3 posisiAwal;

    void Start()
    {
        posisiAwal = transform.position;
    }

    // Dipanggil oleh item jika jawabannya BENAR
    public void ReaksiBenar()
    {
        sudahTerisi = true; // Kunci kotak ini agar tidak bisa diisi item lain
        
        if (vfxLedakanBintang != null)
        {
            vfxLedakanBintang.Play();
        }
        if (gameManager != null)
        {
            gameManager.LaporSatuKotakBenar();
        }
    }

    // Dipanggil oleh item jika jawabannya SALAH
    public void ReaksiSalah()
    {
        // Cegah kotak yang sudah terisi dari gelengan jika tidak sengaja ditimpa item salah
        if (sudahTerisi) return; 

        StopAllCoroutines(); // Hentikan gelengan sebelumnya jika ada
        StartCoroutine(AnimasiGeleng());
    }

    private IEnumerator AnimasiGeleng()
    {
        float durasi = 0.3f; 
        float waktu = 0f;
        float kecepatanGeleng = 50f; 
        float jarakGeleng = 0.15f; 

        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            
            float geserX = Mathf.Sin(waktu * kecepatanGeleng) * jarakGeleng;
            transform.position = posisiAwal + new Vector3(geserX, 0, 0);
            
            yield return null;
        }
        
        transform.position = posisiAwal; 
    }
}