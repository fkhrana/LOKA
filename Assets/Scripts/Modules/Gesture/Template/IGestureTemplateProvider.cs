using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contract untuk provider template gesture yang dipisah per gesture.
/// </summary>
public interface IGestureTemplateProvider
{
    GestureShape Shape { get; }

    /// <summary>
    /// Mengembalikan stroke raw untuk template ini.
    /// </summary>
    List<List<Vector2>> GetStrokes();
}
