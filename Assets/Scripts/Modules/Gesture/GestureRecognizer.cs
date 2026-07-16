using System;
using System.Collections.Generic;
using UnityEngine;

public class GestureRecognizer : MonoBehaviour
{
    public static GestureRecognizer Instance { get; private set; }

    public event Action<GestureRecognitionResult> GestureEvaluated;

    public int sampleCount = 64;
    public float squareSize = 250f;
    [Tooltip("Ambang maksimal jarak rata-rata; semakin rendah berarti gesture harus lebih mirip.")]
    public float maxAverageDistance = 45f;
    [Tooltip("Rasio minimum pemisahan antara hasil terbaik dan kedua terbaik agar bentuk dianggap jelas.")]
    public float recognitionMarginRatio = 0.90f;
    [Tooltip("Rasio jarak titik awal-akhir terhadap ukuran gesture. Makin kecil makin ketat untuk menolak garis terbuka.")]
    public float maxEndpointDistanceRatio = 0.18f;

    private readonly List<GestureTemplate> templates = new List<GestureTemplate>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildTemplates();
    }

    private void BuildTemplates()
    {
        templates.Clear();
        templates.Add(new GestureTemplate(GestureShape.Circle, GenerateCircleTemplate(sampleCount)));
        templates.Add(new GestureTemplate(GestureShape.Square, GenerateSquareTemplate(sampleCount)));
    }

    public GestureRecognitionResult Recognize(List<Vector2> rawPoints, GestureShape expectedShape = GestureShape.None)
    {
        if (rawPoints == null || rawPoints.Count == 0)
        {
            Debug.Log("Tidak ada gesture yang valid untuk dikenali.");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, 1);
        }

        var singleStroke = new List<List<Vector2>> { rawPoints };
        return Recognize(singleStroke, expectedShape);
    }

    public GestureRecognitionResult Recognize(List<List<Vector2>> strokes, GestureShape expectedShape = GestureShape.None)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.Log("Tidak ada stroke yang valid untuk dikenali.");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, 0);
        }

        var combinedPoints = new List<Vector2>();
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.Count < 2)
                continue;

            combinedPoints.AddRange(stroke);
        }

        if (combinedPoints.Count < 5)
        {
            Debug.Log("Tidak ada gesture yang valid untuk dikenali.");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, strokes.Count);
        }

        var candidate = ProcessPoints(combinedPoints);
        if (candidate == null)
        {
            Debug.Log("Gesture tidak valid setelah pemrosesan.");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, strokes.Count);
        }

        if (!HasClosedEnoughShape(combinedPoints))
        {
            Debug.Log("Gesture tidak dikenali. Stroke terlalu terbuka untuk circle/square.");
            var rejectedResult = new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, strokes.Count);
            GestureEvaluated?.Invoke(rejectedResult);
            return rejectedResult;
        }

        GestureShape bestShape = GestureShape.Unknown;
        float bestDistance = float.MaxValue;
        float secondBestDistance = float.MaxValue;

        float angleRange = Mathf.Deg2Rad * 45f;
        foreach (var template in templates)
        {
            float distance = DistanceAtBestAngle(candidate, template.Points, -angleRange, angleRange);
            if (distance < bestDistance)
            {
                secondBestDistance = bestDistance;
                bestDistance = distance;
                bestShape = template.Shape;
            }
            else if (distance < secondBestDistance)
            {
                secondBestDistance = distance;
            }
        }

        float score = bestDistance / candidate.Count;
        float secondAverageDistance = secondBestDistance == float.MaxValue ? float.MaxValue : secondBestDistance / candidate.Count;
        bool hasClearMatch = score <= maxAverageDistance &&
                             (secondBestDistance == float.MaxValue || score <= secondAverageDistance * recognitionMarginRatio || score <= 18f);
        bool isRecognized = hasClearMatch;
        bool matchesExpected = expectedShape == GestureShape.None || (isRecognized && bestShape == expectedShape);

        if (!isRecognized)
        {
            Debug.Log($"Tidak dikenali. Bentuk terlalu ambigu atau tidak jelas. strokeCount: {strokes.Count}, score: {score:F2}");
        }
        else if (expectedShape != GestureShape.None && bestShape != expectedShape)
        {
            Debug.Log($"Gesture salah. Diminta: {expectedShape}, terdeteksi: {bestShape}. strokeCount: {strokes.Count}, score: {score:F2}");
        }
        else
        {
            Debug.Log($"Gesture terdeteksi: {bestShape}. strokeCount: {strokes.Count}, score: {score:F2}");
        }

        var result = new GestureRecognitionResult(bestShape, isRecognized, expectedShape, matchesExpected, score, strokes.Count);
        GestureEvaluated?.Invoke(result);
        return result;
    }

    private List<Vector2> ProcessPoints(List<Vector2> points)
    {
        var resampled = Resample(points, sampleCount);
        resampled = ScaleToSquare(resampled, squareSize);
        resampled = TranslateToOrigin(resampled);
        return resampled;
    }

    private List<Vector2> GenerateCircleTemplate(int numPoints)
    {
        var points = new List<Vector2>(numPoints);
        for (int i = 0; i < numPoints; i++)
        {
            float angle = 2f * Mathf.PI * i / numPoints;
            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        }

        return ProcessPoints(points);
    }

    private List<Vector2> GenerateSquareTemplate(int numPoints)
    {
        var points = new List<Vector2>(numPoints);
        int side = Mathf.Max(1, numPoints / 4);

        for (int i = 0; i < side; i++)
            points.Add(new Vector2(-1f + 2f * i / Mathf.Max(1, side - 1), 1f));
        for (int i = 0; i < side; i++)
            points.Add(new Vector2(1f, 1f - 2f * i / Mathf.Max(1, side - 1)));
        for (int i = 0; i < side; i++)
            points.Add(new Vector2(1f - 2f * i / Mathf.Max(1, side - 1), -1f));
        for (int i = 0; i < side; i++)
            points.Add(new Vector2(-1f, -1f + 2f * i / Mathf.Max(1, side - 1)));

        return ProcessPoints(points);
    }

    private List<Vector2> Resample(List<Vector2> points, int n)
    {
        float pathLength = PathLength(points);
        float interval = pathLength / (n - 1);
        var newPoints = new List<Vector2> { points[0] };
        float distanceSoFar = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            float d = Vector2.Distance(points[i - 1], points[i]);
            if ((distanceSoFar + d) >= interval)
            {
                float t = (interval - distanceSoFar) / d;
                Vector2 newPoint = Vector2.Lerp(points[i - 1], points[i], t);
                newPoints.Add(newPoint);
                points.Insert(i, newPoint);
                distanceSoFar = 0f;
            }
            else
            {
                distanceSoFar += d;
            }
        }

        while (newPoints.Count < n)
            newPoints.Add(points[points.Count - 1]);

        return newPoints;
    }

    private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var p in points)
        {
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxX = Mathf.Max(maxX, p.x);
            maxY = Mathf.Max(maxY, p.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;
        float scale = Mathf.Max(width, height) > 0 ? size / Mathf.Max(width, height) : 1f;

        var newPoints = new List<Vector2>(points.Count);
        foreach (var p in points)
        {
            newPoints.Add(new Vector2(p.x * scale, p.y * scale));
        }

        return newPoints;
    }

    private List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        Vector2 centroid = Vector2.zero;
        foreach (var p in points)
        {
            centroid += p;
        }

        centroid /= points.Count;
        var newPoints = new List<Vector2>(points.Count);
        foreach (var p in points)
        {
            newPoints.Add(p - centroid);
        }

        return newPoints;
    }

    private float PathDistance(List<Vector2> a, List<Vector2> b)
    {
        float distance = 0f;
        int count = Mathf.Min(a.Count, b.Count);
        for (int i = 0; i < count; i++)
        {
            distance += Vector2.Distance(a[i], b[i]);
        }

        return count > 0 ? distance / count : float.MaxValue;
    }

    private List<Vector2> RotateBy(List<Vector2> points, float angle)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        var newPoints = new List<Vector2>(points.Count);
        foreach (var p in points)
        {
            newPoints.Add(new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos));
        }

        return newPoints;
    }

    private float DistanceAtAngle(List<Vector2> points, List<Vector2> template, float angle)
    {
        var rotated = RotateBy(points, angle);
        return PathDistance(rotated, template);
    }

    private float DistanceAtBestAngle(List<Vector2> points, List<Vector2> template, float a, float b, float threshold = 0.0174533f)
    {
        float phi = 0.5f * (-1f + Mathf.Sqrt(5f));
        float x1 = phi * a + (1 - phi) * b;
        float x2 = (1 - phi) * a + phi * b;
        float f1 = DistanceAtAngle(points, template, x1);
        float f2 = DistanceAtAngle(points, template, x2);

        while (Mathf.Abs(b - a) > threshold)
        {
            if (f1 < f2)
            {
                b = x2;
                x2 = x1;
                f2 = f1;
                x1 = phi * a + (1 - phi) * b;
                f1 = DistanceAtAngle(points, template, x1);
            }
            else
            {
                a = x1;
                x1 = x2;
                f1 = f2;
                x2 = (1 - phi) * a + phi * b;
                f2 = DistanceAtAngle(points, template, x2);
            }
        }

        return Mathf.Min(f1, f2);
    }

    private float PathLength(List<Vector2> points)
    {
        float length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            length += Vector2.Distance(points[i - 1], points[i]);
        }

        return length;
    }

    private bool HasClosedEnoughShape(List<Vector2> points)
    {
        if (points == null || points.Count < 2)
            return false;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var point in points)
        {
            minX = Mathf.Min(minX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxX = Mathf.Max(maxX, point.x);
            maxY = Mathf.Max(maxY, point.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;
        float diagonal = Mathf.Sqrt(width * width + height * height);
        if (diagonal <= Mathf.Epsilon)
            return false;

        float endpointDistance = Vector2.Distance(points[0], points[points.Count - 1]);
        float endpointRatio = endpointDistance / diagonal;
        return endpointRatio <= maxEndpointDistanceRatio;
    }

    private class GestureTemplate
    {
        public GestureShape Shape { get; }
        public List<Vector2> Points { get; }

        public GestureTemplate(GestureShape shape, List<Vector2> points)
        {
            Shape = shape;
            Points = points;
        }
    }
}

public enum GestureShape
{
    None,
    Unknown,
    Circle,
    Square
}

public struct GestureRecognitionResult
{
    public GestureShape DetectedShape { get; }
    public bool IsRecognized { get; }
    public GestureShape ExpectedShape { get; }
    public bool MatchesExpected { get; }
    public float Score { get; }
    public int StrokeCount { get; }

    public GestureRecognitionResult(GestureShape detectedShape, bool isRecognized, GestureShape expectedShape, bool matchesExpected, float score, int strokeCount)
    {
        DetectedShape = detectedShape;
        IsRecognized = isRecognized;
        ExpectedShape = expectedShape;
        MatchesExpected = matchesExpected;
        Score = score;
        StrokeCount = strokeCount;
    }
}
