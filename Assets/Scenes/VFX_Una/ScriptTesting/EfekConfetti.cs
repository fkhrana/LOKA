using UnityEngine;
using System.Collections;

public class EfekConfetti : MonoBehaviour
{
    public ParticleSystem vfxConfetti; // Masukkan partikel anakannya ke sini

    private Vector3 skalaAsli;

    void Start()
    {
        skalaAsli = transform.localScale;
        
        // Terompet disembunyikan (skala 0) dari awal sebelum menang
        transform.localScale = Vector3.zero; 
    }

    // Fungsi ini akan dipanggil saat puzzle selesai
    public void MuntahkanConfetti()
    {
        StartCoroutine(AnimasiMuntahJuicy());
    }

    private IEnumerator AnimasiMuntahJuicy()
    {
        float kecepatanFase = 0.15f; // Kecepatan tiap gerakan (sangat cepat)

        // 1. Muncul dan langsung Mengkerut (Squash - Pendek & Lebar)
        Vector3 skalaKerut = new Vector3(skalaAsli.x * 1.5f, skalaAsli.y * 0.4f, 1f);
        float waktu = 0f;
        while (waktu < kecepatanFase)
        {
            waktu += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, skalaKerut, waktu / kecepatanFase);
            yield return null;
        }

        // 2. Memanjang (Stretch - Kurus & Tinggi) SEKALIGUS NEMBAK
        Vector3 skalaPanjang = new Vector3(skalaAsli.x * 0.6f, skalaAsli.y * 1.4f, 1f);
        waktu = 0f;
        
        // Tembakkan partikel tepat saat dia mulai memanjang ke atas!
        if (vfxConfetti != null) vfxConfetti.Play();

        while (waktu < kecepatanFase)
        {
            waktu += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaKerut, skalaPanjang, waktu / kecepatanFase);
            yield return null;
        }

        // 3. Kembali ke ukuran normal (Wobble settle)
        waktu = 0f;
        while (waktu < kecepatanFase)
        {
            waktu += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaPanjang, skalaAsli, waktu / kecepatanFase);
            yield return null;
        }

        transform.localScale = skalaAsli; // Kunci di ukuran asli
    }
}