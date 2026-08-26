using UnityEngine;
using System.Collections; // Wajib untuk menjalankan urutan animasi (Coroutine)

public class ItemCahayaTes : MonoBehaviour
{
    [Header("Hubungkan ke UI")]
    public PengaturBar scriptBar; 
    public Transform targetPosisiBar; 

    [Header("Pengaturan Transisi (Pop!)")]
    public float durasiMenciut = 0.15f; // Kecepatan gambar item menghilang ke dalam
    public float durasiMembesar = 0.15f; // Kecepatan cahaya meledak keluar

    [Header("Pengaturan Terbang (Magis)")]
    public float durasiTerbang = 1.2f; 
    public float tinggiLengkungan = 3f; 
    public float geserSamping = 1f; 
    public float jumlahProgressYangDitambah = 0.2f; 

    [Header("Ritme Kecepatan")]
    public AnimationCurve ritmeTerbang = AnimationCurve.EaseInOut(0, 0, 1, 1); 

    [Header("Visual Item")]
    public SpriteRenderer gambarItem;
    public ParticleSystem vfxCahayaTerbang; 

    private bool sudahDiklik = false;
    private bool sedangTransisi = false; // Pengunci agar tidak bisa dobel klik saat animasi pop berjalan
    private Vector3 posisiAwal;
    private float waktuBerjalan = 0f;

    // Penyimpan ukuran asli agar bisa dikembalikan saat reset
    private Vector3 skalaAwalItem;
    private Vector3 skalaAwalVFX;

    void Start()
    {
        posisiAwal = transform.position;

        // Simpan ukuran aslinya
        if (gambarItem != null) skalaAwalItem = gambarItem.transform.localScale;
        if (vfxCahayaTerbang != null) skalaAwalVFX = vfxCahayaTerbang.transform.localScale;

        // Sistem bersih-bersih awal (Sama persis dengan sistem yang sudah berhasil)
        ParticleSystem[] semuaPartikelAwal = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in semuaPartikelAwal)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void OnMouseDown()
    {
        // Cegah klik jika sedang terbang ATAU sedang animasi pop
        if (!sudahDiklik && !sedangTransisi)
        {
            sedangTransisi = true;
            
            // Panggil urutan animasi Pop, JANGAN langsung terbang
            StartCoroutine(AnimasiPop());
        }
    }

    // --- FASE TRANSISI BARU ---
    private IEnumerator AnimasiPop()
    {
        // 1. FASE ITEM MENCIUT
        float waktu = 0f;
        while (waktu < durasiMenciut)
        {
            waktu += Time.deltaTime;
            float persentase = waktu / durasiMenciut;
            // Gunakan SmoothStep agar gerakannya melambat di akhir (elegan)
            float smooth = Mathf.SmoothStep(0f, 1f, persentase); 
            
            if (gambarItem != null)
                gambarItem.transform.localScale = Vector3.Lerp(skalaAwalItem, Vector3.zero, smooth);
            
            yield return null;
        }

        if (gambarItem != null)
        {
            gambarItem.transform.localScale = Vector3.zero;
            gambarItem.enabled = false; 
        }

        // 2. FASE CAHAYA MUNCUL (Disiapkan dari ukuran 0)
        if (vfxCahayaTerbang != null)
        {
            vfxCahayaTerbang.transform.localScale = Vector3.zero;
            
            // Nyalakan partikel (Sama persis dengan sistem yang sudah berhasil)
            ParticleSystem[] semuaPartikel = GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in semuaPartikel)
            {
                ps.Clear();
                ps.Play();
            }
        }

        // 3. FASE CAHAYA MEMBESAR (POP OUT)
        waktu = 0f;
        while (waktu < durasiMembesar)
        {
            waktu += Time.deltaTime;
            float persentase = waktu / durasiMembesar;
            float smooth = Mathf.SmoothStep(0f, 1f, persentase);
            
            if (vfxCahayaTerbang != null)
                vfxCahayaTerbang.transform.localScale = Vector3.Lerp(Vector3.zero, skalaAwalVFX, smooth);
            
            yield return null;
        }

        if (vfxCahayaTerbang != null) vfxCahayaTerbang.transform.localScale = skalaAwalVFX;

        // 4. TRANSISI SELESAI -> MULAI TERBANG
        waktuBerjalan = 0f;
        
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear(); 
            trail.emitting = true; 
        }

        // Mengaktifkan sistem Update() di bawah untuk mengeksekusi penerbangan
        sudahDiklik = true; 
    }

    void Update()
    {
        // LOGIKA TERBANG INI 100% TIDAK DIUBAH (Sesuai kode terakhir yang sukses)
        if (sudahDiklik && targetPosisiBar != null)
        {
            waktuBerjalan += Time.deltaTime;
            float persentaseWaktu = waktuBerjalan / durasiTerbang; 
            
            float persentaseMentok = Mathf.Clamp01(persentaseWaktu);
            float persentaseRitme = ritmeTerbang.Evaluate(persentaseMentok); 

            Vector3 titikTengah = posisiAwal + (targetPosisiBar.position - posisiAwal) / 2f;
            Vector3 titikKontrol = titikTengah + new Vector3(geserSamping, tinggiLengkungan, 0);

            Vector3 l1 = Vector3.Lerp(posisiAwal, titikKontrol, persentaseRitme);
            Vector3 l2 = Vector3.Lerp(titikKontrol, targetPosisiBar.position, persentaseRitme); 
            
            transform.position = Vector3.Lerp(l1, l2, persentaseRitme);

            if (persentaseWaktu >= 1f)
            {
                if (scriptBar != null) scriptBar.TambahProgress(jumlahProgressYangDitambah);

                ParticleSystem[] semuaPartikelSelesai = GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in semuaPartikelSelesai)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                TrailRenderer trail = GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    trail.emitting = false; 
                    trail.Clear();
                }

                transform.position = posisiAwal;
                
                // --- PENGEMBALIAN SKALA SAAT RESET ---
                if (gambarItem != null) 
                {
                    gambarItem.transform.localScale = skalaAwalItem; // Kembalikan ukuran item
                    gambarItem.enabled = true;
                }
                if (vfxCahayaTerbang != null)
                {
                    vfxCahayaTerbang.transform.localScale = skalaAwalVFX; // Kembalikan ukuran cahaya
                }

                sudahDiklik = false; 
                sedangTransisi = false; // Buka kunci klik untuk tes berikutnya
            }
        }
    }
}