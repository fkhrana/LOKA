using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;

// Tempel script ini SEMENTARA di GameObject EventSystem (atau GameObject apapun di scene).
// Nanti setiap kamu klik di layar, Console bakal nampilin:
// - Apakah EventSystem ada
// - Apa saja GameObject yang "kena" raycast klik itu, urut dari paling atas
public class ClickDiagnostic : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null)
            {
                Debug.LogError("[Diagnostic] TIDAK ADA EventSystem.current! Ini penyebabnya.");
                return;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0)
            {
                Debug.LogWarning("[Diagnostic] Raycast klik TIDAK KENA APAPUN. Kemungkinan: Canvas nonaktif, GraphicRaycaster nonaktif, atau Canvas render mode/Camera salah.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Diagnostic] Klik kena " + results.Count + " objek (urut dari paling atas):");
            foreach (var r in results)
            {
                sb.AppendLine(" - " + r.gameObject.name + " (di GameObject: " + GetPath(r.gameObject) + ")");
            }
            Debug.Log(sb.ToString());
        }
    }

    string GetPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}