using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FaGaGestureDisambiguator
{
    [SerializeField, Tooltip("Bobot pembeda FA/GA berdasarkan jarak titik awal dan akhir gesture.")]
    private float endpointGapWeight = 0.05f;

    public float GetPenalty(
        GestureShape shape,
        List<List<Vector2>> candidateStrokes,
        List<List<Vector2>> templateStrokes)
    {
        if ((shape != GestureShape.Fa && shape != GestureShape.Ga) ||
            candidateStrokes == null || candidateStrokes.Count != 1 ||
            templateStrokes == null || templateStrokes.Count != 1 ||
            candidateStrokes[0] == null || candidateStrokes[0].Count < 2 ||
            templateStrokes[0] == null || templateStrokes[0].Count < 2)
        {
            return 0f;
        }

        List<Vector2> candidateStroke = candidateStrokes[0];
        List<Vector2> templateStroke = templateStrokes[0];
        float candidateEndpointGap = Vector2.Distance(candidateStroke[0], candidateStroke[candidateStroke.Count - 1]);
        float templateEndpointGap = Vector2.Distance(templateStroke[0], templateStroke[templateStroke.Count - 1]);

        return Mathf.Abs(candidateEndpointGap - templateEndpointGap) *
             endpointGapWeight;
    }
}