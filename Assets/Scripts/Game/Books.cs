using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BookOpenSlideAnimator : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private RectTransform hadiahPage;
    [SerializeField] private RectTransform aksaraPage;
    [SerializeField] private CanvasGroup aksaraCanvasGroup;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private Button triggerButton;

    [Header("Target Positions (anchoredPosition)")]
    [SerializeField] private Vector2 centerPos = Vector2.zero;
    [SerializeField] private Vector2 hadiahOpenPos = new Vector2(-260f, 0f);
    [SerializeField] private Vector2 aksaraOpenPos = new Vector2(260f, 0f);

    [Header("Hadiah Popup")]
    [SerializeField] private float popupDuration = 0.25f;
    [SerializeField] private float popupStartScale = 0.85f;
    [SerializeField] private float popupOvershootScale = 1.08f;
    [SerializeField] private float popupReturnDuration = 0.08f;

    [SerializeField] private AnimationCurve popupCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Book Opening")]
    [SerializeField] private float slideDuration = 0.5f;

    // Sedikit rotasi supaya terasa seperti halaman buku
    [SerializeField] private float hadiahOpenRotation = 3f;
    [SerializeField] private float aksaraOpenRotation = -3f;

    // Scale saat buku mulai membuka
    [SerializeField] private float openStartScale = 1f;
    [SerializeField] private float openMidScale = 0.97f;
    [SerializeField] private float openFinalScale = 1f;

    // Overshoot kecil di akhir
    [SerializeField] private float openOvershoot = 1.02f;

    [Header("Buttons Fade")]
    [SerializeField] private float buttonsFadeDuration = 0.3f;

    [SerializeField] private AnimationCurve easeCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isOpened;

    private void Awake()
    {
        triggerButton.onClick.AddListener(OnHadiahClicked);
    }

    private void OnEnable()
    {
        StopAllCoroutines();

        // =====================================================
        // HADIAH - POSISI AWAL
        // =====================================================

        hadiahPage.anchoredPosition = centerPos;

        // Mulai kecil untuk efek popup
        hadiahPage.localScale =
            Vector3.one * popupStartScale;

        // Pastikan rotasi awal normal
        hadiahPage.localRotation =
            Quaternion.identity;


        // =====================================================
        // AKSARA - KONDISI AWAL
        // =====================================================

        aksaraPage.anchoredPosition = centerPos;

        aksaraPage.localScale =
            Vector3.one * 0.8f;

        aksaraPage.localRotation =
            Quaternion.identity;

        aksaraCanvasGroup.alpha = 0f;

        aksaraPage.gameObject.SetActive(false);


        // =====================================================
        // BUTTONS - KONDISI AWAL
        // =====================================================

        buttonsGroup.alpha = 0f;

        buttonsGroup.interactable = false;
        buttonsGroup.blocksRaycasts = false;

        buttonsGroup.gameObject.SetActive(false);


        isOpened = false;
        StartCoroutine(HadiahPopupRoutine());
    }


    // =========================================================
    // HADIAH POPUP ANIMATION
    // =========================================================

    private IEnumerator HadiahPopupRoutine()
    {
        float t = 0f;

        // -----------------------------------------------------
        // 0.85 → 1.08
        // -----------------------------------------------------

        while (t < popupDuration)
        {
            t += Time.unscaledDeltaTime;

            float p = popupCurve.Evaluate(
                Mathf.Clamp01(t / popupDuration)
            );

            float scale = Mathf.Lerp(
                popupStartScale,
                popupOvershootScale,
                p
            );

            hadiahPage.localScale =
                Vector3.one * scale;

            yield return null;
        }


        // Pastikan mencapai 1.08
        hadiahPage.localScale =
            Vector3.one * popupOvershootScale;


        // -----------------------------------------------------
        // 1.08 → 1.0
        // -----------------------------------------------------

        t = 0f;

        while (t < popupReturnDuration)
        {
            t += Time.unscaledDeltaTime;

            float p = Mathf.Clamp01(
                t / popupReturnDuration
            );

            float scale = Mathf.Lerp(
                popupOvershootScale,
                1f,
                p
            );

            hadiahPage.localScale =
                Vector3.one * scale;

            yield return null;
        }


        // Pastikan kembali ke ukuran normal
        hadiahPage.localScale = Vector3.one;
    }


    // =========================================================
    // HADIAH CLICK
    // =========================================================

    private void OnHadiahClicked()
    {
        if (isOpened)
            return;

        isOpened = true;

        // Cegah klik dobel
        triggerButton.interactable = false;
        triggerButton.gameObject.SetActive(false);

        StartCoroutine(OpenBookRoutine());
    }


    // =========================================================
    // OPEN BOOK
    // =========================================================

    private IEnumerator OpenBookRoutine()
    {
        // Aktifkan halaman Aksara
        aksaraPage.gameObject.SetActive(true);

        float t = 0f;

        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(t / slideDuration);

            float p =
                easeCurve.Evaluate(normalizedTime);


            // =================================================
            // HADIAH → KIRI
            // =================================================

            hadiahPage.anchoredPosition =
                Vector2.LerpUnclamped(
                    centerPos,
                    hadiahOpenPos,
                    p
                );


            // =================================================
            // AKSARA → KANAN
            // =================================================

            aksaraPage.anchoredPosition =
                Vector2.LerpUnclamped(
                    centerPos,
                    aksaraOpenPos,
                    p
                );


            // =================================================
            // SCALE BUKU
            // =================================================
            //
            // Awal     = 1.00
            // Tengah   = 0.97
            // Akhir    = 1.00
            //
            // Membuat buku terasa sedikit "menekan"
            // ketika mulai terbuka.
            // =================================================

            float bookScale;

            if (normalizedTime < 0.7f)
            {
                float scaleProgress =
                    normalizedTime / 0.7f;

                bookScale = Mathf.Lerp(
                    openStartScale,
                    openMidScale,
                    scaleProgress
                );
            }
            else
            {
                float scaleProgress =
                    (normalizedTime - 0.7f) / 0.3f;

                bookScale = Mathf.Lerp(
                    openMidScale,
                    openFinalScale,
                    scaleProgress
                );
            }


            hadiahPage.localScale =
                Vector3.one * bookScale;


            // Aksara sedikit membesar saat muncul
            aksaraPage.localScale =
                Vector3.one *
                Mathf.Lerp(
                    0.8f,
                    bookScale,
                    p
                );


            // =================================================
            // ROTASI HALAMAN
            // =================================================

            // Halaman hadiah sedikit miring ke kiri/kanan
            hadiahPage.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        0f,
                        hadiahOpenRotation,
                        p
                    )
                );


            // Halaman aksara berlawanan arah
            aksaraPage.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        0f,
                        aksaraOpenRotation,
                        p
                    )
                );


            // =================================================
            // AKSARA FADE IN
            // =================================================

            aksaraCanvasGroup.alpha = p;


            yield return null;
        }


        // =====================================================
        // SMALL OVERSHOOT
        // =====================================================

        float settleDuration = 0.08f;

        t = 0f;

        while (t < settleDuration)
        {
            t += Time.unscaledDeltaTime;

            float p =
                Mathf.Clamp01(t / settleDuration);

            float smoothP =
                Mathf.SmoothStep(0f, 1f, p);


            // Sedikit membesar → normal
            float scale =
                Mathf.Lerp(
                    openOvershoot,
                    1f,
                    smoothP
                );


            hadiahPage.localScale =
                Vector3.one * scale;

            aksaraPage.localScale =
                Vector3.one * scale;


            // Rotasi kembali ke normal
            hadiahPage.localRotation =
                Quaternion.Lerp(
                    Quaternion.Euler(
                        0f,
                        0f,
                        hadiahOpenRotation
                    ),
                    Quaternion.identity,
                    smoothP
                );

            aksaraPage.localRotation =
                Quaternion.Lerp(
                    Quaternion.Euler(
                        0f,
                        0f,
                        aksaraOpenRotation
                    ),
                    Quaternion.identity,
                    smoothP
                );


            yield return null;
        }


        // =====================================================
        // SNAP KE POSISI FINAL
        // =====================================================

        hadiahPage.anchoredPosition =
            hadiahOpenPos;

        aksaraPage.anchoredPosition =
            aksaraOpenPos;


        hadiahPage.localScale =
            Vector3.one;

        aksaraPage.localScale =
            Vector3.one;


        hadiahPage.localRotation =
            Quaternion.identity;

        aksaraPage.localRotation =
            Quaternion.identity;


        aksaraCanvasGroup.alpha = 1f;


        // =====================================================
        // FADE IN BUTTONS
        // =====================================================

        yield return StartCoroutine(
            FadeInButtons()
        );
    }


    // =========================================================
    // FADE IN BUTTONS
    // =========================================================

    private IEnumerator FadeInButtons()
    {
        buttonsGroup.gameObject.SetActive(true);

        float t = 0f;

        while (t < buttonsFadeDuration)
        {
            t += Time.unscaledDeltaTime;

            buttonsGroup.alpha =
                Mathf.Clamp01(
                    t / buttonsFadeDuration
                );

            yield return null;
        }


        // Pastikan final state
        buttonsGroup.alpha = 1f;

        buttonsGroup.interactable = true;

        buttonsGroup.blocksRaycasts = true;
    }
}