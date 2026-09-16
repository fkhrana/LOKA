using System.Collections.Generic;
using UnityEngine;

public static class TutorialLetterPaths
{
    private static Dictionary<GestureShape, List<Vector2>> templateCache;

    private static readonly HashSet<GestureShape> supportedShapes = new HashSet<GestureShape>
    {
        GestureShape.Na,
        GestureShape.Wa,
        GestureShape.La,
        GestureShape.Da,
    };

    public static bool IsSupported(GestureShape shape) => supportedShapes.Contains(shape);

    // Ambil path untuk shape, fallback ke manual kalau tidak ada.
    public static List<Vector2> GetPath(GestureShape shape)
    {
        EnsureCache();

        if (!supportedShapes.Contains(shape))
        {
            Debug.LogWarning($"[TutorialLetterPaths] {shape} tidak didukung. Fallback ke Na.");
            return ManualNa();
        }

        if (templateCache.TryGetValue(shape, out var path) && path.Count >= 2)
            return path;

        var fallback = GetManualFallback(shape);
        if (fallback != null && fallback.Count >= 2)
        {
            Debug.LogWarning($"[TutorialLetterPaths] {shape} tidak ada template → pakai manual fallback.");
            return fallback;
        }

        Debug.LogWarning($"[TutorialLetterPaths] Path untuk {shape} tidak ada. Fallback ke Na.");
        return ManualNa();
    }

    // Build cache dari semua IGestureTemplateProvider via reflection.
    private static void EnsureCache()
    {
        if (templateCache != null) return;

        templateCache = new Dictionary<GestureShape, List<Vector2>>();

        var providerType = typeof(IGestureTemplateProvider);
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        int found = 0;

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
                        Debug.Log($"[TutorialLetterPaths] Cached {provider.Shape} from {type.Name} ({normalized.Count} pts)");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[TutorialLetterPaths] Gagal instantiate {type.Name}: {e.Message}");
                }
            }
        }

        Debug.Log($"[TutorialLetterPaths] Cache selesai. {found}/{supportedShapes.Count} shapes terdaftar.");
    }

    // Gabung strokes jadi satu path dengan interpolasi antar stroke.
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

    // Normalize path ke range targetRange di sekitar center.
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

    // Ambil fallback manual sesuai shape.
    private static List<Vector2> GetManualFallback(GestureShape shape)
    {
        switch (shape)
        {
            case GestureShape.Na: return ManualNa();
            case GestureShape.Wa: return ManualWa();
            case GestureShape.La: return ManualLa();
            case GestureShape.Qa: return ManualQa();
            case GestureShape.Da: return ManualDa();
            default:              return null;
        }
    }

    // Path Na manual: garis horizontal.
    private static List<Vector2> ManualNa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.6f, 0f),
            new Vector2( 0.6f, 0f),
        };
    }

    // Path Wa manual: bentuk W.
    private static List<Vector2> ManualWa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.6f, -0.6f),
            new Vector2(-0.4f,  0.6f),
            new Vector2( 0f,    0f),
            new Vector2( 0.4f,  0.6f),
            new Vector2( 0.6f, -0.6f),
        };
    }

    // Path La manual: bentuk L.
    private static List<Vector2> ManualLa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.5f,  0.6f),
            new Vector2(-0.5f, -0.6f),
            new Vector2( 0.6f, -0.6f),
        };
    }

    // Path Qa manual: lingkaran + ekor.
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

    // Path Da manual: bentuk kotak.
    private static List<Vector2> ManualDa()
    {
        return new List<Vector2>
        {
            new Vector2(-0.5f,  0.5f),
            new Vector2( 0.5f,  0.5f),
            new Vector2( 0.5f, -0.5f),
            new Vector2(-0.5f, -0.5f),
        };
    }
}