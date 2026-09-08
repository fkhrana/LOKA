using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Pengaturan Kamera")]
    public Camera mainCamera;
    public Transform targetKanan; 
    public float jedaAwal = 0.5f;       
    public float durasiPan = 2.5f;
    public float jedaLihatMusuh = 1.5f;

    [Header("Pengaturan UI Countdown")]
    public Image countdownImage;
    public Sprite gambar3;
    public Sprite gambar2;
    public Sprite gambar1;
    public Sprite gambarMulai;

    // Variabel internal penyimpan posisi
    private Vector3 posisiKiri;
    private Vector3 posisiKanan;

    void Start()
    {
        // Menyembunyikan UI saat mulai
        countdownImage.gameObject.SetActive(false);
        
        // Memastikan gambar tidak penyet
        countdownImage.preserveAspect = true;

        // Menyimpan posisi
        posisiKiri = mainCamera.transform.position;

        if (targetKanan == null)
        {
            Debug.LogError("ERROR: Kamu belum memasukkan objek TargetMusuh ke kolom Target Kanan di Inspector!");
            return;
        }

        posisiKanan = new Vector3(targetKanan.position.x, posisiKiri.y, posisiKiri.z);

        // Mulai cutscene
        StartCoroutine(MainkanIntro());
    }

    IEnumerator MainkanIntro()
    {
        // 0. Jeda awal
        yield return new WaitForSeconds(jedaAwal);

        // 1. Kamera Panning ke Kanan 
        yield return StartCoroutine(GerakkanKamera(posisiKiri, posisiKanan, durasiPan));

        // 2. Tunggu sebentar melihat musuh
        yield return new WaitForSeconds(jedaLihatMusuh);

        // 3. Kamera Panning kembali ke Kiri 
        yield return StartCoroutine(GerakkanKamera(posisiKanan, posisiKiri, durasiPan));

        // 4. Mulai Countdown
        countdownImage.gameObject.SetActive(true);

        // Memanggil fungsi Pop-Up untuk masing-masing gambar
        yield return StartCoroutine(TampilkanEfekPopUp(gambar3));
        yield return StartCoroutine(TampilkanEfekPopUp(gambar2));
        yield return StartCoroutine(TampilkanEfekPopUp(gambar1));
        yield return StartCoroutine(TampilkanEfekPopUp(gambarMulai));

        // Selesai! Hilangkan UI
        countdownImage.gameObject.SetActive(false);
        Debug.Log("Game Dimulai!");
    }

    IEnumerator GerakkanKamera(Vector3 posisiAwal, Vector3 posisiAkhir, float durasi)
    {
        float waktu = 0;
        while (waktu < durasi)
        {
            mainCamera.transform.position = Vector3.Lerp(posisiAwal, posisiAkhir, waktu / durasi);
            waktu += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = posisiAkhir; 
    }

    IEnumerator TampilkanEfekPopUp(Sprite spriteAngka)
    {
        // 1. Ganti gambarnya
        countdownImage.sprite = spriteAngka;

        // 2. KUNCI JAWABAN: Paksa kotak UI untuk mengikuti resolusi/ukuran asli aset PNG!
        countdownImage.SetNativeSize();

        // 3. Set ukuran awal jadi 0 (tidak terlihat) sebelum dianimasikan
        countdownImage.transform.localScale = Vector3.zero;

        // FASE 1: Membesar cepat dari 0 ke 1.2
        float waktu = 0;
        float durasiMembesar = 0.2f;
        while (waktu < durasiMembesar)
        {
            float skala = Mathf.Lerp(0f, 1.2f, waktu / durasiMembesar);
            countdownImage.transform.localScale = new Vector3(skala, skala, 1f);
            waktu += Time.deltaTime;
            yield return null;
        }

        // FASE 2: Memantul/Mengecil sedikit dari 1.2 ke 1.0 (Ukuran Normal)
        waktu = 0;
        float durasiMantul = 0.1f;
        while (waktu < durasiMantul)
        {
            float skala = Mathf.Lerp(1.2f, 1f, waktu / durasiMantul);
            countdownImage.transform.localScale = new Vector3(skala, skala, 1f);
            waktu += Time.deltaTime;
            yield return null;
        }

        countdownImage.transform.localScale = Vector3.one;

        // FASE 3: Berdiam sejenak sebelum pindah ke angka berikutnya
        yield return new WaitForSeconds(0.7f);
    }
}