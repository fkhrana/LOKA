using System.Collections.Generic;
using UnityEngine;

public static class TutorialLetterPaths
{
    private static Dictionary<GestureShape, List<Vector2>> templateCache;

    private static readonly HashSet<GestureShape> supportedShapes = new HashSet<GestureShape>
    {
        GestureShape.Na,
        GestureShape.Da,
        GestureShape.Wa,
        GestureShape.La,
        GestureShape.Ma,
        GestureShape.Ba,
        GestureShape.Fa,
        GestureShape.Ka,
        GestureShape.Qa,
        GestureShape.Ga,
        GestureShape.Ha,
        GestureShape.Pa,
        GestureShape.Sa,
        GestureShape.Za,
        GestureShape.Ta,
    };

    public static bool IsSupported(GestureShape shape) => supportedShapes.Contains(shape);

    public static List<Vector2> GetPath(GestureShape shape)
    {
        EnsureCache();

        // Priority 1: cache dari template (reflection)
        if (templateCache.TryGetValue(shape, out var path) && path.Count >= 2)
        {
            Debug.Log($"[TutorialLetterPaths] {shape} pakai TEMPLATE ({path.Count} pts)");
            return path;
        }

        // Priority 2: manual fallback
        var fallback = GetManualFallback(shape);
        if (fallback != null && fallback.Count >= 2)
        {
            Debug.Log($"[TutorialLetterPaths] {shape} pakai MANUAL fallback ({fallback.Count} pts)");
            return fallback;
        }

        // Priority 3: ultimate fallback
        Debug.LogWarning($"[TutorialLetterPaths] {shape} tidak ada path. Fallback ke Na.");
        return ManualNa();
    }

    private static void EnsureCache()
    {
        if (templateCache != null) return;

        templateCache = new Dictionary<GestureShape, List<Vector2>>();

        var providerType = typeof(IGestureTemplateProvider);
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        int found = 0;
        int scanned = 0;

        foreach (var assembly in assemblies)
        {
            System.Type[] types;
            try { types = assembly.GetTypes(); }
            catch (System.Exception) { continue; }

            foreach (var type in types)
            {
                if (type == null) continue;
                if (type.IsAbstract || type.IsInterface) continue;
                if (!providerType.IsAssignableFrom(type)) continue;

                scanned++;

                string lowerName = type.Name.ToLowerInvariant();
                if (lowerName.Contains("alternative")) continue;
                if (lowerName.Contains("singlestroke")) continue;
                if (lowerName.Contains("twostroke")) continue;
                if (lowerName.Contains("multistroke")) continue;
                if (lowerName.Contains("_alt")) continue;

                var ctor = type.GetConstructor(System.Type.EmptyTypes);
                if (ctor == null) continue;

                try
                {
                    var provider = (IGestureTemplateProvider)System.Activator.CreateInstance(type);
                    if (provider == null) continue;
                    if (!supportedShapes.Contains(provider.Shape)) continue;

                    var strokes = provider.GetStrokes();
                    if (strokes == null || strokes.Count == 0) continue;

                    var combined = CombineStrokes(strokes);
                    if (combined.Count < 2) continue;

                    var normalized = NormalizeToRange(combined, 0.6f);
                    if (normalized.Count < 2) continue;

                    if (!templateCache.ContainsKey(provider.Shape))
                    {
                        templateCache[provider.Shape] = normalized;
                        found++;
                        Debug.Log($"[TutorialLetterPaths] ✅ Cached {provider.Shape} from {type.Name} ({normalized.Count} pts)");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[TutorialLetterPaths] Gagal instantiate {type.Name}: {e.Message}");
                }
            }
        }

        Debug.Log($"[TutorialLetterPaths] Cache selesai. Scanned {scanned} provider, cached {found}/{supportedShapes.Count} shapes.");
    }

    private static List<Vector2> CombineStrokes(List<List<Vector2>> strokes)
    {
        var combined = new List<Vector2>();

        for (int i = 0; i < strokes.Count; i++)
        {
            var s = strokes[i];
            if (s == null || s.Count == 0) continue;

            if (combined.Count > 0)
            {
                Vector2 last = combined[combined.Count - 1];
                Vector2 first = s[0];
                combined.Add(Vector2.Lerp(last, first, 0.33f));
                combined.Add(Vector2.Lerp(last, first, 0.66f));
            }

            combined.AddRange(s);
        }

        return combined;
    }

    private static List<Vector2> NormalizeToRange(List<Vector2> raw, float targetRange)
    {
        if (raw == null || raw.Count == 0) return new List<Vector2>();

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var p in raw)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        float cx = (minX + maxX) * 0.5f;
        float cy = (minY + maxY) * 0.5f;
        float scale = Mathf.Max(maxX - minX, maxY - minY);
        if (scale < 0.0001f) scale = 1f;

        float factor = (targetRange * 2f) / scale;

        var result = new List<Vector2>(raw.Count);
        foreach (var p in raw)
            result.Add(new Vector2((p.x - cx) * factor, (p.y - cy) * factor));

        return result;
    }

