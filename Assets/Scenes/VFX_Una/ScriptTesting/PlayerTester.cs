using UnityEngine;

public class PlayerTester : MonoBehaviour
{
    [Header("Masukkan 5 Hati dari Kiri ke Kanan")]
    public HeartVFX[] daftarHati; 
    
    private int hpSaatIni;

    void Start()
    {
        // Set HP awal sesuai jumlah hati yang kamu masukkan
        hpSaatIni = daftarHati.Length; 
    }

    // FUNGSI TESTING: Dipanggil otomatis oleh Unity kalau Sprite ini di-klik pakai Mouse
    void OnMouseDown()
    {
        KurangiHP();
    }

    public void KurangiHP()
    {
        if (hpSaatIni > 0)
        {
            hpSaatIni--; // Kurangi 1 angka HP (misal dari 5 jadi 4)
            
            // Panggil animasi meledak pada hati yang berada di urutan tersebut
            daftarHati[hpSaatIni].TerkenaDamage(); 
            
            Debug.Log("Aduh kena hit! Sisa HP: " + hpSaatIni);
        }
        else
        {
            Debug.Log("Player Sudah Mati!");
        }
    }
}