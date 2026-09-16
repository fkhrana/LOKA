using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartVFX : MonoBehaviour
{
    public Image gambarHati;
    public ParticleSystem partikelLedakan;

    public void TerkenaDamage()
    {
        StartCoroutine(AnimasiMeledak());
    }

    IEnumerator AnimasiMeledak()
    {
        float waktu = 0;

        // FASE 1: Kedap-kedip dan bergetar EKSTREM (Selama 0.3 detik)
        while (waktu < 0.3f)
        {
            // Kedap-kedip warna (Menggunakan 'waktu' agar mulainya selalu konsisten)
            gambarHati.color = (Mathf.PingPong(waktu * 20, 1) > 0.5f) ? Color.white : Color.red;
            
            // EFEK GETAR EXTREME: PingPong dari 0 ke 1.0, lalu ditambah 0.5
            // Artinya ukuran hati akan naik turun antara 0.5 (setengah) sampai 1.5 (sangat besar)!
            float skalaGetar = Mathf.PingPong(waktu * 15, 1.0f) + 0.5f; 
            transform.localScale = new Vector3(skalaGetar, skalaGetar, 1f);
            
            waktu += Time.deltaTime;
            yield return null;
        }

        // FASE 2: Membesar (Swell) sesaat sebelum meledak (Selama 0.15 detik)
        waktu = 0;
        gambarHati.color = Color.white; 
        
        while (waktu < 0.15f)
        {
            // Kita buat ledakannya membesar sampai 2.5x lipat biar lebih JUICY!
            float skalaSwell = Mathf.Lerp(1.5f, 2.5f, waktu / 0.15f); 
            transform.localScale = new Vector3(skalaSwell, skalaSwell, 1f);
            waktu += Time.deltaTime;
            yield return null;
        }

        // FASE 3: KAPOW! (Meledak)
        gambarHati.enabled = false; 
        
        if (partikelLedakan != null)
        {
            partikelLedakan.Play(); 
        }
    }
}