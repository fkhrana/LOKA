using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(LineRenderer))]
public class GestureDrawer : MonoBehaviour
{
    public event Action<List<Vector2>, GestureRecognitionResult> GestureRecognized;

    public LineRenderer lineRenderer;
    public float minPointDistance = 0.05f;
    public float firstStrokeGracePeriod = 0.35f;

    private readonly List<Vector3> currentStrokePoints = new List<Vector3>();
    private readonly List<List<Vector2>> completedStrokes = new List<List<Vector2>>();
    private readonly List<LineRenderer> completedStrokeRenderers = new List<LineRenderer>();
    private bool isDrawing;
    private bool isAwaitingNextStroke;
    private float pendingRecognitionTime;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("GestureDrawer membutuhkan Main Camera di scene.");
        }

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.widthCurve = AnimationCurve.Constant(0, 1, 0.1f);
        lineRenderer.numCapVertices = 8;
        lineRenderer.numCornerVertices = 8;
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;

        if (lineRenderer.material == null)
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void Update()
    {
        if (IsEscapePressed())
        {
            ResetGesture();
            return;
        }

        if ((IsConfirmPressed()) && completedStrokes.Count > 0 && !isDrawing)
        {
            FinalizeRecognition();
            return;
        }

        if (IsPointerDown())
        {
            StartStroke();
        }
        else if (IsPointerHeld() && isDrawing)
        {
            UpdateStroke();
        }
        else if (IsPointerUp() && isDrawing)
        {
            EndStroke();
        }

        if (isAwaitingNextStroke && !isDrawing && Time.unscaledTime >= pendingRecognitionTime)
        {
            FinalizeRecognition();
            isAwaitingNextStroke = false;
            pendingRecognitionTime = 0f;
        }
    }

    private bool IsPointerDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : false;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private bool IsPointerHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.leftButton.isPressed : false;
#else
        return Input.GetMouseButton(0);
#endif
    }

    private bool IsPointerUp()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.leftButton.wasReleasedThisFrame : false;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }

    private bool IsEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.escapeKey.isPressed);
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private bool IsConfirmPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
            return false;

        return Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private void StartStroke()
    {
        if (isDrawing)
            return;

        if (isAwaitingNextStroke)
        {
            isAwaitingNextStroke = false;
            pendingRecognitionTime = 0f;
        }

        ClearCurrentStrokePreview();
        isDrawing = true;
        AddPoint(GetMouseWorldPosition());
    }

    private void UpdateStroke()
    {
        var position = GetMouseWorldPosition();
        if (currentStrokePoints.Count == 0 || Vector3.Distance(currentStrokePoints[currentStrokePoints.Count - 1], position) > minPointDistance)
        {
            AddPoint(position);
        }
    }

    private void EndStroke()
    {
        isDrawing = false;
        if (currentStrokePoints.Count < 2)
        {
            Debug.Log("Stroke terlalu pendek untuk dikenali.");
            return;
        }

        var points2D = new List<Vector2>(currentStrokePoints.Count);
        foreach (var p in currentStrokePoints)
        {
            points2D.Add(new Vector2(p.x, p.y));
        }

        completedStrokes.Add(points2D);
        PersistStroke(currentStrokePoints);

        if (completedStrokes.Count == 1)
        {
            isAwaitingNextStroke = true;
            pendingRecognitionTime = Time.unscaledTime + firstStrokeGracePeriod;
            return;
        }

        if (completedStrokes.Count >= 2)
        {
            if (GestureRecognizer.Instance != null)
            {
                GestureRecognizer.Instance.Recognize(completedStrokes);
            }

            FinalizeRecognition();
            return;
        }

        isAwaitingNextStroke = false;
        pendingRecognitionTime = 0f;
    }

    private void FinalizeRecognition()
    {
        if (completedStrokes.Count == 0 || isDrawing)
            return;

        isAwaitingNextStroke = false;
        pendingRecognitionTime = 0f;

        GestureRecognitionResult result = default;
        if (GestureRecognizer.Instance != null)
        {
            result = GestureRecognizer.Instance.Recognize(completedStrokes);
            var flattenedPoints = new List<Vector2>();
            foreach (var stroke in completedStrokes)
            {
                flattenedPoints.AddRange(stroke);
            }

            GestureRecognized?.Invoke(flattenedPoints, result);
        }
        else
        {
            Debug.LogWarning("GestureRecognizer belum tersedia di scene.");
        }

        ClearRecognizedStrokes();
        ClearCurrentStrokePreview();
        isAwaitingNextStroke = false;
        pendingRecognitionTime = 0f;
    }

    private void PersistStroke(List<Vector3> points)
    {
        if (points == null || points.Count == 0)
            return;

        var renderer = new GameObject("GestureStroke").AddComponent<LineRenderer>();
        renderer.transform.SetParent(transform, false);
        renderer.positionCount = points.Count;
        renderer.useWorldSpace = true;
        renderer.loop = false;
        renderer.widthCurve = AnimationCurve.Constant(0, 1, 0.08f);
        renderer.numCapVertices = 8;
        renderer.numCornerVertices = 8;
        renderer.startColor = Color.cyan;
        renderer.endColor = Color.cyan;
        renderer.material = lineRenderer.material != null ? lineRenderer.material : new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < points.Count; i++)
        {
            renderer.SetPosition(i, points[i]);
        }

        completedStrokeRenderers.Add(renderer);
    }

    private void AddPoint(Vector3 point)
    {
        currentStrokePoints.Add(point);
        lineRenderer.positionCount = currentStrokePoints.Count;
        lineRenderer.SetPosition(currentStrokePoints.Count - 1, point);
    }

    private void ClearCurrentStrokePreview()
    {
        currentStrokePoints.Clear();
        lineRenderer.positionCount = 0;
    }

    private void ClearRecognizedStrokes()
    {
        completedStrokes.Clear();

        foreach (var renderer in completedStrokeRenderers)
        {
            if (renderer != null)
                Destroy(renderer.gameObject);
        }

        completedStrokeRenderers.Clear();
    }

    private void ResetGesture()
    {
        isDrawing = false;
        isAwaitingNextStroke = false;
        pendingRecognitionTime = 0f;
        ClearCurrentStrokePreview();
        ClearRecognizedStrokes();
    }

    private Vector3 GetMouseWorldPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            var pos2 = Mouse.current.position.ReadValue();
            var screenPos = new Vector3(pos2.x, pos2.y, -mainCamera.transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPos);
        }
#endif
        var legacyPos = Input.mousePosition;
        legacyPos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(legacyPos);
    }
}
