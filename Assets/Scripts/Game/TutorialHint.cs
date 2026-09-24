using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHintManager : MonoBehaviour
{
    #region Inspector Fields
    [Header("Circle Highlight")]
    [SerializeField] private GameObject circleHighlightObj;
    [SerializeField, Min(0.1f)] private float circlePulseSpeed = 3f;
    [SerializeField, Range(0f, 1f)] private float circlePulseAmount = 0.1f;
    [SerializeField, Min(0f)] private float circlePadding = 1.5f;

    [Header("Path / Aksara Dots")]
    [SerializeField] private RectTransform hintContainer;
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private Color dotColor = new Color(1f, 0.9f, 0.2f);
    [SerializeField, Min(1f)] private float dotSize = 18f;
    [SerializeField, Min(1f)] private float displaySize = 220f;
    [SerializeField, Min(0.01f)] private float dotSpacing = 0.1f;
    [SerializeField, Min(0.01f)] private float dotRevealDuration = 0.18f;
    [SerializeField, Min(0f)] private float dotStagger = 0.06f;
    [SerializeField, Min(0f)] private float holdAfterComplete = 0.9f;
    [SerializeField, Min(0f)] private float restartDelay = 0.3f;
    [SerializeField] private bool loopDots = true;

    [Header("Debug")]
    [SerializeField] private bool debugPosition = false;
    [SerializeField] private bool debugDots = true;
    #endregion

    #region Runtime State
    private readonly List<Image> spawnedDots = new List<Image>();
    private readonly List<Coroutine> popInCoroutines = new List<Coroutine>();
    private Coroutine dotsAnimRoutine;
    private Coroutine circlePulseRoutine;
    private Sprite cachedDotSprite;
    #endregion

    private void OnDisable()
    {
        StopCirclePulse();
        StopDotsAnimation();
        ClearDots();
    }

    #region Circle Highlight
    public void ShowCircleHighlight(IReadOnlyList<EnemyGestureCommand> enemies)
    {
        if (circleHighlightObj == null)
        {
            Debug.LogWarning("[TutorialHintManager] circleHighlightObj belum di-assign.");
            return;
        }
        if (enemies == null || enemies.Count == 0) return;

        if (!TryCalculateEnemyBounds(enemies, out Vector3 center, out float maxDistance))
            return;

        circleHighlightObj.SetActive(true);
        PositionAndResizeCircle(center, maxDistance + circlePadding);
        StartCirclePulse();
    }

    public void HideCircleHighlight()
    {
        if (circleHighlightObj != null)
            circleHighlightObj.SetActive(false);
        StopCirclePulse();
    }

    private bool TryCalculateEnemyBounds(
        IReadOnlyList<EnemyGestureCommand> enemies,
        out Vector3 center,
        out float maxDistance)
    {
        center = Vector3.zero;
        maxDistance = 0f;
        int validCount = 0;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;
            center += enemies[i].transform.position;
            validCount++;
        }

        if (validCount == 0) return false;
        center /= validCount;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;
            float distance = Vector3.Distance(center, enemies[i].transform.position);
            if (distance > maxDistance) maxDistance = distance;
        }

        return true;
    }

    private void PositionAndResizeCircle(Vector3 worldCenter, float worldRadius)
    {
        Canvas canvas = circleHighlightObj.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform circleRect = circleHighlightObj.GetComponent<RectTransform>();

        circleRect.anchorMin = new Vector2(0.5f, 0.5f);
        circleRect.anchorMax = new Vector2(0.5f, 0.5f);
        circleRect.pivot = new Vector2(0.5f, 0.5f);

        Camera worldCam = Camera.main;
        if (worldCam == null) return;

        Vector3 screenPos = worldCam.WorldToScreenPoint(worldCenter);

        Camera uiCam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera ||
            canvas.renderMode == RenderMode.WorldSpace)
        {
            uiCam = canvas.worldCamera != null ? canvas.worldCamera : worldCam;
        }

        Vector2 localPoint;
        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, uiCam, out localPoint);

        if (!ok) return;

        circleRect.anchoredPosition = localPoint;

        float screenRadius = worldRadius * (Screen.height / (worldCam.orthographicSize * 2f));
        circleRect.sizeDelta = new Vector2(screenRadius * 2f, screenRadius * 2f);
    }

    private void StartCirclePulse()
    {
        StopCirclePulse();
        circlePulseRoutine = StartCoroutine(PulseCircleRoutine());
    }

    private void StopCirclePulse()
    {
        if (circlePulseRoutine != null)
        {
            StopCoroutine(circlePulseRoutine);
            circlePulseRoutine = null;
        }
    }

    private IEnumerator PulseCircleRoutine()
    {
        RectTransform rt = circleHighlightObj.GetComponent<RectTransform>();
        while (true)
        {
            if (rt == null) yield break;

            float scale = 1f + Mathf.Sin(Time.unscaledTime * circlePulseSpeed) * circlePulseAmount;
            rt.localScale = Vector3.one * scale;
            yield return null;
        }
    }
    #endregion

    #region Path / Aksara
    /// <summary>
    /// Tampilkan dots berdasarkan AksaraData (wrapper lama — tetap kompatibel).
    /// </summary>
    public void ShowPath(AksaraData aksara)
    {
        if (aksara == null)
        {
            Debug.LogWarning("[TutorialHintManager] aksara null — skip dots.");
            return;
        }
        ShowPath(aksara.GestureShape);
    }

    /// <summary>
    /// Tampilkan dots berdasarkan GestureShape langsung (mis. Love).
    /// </summary>
    public void ShowPath(GestureShape shape)
    {
        if (hintContainer == null)
        {
            Debug.LogWarning("[TutorialHintManager] hintContainer belum di-assign.");
            return;
        }

        ForceActivateHierarchy(hintContainer);

        List<Vector2> path = GetHardcodedPath(shape);
        if (path == null || path.Count < 2)
        {
            Debug.LogWarning($"[TutorialHintManager] Path untuk {shape} kosong — dots tidak muncul.");
            return;
        }

        ClearDots();
        List<Vector2> sampledPoints = SamplePath(path, dotSpacing);
        float scale = displaySize / 1.2f;

        foreach (Vector2 point in sampledPoints)
            spawnedDots.Add(CreateDotAt(point * scale));

        StartDotsAnimation();

        if (debugDots)
        {
            Debug.Log($"[TutorialHintManager] Dots muncul: {spawnedDots.Count} titik, shape={shape}");
            Debug.Log($"[THM] container active={hintContainer.gameObject.activeInHierarchy}, " +
                      $"pos={hintContainer.position}, size={hintContainer.rect.size}, " +
                      $"lossyScale={hintContainer.lossyScale}");

            Canvas c = hintContainer.GetComponentInParent<Canvas>();
            Debug.Log($"[THM] canvas={(c != null ? c.name : "NULL")}, " +
                      $"mode={(c != null ? c.renderMode.ToString() : "-")}, " +
                      $"sortOrder={(c != null ? c.sortingOrder.ToString() : "-")}");

            if (spawnedDots.Count > 0 && spawnedDots[0] != null)
            {
                Debug.Log($"[THM] dot[0] worldPos={spawnedDots[0].rectTransform.position}, " +
                          $"localScale={spawnedDots[0].rectTransform.localScale}, " +
                          $"color={spawnedDots[0].color}, " +
                          $"sprite={(spawnedDots[0].sprite != null ? spawnedDots[0].sprite.name : "NULL")}");
            }
        }
    }

    public void HidePath()
    {
        StopDotsAnimation();
        ClearDots();
    }

    public void HideAll()
    {
        HideCircleHighlight();
        HidePath();
    }

    /// <summary>
    /// Aktifkan GameObject ini dan semua parent-nya, supaya UI benar-benar dirender.
    /// </summary>
    private void ForceActivateHierarchy(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                if (debugDots)
                    Debug.Log($"[THM] Mengaktifkan hierarchy: {current.name}");

                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
    }
    #endregion

    #region Dots
    private void StartDotsAnimation()
    {
        StopDotsAnimation();

        if (!isActiveAndEnabled)
        {
            Debug.LogError("[THM] Tidak bisa start coroutine — TutorialHintManager tidak aktif!");
            return;
        }

        dotsAnimRoutine = StartCoroutine(AnimateDotsRoutine());
    }

    private void StopDotsAnimation()
    {
        if (dotsAnimRoutine != null)
        {
            StopCoroutine(dotsAnimRoutine);
            dotsAnimRoutine = null;
        }

        for (int i = 0; i < popInCoroutines.Count; i++)
        {
            if (popInCoroutines[i] != null)
                StopCoroutine(popInCoroutines[i]);
        }
        popInCoroutines.Clear();
    }

    private void ClearDots()
    {
        StopDotsAnimation();

        for (int i = 0; i < spawnedDots.Count; i++)
        {
            if (spawnedDots[i] == null) continue;
            if (spawnedDots[i].gameObject != null)
                Destroy(spawnedDots[i].gameObject);
        }

        spawnedDots.Clear();
    }

    private Image CreateDotAt(Vector2 anchoredPos)
    {
        GameObject go = new GameObject("Dot", typeof(Image));
        go.transform.SetParent(hintContainer, false);

        Image img = go.GetComponent<Image>();
        img.sprite = GetOrCreateDotSprite();
        img.color = dotColor;
        img.raycastTarget = false;
        img.preserveAspect = true;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(dotSize, dotSize);
        rt.localScale = Vector3.zero;

        return img;
    }

    private IEnumerator AnimateDotsRoutine()
    {
        while (true)
        {
            ResetAllDotsScale();
            yield return null;

            for (int i = 0; i < spawnedDots.Count; i++)
            {
                if (spawnedDots[i] == null) continue;
                if (spawnedDots[i].rectTransform == null) continue;

                popInCoroutines.Add(StartCoroutine(PopInDot(spawnedDots[i].rectTransform)));
                yield return new WaitForSecondsRealtime(dotStagger);
            }

            yield return new WaitForSecondsRealtime(dotRevealDuration + holdAfterComplete);

            if (!loopDots) yield break;
            yield return new WaitForSecondsRealtime(restartDelay);
        }
    }

    private void ResetAllDotsScale()
    {
        for (int i = 0; i < spawnedDots.Count; i++)
        {
            if (spawnedDots[i] == null) continue;
            if (spawnedDots[i].rectTransform == null) continue;

            spawnedDots[i].rectTransform.localScale = Vector3.zero;
        }
    }

    private IEnumerator PopInDot(RectTransform rt)
    {
        if (rt == null) yield break;

        float t = 0f;
        while (t < dotRevealDuration)
        {
            if (rt == null) yield break;

            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / dotRevealDuration);
            rt.localScale = Vector3.one * EaseOutBack(progress);
            yield return null;
        }

        if (rt != null)
            rt.localScale = Vector3.one;
    }
    #endregion

    #region Path Data
    private List<Vector2> GetHardcodedPath(GestureShape shape)
    {
        // 1) Coba ambil dari TutorialLetterPaths
        List<Vector2> path = TutorialLetterPaths.GetPath(shape);
        if (path != null && path.Count >= 2)
            return path;

        // 2) Fallback hardcoded
        switch (shape)
        {
            case GestureShape.Na:
                return new List<Vector2> { new Vector2(-0.6f, 0f), new Vector2(0.6f, 0f) };

            case GestureShape.Ka:
                return new List<Vector2> {
                    new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f)
                };

            case GestureShape.Wa:
                return new List<Vector2> {
                    new Vector2(-0.6f, -0.6f), new Vector2(-0.4f, 0.6f),
                    new Vector2(0f, 0f), new Vector2(0.4f, 0.6f),
                    new Vector2(0.6f, -0.6f)
                };

            case GestureShape.La:
                return new List<Vector2> {
                    new Vector2(-0.5f, 0.6f), new Vector2(-0.5f, -0.6f),
                    new Vector2(0.6f, -0.6f)
                };

            case GestureShape.Da:
                return new List<Vector2> {
                    new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f)
                };

            // === FIX: Love → path hati (pakai template LOVE.txt yang sudah di-normalize) ===
            case GestureShape.Love:
                return GetLoveHeartPath();

            default:
                return new List<Vector2> { new Vector2(-0.6f, 0f), new Vector2(0.6f, 0f) };
        }
    }

    /// <summary>
    /// Path hati untuk gesture Love.
    /// Menggunakan kurva hati klasik lalu di-normalize ke bounding box ±0.6.
    /// </summary>
    private static List<Vector2> GetLoveHeartPath()
    {
        var heart = new List<Vector2>();
        const int steps = 64;

        // Parametric heart curve:
        // x = 16 sin³(t)
        // y = 13 cos(t) - 5 cos(2t) - 2 cos(3t) - cos(4t)
        for (int i = 0; i <= steps; i++)
        {
            float t = (i / (float)steps) * Mathf.PI * 2f;

            float sinT = Mathf.Sin(t);
            float x = 16f * sinT * sinT * sinT;
            float y = 13f * Mathf.Cos(t)
                    - 5f * Mathf.Cos(2f * t)
                    - 2f * Mathf.Cos(3f * t)
                    - Mathf.Cos(4f * t);

            heart.Add(new Vector2(x, y));
        }

        // Normalize ke bounding box ±0.6 supaya proporsional dengan displaySize
        return NormalizePath(heart, 0.6f);
    }

    private static List<Vector2> NormalizePath(List<Vector2> path, float halfExtent)
    {
        if (path == null || path.Count == 0) return path;

        Vector2 min = path[0];
        Vector2 max = path[0];

        for (int i = 1; i < path.Count; i++)
        {
            min = Vector2.Min(min, path[i]);
            max = Vector2.Max(max, path[i]);
        }

        Vector2 size = max - min;
        float maxDim = Mathf.Max(size.x, size.y);
        if (maxDim < 0.0001f) maxDim = 1f;

        Vector2 center = (min + max) * 0.5f;
        float scale = (halfExtent * 2f) / maxDim;

        var result = new List<Vector2>(path.Count);
        for (int i = 0; i < path.Count; i++)
            result.Add((path[i] - center) * scale);

        return result;
    }

    private List<Vector2> SamplePath(List<Vector2> path, float spacing)
    {
        var result = new List<Vector2>();
        float totalLength = 0f;

        for (int i = 1; i < path.Count; i++)
            totalLength += Vector2.Distance(path[i - 1], path[i]);

        if (totalLength < 0.0001f)
        {
            result.Add(path[0]);
            return result;
        }

        int dotCount = Mathf.Max(2, Mathf.FloorToInt(totalLength / spacing) + 1);
        float actualSpacing = totalLength / (dotCount - 1);
        result.Add(path[0]);

        float targetDistance = actualSpacing;
        float walked = 0f;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 a = path[i - 1];
            Vector2 b = path[i];
            float segmentLength = Vector2.Distance(a, b);

            if (segmentLength < 0.0001f)
                continue;

            while (targetDistance <= walked + segmentLength)
            {
                float t = (targetDistance - walked) / segmentLength;
                result.Add(Vector2.Lerp(a, b, t));
                targetDistance += actualSpacing;
            }
            walked += segmentLength;
        }

        return result;
    }
    #endregion

    #region Sprite Helpers
    private Sprite GetOrCreateDotSprite()
    {
        if (cachedDotSprite != null) return cachedDotSprite;
        cachedDotSprite = dotSprite != null ? dotSprite : CreateCircleSprite(64);
        return cachedDotSprite;
    }

    private static Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        float radius = size * 0.5f;
        float innerRadius = radius - 1.5f;
        Vector2 center = new Vector2(radius, radius);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center.x;
                float dy = y + 0.5f - center.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha;
                if (dist <= innerRadius) alpha = 1f;
                else if (dist >= radius) alpha = 0f;
                else alpha = 1f - ((dist - innerRadius) / (radius - innerRadius));

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
    #endregion

    #region Math Helpers
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p = t - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }
    #endregion
}