using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Card popup dengan animasi FLIP: saat huruf dipilih, kartu "diputar" dari sisi
// depan (Front Face, misal desain punggung kartu) ke sisi belakang (Back Face,
// berisi gambar huruf, nama, dan tombol suara). Suara diambil dari AksaraSoundLibrary
// (bukan dari AksaraData), karena AksaraData tidak boleh diubah.
public class AksaraCardPopup : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform cardTransform; // object yang diputar (biasanya = root atau child langsung)

    [Header("Front Face (sisi sebelum di-flip)")]
    [SerializeField] private GameObject frontFace;

    [Header("Back Face (sisi berisi konten huruf)")]
    [SerializeField] private GameObject backFace;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private AudioSource audioSource;

    [Header("Sound Data")]
    [SerializeField] private AksaraSoundLibrary soundLibrary;

    [Header("Flip Animation")]
    [SerializeField] private float flipDuration = 0.35f;

    private AksaraData currentData;
    private Coroutine flipCoroutine;

    private void Awake()
    {
        if (soundButton != null)
            soundButton.onClick.AddListener(PlayLetterSound);

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (root != null)
            root.SetActive(false);
    }

    public void Show(AksaraData data)
    {
        if (data == null)
            return;

        currentData = data;

        // Isi konten sisi belakang dulu, walau belum kelihatan
        if (cardImage != null)
        {
            cardImage.sprite = data.FragmentSprite != null ? data.FragmentSprite : data.IconSprite;
            cardImage.enabled = cardImage.sprite != null;
        }

        if (nameText != null)
            nameText.text = data.AksaraName;

        // Reset ke kondisi awal: sisi depan aktif, skala normal
        SetFace(showFront: true);
        if (cardTransform != null)
            cardTransform.localScale = Vector3.one;

        if (root != null)
            root.SetActive(true);

        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);
        flipCoroutine = StartCoroutine(FlipRoutine(toBack: true, hideAfter: false));
    }

    public void Hide()
    {
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);
        // Balik lagi ke depan sebelum ditutup, biar kelihatan seperti kartu dibalik ulang
        flipCoroutine = StartCoroutine(FlipRoutine(toBack: false, hideAfter: true));
    }

    private IEnumerator FlipRoutine(bool toBack, bool hideAfter)
    {
        if (cardTransform == null)
        {
            SetFace(showFront: !toBack);
            if (hideAfter && root != null)
                root.SetActive(false);
            yield break;
        }

        float half = flipDuration / 2f;
        float elapsed = 0f;

        // Tahap 1: pipihkan kartu (scale.x -> 0), simulasi kartu jadi tampak dari samping
        while (elapsed < half)
        {
            float t = Mathf.Clamp01(elapsed / half);
            float scaleX = Mathf.Lerp(1f, 0f, t);
            cardTransform.localScale = new Vector3(scaleX, 1f, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cardTransform.localScale = new Vector3(0f, 1f, 1f);

        // Tepat di titik pipih, tukar wajah kartu yang aktif
        SetFace(showFront: !toBack);

        // Tahap 2: lebarkan lagi kartu (scale.x -> 1), sisi baru mulai kelihatan
        elapsed = 0f;
        while (elapsed < half)
        {
            float t = Mathf.Clamp01(elapsed / half);
            float scaleX = Mathf.Lerp(0f, 1f, t);
            cardTransform.localScale = new Vector3(scaleX, 1f, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cardTransform.localScale = Vector3.one;

        if (hideAfter && root != null)
            root.SetActive(false);
    }

    private void SetFace(bool showFront)
    {
        if (frontFace != null)
            frontFace.SetActive(showFront);

        if (backFace != null)
            backFace.SetActive(!showFront);
    }

    private void PlayLetterSound()
    {
        if (currentData == null || audioSource == null || soundLibrary == null)
            return;

        AudioClip clip = soundLibrary.GetClip(currentData.GestureShape);

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[AksaraCardPopup] Tidak ada AudioClip terdaftar untuk {currentData.AksaraName}.");
        }
    }
}