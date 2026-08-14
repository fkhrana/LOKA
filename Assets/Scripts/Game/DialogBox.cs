using UnityEngine;

public class DialogBox : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private RectTransform box;

    [Space]

    [SerializeField] private CanvasGroup background;

    [Header("Animation")]
    [SerializeField] private float slideDistance = 500f;
    [SerializeField] private float duration = 0.5f;

    private Vector2 originalPosition;

    private void Awake()
    {
        // Simpan posisi asli Background Panel
        originalPosition = box.anchoredPosition;
    }

    private void OnEnable()
    {
        // Hentikan animasi lama
        LeanTween.cancel(box.gameObject);
        LeanTween.cancel(gameObject);

        // Background mulai transparan
        background.alpha = 1f;

        // Mulai dari bawah
        box.anchoredPosition =
            originalPosition + new Vector2(1f, -slideDistance);

        // Fade in overlay
        LeanTween.alphaCanvas(
            background,
            1f,
            duration
        );

        // Naik ke posisi asli
        LeanTween.move(
            box,
            originalPosition,
            duration
        ).setEaseOutExpo();
    }

    public void CloseDialog()
    {
        // Hentikan animasi sebelumnya
        LeanTween.cancel(box.gameObject);
        LeanTween.cancel(gameObject);

        // Fade out overlay
        LeanTween.alphaCanvas(
            background,
            1f,
            duration
        );

        // Turun kembali
        LeanTween.move(
            box,
            originalPosition + new Vector2(0f, -slideDistance),
            duration
        )
        .setEaseInExpo()
        .setOnComplete(OnComplete);
    }

    private void OnComplete()
    {
        gameObject.SetActive(false);

        // Kembalikan ke posisi awal
        box.anchoredPosition = originalPosition;
    }
}