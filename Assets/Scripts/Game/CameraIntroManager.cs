using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CameraIntroManager : MonoBehaviour
{
    public static bool GameStarted = false;

    // Flag: apakah player sudah pernah lihat intro panning
    private const string KEY_HAS_SEEN_FIRST_INTRO = "HasSeenFirstGameplayIntro";

    [Header("Pengaturan Kamera")]
    public Camera mainCamera;
    public Transform targetKanan;
    public float jedaAwal = 0.5f;
    public float durasiPan = 2.5f;
    public float jedaLihatMusuh = 1.5f;

    [Tooltip("Jeda sebelum countdown saat mode countdown-only (tanpa panning).")]
    [SerializeField] private float jedaSebelumCountdown = 0.3f;

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

    // ========================================
    // LIFECYCLE
    // ========================================
    private void Start()
    {
        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        // Setup countdown image
        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
            countdownImage.preserveAspect = true;
        }

        GameStarted = false;
        DisableGesture();

        // ==== Resume Puzzle / Reward → langsung lanjut, tanpa countdown ====
        string savedState = GameProgressManager.GetGameState();

        if (savedState == "Puzzle" || savedState == "Reward")
        {
            Debug.Log($"[CameraIntroManager] Resume {savedState} → langsung lanjut, tanpa countdown.");

            GameStarted = true;
            EnableGesture();

            if (countdownImage != null)
                countdownImage.gameObject.SetActive(false);

            return;
        }

        // ==== Cek apakah player sudah pernah lihat intro panning ====
        bool hasSeenFirstIntro = PlayerPrefs.GetInt(KEY_HAS_SEEN_FIRST_INTRO, 0) == 1;

        if (hasSeenFirstIntro)
        {
            Debug.Log("[CameraIntroManager] Sudah pernah lihat intro → countdown saja.");
            StartCoroutine(MainkanIntro(withPanning: false));
            return;
        }

        // ==== Fresh start → cek setup panning ====
        if (mainCamera == null)
        {
            Debug.LogError("[CameraIntroManager] Main Camera belum diisi! Fallback countdown saja.");
            StartCoroutine(MainkanIntro(withPanning: false));
            return;
        }

        if (targetKanan == null)
        {
            Debug.LogError("[CameraIntroManager] Target Kanan belum diisi! Fallback countdown saja.");
            StartCoroutine(MainkanIntro(withPanning: false));
            return;
        }

        Debug.Log("[CameraIntroManager] Fresh start → panning + countdown.");
        StartCoroutine(MainkanIntro(withPanning: true));
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
            gestureDrawer.enabled = true;
    }

    // ========================================
    // INTRO SEQUENCE
    // ========================================
    private IEnumerator MainkanIntro(bool withPanning)
    {
        DisableGesture();

        // ==== Panning (hanya fresh start) ====
        if (withPanning)
        {
            posisiKiri = mainCamera.transform.position;
            posisiKanan = new Vector3(
                targetKanan.position.x,
                posisiKiri.y,
                posisiKiri.z
            );

            yield return new WaitForSeconds(jedaAwal);

            Debug.Log("Intro: Kamera menuju musuh...");
            yield return StartCoroutine(GerakkanKamera(posisiKiri, posisiKanan, durasiPan));

            Debug.Log("Intro: Melihat musuh...");
            yield return new WaitForSeconds(jedaLihatMusuh);

            Debug.Log("Intro: Kamera kembali ke tengah...");
            yield return StartCoroutine(GerakkanKamera(posisiKanan, posisiKiri, durasiPan));

            // Tandai intro panning sudah dilihat — tidak akan muncul lagi
            PlayerPrefs.SetInt(KEY_HAS_SEEN_FIRST_INTRO, 1);
            PlayerPrefs.Save();
        }
        else
        {
            // Mode countdown-only: kasih jeda singkat
            yield return new WaitForSeconds(jedaSebelumCountdown);
        }

        // ==== Countdown ====
        if (countdownImage == null)
        {
            Debug.LogWarning("[CameraIntroManager] Countdown Image kosong. Langsung mulai.");
            GameStarted = true;
            EnableGesture();
            yield break;
        }

        countdownImage.gameObject.SetActive(true);

        if (AudioManager.Instance != null && countdownSFX != null)
            AudioManager.Instance.PlaySFX(countdownSFX);

        yield return StartCoroutine(TampilkanEfekPopUp(gambar3));
        yield return StartCoroutine(TampilkanEfekPopUp(gambar2));
        yield return StartCoroutine(TampilkanEfekPopUp(gambar1));
        yield return StartCoroutine(TampilkanEfekPopUp(gambarMulai));

        countdownImage.gameObject.SetActive(false);

        // ==== Game dimulai ====
        GameStarted = true;
        EnableGesture();

        Debug.Log("GAME DIMULAI!");
    }

    // ========================================
    // GERAKKAN KAMERA
    // ========================================
    private IEnumerator GerakkanKamera(Vector3 posisiAwal, Vector3 posisiAkhir, float durasi)
    {
        float waktu = 0f;

        while (waktu < durasi)
        {
            float progress = waktu / durasi;
            mainCamera.transform.position = Vector3.Lerp(posisiAwal, posisiAkhir, progress);
            waktu += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = posisiAkhir;
    }

    // ========================================
    // COUNTDOWN POP UP
    // ========================================
    private IEnumerator TampilkanEfekPopUp(Sprite spriteAngka)
    {
        if (spriteAngka == null)
        {
            Debug.LogWarning("Sprite countdown belum diisi!");
            yield break;
        }

        countdownImage.sprite = spriteAngka;
        countdownImage.SetNativeSize();
        countdownImage.transform.localScale = Vector3.zero;

        float waktu = 0f;
        float durasiMembesar = 0.2f;

        while (waktu < durasiMembesar)
        {
            float skala = Mathf.Lerp(0f, 1.2f, waktu / durasiMembesar);
            countdownImage.transform.localScale = Vector3.one * skala;
            waktu += Time.deltaTime;
            yield return null;
        }

        waktu = 0f;
        float durasiMantul = 0.1f;

        while (waktu < durasiMantul)
        {
            float skala = Mathf.Lerp(1.2f, 1f, waktu / durasiMantul);
            countdownImage.transform.localScale = Vector3.one * skala;
            waktu += Time.deltaTime;
            yield return null;
        }

        countdownImage.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.7f);
    }

    // ========================================
    // PUBLIC — dipanggil LevelManager.ResetProgress()
    // ========================================
    public static void ResetIntroFlag()
    {
        PlayerPrefs.DeleteKey(KEY_HAS_SEEN_FIRST_INTRO);
        PlayerPrefs.Save();
        Debug.Log("[CameraIntroManager] Intro flag di-reset — next gameplay akan panning lagi.");
    }
}