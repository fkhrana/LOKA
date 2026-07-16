using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Gesture Template Recorder - Developer tool untuk merekam dan menyimpan template gesture.
/// Script ini HANYA digunakan saat development, bukan saat gameplay.
/// </summary>
public class GestureTemplateRecorder : MonoBehaviour
{
    [SerializeField]
    private GestureDrawer drawer;

    [SerializeField]
    private string gestureName = "NewGesture";

    [SerializeField]
    private bool autoOpenOutputFolder = false;

    private List<List<Vector2>> recordedStrokes = new List<List<Vector2>>();
    private const string OUTPUT_FOLDER = "Assets/GestureTemplates/";

    private void Start()
    {
        // Temukan GestureDrawer jika tidak di-assign
        if (drawer == null)
        {
            drawer = FindAnyObjectByType<GestureDrawer>();
            if (drawer == null)
            {
                Debug.LogError("GestureTemplateRecorder: GestureDrawer tidak ditemukan di scene!");
                enabled = false;
                return;
            }
        }

        drawer.GestureRecognized += OnGestureRecognized;
    }

    private void OnDestroy()
    {
        if (drawer != null)
        {
            drawer.GestureRecognized -= OnGestureRecognized;
        }
    }

    private void Update()
    {
        // Keyboard input untuk save dan clear
        if (IsKeyPressed(KeyCode.S))
        {
            SaveTemplate();
        }

        if (IsKeyPressed(KeyCode.C))
        {
            ClearTemplate();
        }
    }

    /// <summary>
    /// Cek apakah tombol keyboard ditekan. Kompatibel dengan Input System dan legacy input.
    /// </summary>
    private bool IsKeyPressed(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        switch (key)
        {
            case KeyCode.S:
                return Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
            case KeyCode.C:
                return Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
            default:
                return Input.GetKeyDown(key);
        }
#else
        return Input.GetKeyDown(key);
#endif
    }

    /// <summary>
    /// Callback ketika gesture selesai dikenali dari GestureDrawer.
    /// Menambahkan semua stroke yang dipakai untuk recognition ke recordedStrokes.
    /// </summary>
    private void OnGestureRecognized(List<Vector2> points, GestureRecognitionResult result)
    {
        if (points == null || points.Count == 0)
        {
            Debug.LogWarning("GestureTemplateRecorder: Gesture kosong diterima.");
            return;
        }

        recordedStrokes.Clear();
        recordedStrokes.Add(new List<Vector2>(points));

        Debug.Log($"Gesture direkam | Jumlah titik: {points.Count} | Hasil: {result.DetectedShape}");
    }

    /// <summary>
    /// Simpan template gesture ke console dan file.
    /// Memproses setiap stroke melalui GestureRecognizer.ProcessPoints().
    /// </summary>
    [ContextMenu("Save Template")]
    public void SaveTemplate()
    {
        // Validasi: jika tidak ada stroke, jangan simpan
        if (recordedStrokes.Count == 0)
        {
            Debug.LogWarning("Belum ada gesture yang direkam.");
            return;
        }

        // Proses strokes melalui GestureRecognizer
        var processedStrokes = new List<List<Vector2>>();
        foreach (var stroke in recordedStrokes)
        {
            var processedStroke = GestureRecognizer.Instance.ProcessPoints(stroke);
            processedStrokes.Add(processedStroke);
        }

        // Generate output string
        string output = GenerateConsoleOutput(processedStrokes);

        // Tampilkan ke Console
        Debug.Log(output);

        // Simpan ke file
        SaveToFile(processedStrokes, output);

        // Auto clear setelah save berhasil
        recordedStrokes.Clear();
        Debug.Log("Template telah disimpan. Recorder dibersihkan untuk merekam gesture berikutnya.");

        // Buka folder output jika diaktifkan
        if (autoOpenOutputFolder)
        {
            OpenOutputFolder();
        }
    }

    /// <summary>
    /// Generate output string untuk console dalam format yang sesuai.
    /// </summary>
    private string GenerateConsoleOutput(List<List<Vector2>> processedStrokes)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("==================================");
        sb.AppendLine($"Gesture : {gestureName}");
        sb.AppendLine($"Stroke Count : {processedStrokes.Count}");
        sb.AppendLine();

        for (int i = 0; i < processedStrokes.Count; i++)
        {
            var stroke = processedStrokes[i];
            sb.AppendLine($"Stroke {i + 1}");
            sb.AppendLine();
            sb.AppendLine("new List<Vector2>()");
            sb.AppendLine("{");

            foreach (var point in stroke)
            {
                sb.AppendLine($"    new Vector2({point.x:F6}f, {point.y:F6}f),");
            }

            sb.AppendLine("};");
            sb.AppendLine();
        }

