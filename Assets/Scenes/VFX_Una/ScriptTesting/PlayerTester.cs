using UnityEngine;

public class PlayerTester : MonoBehaviour
{
    [Header("UI HP Bar")]
    public HeartVFX[] daftarHati; 
    private int hpSaatIni;

    [Header("Helper dan VFX")]
    public GameObject helperKarakter; // Karakter Helper yang akan dimunculkan
    public GameObject vfxHealObjek;   // Objek induk VFX_Heal yang baru kita buat

    void Start()
    {
        hpSaatIni = daftarHati.Length; 
        
        // Matikan helper dan VFX saat game mulai
        if(helperKarakter != null) helperKarakter.SetActive(false);
        if(vfxHealObjek != null) vfxHealObjek.SetActive(false);
    }

    // Dipanggil saat klik Player
    void OnMouseDown()
    {
        KurangiHP();
    }

    public void KurangiHP()
    {
        if (hpSaatIni > 0)
        {
            hpSaatIni--; 
            daftarHati[hpSaatIni].TerkenaDamage(); 
            Debug.Log("Sisa HP: " + hpSaatIni);

            // Jika HP berkurang, panggil sang Helper!
            if (helperKarakter != null && !helperKarakter.activeSelf)
            {
                helperKarakter.SetActive(true); 
            }
        }
    }

    // FUNGSI BARU UNTUK HEALING
    public void TambahHP()
    {
        if (hpSaatIni < daftarHati.Length)
        {
            // Panggil VFX Heal
            if(vfxHealObjek != null)
            {
                vfxHealObjek.SetActive(false); // Matikan dulu biar me-reset
                vfxHealObjek.SetActive(true);  // Nyalakan lagi
                
                // Cari semua Particle System di dalam objek ini dan Play
                ParticleSystem[] semuaPartikel = vfxHealObjek.GetComponentsInChildren<ParticleSystem>();
                foreach(ParticleSystem ps in semuaPartikel)
                {
                    ps.Play();
                }
            }

            // Kembalikan gambar Hati (Tidak perlu animasi, langsung muncul)
            daftarHati[hpSaatIni].gambarHati.enabled = true;
            daftarHati[hpSaatIni].transform.localScale = Vector3.one; 
            daftarHati[hpSaatIni].gambarHati.color = Color.white;
            
            hpSaatIni++;
            Debug.Log("Healed! HP sekarang: " + hpSaatIni);
        }
        else
        {
            Debug.Log("HP Sudah Penuh!");
        }
    }
}