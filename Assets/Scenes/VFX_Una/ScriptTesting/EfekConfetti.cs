using UnityEngine;
using System.Collections;

public class EfekConfetti : MonoBehaviour
{
    public ParticleSystem vfxConfetti;

    [SerializeField] private float durasiFase = 0.15f;

    private Vector3 skalaAsli;
    private bool sudahDiinisialisasi;

    private void Awake()
    {
        Inisialisasi();
    }

    private void Inisialisasi()
    {
        if (sudahDiinisialisasi)
            return;

        skalaAsli = transform.localScale;
        transform.localScale = Vector3.zero;
        sudahDiinisialisasi = true;
    }

    public void MuntahkanConfetti()
    {
        Inisialisasi();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "[EfekConfetti] " +
                gameObject.name +
                " masih inactive di hierarchy. Coroutine tidak dijalankan."
            );

            return;
        }

        StopAllCoroutines();
        StartCoroutine(AnimasiMuntahJuicy());
    }

    private IEnumerator AnimasiMuntahJuicy()
    {
        float durasi = Mathf.Max(0.01f, durasiFase);

        Vector3 skalaKerut = new Vector3(
            skalaAsli.x * 1.5f,
            skalaAsli.y * 0.4f,
            1f
        );

        float waktu = 0f;

        while (waktu < durasi)
        {
            waktu += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                Vector3.zero,
                skalaKerut,
                waktu / durasi
            );

            yield return null;
        }

        Vector3 skalaPanjang = new Vector3(
            skalaAsli.x * 0.6f,
            skalaAsli.y * 1.4f,
            1f
        );

        waktu = 0f;

        while (waktu < durasi)
        {
            waktu += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                skalaKerut,
                skalaPanjang,
                waktu / durasi
            );

            yield return null;
        }

        waktu = 0f;

        while (waktu < durasi)
        {
            waktu += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                skalaPanjang,
                skalaAsli,
                waktu / durasi
            );

            yield return null;
        }

        transform.localScale = skalaAsli;

        if (vfxConfetti != null)
        {
            vfxConfetti.gameObject.SetActive(true);
            vfxConfetti.Play();
        }
    }
}