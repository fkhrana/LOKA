using System;
using System.Collections.Generic;
using UnityEngine;

public class GestureRecognizer : MonoBehaviour
{
    public static GestureRecognizer Instance { get; private set; }

    private const int SampleCount = 64;
    private const float SquareSize = 250f;
    private const float MaxAverageDistance = 30f;
    private const float RotationAngleRangeDegrees = 15f;
    private const float MaxAbsoluteScore = 50f;
    private const float CornerThresholdDegrees = 45f;
    private const int MaxAllowedCorners = 5;
    private const float RecognitionMarginRatio = 0.90f;
    private const float MaxStraightnessRatio = 0.95f;

    private readonly FaGaGestureDisambiguator faGaDisambiguator = new FaGaGestureDisambiguator();

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

        templateProviders.Add(new NaGestureTemplate());
        templateProviders.Add(new NaAlternativeGestureTemplate());
        templateProviders.Add(new KaGestureTemplate());
        templateProviders.Add(new PaAlternativeGestureTemplate());
        templateProviders.Add(new ZaAlternativeGestureTemplate());
        templateProviders.Add(new DaGestureTemplate());
        templateProviders.Add(new DaAlternativeGestureTemplate());
        templateProviders.Add(new WaGestureTemplate());
        templateProviders.Add(new WaAlternativeGestureTemplate());
        templateProviders.Add(new LaGestureTemplate());
        templateProviders.Add(new LaAlternativeGestureTemplate());
        templateProviders.Add(new MaGestureTemplate());
        templateProviders.Add(new MaAlternativeGestureTemplate());
        templateProviders.Add(new BaGestureTemplate());
        templateProviders.Add(new BaAlternativeGestureTemplate());
        templateProviders.Add(new FaGestureTemplate());
        templateProviders.Add(new FaAlternativeGestureTemplate());
        templateProviders.Add(new QaGestureTemplate());
        templateProviders.Add(new QaSingleStrokeTemplate());
        templateProviders.Add(new GaGestureTemplate());
        templateProviders.Add(new GaAlternativeGestureTemplate());
        templateProviders.Add(new HaGestureTemplate());
        templateProviders.Add(new HaAlternativeGestureTemplate());
        templateProviders.Add(new PaGestureTemplate());
        templateProviders.Add(new ZaGestureTemplate());
        templateProviders.Add(new TaGestureTemplate());
        templateProviders.Add(new TaAlternativeTwoStrokeTemplate());
        templateProviders.Add(new TaSingleStrokeTemplate());
        templateProviders.Add(new SaGestureTemplate());
        templateProviders.Add(new SaAlternativeGestureTemplate());
        templateProviders.Add(new LoveGestureTemplate());
        templateProviders.Add(new LoveAlternativeGestureTemplate());
        templateProviders.Add(new LoveAlternativeGestureTemplate2());
        templateProviders.Add(new LoveAlternativeGestureTemplate3());

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

    public GestureRecognitionResult Recognize(List<Vector2> rawPoints, GestureShape expectedShape = GestureShape.Unknown)
    {
        if (rawPoints == null || rawPoints.Count == 0)
        {
            Debug.Log("Tidak ada gesture yang valid untuk dikenali.");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, 1);
        }

