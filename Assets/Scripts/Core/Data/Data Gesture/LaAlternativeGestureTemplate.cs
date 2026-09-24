using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template alternatif untuk gesture La dengan arah stroke berbeda.
/// </summary>
public class LaAlternativeGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.La;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(125.488800f, 72.079700f),
                new Vector2(122.274500f, 61.323350f),
                new Vector2(119.192900f, 51.078000f),
                new Vector2(117.085400f, 40.716610f),
                new Vector2(114.908800f, 30.025030f),
                new Vector2(110.640300f, 21.374360f),
                new Vector2(108.682000f, 10.602310f),
                new Vector2(106.581200f, -0.136110f),
                new Vector2(102.056500f, -8.721216f),
                new Vector2(100.278700f, -19.667040f),
                new Vector2(97.265460f, -30.129190f),
                new Vector2(93.976110f, -40.367580f),
                new Vector2(90.788210f, -50.929590f),
                new Vector2(88.048320f, -60.273260f),
                new Vector2(83.471920f, -68.536100f),
                new Vector2(79.270290f, -78.030080f),
                new Vector2(77.169360f, -88.768420f),
                new Vector2(68.952470f, -91.785870f),
                new Vector2(57.718130f, -91.785870f),
                new Vector2(46.483800f, -91.785870f),
                new Vector2(35.745410f, -89.685040f),
                new Vector2(27.107850f, -85.483350f),
                new Vector2(16.108200f, -84.037180f),
                new Vector2(5.060416f, -83.042320f),
                new Vector2(-5.653244f, -80.836720f),
                new Vector2(-7.138406f, -71.874090f),
                new Vector2(-6.864254f, -60.673510f),
                new Vector2(-4.763364f, -49.935130f),
                new Vector2(-4.763364f, -38.700810f),
                new Vector2(-4.763364f, -27.466470f),
                new Vector2(-4.763364f, -16.232140f),
                new Vector2(-2.662511f, -5.493753f),
                new Vector2(-0.561733f, 5.244661f),
                new Vector2(-0.561733f, 16.478990f),
                new Vector2(-0.561733f, 27.713330f),
                new Vector2(-0.561733f, 38.947660f),
                new Vector2(1.936045f, 49.565870f),
                new Vector2(5.818427f, 59.629880f),
                new Vector2(7.841677f, 70.386580f),
                new Vector2(9.186523f, 80.735090f),
                new Vector2(-1.622572f, 83.114340f),
                new Vector2(-12.114440f, 86.259450f),
                new Vector2(-22.606320f, 89.404550f),
                new Vector2(-33.467020f, 90.987280f),
                new Vector2(-44.701350f, 90.987280f),
                new Vector2(-55.935680f, 90.987280f),
                new Vector2(-67.170010f, 90.987280f),
                new Vector2(-76.020030f, 87.129410f),
                new Vector2(-82.494500f, 79.308700f),
                new Vector2(-85.591320f, 68.982860f),
                new Vector2(-89.914660f, 59.340190f),
                new Vector2(-91.180710f, 48.404730f),
                new Vector2(-93.865360f, 38.466260f),
                new Vector2(-97.200360f, 30.566930f),
                new Vector2(-99.301210f, 19.828540f),
                new Vector2(-101.402000f, 9.090136f),
                new Vector2(-107.015000f, -0.070641f),
                new Vector2(-109.220600f, -10.784310f),
                new Vector2(-111.426200f, -21.497970f),
                new Vector2(-113.663700f, -30.861480f),
                new Vector2(-116.230200f, -39.631790f),
                new Vector2(-118.555800f, -50.463130f),
                new Vector2(-120.309600f, -61.283440f),
                new Vector2(-124.511300f, -70.777400f)
            }
        };
    }
}
