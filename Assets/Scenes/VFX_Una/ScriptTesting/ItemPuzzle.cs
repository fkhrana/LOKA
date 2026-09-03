using UnityEngine;
using System.Collections;

public class ItemPuzzle : MonoBehaviour
{
    [Header("Pengaturan Puzzle")]
    public KotakPuzzle kotakTargetBenar; 
    public float jarakToleransi = 1f; 

    [Header("Kecepatan Animasi")]
    public float kecepatanMeluncur = 0.3f; 

    private Vector3 posisiAwal;
    private bool sedangDitarik = false;
    private Vector3 offsetMouse;

    void Start()
    {
        posisiAwal = transform.position;
    }

    void OnMouseDown()
    {
        sedangDitarik = true;
        offsetMouse = transform.position - DapatkanPosisiMouse();
    }

    void OnMouseDrag()
    {
        if (sedangDitarik)
        {
            transform.position = DapatkanPosisiMouse() + offsetMouse;
        }
    }

    void OnMouseUp()
    {
        sedangDitarik = false;

        KotakPuzzle[] semuaKotak = FindObjectsByType<KotakPuzzle>(FindObjectsSortMode.None);
        KotakPuzzle kotakTerdekat = null;
        float jarakMin = float.MaxValue;

        foreach (KotakPuzzle kotak in semuaKotak)
        {
            float jarak = Vector3.Distance(transform.position, kotak.transform.position);
            if (jarak < jarakToleransi && jarak < jarakMin)
            {
                jarakMin = jarak;
                kotakTerdekat = kotak;
            }
        }

        if (kotakTerdekat != null)
        {
            if (kotakTerdekat == kotakTargetBenar && !kotakTerdekat.sudahTerisi)
            {
                // JIKA BENAR: Snap ke kotak, panggil VFX, mainkan Bounce, lalu matikan script
                transform.position = kotakTerdekat.transform.position;
                kotakTerdekat.ReaksiBenar();
                
                // Mainkan animasi juicy bounce langsung dari kode!
                StartCoroutine(AnimasiBounce());
                
                Collider2D kolider = GetComponent<Collider2D>();
                if (kolider != null) kolider.enabled = false;
                
                // Kita delay sedikit disable scriptnya agar coroutine bounce sempat selesai
                Invoke("MatikanScript", 0.5f); 
            }
            else
            {
                kotakTerdekat.ReaksiSalah();
                StartCoroutine(MeluncurKembali());
            }
        }
        else
        {
            StartCoroutine(MeluncurKembali());
        }
    }

    private void MatikanScript()
    {
        this.enabled = false;
    }

    // --- ANIMASI JUICY BOUNCE (BARU) ---
    private IEnumerator AnimasiBounce()
    {
        Vector3 skalaAwal = transform.localScale;
        Vector3 skalaMembesar = skalaAwal * 1.3f; // Membesar 30%
        float durasi = 0.15f;
        float waktu = 0f;

        // Fase 1: Membesar dengan cepat
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaAwal, skalaMembesar, waktu / durasi);
            yield return null;
        }

        // Fase 2: Kembali normal (membentuk efek pantulan)
        waktu = 0f;
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaMembesar, skalaAwal, waktu / durasi);
            yield return null;
        }
        transform.localScale = skalaAwal;
    }
    // -----------------------------------

    private IEnumerator MeluncurKembali()
    {
        Vector3 posisiSekarang = transform.position;
        float waktu = 0f;

        while (waktu < kecepatanMeluncur)
        {
            waktu += Time.deltaTime;
            float persentase = waktu / kecepatanMeluncur;
            
            float ritme = Mathf.SmoothStep(0f, 1f, persentase);
            transform.position = Vector3.Lerp(posisiSekarang, posisiAwal, ritme);
            yield return null;
        }
        
        transform.position = posisiAwal;
    }

    private Vector3 DapatkanPosisiMouse()
    {
        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePoint.z = 0f; 
        return mousePoint;
    }
}