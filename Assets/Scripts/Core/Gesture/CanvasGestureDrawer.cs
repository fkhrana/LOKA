using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Gesture drawer khusus UI Canvas. Class ini tidak mengubah GestureDrawer yang
/// dipakai scene lain dan hanya aktif pada scene tempat component ini dipasang.
/// </summary>
public class CanvasGestureDrawer : MonoBehaviour
{
    public event Action<List<List<Vector2>>, GestureRecognitionResult> GestureRecognized;
    public event Action DrawingStarted;
    public event Action DrawingStopped;

    [Header("Drawing Area")]
    [SerializeField] private RectTransform drawingArea;
    [SerializeField] private Camera eventCamera;

    [Header("VFX Brush")]
    [SerializeField, Min(0.1f)] private float minPointDistance = 4f;
    [SerializeField] private GameObject brushPrefab;
    [SerializeField, Min(0f)] private float brushFadeDuration = 0.5f;

    [Header("Recognition")]
    [SerializeField, Min(0f)] private float firstStrokeGracePeriod = 0.35f;
    [SerializeField] private GestureShape expectedShape = GestureShape.Unknown;

    private readonly List<Vector2> currentStroke = new List<Vector2>();
    private readonly List<List<Vector2>> completedStrokes = new List<List<Vector2>>();
    private readonly List<GameObject> brushInstances = new List<GameObject>();

    private bool isDrawing;
    private bool isAwaitingNextStroke;
    private float pendingRecognitionTime;
    private GameObject activeBrush;

    public RectTransform DrawingArea
    {
        get { return drawingArea; }
        set { drawingArea = value; }
    }