        sb.AppendLine("==================================");
        return sb.ToString();
    }

    /// <summary>
    /// Simpan template ke file dalam folder Assets/GestureTemplates/.
    /// Jika file sudah ada, auto-increment suffix (_1, _2, dst).
    /// </summary>
    private void SaveToFile(List<List<Vector2>> processedStrokes, string consoleOutput)
    {
        // Buat folder jika belum ada
        string folderPath = Path.Combine(Application.dataPath, "GestureTemplates");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"Folder dibuat: {folderPath}");
        }

        // Generate unique filename jika sudah ada
        string filePath = GetUniqueFilePath(folderPath, gestureName);

        // Generate file content
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Gesture : {gestureName}");
        sb.AppendLine($"StrokeCount : {processedStrokes.Count}");
        sb.AppendLine();

        for (int i = 0; i < processedStrokes.Count; i++)
        {
            var stroke = processedStrokes[i];
            sb.AppendLine($"Stroke {i + 1}");
            sb.AppendLine();
            sb.AppendLine("new List<Vector2>()");
            sb.AppendLine("{");

            foreach (var point in stroke)
            {
                sb.AppendLine($"    new Vector2({point.x:F6}f, {point.y:F6}f),");
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Simpan file
        try
        {
            File.WriteAllText(filePath, sb.ToString());
            Debug.Log($"Template disimpan ke: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Gagal menyimpan template: {e.Message}");
        }
    }

    /// <summary>
    /// Generate unique filename dengan auto-increment jika file sudah ada.
    /// Circle.txt → Circle_1.txt → Circle_2.txt, dst.
    /// </summary>
    private string GetUniqueFilePath(string folderPath, string gestureName)
    {
        string baseFilePath = Path.Combine(folderPath, $"{gestureName}.txt");

        // Jika file belum ada, gunakan nama original
        if (!File.Exists(baseFilePath))
        {
            return baseFilePath;
        }

        // Jika sudah ada, cari nama dengan suffix _1, _2, dst
        int counter = 1;
        string uniqueFilePath;
        do
        {
            uniqueFilePath = Path.Combine(folderPath, $"{gestureName}_{counter}.txt");
            counter++;
        } while (File.Exists(uniqueFilePath));

        Debug.LogWarning($"File '{gestureName}.txt' sudah ada. Menyimpan sebagai '{Path.GetFileName(uniqueFilePath)}'");
        return uniqueFilePath;
    }

    /// <summary>
    /// Buka folder output di explorer/finder.
    /// </summary>
    private void OpenOutputFolder()
    {
        string folderPath = Path.Combine(Application.dataPath, "GestureTemplates");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Windows
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select, " + folderPath);
        }
        // macOS
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            System.Diagnostics.Process.Start("open", "-R " + folderPath);
        }
        // Linux
        else if (Application.platform == RuntimePlatform.LinuxEditor)
        {
            System.Diagnostics.Process.Start("nautilus", folderPath);
        }
    }

    /// <summary>
    /// Bersihkan recorded strokes.
    /// </summary>
    [ContextMenu("Clear Template")]
    public void ClearTemplate()
    {
        recordedStrokes.Clear();
        Debug.Log("Recorder telah dibersihkan. Siap merekam gesture baru.");
    }

    /// <summary>
    /// Export template sebagai C# code format (GestureTemplate constructor style).
    /// Jika file sudah ada, auto-increment suffix.
    /// </summary>
    [ContextMenu("Export as C# Code")]
    public void ExportAsCode()
    {
        if (recordedStrokes.Count == 0)
        {
            Debug.LogWarning("Belum ada gesture yang direkam.");
            return;
        }

        // Proses strokes
        var processedStrokes = new List<List<Vector2>>();
        foreach (var stroke in recordedStrokes)
        {
            var processedStroke = GestureRecognizer.Instance.ProcessPoints(stroke);
            processedStrokes.Add(processedStroke);
        }

        // Generate C# code
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("new GestureTemplate(");
        sb.AppendLine($"    GestureShape.{gestureName},");
        sb.AppendLine("    new List<List<Vector2>>");
        sb.AppendLine("    {");

        for (int i = 0; i < processedStrokes.Count; i++)
        {
            var stroke = processedStrokes[i];
            sb.AppendLine("        new List<Vector2>()");
            sb.AppendLine("        {");

            foreach (var point in stroke)
            {
                sb.AppendLine($"            new Vector2({point.x:F6}f, {point.y:F6}f),");
            }

            sb.AppendLine("        }" + (i < processedStrokes.Count - 1 ? "," : ""));
        }

        sb.AppendLine("    }");
        sb.AppendLine(");");

        string codeOutput = sb.ToString();
        Debug.Log("C# Export:\n" + codeOutput);

        // Simpan ke file juga dengan unique filename
        string folderPath = Path.Combine(Application.dataPath, "GestureTemplates");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = GetUniqueFilePathForCode(folderPath, gestureName);
        try
        {
            File.WriteAllText(filePath, codeOutput);
            Debug.Log($"C# code disimpan ke: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Gagal menyimpan C# code: {e.Message}");
        }
    }

    /// <summary>
    /// Generate unique filename untuk C# code export dengan auto-increment.
    /// Circle_code.txt → Circle_code_1.txt → Circle_code_2.txt, dst.
    /// </summary>
    private string GetUniqueFilePathForCode(string folderPath, string gestureName)
    {
        string baseFilePath = Path.Combine(folderPath, $"{gestureName}_code.txt");

        // Jika file belum ada, gunakan nama original
        if (!File.Exists(baseFilePath))
        {
            return baseFilePath;
        }

        // Jika sudah ada, cari nama dengan suffix _1, _2, dst
        int counter = 1;
        string uniqueFilePath;
        do
        {
            uniqueFilePath = Path.Combine(folderPath, $"{gestureName}_code_{counter}.txt");
            counter++;
        } while (File.Exists(uniqueFilePath));

        Debug.LogWarning($"File '{gestureName}_code.txt' sudah ada. Menyimpan sebagai '{Path.GetFileName(uniqueFilePath)}'");
        return uniqueFilePath;
    }
}
