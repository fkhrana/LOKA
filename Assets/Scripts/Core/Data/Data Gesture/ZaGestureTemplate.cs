using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Za.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class ZaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Za;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-87.091100f, 119.629400f),
            new Vector2(-75.940970f, 121.250300f),
            new Vector2(-64.527790f, 121.250300f),
            new Vector2(-53.114620f, 121.250300f),
            new Vector2(-41.701450f, 121.250300f),
            new Vector2(-30.288270f, 121.250300f),
            new Vector2(-18.875090f, 121.250300f),
            new Vector2(-7.661469f, 122.871200f),
            new Vector2(3.718803f, 122.539000f),
            new Vector2(15.004360f, 121.250300f),
            new Vector2(26.154490f, 122.871200f),
            new Vector2(36.025150f, 119.522500f),
            new Vector2(30.978050f, 109.392700f),
            new Vector2(25.873920f, 99.184480f),
            new Vector2(19.347330f, 89.913250f),
            new Vector2(12.167690f, 81.112730f),
            new Vector2(6.100876f, 71.804150f),
            new Vector2(-1.969437f, 63.733800f),
            new Vector2(-9.149086f, 54.933290f),
            new Vector2(-16.226500f, 46.007740f),
            new Vector2(-22.535530f, 36.497570f),
            new Vector2(-28.890230f, 27.087710f),
            new Vector2(-36.380190f, 18.497920f),
            new Vector2(-43.310480f, 9.431211f),
            new Vector2(-49.350260f, -0.242704f),
            new Vector2(-55.749870f, -9.674675f),
            new Vector2(-62.237970f, -19.047150f),
            new Vector2(-68.327950f, -28.558940f),
            new Vector2(-74.394800f, -37.867500f),
            new Vector2(-82.414120f, -45.979660f),
            new Vector2(-87.641230f, -55.976620f),
            new Vector2(-94.552270f, -64.997340f),
            new Vector2(-100.883200f, -74.493660f),
            new Vector2(-106.925000f, -84.131360f),
            new Vector2(-100.663200f, -87.844250f),
            new Vector2(-89.250010f, -87.844250f),
            new Vector2(-77.836830f, -87.844250f),
            new Vector2(-66.423650f, -87.844250f),
            new Vector2(-55.010480f, -87.844250f),
            new Vector2(-43.597300f, -87.844250f),
            new Vector2(-32.184140f, -87.844250f),
            new Vector2(-20.847150f, -87.374760f),
            new Vector2(-9.620834f, -86.223360f),
            new Vector2(1.792343f, -86.223360f),
            new Vector2(12.422160f, -83.636120f),
            new Vector2(16.994090f, -73.544170f),
            new Vector2(21.673420f, -63.200930f),
            new Vector2(26.875220f, -53.049360f),
            new Vector2(33.904820f, -44.125830f),
            new Vector2(42.305390f, -36.699170f),
            new Vector2(51.429280f, -29.995850f),
            new Vector2(60.108290f, -23.159420f),
            new Vector2(69.128360f, -16.320460f),
            new Vector2(79.955840f, -12.711260f),
            new Vector2(91.198820f, -11.662500f),
            new Vector2(102.612000f, -11.662500f),
            new Vector2(112.819400f, -15.445570f),
            new Vector2(119.713400f, -24.242920f),
            new Vector2(123.322600f, -35.070400f),
            new Vector2(126.728500f, -45.494750f),
            new Vector2(130.491500f, -56.193530f),
            new Vector2(134.463000f, -66.870920f),
            new Vector2(139.188800f, -77.152270f),
            new Vector2(143.075000f, -87.844250f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-99.574580f, 137.047000f),
            new Vector2(-88.406520f, 137.047000f),
            new Vector2(-77.238480f, 137.047000f),
            new Vector2(-66.070430f, 137.047000f),
            new Vector2(-54.981660f, 136.403000f),
            new Vector2(-43.992950f, 134.946200f),
            new Vector2(-32.824890f, 134.946200f),
            new Vector2(-21.997760f, 137.047000f),
            new Vector2(-11.170640f, 139.147900f),
            new Vector2(-6.257713f, 134.264000f),
            new Vector2(-11.661720f, 124.712300f),
            new Vector2(-16.644550f, 114.935100f),
            new Vector2(-21.888810f, 105.398600f),
            new Vector2(-27.244960f, 95.931270f),
            new Vector2(-31.035190f, 85.574420f),
            new Vector2(-36.418080f, 76.253920f),
            new Vector2(-39.145290f, 65.926240f),
            new Vector2(-43.367290f, 55.668930f),
            new Vector2(-47.705110f, 45.407910f),
            new Vector2(-51.967380f, 35.138530f),
            new Vector2(-56.639830f, 25.172650f),
            new Vector2(-59.989430f, 14.536610f),
            new Vector2(-64.654850f, 4.411301f),
            new Vector2(-68.186510f, -6.183642f),
            new Vector2(-72.891020f, -16.204950f),
            new Vector2(-77.056150f, -26.490070f),
            new Vector2(-80.116680f, -37.222210f),
            new Vector2(-84.646790f, -47.383010f),
            new Vector2(-89.641250f, -57.372040f),
            new Vector2(-94.236330f, -67.526490f),
            new Vector2(-99.574170f, -77.238040f),
            new Vector2(-97.911300f, -86.050580f),
            new Vector2(-87.255630f, -87.742870f),
            new Vector2(-76.087590f, -87.742870f),
            new Vector2(-64.919530f, -87.742870f),
            new Vector2(-53.751480f, -87.742870f),
            new Vector2(-42.583440f, -87.742870f),
            new Vector2(-31.415390f, -87.742870f),
            new Vector2(-20.483170f, -85.827130f),
            new Vector2(-9.548424f, -84.344790f),
            new Vector2(0.196617f, -79.133150f),
            new Vector2(11.177080f, -77.238670f),
            new Vector2(22.345120f, -77.238670f),
            new Vector2(33.036920f, -75.665710f),
            new Vector2(39.013030f, -66.937850f),
            new Vector2(43.148950f, -56.564250f),
            new Vector2(47.931110f, -46.486340f),
            new Vector2(52.878720f, -36.583930f),
            new Vector2(57.929440f, -26.877490f),
            new Vector2(66.351040f, -19.829300f),
            new Vector2(76.164980f, -14.700810f),
            new Vector2(86.657460f, -10.959650f),
            new Vector2(95.931280f, -5.810088f),
            new Vector2(107.002400f, -6.789332f),
            new Vector2(116.463300f, -11.764010f),
            new Vector2(125.326200f, -18.481680f),
            new Vector2(134.902700f, -24.227610f),
            new Vector2(143.889400f, -30.786670f),
            new Vector2(148.711400f, -40.584130f),
            new Vector2(150.425400f, -51.474040f),
            new Vector2(150.425400f, -62.642090f),
            new Vector2(150.425400f, -73.810130f),
            new Vector2(150.425400f, -84.978180f),
            new Vector2(150.425400f, -96.146210f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-108.585600f, 110.885100f),
            new Vector2(-97.434660f, 110.885100f),
            new Vector2(-86.283750f, 110.885100f),
            new Vector2(-75.132840f, 110.885100f),
            new Vector2(-63.981930f, 110.885100f),
            new Vector2(-52.831020f, 110.885100f),
            new Vector2(-41.888320f, 109.193700f),
            new Vector2(-30.932990f, 107.130400f),
            new Vector2(-19.880330f, 105.748100f),
            new Vector2(-8.729416f, 105.748100f),
            new Vector2(2.421494f, 105.748100f),
            new Vector2(6.121361f, 98.879800f),
            new Vector2(-1.763458f, 90.994850f),
            new Vector2(-7.658154f, 81.675540f),
            new Vector2(-13.268890f, 72.353320f),
            new Vector2(-18.328400f, 62.609060f),
            new Vector2(-24.320870f, 54.108230f),
            new Vector2(-31.515100f, 45.832340f),
            new Vector2(-36.522270f, 35.979060f),
            new Vector2(-44.958000f, 28.874310f),
            new Vector2(-50.767040f, 19.731030f),
            new Vector2(-56.911450f, 10.770500f),
            new Vector2(-64.420250f, 2.653183f),
            new Vector2(-71.364200f, -6.003082f),
            new Vector2(-76.998600f, -15.535790f),
            new Vector2(-83.184040f, -24.813900f),
            new Vector2(-89.369450f, -34.092020f),
            new Vector2(-93.350910f, -44.345100f),
            new Vector2(-99.430630f, -53.430790f),
            new Vector2(-103.448600f, -63.676950f),
            new Vector2(-102.988000f, -72.768650f),
            new Vector2(-91.963640f, -74.046390f),
            new Vector2(-80.812730f, -74.046390f),
            new Vector2(-69.661820f, -74.046390f),
            new Vector2(-58.510910f, -74.046390f),
            new Vector2(-47.359990f, -74.046390f),
            new Vector2(-36.209080f, -74.046390f),
            new Vector2(-25.058170f, -74.046390f),
            new Vector2(-13.907260f, -74.046390f),
            new Vector2(-2.950851f, -75.626340f),
            new Vector2(7.867126f, -78.330810f),
            new Vector2(18.913080f, -79.183390f),
            new Vector2(28.072270f, -74.374920f),
            new Vector2(30.992540f, -63.679130f),
            new Vector2(35.268750f, -53.442220f),
            new Vector2(38.767040f, -42.855400f),
            new Vector2(42.867260f, -32.654870f),
            new Vector2(49.052670f, -23.376750f),
            new Vector2(54.733510f, -14.253800f),
            new Vector2(63.806750f, -7.818474f),
            new Vector2(73.316150f, -2.436119f),
            new Vector2(82.427390f, 3.952858f),
            new Vector2(92.422330f, 7.225647f),
            new Vector2(103.331300f, 7.392204f),
            new Vector2(113.155500f, 2.441292f),
            new Vector2(122.241300f, -4.015896f),
            new Vector2(132.221300f, -8.346382f),
            new Vector2(137.043200f, -18.124290f),
            new Vector2(140.569400f, -28.702990f),
            new Vector2(141.414400f, -39.716760f),
            new Vector2(141.414400f, -50.867670f),
            new Vector2(141.414400f, -62.018590f),
            new Vector2(141.414400f, -73.169490f),
            new Vector2(141.414400f, -84.320400f),
        };

        return new List<List<Vector2>>
        {
            AverageStroke(firstStroke, secondStroke, thirdStroke)
        };
    }

    private List<Vector2> AverageStroke(List<Vector2> first, List<Vector2> second, List<Vector2> third)
    {
        int count = Mathf.Max(first.Count, Mathf.Max(second.Count, third.Count));
        var averaged = new List<Vector2>(count);

        for (int i = 0; i < count; i++)
        {
            Vector2 firstPoint = i < first.Count ? first[i] : first[first.Count - 1];
            Vector2 secondPoint = i < second.Count ? second[i] : second[second.Count - 1];
            Vector2 thirdPoint = i < third.Count ? third[i] : third[third.Count - 1];
            averaged.Add((firstPoint + secondPoint + thirdPoint) / 3f);
        }

        return averaged;
    }
}
