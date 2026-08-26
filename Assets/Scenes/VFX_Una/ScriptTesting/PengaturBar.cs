using UnityEngine;
using UnityEngine.UI;

public class PengaturBar : MonoBehaviour
{
    [Header("Komponen UI")]
    public Image barAsli;
    public Image barGlow;
    public ParticleSystem vfxKembangApi;

    private float targetIsi = 0f;
    private float transparansiGlow = 0f;

    void Start()
    {
        // Mulai dari 0
        barAsli.fillAmount = 0f;
        barGlow.fillAmount = 0f;
        AturTransparansiGlow(0f);
    }

    // Fungsi ini akan dipanggil oleh item saat sampai
    public void TambahProgress(float jumlahNambah)
    {
        targetIsi += jumlahNambah;
        transparansiGlow = 1f; // Nyalakan Glow!
        
        if (vfxKembangApi != null) vfxKembangApi.Play(); // Ledakkan debu!
    }

    void Update()
    {
        // 1. Bar bergerak halus menuju target
        barAsli.fillAmount = Mathf.Lerp(barAsli.fillAmount, targetIsi, Time.deltaTime * 5f);
        
        // 2. Bar Glow selalu sama bentuk/isinya dengan Bar Asli
        barGlow.fillAmount = barAsli.fillAmount; 

        // 3. Efek Glow memudar pelan-pelan (fade out)
        if (transparansiGlow > 0)
        {
            transparansiGlow -= Time.deltaTime * 2f; // Kecepatan memudar
            AturTransparansiGlow(transparansiGlow);
        }
    }

    private void AturTransparansiGlow(float alpha)
    {
        Color warna = barGlow.color;
        warna.a = Mathf.Clamp01(alpha);
        barGlow.color = warna;
    }
}