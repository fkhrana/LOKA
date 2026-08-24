using UnityEngine;
using System.Collections;

public class EfekTasPop : MonoBehaviour
{
    private Vector3 skalaAwal;

    void Start()
    {
        // Menyimpan ukuran asli tas saat game mulai
        skalaAwal = transform.localScale;
    }

    public void MainkanPop()
    {
        StopAllCoroutines();
        StartCoroutine(PopAnimasi());
    }

    private IEnumerator PopAnimasi()
    {
        float targetSkala = 1.3f; // Kembali ke ukuran yang pas (tidak lebay)
        float waktuNaik = 0.12f;  // Sedikit lebih lama dari 0.1 agar mata sempat melihat prosesnya
        float waktuTurun = 0.25f; // Mengecilnya lebih santai agar ritmenya asyik

        // 1. FASE MEMBESAR (POP!)
        float waktu = 0;
        while (waktu < waktuNaik)
        {
            waktu += Time.deltaTime;
            float persentase = waktu / waktuNaik;

            // RITME EASE-OUT: Melompat kencang di awal, meredam halus di akhir (Cubic Curve)
            float ritme = 1f - Mathf.Pow(1f - persentase, 3f); 
            
            transform.localScale = Vector3.Lerp(skalaAwal, skalaAwal * targetSkala, ritme);
            yield return null;
        }

        // 2. FASE MENGECIL (PULIH)
        waktu = 0;
        while (waktu < waktuTurun)
        {
            waktu += Time.deltaTime;
            float persentase = waktu / waktuTurun;

            // Ritme meredam untuk turun kembali ke normal
            float ritme = 1f - Mathf.Pow(1f - persentase, 2f);

            transform.localScale = Vector3.Lerp(skalaAwal * targetSkala, skalaAwal, ritme);
            yield return null;
        }
        
        // Pastikan ukuran kembali presisi
        transform.localScale = skalaAwal; 
    }
}