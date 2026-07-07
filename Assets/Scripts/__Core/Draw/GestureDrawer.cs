using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(LineRenderer))]
public class GestureDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float minPointDistance = 0.05f;
    private readonly List<Vector3> drawPoints = new List<Vector3>();
    private bool isDrawing;
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
    }

    private bool IsPointerDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private bool IsPointerHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.leftButton.isPressed : Input.GetMouseButton(0);
#else
        return Input.GetMouseButton(0);
#endif
    }

    private bool IsPointerUp()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.leftButton.wasReleasedThisFrame : Input.GetMouseButtonUp(0);
#else
        return Input.GetMouseButtonUp(0);
#endif
    }

    private void StartStroke()
    {
        drawPoints.Clear();
        lineRenderer.positionCount = 0;
        isDrawing = true;
        AddPoint(GetMouseWorldPosition());
    }

    private void UpdateStroke()
    {
        var position = GetMouseWorldPosition();
        if (drawPoints.Count == 0 || Vector3.Distance(drawPoints[drawPoints.Count - 1], position) > minPointDistance)
        {
            AddPoint(position);
        }
    }

    private void EndStroke()
    {
        isDrawing = false;
        if (drawPoints.Count < 5)
        {
            Debug.Log("Gesture terlalu sedikit titik untuk dikenali.");
            return;
        }

        var points2D = new List<Vector2>(drawPoints.Count);
        foreach (var p in drawPoints)
        {
            points2D.Add(new Vector2(p.x, p.y));
        }

        if (GestureRecognizer.Instance != null)
        {
            GestureRecognizer.Instance.Recognize(points2D);
        }
        else
        {
            Debug.LogWarning("GestureRecognizer belum tersedia di scene.");
        }
    }

    private void AddPoint(Vector3 point)
    {
        drawPoints.Add(point);
        lineRenderer.positionCount = drawPoints.Count;
        lineRenderer.SetPosition(drawPoints.Count - 1, point);
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
