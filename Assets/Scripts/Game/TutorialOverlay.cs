using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Komponen reusable untuk overlay gelap dengan lubang di target.
/// Dipakai oleh PowerUpTutorialManager dan BossLevelPowerUpTutorial.
/// </summary>
public class TutorialOverlay : MonoBehaviour
{
    [Header("Canvas")]
    [Tooltip("Kosongkan untuk auto-cari via nama.")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private string canvasName = "Canvas_Tutorial";

    [Header("Appearance")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.86f);
    [SerializeField, Min(0f)] private float padding = 60f;

    [Header("Cover Fade")]
    [SerializeField, Min(0f)] private float coverFadeOutDuration = 0.4f;

    // ---- Runtime ----
    private GameObject overlayRoot;
    private GameObject cover;
    private GameObject top, bottom, left, right;
    private CanvasGroup coverCanvasGroup;
    private RectTransform currentTarget;

    public bool IsBuilt => overlayRoot != null;

    // =====================================================
    // BUILD
    // =====================================================
    public void Build()
    {
        if (IsBuilt) return;

        Canvas canvas = targetCanvas != null ? targetCanvas : FindCanvas();
        if (canvas == null)
        {
            Debug.LogError("[TutorialOverlay] Canvas tidak ditemukan.");
            return;
        }

        overlayRoot = CreateEmptyRect("TutOverlay_Root", canvas.transform);
        top    = CreateImage("TutOverlay_Top",    overlayRoot.transform);
        bottom = CreateImage("TutOverlay_Bottom", overlayRoot.transform);
        left   = CreateImage("TutOverlay_Left",   overlayRoot.transform);
        right  = CreateImage("TutOverlay_Right",  overlayRoot.transform);
        cover  = CreateImage("TutOverlay_Cover",  overlayRoot.transform);

        StretchFull(cover.GetComponent<RectTransform>());

        coverCanvasGroup = cover.AddComponent<CanvasGroup>();
        coverCanvasGroup.alpha = 1f;

        overlayRoot.SetActive(false);
        cover.SetActive(false);
    }

    // =====================================================
    // SHOW / HIDE
    // =====================================================
    public void ShowCover()
    {
        if (cover == null) return;

        cover.SetActive(true);
        if (coverCanvasGroup != null) coverCanvasGroup.alpha = 1f;
        if (overlayRoot != null) overlayRoot.SetActive(false);
    }

    public void ShowOverlay(RectTransform target)
    {
        if (overlayRoot == null || target == null) return;

        currentTarget = target;

        Canvas.ForceUpdateCanvases();
        RefreshHole(target);

        if (cover != null) cover.SetActive(false);
        overlayRoot.SetActive(true);
    }

    public void HideAll()
    {
        if (overlayRoot != null) overlayRoot.SetActive(false);
        if (cover != null) cover.SetActive(false);
    }

    public void RefreshHoleIfActive()
    {
        if (overlayRoot == null || !overlayRoot.activeSelf) return;
        if (currentTarget == null) return;

        RefreshHole(currentTarget);
    }

    public IEnumerator FadeOutCoverRoutine()
    {
        if (cover == null) yield break;

        float t = 0f;
        while (t < coverFadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            if (coverCanvasGroup != null)
                coverCanvasGroup.alpha = 1f - Mathf.Clamp01(t / coverFadeOutDuration);
            yield return null;
        }

        if (coverCanvasGroup != null) coverCanvasGroup.alpha = 0f;
        cover.SetActive(false);
    }

    // =====================================================
    // HOLE CALCULATION
    // =====================================================
    private void RefreshHole(RectTransform target)
    {
        if (overlayRoot == null || target == null) return;
        if (top == null || bottom == null || left == null || right == null) return;

        RectTransform panel = overlayRoot.GetComponent<RectTransform>();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Canvas canvas = panel.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera : null;

        Vector2 bl = WorldToLocal(panel, cam, corners[0]);
        Vector2 tr = WorldToLocal(panel, cam, corners[2]);

        float holeL = bl.x - padding;
        float holeR = tr.x + padding;
        float holeB = bl.y - padding;
        float holeT = tr.y + padding;

        float w = panel.rect.width;
        float h = panel.rect.height;
        float cL = -w * 0.5f, cR = w * 0.5f;
        float cB = -h * 0.5f, cT = h * 0.5f;

        SetStretch(top.GetComponent<RectTransform>(),    1f, 1f, cT - holeT);
        SetStretch(bottom.GetComponent<RectTransform>(), 0f, 0f, holeB - cB);
        SetSide(left.GetComponent<RectTransform>(),  0f, 0f, holeL - cL, holeT, holeB);
        SetSide(right.GetComponent<RectTransform>(), 1f, 1f, cR - holeR, holeT, holeB);
    }

    // =====================================================
    // HELPERS
    // =====================================================
    private static void SetStretch(RectTransform rt, float anchorY, float pivotY, float height)
    {
        rt.anchorMin = new Vector2(0f, anchorY);
        rt.anchorMax = new Vector2(1f, anchorY);
        rt.pivot = new Vector2(0.5f, pivotY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, height);
        rt.anchoredPosition = Vector2.zero;
    }

    private static void SetSide(RectTransform rt, float anchorX, float pivotX,
        float width, float topY, float bottomY)
    {
        float centerY = (topY + bottomY) * 0.5f;
        rt.anchorMin = new Vector2(anchorX, 0.5f);
        rt.anchorMax = new Vector2(anchorX, 0.5f);
        rt.pivot = new Vector2(pivotX, 0.5f);
        rt.sizeDelta = new Vector2(width, topY - bottomY);
        rt.anchoredPosition = new Vector2(0f, centerY);
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private GameObject CreateEmptyRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        StretchFull(go.GetComponent<RectTransform>());
        return go;
    }

    private GameObject CreateImage(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = overlayColor;
        img.raycastTarget = false;
        return go;
    }

    private Canvas FindCanvas()
    {
        if (!string.IsNullOrEmpty(canvasName))
        {
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c != null && c.gameObject.name == canvasName) return c;
        }

        return FindFirstObjectByType<Canvas>();
    }

    private static Vector2 WorldToLocal(RectTransform parent, Camera cam, Vector3 world)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, cam, out Vector2 local);
        return local;
    }
}