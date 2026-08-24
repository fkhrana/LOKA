using UnityEngine;

public class AmbilItem : MonoBehaviour
{
    [Header("Pengaturan Target")]
    public Transform targetTas; 
    public GameObject vfxTas;   

    [Header("Pengaturan Kecepatan")]
    public float kecepatanTerbang = 5f;
    public float kecepatanPutar = 1000f;

    private bool sudahDiklik = false;
    private Vector3 posisiAwal;
    private Vector3 skalaAwal;
    private Quaternion rotasiAwal;

    void Start()
    {
        posisiAwal = transform.position;
        skalaAwal = transform.localScale;
        rotasiAwal = transform.rotation;
    }

   void OnMouseDown()
    {
        if (!sudahDiklik)
        {
            sudahDiklik = true;

            StopAllCoroutines(); 

            // --- SOLUSI 2: HAPUS VFX DROP SAAT DIKLIK ---
            AnimasiDrop dropScript = GetComponent<AnimasiDrop>();
            if (dropScript != null)
            {
                dropScript.MatikanVfxDrop(); // Panggil fungsi pembunuh VFX
            }
            // ---------------------------------------------

            TrailRenderer trail = GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.Clear();         
                trail.emitting = true; 
            }
            
            // Nyalakan lagi serbuk sparkle saat terbang
            ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
            if (ps != null) { ps.Play(); }
        }
    }

    void Update()
    {
        if (sudahDiklik && targetTas != null)
        {
            transform.Rotate(0, 0, kecepatanPutar * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetTas.position, kecepatanTerbang * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, skalaAwal * 0.4f, kecepatanTerbang * Time.deltaTime);

            float jarak = Vector3.Distance(transform.position, targetTas.position);
            if (jarak < 0.5f)
            {
               // 1. Munculkan Ledakan Tas
                if (vfxTas != null)
                {
                    // Kita masukkan kembali ke dalam targetTas agar TIDAK tertimpa layar UI
                    GameObject efekTas = Instantiate(vfxTas, targetTas.position, vfxTas.transform.rotation, targetTas);
                    
                    // Kita PAKSA ukurannya 1 (normal) agar bebas dari kutukan skala gepeng UI
                    efekTas.transform.localScale = Vector3.one;

                    // Paksa partikel langsung menyala (Jaga-jaga mengatasi bug Play On Awake)
                    ParticleSystem partikelTas = efekTas.GetComponent<ParticleSystem>();
                    if (partikelTas != null)
                    {
                        partikelTas.Play();
                    }
                }

                // 2. Bersihkan debu-debu partikel di udara
                ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    ps.Stop();  
                    ps.Clear(); 
                    ps.Play();  
                }

                // 3. Matikan dan bersihkan ekor komet
                TrailRenderer trail = GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    trail.emitting = false; 
                    trail.Clear();
                }

                // 4. Suruh item lompat ulang dari awal
                AnimasiDrop dropScript = GetComponent<AnimasiDrop>();
                if (dropScript != null)
                {
                    dropScript.MulaiLompat(); 
                }
                else
                {
                    transform.position = posisiAwal;
                }

                // 5. Kembalikan ukuran dan rotasi asli
                transform.localScale = skalaAwal;
                transform.rotation = rotasiAwal;
                sudahDiklik = false; 
            }
        }
    }
}