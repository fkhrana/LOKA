using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class Carousel : MonoBehaviour, IEndDragHandler
{
    [Header("Parts Setup")]
    [SerializeField] private List<CarouselEntry> entries = new List<CarouselEntry>();

    [Space]
    [SerializeField] private ScrollRect scrollRect;

    [Space]
    [SerializeField] private RectTransform contentBoxHorizontal;
    [SerializeField] private Image carouselEntryPrefab;
    private List<Image> _imagesForEntries = new List<Image>();

    [Space]
    [SerializeField] private Transform indicatorParent;
    [SerializeField] private CarouselIndicator indicatorPrefab;
    private List<CarouselIndicator> _indicators = new List<CarouselIndicator>();

    [Header("Video Setup")]
    [Tooltip("Satu VideoPlayer dipakai bergantian untuk semua slide.")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("Resolusi RenderTexture. Samakan rasionya dengan video (16:9).")]
    [SerializeField] private Vector2Int renderTextureSize = new Vector2Int(1280, 720);
    [SerializeField] private bool loopVideo = true;

    // Satu RawImage per slide (null kalau slide itu tidak punya video)
    private List<RawImage> _videoImages = new List<RawImage>();
    private RenderTexture _renderTexture;
    private Coroutine _videoCoroutine;
    private int _preparingIndex = -1;
    private int _activeVideoIndex = -1;
    private bool _initialized;

    [Header("Special Slide Setup")]
    [Tooltip("Prefab panel khusus untuk slide yang IsSpecialSlide = true. " +
             "Konten + tombol YA sudah ada di dalam prefab.")]
    [SerializeField] private GameObject specialSlidePrefab;

    private List<GameObject> _specialSlides = new List<GameObject>();

    [Header("Animation Setup")]
    [SerializeField, Range(0.25f, 1f)]
    private float duration = 0.5f;

    [SerializeField]
    private AnimationCurve easeCurve;

    [Header("Auto Scroll Setup")]
    [Tooltip("Sebaiknya OFF kalau slide berisi video, supaya video tidak terpotong.")]
    [SerializeField]
    private bool autoScroll = false;

    [SerializeField]
    private float autoScrollInterval = 5f;

    private float _autoScrollTimer;

    [Header("Info Setup")]
    [SerializeField]
    private CarouselTextBox textBoxController;

    private int _currentIndex = 0;
    private Coroutine _scrollCoroutine;

    private void Reset()
    {
        scrollRect = GetComponentInChildren<ScrollRect>();
        textBoxController = GetComponentInChildren<CarouselTextBox>();
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        SetupVideoPlayer();

        foreach (var entry in entries)
        {
            Image carouselEntry = Instantiate(
                carouselEntryPrefab,
                contentBoxHorizontal
            );

            carouselEntry.sprite = entry.EntryGraphic;
            _imagesForEntries.Add(carouselEntry);

            // RawImage untuk video (hanya kalau entry punya video)
            RawImage videoImage = null;
            if (videoPlayer != null && !string.IsNullOrEmpty(entry.VideoFileName))
                videoImage = CreateVideoImage(carouselEntry.transform);
            _videoImages.Add(videoImage);

            // Panel khusus (slide tanpa video)
            GameObject specialPanel = null;
            if (entry.IsSpecialSlide && specialSlidePrefab != null)
            {
                specialPanel = Instantiate(specialSlidePrefab, carouselEntry.transform);
                SetupSpecialSlide(specialPanel, entry, entries.IndexOf(entry));
            }
            _specialSlides.Add(specialPanel);

            var indicator = Instantiate(
                indicatorPrefab,
                indicatorParent
            );

            indicator.Initialize(
                () => ScrollToSpecificIndex(entries.IndexOf(entry))
            );

            _indicators.Add(indicator);
        }

        if (_indicators.Count > 0)
        {
            _indicators[0].Activate(0.1f);
        }

        _autoScrollTimer = autoScrollInterval;

        if (entries.Count > 0)
        {
            ApplyTextBoxForIndex(0, instant: true);
        }

        _initialized = true;
        PlayVideoForCurrent(0f);
    }

    // Panel tutorial dibuka lagi -> putar ulang video slide aktif
    private void OnEnable()
    {
        if (_initialized)
            PlayVideoForCurrent(0f);
    }

    // Panel ditutup -> stop video
    private void OnDisable()
    {
        StopVideo();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }

        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }

    // ---------------------------------------------------------------
    // SPECIAL SLIDE
    // ---------------------------------------------------------------

    private void SetupSpecialSlide(GameObject panel, CarouselEntry entry, int index)
    {
        // Konten TitleText & tombol YA sudah ada di prefab.
        // Tombol YA disambungkan via TutorialButtonBridge (di dalam prefab).

        // Cari bridge di prefab, sambungkan ke TutorialFlowController di scene
        var bridge = panel.GetComponentInChildren<TutorialButtonBridge>(true);

        if (bridge != null)
        {
            var flowController = FindFirstObjectByType<TutorialFlowController>();

            if (flowController != null)
            {
                bridge.SetFlowController(flowController);
                Debug.Log("[Carousel] TutorialButtonBridge → TutorialFlowController terhubung.");
            }
            else
            {
                Debug.LogWarning("[Carousel] TutorialFlowController tidak ditemukan di scene!");
            }
        }

        // Supaya drag carousel tetap tembus, kecuali komponen di dalam Button
        var graphics = panel.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
        {
            if (g.GetComponentInParent<Button>() != null) continue;
            g.raycastTarget = false;
        }

        // Sembunyikan dulu, nanti diaktifkan saat slide-nya aktif
        panel.SetActive(false);
    }

    // ---------------------------------------------------------------
    // VIDEO
    // ---------------------------------------------------------------

    private void SetupVideoPlayer()
    {
        if (videoPlayer == null)
            return;

        _renderTexture = new RenderTexture(
            renderTextureSize.x,
            renderTextureSize.y,
            0,
            RenderTextureFormat.ARGB32
        );
        _renderTexture.Create();

        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = _renderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        videoPlayer.isLooping = loopVideo;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;
    }

    private RawImage CreateVideoImage(Transform parent)
    {
        var go = new GameObject("VideoImage", typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var raw = go.GetComponent<RawImage>();
        raw.texture = _renderTexture;
        raw.raycastTarget = false; // supaya drag/swipe tetap tembus ke ScrollRect
        raw.enabled = false;       // muncul setelah video siap

        return raw;
    }

    private void PlayVideoForCurrent(float delay)
    {
        if (videoPlayer == null || _currentIndex < 0 || _currentIndex >= entries.Count)
            return;

        // Skip kalau slide khusus atau tidak punya video
        var entry = entries[_currentIndex];
        if (entry.IsSpecialSlide || string.IsNullOrEmpty(entry.VideoFileName))
        {
            StopVideo();
            return;
        }

        // Sudah memutar video slide ini (mis. snap balik setelah drag) -> jangan restart
        if (_activeVideoIndex == _currentIndex && videoPlayer.isPlaying)
            return;

        StopVideo();

        _videoCoroutine = StartCoroutine(PrepareVideoRoutine(_currentIndex, entry.VideoFileName, delay));
    }

    private IEnumerator PrepareVideoRoutine(int index, string file, float delay)
    {
        // Tunggu animasi geser selesai dulu. Realtime supaya jalan walau timeScale = 0
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        _preparingIndex = index;
        videoPlayer.url = Application.streamingAssetsPath + "/" + file;

        Debug.Log($"[Carousel] Video URL: {videoPlayer.url}");

        videoPlayer.Prepare();
        _videoCoroutine = null;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // Abaikan kalau user sudah pindah slide saat video masih loading
        if (_preparingIndex != _currentIndex)
            return;

        if (_currentIndex >= _videoImages.Count || _videoImages[_currentIndex] == null)
            return;

        _videoImages[_currentIndex].enabled = true;
        _activeVideoIndex = _currentIndex;
        vp.Play();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[Carousel] VideoPlayer error: {message}");
    }

    private void StopVideo()
    {
        if (_videoCoroutine != null)
        {
            StopCoroutine(_videoCoroutine);
            _videoCoroutine = null;
        }

        _preparingIndex = -1;
        _activeVideoIndex = -1;

        if (videoPlayer != null)
            videoPlayer.Stop();

        foreach (var raw in _videoImages)
        {
            if (raw != null)
                raw.enabled = false;
        }

        // Bersihkan sisa frame video sebelumnya
        if (_renderTexture != null)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = previous;
        }
    }

    // ---------------------------------------------------------------
    // TEXTBOX VISIBILITY
    // ---------------------------------------------------------------

    private void ApplyTextBoxForIndex(int index, bool instant)
    {
        if (textBoxController == null || entries.Count == 0)
            return;

        bool isSpecial = entries[index].IsSpecialSlide;

        if (isSpecial)
        {
            // Sembunyikan textbox di slide khusus
            textBoxController.gameObject.SetActive(false);
            return;
        }

        // Slide biasa -> tampilkan dan isi teksnya
        if (!textBoxController.gameObject.activeSelf)
            textBoxController.gameObject.SetActive(true);

        var headline = entries[index].Headline;
        var description = entries[index].Description;

        if (instant)
            textBoxController.SetTextWithoutFade(headline, description);
        else
            textBoxController.SetText(headline, description, duration);
    }

    // ---------------------------------------------------------------
    // CAROUSEL
    // ---------------------------------------------------------------

    private void ClearCurrentIndex()
    {
        if (_indicators.Count > 0)
        {
            _indicators[_currentIndex].Deactivate(duration);
        }
    }

    private void ScrollToSpecificIndex(int index)
    {
        ClearCurrentIndex();
        ScrollTo(index);
    }

    public void ScrollToNext()
    {
        if (_imagesForEntries.Count == 0)
            return;

        ClearCurrentIndex();

        _currentIndex = (_currentIndex + 1) % _imagesForEntries.Count;

        ScrollTo(_currentIndex);
    }

    public void ScrollToPrevious()
    {
        if (_imagesForEntries.Count == 0)
            return;

        ClearCurrentIndex();

        _currentIndex =
            (_currentIndex - 1 + _imagesForEntries.Count)
            % _imagesForEntries.Count;

        ScrollTo(_currentIndex);
    }

    private void ScrollTo(int index)
    {
        _currentIndex = index;
        _autoScrollTimer = autoScrollInterval;

        float targetHorizontalPosition = 0f;

        if (_imagesForEntries.Count > 1)
        {
            targetHorizontalPosition =
                (float)_currentIndex / (_imagesForEntries.Count - 1);
        }

        if (_scrollCoroutine != null)
        {
            StopCoroutine(_scrollCoroutine);
        }

        _scrollCoroutine = StartCoroutine(
            LerpToPos(targetHorizontalPosition)
        );

        // Update textbox (hide kalau slide khusus)
        ApplyTextBoxForIndex(_currentIndex, instant: false);

        // Aktifkan panel khusus hanya untuk slide yang sedang aktif
        for (int i = 0; i < _specialSlides.Count; i++)
        {
            if (_specialSlides[i] == null) continue;
            _specialSlides[i].SetActive(i == _currentIndex);
        }

        if (_indicators.Count > 0)
        {
            _indicators[_currentIndex].Activate(duration);
        }

        // Video slide baru diputar setelah animasi geser selesai
        PlayVideoForCurrent(duration);
    }

    private IEnumerator LerpToPos(float targetHorizontalPosition)
    {
        float elapsedTime = 0f;
        float initialPos = scrollRect.horizontalNormalizedPosition;

        if (duration > 0)
        {
            while (elapsedTime <= duration)
            {
                float easeValue =
                    easeCurve.Evaluate(elapsedTime / duration);

                float newPosition = Mathf.Lerp(
                    initialPos,
                    targetHorizontalPosition,
                    easeValue
                );

                scrollRect.horizontalNormalizedPosition = newPosition;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        scrollRect.horizontalNormalizedPosition =
            targetHorizontalPosition;
    }

    private void Update()
    {
        if (!autoScroll)
            return;

        _autoScrollTimer -= Time.unscaledDeltaTime;

        if (_autoScrollTimer <= 0)
        {
            ScrollToNext();
            _autoScrollTimer = autoScrollInterval;
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (data.delta.x != 0)
        {
            if (data.delta.x > 0)
            {
                ScrollToPrevious();
            }
            else
            {
                ScrollToNext();
            }
        }
        else
        {
            ScrollToSpecificIndex(_currentIndex);
        }
    }
}
