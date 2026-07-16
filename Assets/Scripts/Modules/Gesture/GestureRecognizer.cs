using System;
using System.Collections.Generic;
using UnityEngine;

public class GestureRecognizer : MonoBehaviour
{
    public static GestureRecognizer Instance { get; private set; }

    public int sampleCount = 64;
    public float squareSize = 250f;
    [Tooltip("Ambang maksimal jarak rata-rata; semakin rendah berarti gesture harus lebih mirip.")]
    public float maxAverageDistance = 45f;
    [Tooltip("Ambang skor absolut agar gesture dianggap valid. Jika skor di atas nilai ini, hasil dianggap terlalu lemah.")]
    public float maxAbsoluteScore = 50f;
    [SerializeField, Tooltip("Ambang sudut tajam untuk corner detection (derajat).")]
    private float cornerThresholdDegrees = 45f;
    [SerializeField, Tooltip("Jumlah corner maksimum yang diizinkan sebelum gesture ditolak.")]
    private int maxAllowedCorners = 12;
    [Tooltip("Rasio minimum pemisahan antara hasil terbaik dan kedua terbaik agar bentuk dianggap jelas.")]
    public float recognitionMarginRatio = 0.90f;
    [Tooltip("Rasio jarak titik awal-akhir terhadap ukuran gesture. Makin kecil makin ketat untuk menolak garis terbuka.")]
    public float maxEndpointDistanceRatio = 0.18f;

    private readonly List<GestureTemplate> templates = new List<GestureTemplate>();
    private readonly List<IGestureTemplateProvider> templateProviders = new List<IGestureTemplateProvider>();

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
        templateProviders.Clear();

        templateProviders.Add(new CircleGestureTemplate());
        templateProviders.Add(new SquareGestureTemplate());
        templateProviders.Add(new NaGestureTemplate());
        templateProviders.Add(new KaGestureTemplate());

        foreach (var provider in templateProviders)
        {
            var strokes = provider.GetStrokes();
            if (strokes == null || strokes.Count == 0)
            {
                continue;
            }

            templates.Add(new GestureTemplate(provider.Shape, strokes));
        }
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

        // Tidak menggabungkan stroke menjadi satu list. Setiap stroke diproses terpisah.
        var validStrokes = new List<List<Vector2>>();
        int totalPointCount = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.Count < 2)
                continue;

            validStrokes.Add(stroke);
            totalPointCount += stroke.Count;
        }

        if (validStrokes.Count == 0 || totalPointCount < 5)
        {
            Debug.Log("Tidak ada gesture yang valid untuk dikenali.");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, strokes.Count);
        }

        var candidateStrokes = new List<List<Vector2>>(validStrokes.Count);
        var candidateStrokeCorners = new List<int>(validStrokes.Count);
        foreach (var stroke in validStrokes)
        {
            var processedStroke = GestureNormalizationHelper.ProcessPoints(stroke, sampleCount, squareSize);
            candidateStrokes.Add(processedStroke);
            candidateStrokeCorners.Add(GestureNormalizationHelper.CountCorners(processedStroke, cornerThresholdDegrees));
        }

        int totalCornerCount = 0;
        foreach (var strokeCorners in candidateStrokeCorners)
        {
            totalCornerCount += strokeCorners;
        }

        if (totalCornerCount > maxAllowedCorners && candidateStrokes.Count > 1)
        {
            Debug.Log($"Gesture ditolak oleh corner detection. corners: {totalCornerCount}, threshold: {cornerThresholdDegrees:F1}, maxAllowedCorners: {maxAllowedCorners}");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, strokes.Count);
        }

        GestureShape bestShape = GestureShape.Unknown;
        float bestDistance = float.MaxValue;
        float secondBestDistance = float.MaxValue;

        float angleRange = Mathf.Deg2Rad * 45f;
        foreach (var template in templates)
        {
            // Pastikan jumlah stroke sama dengan template. Circle, Square, Na = 1 stroke, Ka = 2 stroke.
            if (template.Strokes.Count != candidateStrokes.Count)
                continue;

            // Hitung total distance dengan menjumlahkan jarak setiap pasangan stroke.
            float totalDistance = 0f;
            for (int i = 0; i < candidateStrokes.Count; i++)
            {
                totalDistance += GestureNormalizationHelper.DistanceAtBestAngle(candidateStrokes[i], template.Strokes[i], -angleRange, angleRange);
            }

            if (totalDistance < bestDistance)
            {
                secondBestDistance = bestDistance;
                bestDistance = totalDistance;
                bestShape = template.Shape;
            }
            else if (totalDistance < secondBestDistance)
            {
                secondBestDistance = totalDistance;
            }
        }

        int totalCandidatePoints = candidateStrokes.Count * sampleCount;
        float averageDistance = bestDistance / totalCandidatePoints;
        float secondAverageDistance = secondBestDistance == float.MaxValue ? float.MaxValue : secondBestDistance / totalCandidatePoints;
        bool hasClearMatch = averageDistance <= maxAverageDistance &&
                             bestDistance <= maxAbsoluteScore &&
                             (secondBestDistance == float.MaxValue || averageDistance <= secondAverageDistance * recognitionMarginRatio || averageDistance <= 18f);
        bool isRecognized = hasClearMatch;
        bool matchesExpected = expectedShape == GestureShape.None || (isRecognized && bestShape == expectedShape);

        if (!isRecognized)
        {
            Debug.Log($"Tidak dikenali. Bentuk terlalu ambigu atau tidak jelas. strokeCount: {strokes.Count}, score: {bestDistance:F2}");
        }
        else if (expectedShape != GestureShape.None && bestShape != expectedShape)
        {
            Debug.Log($"Gesture salah. Diminta: {expectedShape}, terdeteksi: {bestShape}. strokeCount: {strokes.Count}, score: {bestDistance:F2}");
        }
        else
        {
            Debug.Log($"Gesture terdeteksi: {bestShape}. strokeCount: {strokes.Count}, score: {bestDistance:F2}");
        }

        return new GestureRecognitionResult(bestShape, isRecognized, expectedShape, matchesExpected, bestDistance, strokes.Count);
    }

    public List<Vector2> ProcessPoints(List<Vector2> points)
    {
        return GestureNormalizationHelper.ProcessPoints(points, sampleCount, squareSize);
    }

    private class GestureTemplate
    {
        public GestureShape Shape { get; }
        public List<List<Vector2>> Strokes { get; }

        public GestureTemplate(GestureShape shape, List<List<Vector2>> strokes)
        {
            Shape = shape;
            Strokes = strokes;
        }
    }
}

public enum GestureShape
{
    None,
    Unknown,
    Circle,
    Square,
    Na,
    Ka
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