        var singleStroke = new List<List<Vector2>> { rawPoints };
        return Recognize(singleStroke, expectedShape);
    }

    public GestureRecognitionResult Recognize(List<List<Vector2>> strokes, GestureShape expectedShape = GestureShape.Unknown)
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

        if (validStrokes.Count == 1)
        {
            var stroke = validStrokes[0];
            float pathLength = GestureNormalizationHelper.PathLength(stroke);
            float endpointDistance = Vector2.Distance(stroke[0], stroke[stroke.Count - 1]);
            float straightness = pathLength > Mathf.Epsilon ? endpointDistance / pathLength : 1f;

            if (straightness >= MaxStraightnessRatio)
            {
                Debug.Log($"Gesture ditolak karena terlalu lurus. straightness: {straightness:F3}");
                return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, strokes.Count);
            }
        }

        var candidateStrokes = new List<List<Vector2>>(validStrokes.Count);
        var candidateStrokeCorners = new List<int>(validStrokes.Count);
        foreach (var stroke in validStrokes)
        {
            var processedStroke = GestureNormalizationHelper.ProcessPoints(stroke, SampleCount, SquareSize);
            candidateStrokes.Add(processedStroke);
            candidateStrokeCorners.Add(GestureNormalizationHelper.CountCorners(processedStroke, CornerThresholdDegrees));
        }

        int totalCornerCount = 0;
        foreach (var strokeCorners in candidateStrokeCorners)
        {
            totalCornerCount += strokeCorners;
        }

        if (totalCornerCount > MaxAllowedCorners && candidateStrokes.Count > 1)
        {
            Debug.Log($"Gesture ditolak oleh corner detection. corners: {totalCornerCount}, threshold: {CornerThresholdDegrees:F1}, maxAllowedCorners: {MaxAllowedCorners}");
            return new GestureRecognitionResult(GestureShape.Unknown, false, expectedShape, false, 0f, strokes.Count);
        }

        GestureShape bestShape = GestureShape.Unknown;
        float bestDistance = float.MaxValue;
        float secondBestDistance = float.MaxValue;

        float angleRange = Mathf.Deg2Rad * RotationAngleRangeDegrees;
        foreach (var template in templates)
        {
            // Pastikan jumlah stroke sama dengan template.
            if (template.Strokes.Count != candidateStrokes.Count)
                continue;

            // Hitung total distance dengan menjumlahkan jarak setiap pasangan stroke.
            float totalDistance = 0f;
            for (int i = 0; i < candidateStrokes.Count; i++)
            {
                totalDistance += GestureNormalizationHelper.DistanceAtBestAngle(candidateStrokes[i], template.Strokes[i], -angleRange, angleRange);
            }

            totalDistance += faGaDisambiguator.GetPenalty(
                template.Shape,
                candidateStrokes,
                template.Strokes);

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

        int totalCandidatePoints = candidateStrokes.Count * SampleCount;
        float averageDistance = bestDistance / totalCandidatePoints;
        float secondAverageDistance = secondBestDistance == float.MaxValue
            ? float.MaxValue
            : secondBestDistance / totalCandidatePoints;
        bool hasClearMatch = averageDistance <= MaxAverageDistance &&
                             bestDistance <= MaxAbsoluteScore &&
                             (secondBestDistance == float.MaxValue ||
                              averageDistance <= secondAverageDistance * RecognitionMarginRatio ||
                              averageDistance <= 18f);
        bool isRecognized = hasClearMatch;
        bool matchesExpected = expectedShape == GestureShape.Unknown || (isRecognized && bestShape == expectedShape);

        if (!isRecognized)
        {
            Debug.Log($"Tidak dikenali. Bentuk terlalu ambigu atau tidak jelas. strokeCount: {strokes.Count}, score: {bestDistance:F2}");
        }
        else if (expectedShape != GestureShape.Unknown && bestShape != expectedShape)
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
        return GestureNormalizationHelper.ProcessPoints(points, SampleCount, SquareSize);
    }

    public GestureShape GetClosestActiveTarget(
        List<List<Vector2>> strokes,
        IReadOnlyList<GestureShape> activeTargets,
        out float closestDistance)
    {
        closestDistance = float.MaxValue;
        if (strokes == null || strokes.Count == 0 ||
            activeTargets == null || activeTargets.Count == 0)
        {
            return GestureShape.Unknown;
        }

        var candidateStrokes = new List<List<Vector2>>(strokes.Count);
        int totalPointCount = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.Count < 2)
                continue;

            totalPointCount += stroke.Count;
            candidateStrokes.Add(
                GestureNormalizationHelper.ProcessPoints(stroke, SampleCount, SquareSize));
        }

        if (candidateStrokes.Count == 0 || totalPointCount < 5)
            return GestureShape.Unknown;

        GestureShape closestShape = GestureShape.Unknown;
        float angleRange = Mathf.Deg2Rad * RotationAngleRangeDegrees;
        foreach (var targetShape in activeTargets)
        {
            foreach (var template in templates)
            {
                if (template.Shape != targetShape ||
                    template.Strokes.Count != candidateStrokes.Count)
                {
                    continue;
                }

                float totalDistance = 0f;
                for (int i = 0; i < candidateStrokes.Count; i++)
                {
                    totalDistance += GestureNormalizationHelper.DistanceAtBestAngle(
                        candidateStrokes[i], template.Strokes[i], -angleRange, angleRange);
                }

                totalDistance += faGaDisambiguator.GetPenalty(
                    template.Shape,
                    candidateStrokes,
                    template.Strokes);

                if (totalDistance < closestDistance)
                {
                    closestDistance = totalDistance;
                    closestShape = template.Shape;
                }
            }
        }

        return closestShape;
    }

    public bool HasTemplateForStrokeCount(GestureShape shape, int strokeCount)
    {
        foreach (var template in templates)
        {
            if (template.Shape == shape && template.Strokes.Count == strokeCount)
                return true;
        }

        return false;
    }

    public bool HasTemplateWithMoreStrokes(GestureShape shape, int strokeCount)
    {
        foreach (var template in templates)
        {
            if (template.Shape == shape && template.Strokes.Count > strokeCount)
                return true;
        }

        return false;
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
    Unknown = 0,
    Na = 4,
    Ka = 5,
    Da = 6,
    Wa = 7,
    La = 8,
    Ma = 9,
    Ba = 10,
    Fa = 11,
    Qa = 12,
    Ga = 13,
    Ha = 14,
    Pa = 15,
    Za = 16,
    Ta = 17,
    Sa = 18,
    Love = 19
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
