using System.Collections.Generic;
using UnityEngine;

public static class GestureNormalizationHelper
{
    public static List<Vector2> ProcessPoints(List<Vector2> points, int sampleCount, float squareSize)
    {
        var resampled = Resample(points, sampleCount);
        resampled = ScaleToSquare(resampled, squareSize);
        resampled = TranslateToOrigin(resampled);
        return resampled;
    }

    public static List<Vector2> Resample(List<Vector2> points, int n)
    {
        if (points == null || points.Count < 2 || n <= 1)
            return points == null ? new List<Vector2>() : new List<Vector2>(points);

        float pathLength = PathLength(points);
        float interval = pathLength / (n - 1);
        var workingPoints = new List<Vector2>(points);
        var newPoints = new List<Vector2> { workingPoints[0] };
        float distanceSoFar = 0f;

        for (int i = 1; i < workingPoints.Count; i++)
        {
            float d = Vector2.Distance(workingPoints[i - 1], workingPoints[i]);
            if ((distanceSoFar + d) >= interval)
            {
                float t = (interval - distanceSoFar) / d;
                Vector2 newPoint = Vector2.Lerp(workingPoints[i - 1], workingPoints[i], t);
                newPoints.Add(newPoint);
                workingPoints.Insert(i, newPoint);
                distanceSoFar = 0f;
            }
            else
            {
                distanceSoFar += d;
            }
        }

        while (newPoints.Count < n)
            newPoints.Add(workingPoints[workingPoints.Count - 1]);

        return newPoints;
    }

    public static List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        if (points == null || points.Count == 0)
            return new List<Vector2>();

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

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

    public static List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
            return new List<Vector2>();

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

    public static float PathDistance(List<Vector2> a, List<Vector2> b)
    {
        if (a == null || b == null || a.Count == 0 || b.Count == 0)
            return float.MaxValue;

        float distance = 0f;
        int count = Mathf.Min(a.Count, b.Count);
        for (int i = 0; i < count; i++)
        {
            distance += Vector2.Distance(a[i], b[i]);
        }

        return count > 0 ? distance / count : float.MaxValue;
    }

    public static List<Vector2> RotateBy(List<Vector2> points, float angle)
    {
        if (points == null || points.Count == 0)
            return new List<Vector2>();

        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        var newPoints = new List<Vector2>(points.Count);
        foreach (var p in points)
        {
            newPoints.Add(new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos));
        }

        return newPoints;
    }

    public static float DistanceAtAngle(List<Vector2> points, List<Vector2> template, float angle)
    {
        var rotated = RotateBy(points, angle);
        return PathDistance(rotated, template);
    }

    public static float DistanceAtBestAngle(List<Vector2> points, List<Vector2> template, float a, float b, float threshold = 0.0174533f)
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

    public static float PathLength(List<Vector2> points)
    {
        if (points == null || points.Count < 2)
            return 0f;

        float length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            length += Vector2.Distance(points[i - 1], points[i]);
        }

        return length;
    }

    public static int CountCorners(List<Vector2> points, float threshold)
    {
        if (points == null || points.Count < 3)
            return 0;

        int corners = 0;
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 prevDirection = points[i] - points[i - 1];
            Vector2 nextDirection = points[i + 1] - points[i];

            if (prevDirection.sqrMagnitude < 1e-6f || nextDirection.sqrMagnitude < 1e-6f)
                continue;

            float turnAngle = Mathf.Abs(Vector2.SignedAngle(prevDirection, nextDirection));
            if (turnAngle > threshold)
                corners++;
        }

        return corners;
    }
}
