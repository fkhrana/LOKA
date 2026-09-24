using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Wa (single stroke).
/// </summary>
public class WaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Wa;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(94.262380f, 136.882800f),
                new Vector2(80.489300f, 136.882800f),
                new Vector2(67.131970f, 139.444700f),
                new Vector2(53.393890f, 139.660500f),
                new Vector2(39.620800f, 139.660500f),
                new Vector2(25.847710f, 139.660500f),
                new Vector2(12.074620f, 139.660500f),
                new Vector2(-1.698467f, 139.660500f),
                new Vector2(-15.471550f, 139.660500f),
                new Vector2(-29.244640f, 139.660500f),
                new Vector2(-43.017730f, 139.660500f),
                new Vector2(-54.855140f, 134.987400f),
                new Vector2(-58.515290f, 122.235500f),
                new Vector2(-58.515290f, 108.462400f),
                new Vector2(-66.146770f, 98.344780f),
                new Vector2(-69.626400f, 85.661200f),
                new Vector2(-72.404210f, 72.543880f),
                new Vector2(-75.181980f, 59.426530f),
                new Vector2(-80.737560f, 51.209020f),
                new Vector2(-83.515320f, 38.091670f),
                new Vector2(-85.407400f, 24.765240f),
                new Vector2(-86.293080f, 11.201230f),
                new Vector2(-91.848620f, 0.861649f),
                new Vector2(-94.626430f, -12.255680f),
                new Vector2(-100.182000f, -23.727590f),
                new Vector2(-102.622200f, -37.104680f),
                new Vector2(-105.220800f, -50.456080f),
                new Vector2(-108.515300f, -63.489560f),
                new Vector2(-111.114600f, -76.649040f),
                new Vector2(-111.293100f, -90.379970f),
                new Vector2(-101.919300f, -98.359700f),
                new Vector2(-89.013750f, -102.034700f),
                new Vector2(-75.889680f, -104.783900f),
                new Vector2(-62.116580f, -104.783900f),
                new Vector2(-48.343500f, -104.783900f),
                new Vector2(-34.570410f, -104.783900f),
                new Vector2(-20.797320f, -104.783900f),
                new Vector2(-7.181650f, -105.450800f),
                new Vector2(6.017155f, -107.883500f),
                new Vector2(19.210450f, -110.339500f),
                new Vector2(32.983540f, -110.339500f),
                new Vector2(46.756620f, -110.339500f),
                new Vector2(60.529710f, -110.339500f),
                new Vector2(74.302800f, -110.339500f),
                new Vector2(84.642420f, -104.783900f),
                new Vector2(97.759760f, -102.006100f),
                new Vector2(108.151400f, -96.502660f),
                new Vector2(108.151400f, -82.729570f),
                new Vector2(108.151400f, -68.956480f),
                new Vector2(110.660200f, -55.590500f),
                new Vector2(113.706900f, -42.516820f),
                new Vector2(115.792700f, -29.082200f),
                new Vector2(116.484700f, -15.421410f),
                new Vector2(116.164700f, -1.846123f),
                new Vector2(103.009500f, 0.771631f),
                new Vector2(90.918540f, 6.327173f),
                new Vector2(77.193010f, 6.528614f),
                new Vector2(65.386050f, 11.302150f),
                new Vector2(53.688570f, 14.660540f),
                new Vector2(39.915480f, 14.660540f),
                new Vector2(26.142400f, 14.660540f),
                new Vector2(12.369310f, 14.660540f),
                new Vector2(-0.953003f, 11.882770f),
                new Vector2(-14.070320f, 9.104944f)
            }
        };
    }
}
