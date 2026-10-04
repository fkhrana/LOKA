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
    private Vector3 cameraStartPosition;
    private bool cameraStartCaptured = false;
    private bool isCountdownActive = false;
    private Coroutine introCoroutine;
    private bool introCancelled = false;
    public bool IsCountdownActive => isCountdownActive;

    // ⬇️ TAMBAH — buat Fix 2 (blok koleksi selama pan)
    public bool IsIntroRunning { get; private set; }

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
        introCancelled = false;
        DisableGesture();

        // ⬇️ TAMBAH — bersihkan flag training basi dari scene sebelumnya,
        // kecuali ada tutorial yang memang sedang aktif di scene ini.
        if (!PowerUpTutorialManager.IsPowerUpTutorial &&
            !BossLevelPowerUpTutorial.IsBossLevelTutorial &&
            !GameProgressManager.IsGuidedTutorialActive)
        {
            TutorialManager.IsTrainingMode = false;
        }
        // ⬆️ SAMPAI SINI

        // ✅ Simpan posisi awal kamera
        if (mainCamera != null && !cameraStartCaptured)
        {
            cameraStartPosition = mainCamera.transform.position;
            cameraStartCaptured = true;
            Debug.Log($"[CameraIntroManager] Posisi awal kamera disimpan: {cameraStartPosition}");
        }

        if (PowerUpTutorialManager.IsPowerUpTutorial || BossLevelPowerUpTutorial.IsBossLevelTutorial)
        {
            Debug.Log("[CameraIntroManager] Tutorial power-up aktif → intro ditunda.");
            return;
        }

        if (GameProgressManager.IsGuidedTutorialActive)
        {
            Debug.Log("[CameraIntroManager] Guided tutorial aktif → intro ditunda.");
            return;
        }

        if (!GameProgressManager.IsGuidedTutorialCompleted())
        {
            Debug.Log("[CameraIntroManager] Guided tutorial belum selesai → intro ditunda.");
            return;
        }

        string savedState = GameProgressManager.GetGameState();
        Debug.Log($"[CameraIntroManager] Start() dipanggil, savedState='{savedState}'");

        if (savedState == "Puzzle" || savedState == "Reward")
        {
            Debug.Log($"[CameraIntroManager] Resume {savedState} → langsung lanjut.");
            GameStarted = true;
            EnableGesture();
            if (countdownImage != null) countdownImage.gameObject.SetActive(false);
            return;
        }

        if (savedState == "Gameplay")
        {
            Debug.Log("[CameraIntroManager] Resume Gameplay → countdown saja.");
            introCoroutine = StartCoroutine(MainkanIntro(withPanning: false));
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("[CameraIntroManager] Main Camera belum diisi! Fallback countdown saja.");
            introCoroutine = StartCoroutine(MainkanIntro(withPanning: false));
            return;
        }

        if (targetKanan == null)
        {
            Debug.LogError("[CameraIntroManager] Target Kanan belum diisi! Fallback countdown saja.");
            introCoroutine = StartCoroutine(MainkanIntro(withPanning: false));
            return;
        }

        Debug.Log("[CameraIntroManager] Panning + countdown.");
        introCoroutine = StartCoroutine(MainkanIntro(withPanning: true));
    }

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
        if (gestureDrawer != null) gestureDrawer.enabled = true;
    }

    public void StartIntroAfterTutorial(System.Action onCompleted)
    {
        Debug.Log("[CameraIntroManager] StartIntroAfterTutorial dipanggil.");

        introCancelled = false;
        GameStarted = false;
        isCountdownActive = false;
        DisableGesture();

        // ✅ Reset kamera ke posisi awal
        if (mainCamera != null && cameraStartCaptured)
        {
            mainCamera.transform.position = cameraStartPosition;
            Debug.Log($"[CameraIntroManager] Kamera direset ke: {cameraStartPosition}");
        }

        if (countdownImage != null)
            countdownImage.gameObject.SetActive(false);

        bool withPanning = mainCamera != null && targetKanan != null;
        Debug.Log($"[CameraIntroManager] withPanning={withPanning}, mainCamera={(mainCamera != null)}, targetKanan={(targetKanan != null)}");

        introCoroutine = StartCoroutine(MainkanIntro(withPanning, onCompleted));
    }

    // ✅ Stop + reset kamera
    public void StopCountdown()
    {
        Debug.Log("[CameraIntroManager] ⏹️ StopCountdown() dipanggil.");

        introCancelled = true;
        StopAllCoroutines();
        introCoroutine = null;

        // ✅ Reset kamera ke posisi awal
        if (mainCamera != null && cameraStartCaptured)
        {
            mainCamera.transform.position = cameraStartPosition;
            Debug.Log($"[CameraIntroManager] Kamera dikembalikan ke: {cameraStartPosition}");
        }

        if (countdownImage != null)
            countdownImage.gameObject.SetActive(false);

        isCountdownActive = false;
        GameStarted = false;
        IsIntroRunning = false;   // ⬅️ TAMBAH
        DisableGesture();

        Debug.Log("[CameraIntroManager] ⏹️ Semua intro/panning/countdown di-stop + kamera direset.");
    }

    private IEnumerator MainkanIntro(bool withPanning, System.Action onCompleted = null)
    {
        IsIntroRunning = true;   // ⬅️ TAMBAH
        introCancelled = false;
        DisableGesture();

        try
        {
            if (withPanning)
            {
                posisiKiri = mainCamera.transform.position;
                posisiKanan = new Vector3(targetKanan.position.x, posisiKiri.y, posisiKiri.z);

                yield return new WaitForSeconds(jedaAwal);
                if (introCancelled) yield break;

                Debug.Log("Intro: Kamera menuju musuh...");
                yield return StartCoroutine(GerakkanKamera(posisiKiri, posisiKanan, durasiPan));
                if (introCancelled) yield break;

                Debug.Log("Intro: Melihat musuh...");
                yield return new WaitForSeconds(jedaLihatMusuh);
                if (introCancelled) yield break;

                Debug.Log("Intro: Kamera kembali ke tengah...");
                yield return StartCoroutine(GerakkanKamera(posisiKanan, posisiKiri, durasiPan));
                if (introCancelled) yield break;
            }
            else
            {
                yield return new WaitForSeconds(jedaSebelumCountdown);
                if (introCancelled) yield break;
            }

            if (countdownImage == null)
            {
                Debug.LogWarning("[CameraIntroManager] Countdown Image kosong. Langsung mulai.");
                GameStarted = true;
                EnableGesture();
                onCompleted?.Invoke();
                yield break;
            }

            GameStarted = false;
            countdownImage.gameObject.SetActive(true);
            isCountdownActive = true;

            if (AudioManager.Instance != null && countdownSFX != null)
                AudioManager.Instance.PlaySFX(countdownSFX);

            yield return StartCoroutine(TampilkanEfekPopUp(gambar3));
            if (introCancelled) yield break;

            yield return StartCoroutine(TampilkanEfekPopUp(gambar2));
            if (introCancelled) yield break;

            yield return StartCoroutine(TampilkanEfekPopUp(gambar1));
            if (introCancelled) yield break;

            yield return StartCoroutine(TampilkanEfekPopUp(gambarMulai));
            if (introCancelled) yield break;

            countdownImage.gameObject.SetActive(false);
            isCountdownActive = false;

            GameStarted = true;
            EnableGesture();

            Debug.Log("GAME DIMULAI! GameStarted=" + GameStarted);
            onCompleted?.Invoke();
        }
        finally
        {
            IsIntroRunning = false;   // ⬅️ TAMBAH — selalu ke-reset, apapun yang terjadi
        }
    }

    private IEnumerator GerakkanKamera(Vector3 posisiAwal, Vector3 posisiAkhir, float durasi)
    {
        float waktu = 0f;

        while (waktu < durasi)
        {
            if (introCancelled) yield break;

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
            if (introCancelled) yield break;
            float skala = Mathf.Lerp(0f, 1.2f, waktu / durasiMembesar);
            countdownImage.transform.localScale = Vector3.one * skala;
            waktu += Time.deltaTime;
            yield return null;
        }

        waktu = 0f;
        float durasiMantul = 0.1f;

        while (waktu < durasiMantul)
        {
            if (introCancelled) yield break;
            float skala = Mathf.Lerp(1.2f, 1f, waktu / durasiMantul);
            countdownImage.transform.localScale = Vector3.one * skala;
            waktu += Time.deltaTime;
            yield return null;
        }

        countdownImage.transform.localScale = Vector3.one;

        float t = 0f;
        while (t < 0.7f)
        {
            if (introCancelled) yield break;
            t += Time.deltaTime;
            yield return null;
        }
    }

    public static void ResetIntroFlag()
    {
        Debug.Log("[CameraIntroManager] ResetIntroFlag (no-op).");
    }
}