using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CameraIntroManager : MonoBehaviour
{
    public static bool GameStarted = false;
    public static CameraIntroManager Instance { get; private set; }

    [Header("Pengaturan Kamera")]
    public Camera mainCamera;
    public Transform targetKanan;
    public float jedaAwal = 0.5f;
    public float durasiPan = 2.5f;
    public float jedaLihatMusuh = 1.5f;

    [Tooltip("Jeda sebelum countdown saat mode countdown-only.")]
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
    private bool isCountdownActive = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
            countdownImage.preserveAspect = true;
        }

        GameStarted = false;
        isCountdownActive = false;
        DisableGesture();

        string savedState = GameProgressManager.GetGameState();

        Debug.Log($"[CameraIntroManager] Start() dipanggil, savedState='{savedState}'");

        // 1. Resume Puzzle / Reward → skip total (langsung lanjut)
        if (savedState == "Puzzle" || savedState == "Reward")
        {
            Debug.Log($"[CameraIntroManager] Resume {savedState} → langsung lanjut.");
            GameStarted = true;
            EnableGesture();

            if (countdownImage != null)
                countdownImage.gameObject.SetActive(false);

            return;
        }

        // 2. Resume Gameplay → countdown saja (nggak panning)
        if (savedState == "Gameplay")
        {
            Debug.Log("[CameraIntroManager] Resume Gameplay → countdown saja.");
            StartCoroutine(MainkanIntro(withPanning: false));
            return;
        }

        // 3. Fallback check: kamera / target harus ada buat panning
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

        // 4. State kosong (fresh start / level baru) → panning + countdown
        Debug.Log("[CameraIntroManager] Panning + countdown.");
        StartCoroutine(MainkanIntro(withPanning: true));
    }

    // ============================================
    // PAUSE / RESUME UI SAAT PAUSE PANEL TERBUKA
    // ============================================

    public void SetIntroUIVisible(bool visible)
    {
        if (GameStarted) return;
        if (!isCountdownActive) return;
        if (countdownImage == null) return;

        countdownImage.gameObject.SetActive(visible);
    }

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

    private IEnumerator MainkanIntro(bool withPanning)
    {
        DisableGesture();

        if (withPanning)
        {
            posisiKiri = mainCamera.transform.position;
            posisiKanan = new Vector3(targetKanan.position.x, posisiKiri.y, posisiKiri.z);

            yield return new WaitForSeconds(jedaAwal);

            Debug.Log("Intro: Kamera menuju musuh...");
            yield return StartCoroutine(GerakkanKamera(posisiKiri, posisiKanan, durasiPan));

            Debug.Log("Intro: Melihat musuh...");
            yield return new WaitForSeconds(jedaLihatMusuh);

            Debug.Log("Intro: Kamera kembali ke tengah...");
            yield return StartCoroutine(GerakkanKamera(posisiKanan, posisiKiri, durasiPan));
        }
        else
        {
            yield return new WaitForSeconds(jedaSebelumCountdown);
        }

        if (countdownImage == null)
        {
            Debug.LogWarning("[CameraIntroManager] Countdown Image kosong. Langsung mulai.");
            GameStarted = true;
            EnableGesture();
            yield break;
        }

        countdownImage.gameObject.SetActive(true);
        isCountdownActive = true;

        if (AudioManager.Instance != null && countdownSFX != null)
            AudioManager.Instance.PlaySFX(countdownSFX);

        yield return StartCoroutine(TampilkanEfekPopUp(gambar3));
        yield return StartCoroutine(TampilkanEfekPopUp(gambar2));
        yield return StartCoroutine(TampilkanEfekPopUp(gambar1));
        yield return StartCoroutine(TampilkanEfekPopUp(gambarMulai));

        countdownImage.gameObject.SetActive(false);
        isCountdownActive = false;

        GameStarted = true;
        EnableGesture();

        Debug.Log("GAME DIMULAI! GameStarted=" + GameStarted);
    }

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

    public static void ResetIntroFlag()
    {
        Debug.Log("[CameraIntroManager] ResetIntroFlag (no-op).");
    }
}