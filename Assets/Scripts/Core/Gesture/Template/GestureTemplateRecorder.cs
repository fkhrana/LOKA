using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GestureTemplateRecorder : MonoBehaviour
{
    [SerializeField] private GestureDrawer drawer;
    [SerializeField] private string gestureName = "NewGesture";

    private List<List<Vector2>> recordedStrokes = new List<List<Vector2>>();

    private void Start()
    {
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
        if (IsKeyPressed(KeyCode.S))
            SaveTemplate();

        if (IsKeyPressed(KeyCode.C))
            ClearTemplate();
    }

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

    [ContextMenu("Save Template")]
    public void SaveTemplate()
    {
        if (recordedStrokes.Count == 0)
        {
            Debug.LogWarning("Belum ada gesture yang direkam.");
            return;
        }

        var processedStrokes = new List<List<Vector2>>();
        foreach (var stroke in recordedStrokes)
        {
            processedStrokes.Add(GestureRecognizer.Instance.ProcessPoints(stroke));
        }

        string output = GenerateConsoleOutput(processedStrokes);
        Debug.Log(output);

        SaveToFile(processedStrokes);

        recordedStrokes.Clear();
        Debug.Log("Template telah disimpan. Recorder dibersihkan untuk merekam gesture berikutnya.");
    }

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

    private void SaveToFile(List<List<Vector2>> processedStrokes)
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

    [ContextMenu("Clear Template")]
    public void ClearTemplate()
    {
        recordedStrokes.Clear();
        Debug.Log("Recorder telah dibersihkan. Siap merekam gesture baru.");
    }

}
