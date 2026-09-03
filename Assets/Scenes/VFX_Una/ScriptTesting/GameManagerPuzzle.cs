using UnityEngine;
using System.Collections;

public class GameManagerPuzzle : MonoBehaviour
{
    [Header("Pengaturan Puzzle")]
    public int totalKotakPuzzle = 2; 
    private int kotakBenarTerkumpul = 0;

    [Header("VFX & Objek Animasi - KIRI")]
    public GameObject terompetKiriObject; 
    public GameObject confettiKiriVFX;    

    [Header("VFX & Objek Animasi - KANAN")]
    public GameObject terompetKananObject; 
    public GameObject confettiKananVFX;

    // Dipanggil oleh KotakPuzzle
    public void LaporSatuKotakBenar() 
    {
        kotakBenarTerkumpul++;
        
        // Jika semua kotak sudah benar, jalankan sequence VFX
        if (kotakBenarTerkumpul >= totalKotakPuzzle) 
        {
            StartCoroutine(SequencedVFX());
        }
    }

    private IEnumerator SequencedVFX()
    {
        // 1. Tampilkan Kedua Terompet dan mainkan animasinya berbarengan
        if (terompetKiriObject != null)
        {
            terompetKiriObject.SetActive(true);
            StartCoroutine(AnimasiJuicyTerompet(terompetKiriObject)); // Kirim objek kiri untuk dianimasikan
        }
        
        if (terompetKananObject != null)
        {
            terompetKananObject.SetActive(true);
            StartCoroutine(AnimasiJuicyTerompet(terompetKananObject)); // Kirim objek kanan untuk dianimasikan
        }
        
        // 2. Tunggu durasi animasi terompet mengkerut
        yield return new WaitForSeconds(0.6f);
        
        // 3. Ledakkan Confetti Kiri
        if (confettiKiriVFX != null)
        {
            confettiKiriVFX.SetActive(true);
            foreach (ParticleSystem ps in confettiKiriVFX.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
            }
        }

        // 4. Ledakkan Confetti Kanan
        if (confettiKananVFX != null)
        {
            confettiKananVFX.SetActive(true);
            foreach (ParticleSystem ps in confettiKananVFX.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
            }
        }
    }

    // ANIMASI JUICY TEROMPET (Diubah agar menerima objek spesifik yang mau dianimasikan)
    private IEnumerator AnimasiJuicyTerompet(GameObject targetTerompet)
    {
        Vector3 skalaAwal = targetTerompet.transform.localScale;
        Vector3 skalaKecil = skalaAwal * 0.5f; // Mengkerut
        Vector3 skalaBesar = skalaAwal * 1.2f; // Overshoot (membesar sedikit)
        float durasi = 0.15f;
        float waktu = 0f;

        // Fase 1: Mengkerut cepat
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            targetTerompet.transform.localScale = Vector3.Lerp(skalaAwal, skalaKecil, waktu / durasi);
            yield return null;
        }

        // Fase 2: Overshoot (membesar sedikit dari ukuran asli)
        waktu = 0f;
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            targetTerompet.transform.localScale = Vector3.Lerp(skalaKecil, skalaBesar, waktu / durasi);
            yield return null;
        }

        // Fase 3: Kembali ke ukuran normal
        waktu = 0f;
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            targetTerompet.transform.localScale = Vector3.Lerp(skalaBesar, skalaAwal, waktu / durasi);
            yield return null;
        }
        targetTerompet.transform.localScale = skalaAwal;
    }
}