    private static List<Vector2> GetManualFallback(GestureShape shape)
    {
        switch (shape)
        {
            case GestureShape.Na: return ManualNa();
            case GestureShape.Da: return ManualDa();
            case GestureShape.Wa: return ManualWa();
            case GestureShape.La: return ManualLa();
            case GestureShape.Ma: return ManualMa();
            case GestureShape.Ba: return ManualBa();
            case GestureShape.Fa: return ManualFa();
            case GestureShape.Ka: return ManualKa();
            case GestureShape.Qa: return ManualQa();
            case GestureShape.Ga: return ManualGa();
            case GestureShape.Ha: return ManualHa();
            case GestureShape.Pa: return ManualPa();
            case GestureShape.Sa: return ManualSa();
            case GestureShape.Za: return ManualZa();
            case GestureShape.Ta: return ManualTa();
            default:              return null;
        }
    }

    // ===== MANUAL FALLBACK (approximate shape) =====

    private static List<Vector2> ManualNa()
    {
        return new List<Vector2> { new Vector2(-0.6f, 0f), new Vector2(0.6f, 0f) };
    }

    private static List<Vector2> ManualDa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f),
            new Vector2(-0.5f, 0.5f),
        };
    }

    private static List<Vector2> ManualWa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.6f, -0.6f), new Vector2(-0.4f, 0.6f),
            new Vector2(0f, 0f), new Vector2(0.4f, 0.6f),
            new Vector2(0.6f, -0.6f),
        };
    }

    private static List<Vector2> ManualLa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.5f, 0.6f), new Vector2(-0.5f, -0.6f),
            new Vector2(0.6f, -0.6f),
        };
    }

    private static List<Vector2> ManualMa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.6f, -0.6f), new Vector2(-0.6f, 0.6f),
            new Vector2(0f, -0.1f), new Vector2(0.6f, 0.6f),
            new Vector2(0.6f, -0.6f),
        };
    }

    private static List<Vector2> ManualBa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.4f, -0.7f), new Vector2(-0.4f, 0.7f),
            new Vector2(0.2f, 0.7f), new Vector2(0.5f, 0.35f),
            new Vector2(0.2f, 0f), new Vector2(0.5f, -0.35f),
            new Vector2(0.2f, -0.7f), new Vector2(-0.4f, -0.7f),
        };
    }

    private static List<Vector2> ManualFa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.4f, -0.6f), new Vector2(-0.4f, 0.6f),
            new Vector2(0.5f, 0.6f), new Vector2(-0.4f, 0.6f),
            new Vector2(-0.4f, 0f), new Vector2(0.3f, 0f),
        };
    }

    private static List<Vector2> ManualKa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.4f, -0.6f), new Vector2(-0.4f, 0.6f),
            new Vector2(-0.4f, 0f), new Vector2(0.5f, 0.6f),
            new Vector2(-0.4f, 0f), new Vector2(0.5f, -0.6f),
        };
    }

    private static List<Vector2> ManualQa()
    {
        var pts = new List<Vector2>();
        int seg = 20;
        for (int i = 0; i <= seg; i++)
        {
            float a = (i / (float)seg) * Mathf.PI * 2f;
            pts.Add(new Vector2(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f));
        }
        pts.Add(new Vector2(0.6f, -0.6f));
        return pts;
    }

    private static List<Vector2> ManualGa()
    {
        var pts = new List<Vector2>();
        int seg = 16;
        for (int i = 0; i <= seg; i++)
        {
            float a = (i / (float)seg) * Mathf.PI * 1.5f + Mathf.PI * 0.25f;
            pts.Add(new Vector2(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f));
        }
        pts.Add(new Vector2(0.4f, -0.1f));
        pts.Add(new Vector2(0.1f, -0.1f));
        return pts;
    }

    private static List<Vector2> ManualHa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.5f, -0.6f), new Vector2(-0.5f, 0.6f),
            new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, -0.6f),
        };
    }

    private static List<Vector2> ManualPa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.4f, -0.7f), new Vector2(-0.4f, 0.7f),
            new Vector2(0.3f, 0.7f), new Vector2(0.5f, 0.35f),
            new Vector2(0.3f, 0f), new Vector2(-0.4f, 0f),
        };
    }

    private static List<Vector2> ManualSa()
    {
        var pts = new List<Vector2>();
        int seg = 20;
        for (int i = 0; i <= seg; i++)
        {
            float t = i / (float)seg;
            float x = Mathf.Sin(t * Mathf.PI * 2f) * 0.4f;
            float y = 0.6f - t * 1.2f;
            pts.Add(new Vector2(x, y));
        }
        return pts;
    }

    private static List<Vector2> ManualZa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.5f, 0.6f), new Vector2(0.5f, 0.6f),
            new Vector2(-0.5f, -0.6f), new Vector2(0.5f, -0.6f),
        };
    }

    private static List<Vector2> ManualTa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.5f, 0.6f), new Vector2(0.5f, 0.6f),
            new Vector2(0f, 0.6f), new Vector2(0f, -0.6f),
        };
    }
}