    private void Awake()
    {
        if (drawingArea == null)
        {
            Debug.LogError("CanvasGestureDrawer membutuhkan Drawing Area RectTransform.", this);
            enabled = false;
            return;
        }

        Canvas canvas = drawingArea.GetComponentInParent<Canvas>();
        if (eventCamera == null && canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = canvas.worldCamera;

        if (brushPrefab == null)
            Debug.LogWarning("CanvasGestureDrawer tidak memiliki Brush Prefab. Gesture tetap direkam tanpa visual VFX.", this);
    }

    private void Update()
    {
        MaintainBrushTrails();

        if (WasCancelPressed())
        {
            ResetGesture();
            return;
        }

        if (WasConfirmPressed() && !isDrawing && completedStrokes.Count > 0)
        {
            FinalizeRecognition();
            return;
        }

        if (WasPointerDown())
        {
            StartStroke();
        }
        else if (WasPointerHeld() && isDrawing)
        {
            UpdateStroke();
        }
        else if (WasPointerUp() && isDrawing)
        {
            EndStroke();
        }

        if (isAwaitingNextStroke &&
            !isDrawing &&
            Time.unscaledTime >= pendingRecognitionTime)
        {
            FinalizeRecognition();
        }
    }

    public void SetExpectedShape(GestureShape shape)
    {
        expectedShape = shape;
        ResetGesture();
    }

    public void ResetGesture()
    {
        bool wasDrawing = isDrawing;
        isDrawing = false;
        isAwaitingNextStroke = false;
        pendingRecognitionTime = 0f;
        currentStroke.Clear();
        completedStrokes.Clear();
        ClearBrushes(false);

        if (wasDrawing)
            DrawingStopped?.Invoke();
    }

    private void StartStroke()
    {
        Vector2 screenPoint = GetPointerScreenPosition();
        if (isDrawing ||
            !RectTransformUtility.RectangleContainsScreenPoint(
                drawingArea,
                screenPoint,
                eventCamera) ||
            !TryGetClampedLocalPoint(screenPoint, out Vector2 localPoint))
            return;

        isAwaitingNextStroke = false;
        pendingRecognitionTime = 0f;
        currentStroke.Clear();

        isDrawing = true;
        DrawingStarted?.Invoke();
        AddPoint(localPoint);
    }

    private void UpdateStroke()
    {
        if (!TryGetClampedLocalPoint(GetPointerScreenPosition(), out Vector2 localPoint))
            return;

        if (currentStroke.Count == 0 ||
            Vector2.Distance(currentStroke[currentStroke.Count - 1], localPoint) >= minPointDistance)
        {
            AddPoint(localPoint);
        }
    }

    private void EndStroke()
    {
        isDrawing = false;
        DrawingStopped?.Invoke();
        activeBrush = null;

        if (currentStroke.Count < 2)
        {
            currentStroke.Clear();
            return;
        }

        completedStrokes.Add(new List<Vector2>(currentStroke));
        currentStroke.Clear();

        UpdateStrokeRecognitionState();
    }

    private void UpdateStrokeRecognitionState()
    {
        if (GestureRecognizer.Instance == null)
        {
            isAwaitingNextStroke = true;
            pendingRecognitionTime = Time.unscaledTime + firstStrokeGracePeriod;
            return;
        }

        bool hasMatchingTemplate =
            GestureRecognizer.Instance.HasTemplateForStrokeCount(
                expectedShape,
                completedStrokes.Count);
        bool hasLongerTemplate =
            GestureRecognizer.Instance.HasTemplateWithMoreStrokes(
                expectedShape,
                completedStrokes.Count);

        if (hasLongerTemplate)
        {
            isAwaitingNextStroke = true;
            pendingRecognitionTime = hasMatchingTemplate
                ? Time.unscaledTime + firstStrokeGracePeriod
                : float.PositiveInfinity;
            return;
        }

        if (hasMatchingTemplate)
        {
            FinalizeRecognition();
            return;
        }

        isAwaitingNextStroke = true;
        pendingRecognitionTime = Time.unscaledTime + firstStrokeGracePeriod;
    }

    private void FinalizeRecognition()
    {
        if (isDrawing || completedStrokes.Count == 0)
            return;

        isAwaitingNextStroke = false;
        pendingRecognitionTime = 0f;

        GestureRecognitionResult result = new GestureRecognitionResult(
            GestureShape.Unknown,
            false,
            GestureShape.Unknown,
            false,
            0f,
            completedStrokes.Count);

        if (GestureRecognizer.Instance != null)
        {
            result = GestureRecognizer.Instance.Recognize(
                completedStrokes,
                expectedShape);
        }
        else
        {
            Debug.LogWarning("CanvasGestureDrawer tidak menemukan GestureRecognizer di scene.", this);
        }

        var recognizedStrokes = new List<List<Vector2>>(completedStrokes.Count);
        foreach (List<Vector2> stroke in completedStrokes)
            recognizedStrokes.Add(new List<Vector2>(stroke));

        GestureRecognized?.Invoke(recognizedStrokes, result);

        ClearBrushes(true);
        completedStrokes.Clear();
    }

    private void AddPoint(Vector2 localPoint)
    {
        if (currentStroke.Count == 0)
            StartBrush(localPoint);

        currentStroke.Add(localPoint);

        if (activeBrush != null)
            activeBrush.transform.position = drawingArea.TransformPoint(localPoint);
    }

    private void StartBrush(Vector2 localPoint)
    {
        if (brushPrefab == null)
            return;

        activeBrush = Instantiate(brushPrefab);
        activeBrush.name = "CanvasGestureBrush";
        activeBrush.transform.position = drawingArea.TransformPoint(localPoint);

        TesKuas brushController = activeBrush.GetComponent<TesKuas>();
        if (brushController != null)
            brushController.enabled = false;

        TrailRenderer trail = activeBrush.GetComponent<TrailRenderer>();
        if (trail != null)
            trail.Clear();

        brushInstances.Add(activeBrush);
    }

    private bool TryGetClampedLocalPoint(Vector2 screenPoint, out Vector2 localPoint)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingArea,
                screenPoint,
                eventCamera,
                out localPoint))
        {
            return false;
        }

        Rect rect = drawingArea.rect;
        localPoint.x = Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax);
        localPoint.y = Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax);
        return true;
    }

    private void ClearBrushes(bool fade)
    {
        foreach (GameObject brush in brushInstances)
        {
            if (brush == null)
                continue;

            if (fade)
                StartCoroutine(FadeAndDestroyBrush(brush));
            else
                Destroy(brush);
        }

        brushInstances.Clear();
        activeBrush = null;
    }

    private void MaintainBrushTrails()
    {
        foreach (GameObject brush in brushInstances)
        {
            if (brush == null)
                continue;

            foreach (TrailRenderer trail in brush.GetComponentsInChildren<TrailRenderer>())
                trail.time += Time.unscaledDeltaTime;
        }
    }

    private System.Collections.IEnumerator FadeAndDestroyBrush(GameObject brush)
    {
        TrailRenderer[] trails = brush.GetComponentsInChildren<TrailRenderer>();
        GradientAlphaKey[][] originalAlphaKeys = new GradientAlphaKey[trails.Length][];

        for (int i = 0; i < trails.Length; i++)
            originalAlphaKeys[i] = trails[i].colorGradient.alphaKeys;

        float elapsed = 0f;
        while (elapsed < brushFadeDuration && brush != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float fade = brushFadeDuration > 0f
                ? Mathf.Clamp01(elapsed / brushFadeDuration)
                : 1f;

            for (int i = 0; i < trails.Length; i++)
            {
                if (trails[i] == null)
                    continue;

                Gradient gradient = trails[i].colorGradient;
                GradientAlphaKey[] alphaKeys =
                    (GradientAlphaKey[])originalAlphaKeys[i].Clone();

                for (int key = 0; key < alphaKeys.Length; key++)
                    alphaKeys[key].alpha *= 1f - fade;

                gradient.alphaKeys = alphaKeys;
                trails[i].colorGradient = gradient;
            }

            yield return null;
        }

        if (brush != null)
            Destroy(brush);
    }

    private Vector2 GetPointerScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif

        return Vector2.zero;
    }

    private bool WasPointerDown()
    {
#if ENABLE_INPUT_SYSTEM
        return (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame) ||
               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private bool WasPointerHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.isPressed) ||
               (Mouse.current != null && Mouse.current.leftButton.isPressed);
#else
        return Input.GetMouseButton(0);
#endif
    }

    private bool WasPointerUp()
    {
#if ENABLE_INPUT_SYSTEM
        return (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) ||
               (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame);
#else
        return Input.GetMouseButtonUp(0);
#endif
    }

    private bool WasCancelPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private bool WasConfirmPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private void OnDestroy()
    {
        ClearBrushes(false);
    }
}
