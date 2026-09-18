using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ItemTester : MonoBehaviour, IPointerClickHandler
{
    [Header("Referensi Objek")]
    public GameObject vfxPrefab;          
    public Image cooldownOverlay;         
    
    // TAMBAHAN BARU: Referensi ke objek Player
    public Transform targetPlayer; 

    [Header("Pengaturan")]
    public float durasiCooldown = 5f;     
    private bool sedangCooldown = false;

    void Start()
    {
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0; 
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!sedangCooldown)
        {
            MulaiEfek();
        }
        else
        {
            Debug.Log("Gagal: Sabar, item sedang cooldown!");
        }
    }

    void MulaiEfek()
    {
        if (vfxPrefab != null)
        {
            // MENCARI POSISI PLAYER
            Vector3 posisiMuncul;

            // Cek apakah targetPlayer sudah dimasukkan di Inspector
            if (targetPlayer != null)
            {
                // Ambil kordinat Player, tapi paksa Z-nya ke -5 agar tidak tertutup background
                posisiMuncul = new Vector3(targetPlayer.position.x, targetPlayer.position.y, -5f);
            }
            else
            {
                // Kalau lupa memasukkan Player, pakai kode lamamu sebagai cadangan (di depan kamera)
                posisiMuncul = Camera.main.transform.position + Camera.main.transform.forward * 5f;
                Debug.LogWarning("Target Player belum dimasukkan! VFX muncul di depan kamera.");
            }

            GameObject vfxInstance = Instantiate(vfxPrefab, posisiMuncul, Quaternion.identity);

            ParticleSystem[] semuaPS = vfxInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in semuaPS)
            {
                ps.Play();
            }

            Debug.Log("VFX Berhasil di-spawn di posisi: " + posisiMuncul);
        }
        else
        {
            Debug.LogWarning("Peringatan: Kolom vfxPrefab masih kosong di Inspector!");
        }

        if (cooldownOverlay != null)
        {
            StartCoroutine(ProsesCooldown());
        }
    }

    IEnumerator ProsesCooldown()
    {
        sedangCooldown = true;
        float waktuBerjalan = 0;

        cooldownOverlay.fillAmount = 1;

        while (waktuBerjalan < durasiCooldown)
        {
            waktuBerjalan += Time.deltaTime;
            cooldownOverlay.fillAmount = 1 - (waktuBerjalan / durasiCooldown);
            yield return null;
        }

        cooldownOverlay.fillAmount = 0;
        sedangCooldown = false;
        
        Debug.Log("Cooldown selesai, item siap dipakai lagi!");
    }
}