using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Circle.
/// Data ini diproses menjadi satu stroke averaged dari dua recording.
/// </summary>
public class CircleGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Circle;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-117.803900f, 89.471100f),
            new Vector2(-117.803900f, 75.412320f),
            new Vector2(-118.573500f, 61.429750f),
            new Vector2(-120.492100f, 47.560940f),
            new Vector2(-120.492100f, 33.502180f),
            new Vector2(-120.492100f, 19.443410f),
            new Vector2(-120.492100f, 5.384628f),
            new Vector2(-120.492100f, -8.674149f),
            new Vector2(-120.492100f, -22.732910f),
            new Vector2(-120.492100f, -36.791690f),
            new Vector2(-120.492100f, -50.850460f),
            new Vector2(-120.492100f, -64.909230f),
            new Vector2(-120.492100f, -78.968000f),
            new Vector2(-118.801900f, -92.326670f),
            new Vector2(-107.642200f, -100.296000f),
            new Vector2(-94.197070f, -104.077300f),
            new Vector2(-80.299730f, -105.388700f),
            new Vector2(-66.410420f, -106.765300f),
            new Vector2(-52.351640f, -106.765300f),
            new Vector2(-38.292880f, -106.765300f),
            new Vector2(-24.234100f, -106.765300f),
            new Vector2(-10.175320f, -106.765300f),
            new Vector2(3.883438f, -106.765300f),
            new Vector2(17.942210f, -106.765300f),
            new Vector2(31.586690f, -109.967300f),
            new Vector2(45.292600f, -112.141700f),
            new Vector2(59.351380f, -112.141700f),
            new Vector2(73.410160f, -112.141700f),
            new Vector2(87.468930f, -112.141700f),
            new Vector2(101.527700f, -112.141700f),
            new Vector2(115.586500f, -112.141700f),
            new Vector2(116.067100f, -98.563610f),
            new Vector2(116.067100f, -84.504830f),
            new Vector2(116.067100f, -70.446060f),
            new Vector2(116.067100f, -56.387280f),
            new Vector2(116.067100f, -42.328520f),
            new Vector2(116.067100f, -28.269740f),
            new Vector2(117.786600f, -14.616870f),
            new Vector2(121.443400f, -1.421356f),
            new Vector2(121.443400f, 12.637410f),
            new Vector2(121.443400f, 26.696180f),
            new Vector2(121.443400f, 40.754960f),
            new Vector2(121.443400f, 54.813720f),
            new Vector2(121.443400f, 68.872500f),
            new Vector2(121.443400f, 82.931270f),
            new Vector2(119.915800f, 96.742160f),
            new Vector2(108.859100f, 103.369900f),
            new Vector2(95.324460f, 107.138000f),
            new Vector2(81.452350f, 108.288300f),
            new Vector2(67.393590f, 108.288300f),
            new Vector2(53.381210f, 108.002400f),
            new Vector2(39.712280f, 105.600200f),
            new Vector2(26.016830f, 103.361200f),
            new Vector2(12.030960f, 102.912000f),
            new Vector2(-2.027817f, 102.912000f),
            new Vector2(-16.086580f, 102.912000f),
            new Vector2(-30.145360f, 102.912000f),
            new Vector2(-44.204130f, 102.912000f),
            new Vector2(-58.262910f, 102.912000f),
            new Vector2(-72.321670f, 102.912000f),
            new Vector2(-86.380450f, 102.912000f),
            new Vector2(-100.439200f, 102.912000f),
            new Vector2(-114.498000f, 102.912000f),
            new Vector2(-128.556600f, 102.912000f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(2.118805f, 111.604700f),
            new Vector2(-9.822311f, 108.593400f),
            new Vector2(-21.318860f, 104.246300f),
            new Vector2(-32.484800f, 99.101180f),
            new Vector2(-44.168730f, 95.206530f),
            new Vector2(-55.852690f, 91.311980f),
            new Vector2(-66.120820f, 84.522550f),
            new Vector2(-76.368270f, 77.690870f),
            new Vector2(-86.615750f, 70.859220f),
            new Vector2(-95.968430f, 62.936100f),
            new Vector2(-101.884600f, 52.501540f),
            new Vector2(-108.692800f, 42.344210f),
            new Vector2(-114.852700f, 31.731460f),
            new Vector2(-121.152100f, 21.175080f),
            new Vector2(-125.788100f, 9.854401f),
            new Vector2(-125.788100f, -2.461540f),
            new Vector2(-125.788100f, -14.777480f),
            new Vector2(-125.788100f, -27.093420f),
            new Vector2(-125.788100f, -39.409360f),
            new Vector2(-124.698300f, -51.548440f),
            new Vector2(-120.803700f, -63.232370f),
            new Vector2(-114.597600f, -73.785800f),
            new Vector2(-104.938200f, -81.358270f),
            new Vector2(-97.368870f, -90.907930f),
            new Vector2(-87.369450f, -97.906970f),
            new Vector2(-75.685500f, -101.801600f),
            new Vector2(-63.647090f, -103.511600f),
            new Vector2(-51.407490f, -104.585900f),
            new Vector2(-39.215320f, -106.327600f),
            new Vector2(-27.504870f, -110.109800f),
            new Vector2(-15.533390f, -112.232500f),
            new Vector2(-3.217438f, -112.232500f),
            new Vector2(9.098495f, -112.232500f),
            new Vector2(21.319310f, -111.646200f),
            new Vector2(33.003230f, -107.751600f),
            new Vector2(44.687180f, -103.857000f),
            new Vector2(56.134200f, -99.477960f),
            new Vector2(66.381690f, -92.646320f),
            new Vector2(76.629170f, -85.814650f),
            new Vector2(85.935230f, -77.834660f),
            new Vector2(96.052140f, -71.079770f),
            new Vector2(104.746400f, -62.942280f),
            new Vector2(111.578100f, -52.694780f),
            new Vector2(116.076700f, -41.293420f),
            new Vector2(121.304800f, -30.211650f),
            new Vector2(121.720200f, -17.963120f),
            new Vector2(124.211900f, -6.051529f),
            new Vector2(124.211900f, 6.264404f),
            new Vector2(124.211900f, 18.580350f),
            new Vector2(124.211900f, 30.896290f),
            new Vector2(124.211900f, 43.212230f),
            new Vector2(120.689200f, 54.696590f),
            new Vector2(114.408300f, 65.263640f),
            new Vector2(106.200800f, 74.383210f),
            new Vector2(96.655170f, 82.126890f),
            new Vector2(86.584790f, 89.208880f),
            new Vector2(75.146190f, 93.347890f),
            new Vector2(63.390150f, 96.994890f),
            new Vector2(52.928670f, 103.389000f),
            new Vector2(41.244740f, 107.283700f),
            new Vector2(29.392490f, 110.600300f),
            new Vector2(17.200210f, 111.604700f),
            new Vector2(5.148880f, 109.455300f),
            new Vector2(-6.602081f, 105.790800f),
        };

        return new List<List<Vector2>>
        {
            AverageStroke(firstStroke, secondStroke)
        };
    }

    private List<Vector2> AverageStroke(List<Vector2> first, List<Vector2> second)
    {
        int count = Mathf.Max(first.Count, second.Count);
        var averaged = new List<Vector2>(count);

        for (int i = 0; i < count; i++)
        {
            Vector2 firstPoint = i < first.Count ? first[i] : first[first.Count - 1];
            Vector2 secondPoint = i < second.Count ? second[i] : second[second.Count - 1];
            averaged.Add((firstPoint + secondPoint) * 0.5f);
        }

        return averaged;
    }
}
