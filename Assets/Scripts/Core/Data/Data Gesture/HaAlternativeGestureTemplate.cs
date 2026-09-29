using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template alternatif untuk gesture Ha dengan arah stroke berbeda.
/// </summary>
public class HaAlternativeGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ha;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(90.795720f, -106.231600f),
                new Vector2(93.044930f, -93.353360f),
                new Vector2(94.985650f, -80.171800f),
                new Vector2(96.652630f, -66.900510f),
                new Vector2(97.778970f, -53.584470f),
                new Vector2(99.175670f, -40.313520f),
                new Vector2(99.175670f, -26.904270f),
                new Vector2(99.175670f, -13.495010f),
                new Vector2(99.175670f, -0.085757f),
                new Vector2(99.175670f, 13.323500f),
                new Vector2(100.572300f, 26.594460f),
                new Vector2(102.413100f, 39.844150f),
                new Vector2(103.365500f, 53.159100f),
                new Vector2(103.365500f, 66.568350f),
                new Vector2(103.728700f, 79.941640f),
                new Vector2(104.762200f, 93.248550f),
                new Vector2(103.655300f, 106.322600f),
                new Vector2(93.008600f, 114.030700f),
                new Vector2(79.695690f, 114.438800f),
                new Vector2(66.286430f, 114.438800f),
                new Vector2(52.877180f, 114.438800f),
                new Vector2(39.467930f, 114.438800f),
                new Vector2(26.058670f, 114.438800f),
                new Vector2(12.649410f, 114.438800f),
                new Vector2(0.512169f, 110.622800f),
                new Vector2(-2.071274f, 97.719450f),
                new Vector2(-2.779697f, 84.397410f),
                new Vector2(-2.779697f, 70.988150f),
                new Vector2(-2.779697f, 57.578900f),
                new Vector2(-2.779697f, 44.169640f),
                new Vector2(-2.779697f, 30.760390f),
                new Vector2(-2.779697f, 17.351130f),
                new Vector2(-2.779697f, 3.941878f),
                new Vector2(-2.779697f, -9.467376f),
                new Vector2(-2.779697f, -22.876630f),
                new Vector2(-2.779697f, -36.285880f),
                new Vector2(-4.176315f, -49.468510f),
                new Vector2(-4.176315f, -62.877760f),
                new Vector2(-6.969627f, -75.627600f),
                new Vector2(-10.861210f, -87.776770f),
                new Vector2(-22.550670f, -93.661780f),
                new Vector2(-35.959920f, -93.661780f),
                new Vector2(-49.369170f, -93.661780f),
                new Vector2(-62.606490f, -92.265140f),
                new Vector2(-76.015750f, -92.265140f),
                new Vector2(-89.425000f, -92.265140f),
                new Vector2(-102.607600f, -90.868480f),
                new Vector2(-114.511600f, -89.363200f),
                new Vector2(-110.590900f, -77.171380f),
                new Vector2(-102.441800f, -66.729080f),
                new Vector2(-94.733530f, -56.267890f),
                new Vector2(-86.592440f, -46.189610f),
                new Vector2(-79.163430f, -35.357790f),
                new Vector2(-70.103180f, -25.510430f),
                new Vector2(-61.768030f, -15.668930f),
                new Vector2(-56.122780f, -3.690964f),
                new Vector2(-53.208340f, 5.084455f),
                new Vector2(-65.772090f, 8.909464f),
                new Vector2(-78.762020f, 11.086830f),
                new Vector2(-92.099300f, 11.530410f),
                new Vector2(-105.353900f, 12.483440f),
                new Vector2(-118.677900f, 13.175870f),
                new Vector2(-131.828500f, 15.276760f),
                new Vector2(-145.237800f, 15.276760f)
            }
        };
    }
}
