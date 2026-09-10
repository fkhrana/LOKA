using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraIntroManager : MonoBehaviour
{
    public static bool GameStarted = false;

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

    [Header("SFX Countdown")]
    [SerializeField] private AudioClip countdownSFX;

    [Header("Gesture")]
    [SerializeField] private GestureDrawer gestureDrawer;

    private Vector3 posisiKiri;
    private Vector3 posisiKanan;

    private void Start()
    {
        // Cari GestureDrawer otomatis kalau belum diisi
        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        string savedState =
            GameProgressManager.GetGameState();

        // ========================================
        // RESUME PUZZLE / REWARD
        // ========================================

        if (
            savedState == "Puzzle" ||
            savedState == "Reward"
        )
        {
            Debug.Log(
                "[CameraIntroManager] Resume "
                + savedState
                + " → Intro dilewati."
            );

            GameStarted = true;

            // Gesture langsung aktif karena intro dilewati
            EnableGesture();

            // Pastikan countdown tidak muncul
            if (countdownImage != null)
                countdownImage.gameObject.SetActive(false);

            return;
        }

        // ========================================
        // GAME BARU
        // ========================================

        GameStarted = false;

        // Gesture tidak boleh digunakan saat intro
        DisableGesture();

        if (mainCamera == null)
        {
            Debug.LogError(
                "Main Camera belum diisi!"
            );
            return;
        }

        if (targetKanan == null)
        {
            Debug.LogError(
                "Target Kanan belum diisi!"
            );
            return;
        }

        if (countdownImage == null)
        {
            Debug.LogError(
                "Countdown Image belum diisi!"
            );
            return;
        }

        countdownImage.gameObject.SetActive(false);
        countdownImage.preserveAspect = true;

        posisiKiri =
            mainCamera.transform.position;

        posisiKanan = new Vector3(
            targetKanan.position.x,
            posisiKiri.y,
            posisiKiri.z
        );

        StartCoroutine(
            MainkanIntro()
        );
    }

    // ========================================
    // GESTURE
    // ========================================

    private void DisableGesture()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.ResetGestureInput();
            gestureDrawer.enabled = false;
        }
    }

    private void EnableGesture()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.enabled = true;
        }
    }

    // ========================================
    // INTRO
    // ========================================

    IEnumerator MainkanIntro()
    {
        // Gesture tetap mati selama seluruh intro
        DisableGesture();

        yield return new WaitForSeconds(
            jedaAwal
        );

        Debug.Log(
            "Intro: Kamera menuju musuh..."
        );

        yield return StartCoroutine(
            GerakkanKamera(
                posisiKiri,
                posisiKanan,
                durasiPan
            )
        );

        Debug.Log(
            "Intro: Melihat musuh..."
        );

        yield return new WaitForSeconds(
            jedaLihatMusuh
        );

        Debug.Log(
            "Intro: Kamera kembali ke tengah..."
        );

        yield return StartCoroutine(
            GerakkanKamera(
                posisiKanan,
                posisiKiri,
                durasiPan
            )
        );

        countdownImage.gameObject.SetActive(true);

        // ========================================
        // PLAY SFX COUNTDOWN SEKALI SAJA
        // ========================================

        if (
            AudioManager.Instance != null &&
            countdownSFX != null
        )
        {
            AudioManager.Instance.PlaySFX(
                countdownSFX
            );
        }

        // ========================================
        // COUNTDOWN
        // ========================================

        yield return StartCoroutine(
            TampilkanEfekPopUp(gambar3)
        );

        yield return StartCoroutine(
            TampilkanEfekPopUp(gambar2)
        );

        yield return StartCoroutine(
            TampilkanEfekPopUp(gambar1)
        );

        yield return StartCoroutine(
            TampilkanEfekPopUp(gambarMulai)
        );

        countdownImage.gameObject.SetActive(false);

        // ========================================
        // GAME DIMULAI
        // ========================================

        GameStarted = true;

        // Gesture baru boleh digunakan sekarang
        EnableGesture();

        Debug.Log(
            "GAME DIMULAI! Gesture aktif."
        );
    }

    // ========================================
    // GERAKKAN KAMERA
    // ========================================

    IEnumerator GerakkanKamera(
        Vector3 posisiAwal,
        Vector3 posisiAkhir,
        float durasi
    )
    {
        float waktu = 0f;

        Debug.Log(
            "Kamera bergerak dari X = "
            + posisiAwal.x
            + " ke X = "
            + posisiAkhir.x
        );

        while (waktu < durasi)
        {
            float progress =
                waktu / durasi;

            mainCamera.transform.position =
                Vector3.Lerp(
                    posisiAwal,
                    posisiAkhir,
                    progress
                );

            waktu += Time.deltaTime;

            yield return null;
        }

        mainCamera.transform.position =
            posisiAkhir;
    }

    // ========================================
    // COUNTDOWN POP UP
    // ========================================

    IEnumerator TampilkanEfekPopUp(
        Sprite spriteAngka
    )
    {
        if (spriteAngka == null)
        {
            Debug.LogWarning(
                "Sprite countdown belum diisi!"
            );

            yield break;
        }

        countdownImage.sprite =
            spriteAngka;

        countdownImage.SetNativeSize();

        countdownImage.transform.localScale =
            Vector3.zero;

        float waktu = 0f;

        float durasiMembesar = 0.2f;

        while (
            waktu < durasiMembesar
        )
        {
            float skala =
                Mathf.Lerp(
                    0f,
                    1.2f,
                    waktu / durasiMembesar
                );

            countdownImage.transform.localScale =
                Vector3.one * skala;

            waktu += Time.deltaTime;

            yield return null;
        }

        waktu = 0f;

        float durasiMantul = 0.1f;

        while (
            waktu < durasiMantul
        )
        {
            float skala =
                Mathf.Lerp(
                    1.2f,
                    1f,
                    waktu / durasiMantul
                );

            countdownImage.transform.localScale =
                Vector3.one * skala;

            waktu += Time.deltaTime;

            yield return null;
        }

        countdownImage.transform.localScale =
            Vector3.one;

        yield return new WaitForSeconds(
            0.7f
        );
    }
}