using UnityEngine;

public class AmbilItemCahaya : MonoBehaviour
{
    [Header("Pengaturan Target")]
    public Transform targetProgressBar; // Target ke UI Progress Bar
    public GameObject vfxProgressTerisi; // Efek meledak/feedback di UI Progress Bar

    [Header("Pengaturan Kecepatan")]
    public float kecepatanTerbang = 5f;

    [Header("Komponen Visual")]
    public SpriteRenderer gambarItem; // Masukkan SpriteRenderer item di sini
    public ParticleSystem vfxCahaya; // Partikel cahaya pengganti item

    private bool sudahDiklik = false;
    private Vector3 posisiAwal;
    private Vector3 skalaAwal;
    private TrailRenderer trail;

    void Start()
    {
        // Simpan data awal untuk keperluan testing (reset)
        posisiAwal = transform.position;
        skalaAwal = transform.localScale;
        
        trail = GetComponent<TrailRenderer>();
        
        // Pastikan saat mulai, trail dan cahaya mati, gambar item nyala
        if (trail != null) trail.emitting = false;
        if (vfxCahaya != null) vfxCahaya.Stop();
    }

    void OnMouseDown()
    {
        if (!sudahDiklik)
        {
            sudahDiklik = true;

            // 1. Matikan gambar asli item agar seolah-olah "berubah" jadi cahaya
            if (gambarItem != null) gambarItem.enabled = false;

            // 2. Nyalakan efek cahaya
            if (vfxCahaya != null) vfxCahaya.Play();

            // 3. Nyalakan Ekor Komet (Trail)
            if (trail != null)
            {
                trail.Clear();         
                trail.emitting = true; 
            }
        }
    }

    void Update()
    {
        if (sudahDiklik && targetProgressBar != null)
        {
            // Terbang mendekati Progress Bar
            transform.position = Vector3.Lerp(transform.position, targetProgressBar.position, kecepatanTerbang * Time.deltaTime);
            
            // Mengecil saat terbang (seperti script sebelumnya)
            transform.localScale = Vector3.Lerp(transform.localScale, skalaAwal * 0.4f, kecepatanTerbang * Time.deltaTime);

            float jarak = Vector3.Distance(transform.position, targetProgressBar.position);
            
            // JIKA SUDAH SAMPAI DI PROGRESS BAR
            if (jarak < 0.5f)
            {
                // 1. Munculkan VFX di target UI
                if (vfxProgressTerisi != null)
                {
                    GameObject efekUi = Instantiate(vfxProgressTerisi, targetProgressBar.position, vfxProgressTerisi.transform.rotation, targetProgressBar);
                    efekUi.transform.localScale = Vector3.one;

                    ParticleSystem partikelUi = efekUi.GetComponent<ParticleSystem>();
                    if (partikelUi != null) partikelUi.Play();
                }

                // (Opsional) Jika ada script khusus di progress bar, panggil di sini
                // Contoh: targetProgressBar.GetComponent<ScriptProgressBar>().TambahProgress();

                // 2. RESET ITEM UNTUK TESTING
                ResetItem();
            }
        }
    }

    // Fungsi untuk mengembalikan item ke posisi statis awal
    private void ResetItem()
    {
        // Matikan efek cahaya & komet
        if (vfxCahaya != null) vfxCahaya.Stop();
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        // Kembalikan gambar asli
        if (gambarItem != null) gambarItem.enabled = true;

        // Kembalikan posisi dan ukuran
        transform.position = posisiAwal;
        transform.localScale = skalaAwal;

        sudahDiklik = false;
    }
}