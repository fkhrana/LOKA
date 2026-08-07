using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Wa.
/// Data ini diproses menjadi satu stroke averaged dari dua recording.
/// </summary>
public class WaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Wa;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(88.384350f, 109.132300f),
            new Vector2(75.120460f, 113.264300f),
            new Vector2(61.460620f, 116.440300f),
            new Vector2(47.842650f, 119.132300f),
            new Vector2(34.536880f, 123.748200f),
            new Vector2(20.643420f, 125.879300f),
            new Vector2(6.932022f, 129.132300f),
            new Vector2(-6.717120f, 131.632300f),
            new Vector2(-20.550710f, 134.132300f),
            new Vector2(-34.199860f, 136.632300f),
            new Vector2(-48.439160f, 136.632300f),
            new Vector2(-62.272750f, 139.132300f),
            new Vector2(-76.512050f, 139.132300f),
            new Vector2(-89.540030f, 136.207900f),
            new Vector2(-94.115640f, 123.418500f),
            new Vector2(-97.995330f, 109.993300f),
            new Vector2(-99.115660f, 95.935820f),
            new Vector2(-101.615700f, 82.286710f),
            new Vector2(-101.615700f, 68.047410f),
            new Vector2(-101.615700f, 53.808110f),
            new Vector2(-101.615700f, 39.568820f),
            new Vector2(-101.615700f, 25.329510f),
            new Vector2(-101.615700f, 11.090220f),
            new Vector2(-101.615700f, -3.149078f),
            new Vector2(-101.615700f, -17.388370f),
            new Vector2(-101.615700f, -31.627670f),
            new Vector2(-101.615700f, -45.866970f),
            new Vector2(-101.615700f, -60.106270f),
            new Vector2(-101.615700f, -74.345570f),
            new Vector2(-101.615700f, -88.584860f),
            new Vector2(-100.232200f, -102.251100f),
            new Vector2(-87.763340f, -108.080700f),
            new Vector2(-73.667270f, -109.457300f),
            new Vector2(-59.567620f, -110.867700f),
            new Vector2(-45.328320f, -110.867700f),
            new Vector2(-31.089030f, -110.867700f),
            new Vector2(-16.849730f, -110.867700f),
            new Vector2(-2.610428f, -110.867700f),
            new Vector2(11.628870f, -110.867700f),
            new Vector2(25.868170f, -110.867700f),
            new Vector2(40.107470f, -110.867700f),
            new Vector2(54.346770f, -110.867700f),
            new Vector2(68.586060f, -110.867700f),
            new Vector2(82.825360f, -110.867700f),
            new Vector2(97.064660f, -110.867700f),
            new Vector2(105.884300f, -105.448000f),
            new Vector2(108.384400f, -91.614430f),
            new Vector2(110.884400f, -77.780840f),
            new Vector2(113.384400f, -64.131730f),
            new Vector2(118.375200f, -50.886120f),
            new Vector2(124.743100f, -38.150070f),
            new Vector2(127.999100f, -24.523340f),
            new Vector2(133.809200f, -12.517970f),
            new Vector2(134.581100f, -2.099899f),
            new Vector2(121.105800f, 1.521599f),
            new Vector2(107.298400f, 4.132301f),
            new Vector2(93.059070f, 4.132301f),
            new Vector2(78.819770f, 4.132301f),
            new Vector2(64.580470f, 4.132301f),
            new Vector2(50.341180f, 4.132301f),
            new Vector2(36.101880f, 4.132301f),
            new Vector2(21.862580f, 4.132301f),
            new Vector2(7.623280f, 4.132301f),
            new Vector2(-6.615587f, 4.132301f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(100.291900f, 105.407400f),
            new Vector2(88.327620f, 107.609800f),
            new Vector2(76.517820f, 110.764300f),
            new Vector2(64.719510f, 113.818700f),
            new Vector2(52.941380f, 116.121400f),
            new Vector2(40.981100f, 117.652500f),
            new Vector2(29.562570f, 121.478400f),
            new Vector2(17.516640f, 123.177900f),
            new Vector2(5.353790f, 124.156900f),
            new Vector2(-3.940575f, 119.258500f),
            new Vector2(-11.048220f, 109.243100f),
            new Vector2(-18.709820f, 99.668560f),
            new Vector2(-26.485900f, 90.239000f),
            new Vector2(-33.965290f, 80.637250f),
            new Vector2(-39.475730f, 69.616380f),
            new Vector2(-46.913820f, 59.984860f),
            new Vector2(-54.070040f, 50.150190f),
            new Vector2(-61.913840f, 40.811110f),
            new Vector2(-69.393240f, 31.209410f),
            new Vector2(-76.872630f, 21.607680f),
            new Vector2(-82.694760f, 10.811450f),
            new Vector2(-89.862430f, 0.985031f),
            new Vector2(-97.227710f, -7.698479f),
            new Vector2(-103.274700f, -17.689300f),
            new Vector2(-109.750900f, -26.518540f),
            new Vector2(-115.409800f, -37.345960f),
            new Vector2(-121.163900f, -48.225440f),
            new Vector2(-129.352900f, -56.567760f),
            new Vector2(-134.515500f, -66.618210f),
            new Vector2(-123.826100f, -72.559400f),
            new Vector2(-111.746700f, -74.052810f),
            new Vector2(-99.425020f, -74.052810f),
            new Vector2(-87.103300f, -74.052810f),
            new Vector2(-74.781590f, -74.052810f),
            new Vector2(-62.459860f, -74.052810f),
            new Vector2(-50.138150f, -74.052810f),
            new Vector2(-37.816420f, -74.052810f),
            new Vector2(-25.494710f, -74.052810f),
            new Vector2(-13.173000f, -74.052810f),
            new Vector2(-0.851284f, -74.052810f),
            new Vector2(11.470430f, -74.052810f),
            new Vector2(23.792140f, -74.052810f),
            new Vector2(35.696050f, -76.627440f),
            new Vector2(48.000900f, -76.731340f),
            new Vector2(60.322620f, -76.731340f),
            new Vector2(72.644330f, -76.731340f),
            new Vector2(84.966050f, -76.731340f),
            new Vector2(94.223740f, -75.475170f),
            new Vector2(97.338420f, -63.888730f),
            new Vector2(98.057390f, -51.736750f),
            new Vector2(101.172100f, -40.150340f),
            new Vector2(105.595300f, -28.679110f),
            new Vector2(110.276300f, -17.314680f),
            new Vector2(115.484500f, -6.168602f),
            new Vector2(111.890700f, 3.181610f),
            new Vector2(100.108000f, 6.302452f),
            new Vector2(88.258240f, 8.301559f),
            new Vector2(76.096920f, 8.980988f),
            new Vector2(63.775200f, 8.980988f),
            new Vector2(51.453490f, 8.980988f),
            new Vector2(39.131770f, 8.980988f),
            new Vector2(27.469570f, 6.134911f),
            new Vector2(15.555330f, 3.623924f),
            new Vector2(3.865891f, 0.945572f),
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
            averaged.Add((firstPoint + secondPoint) / 2f);
        }

        return averaged;
    }
}