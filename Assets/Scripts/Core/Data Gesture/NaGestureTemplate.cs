using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Na.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class NaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Na;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-106.920900f, 124.939300f),
            new Vector2(-93.898960f, 123.568500f),
            new Vector2(-80.876990f, 122.197800f),
            new Vector2(-67.855030f, 120.827100f),
            new Vector2(-54.833080f, 119.456300f),
            new Vector2(-41.811120f, 118.085600f),
            new Vector2(-28.741890f, 117.615300f),
            new Vector2(-15.647990f, 117.615300f),
            new Vector2(-2.835999f, 115.878000f),
            new Vector2(9.736359f, 112.253800f),
            new Vector2(22.588680f, 110.291400f),
            new Vector2(35.682590f, 110.291400f),
            new Vector2(48.776500f, 110.291400f),
            new Vector2(61.870410f, 110.291400f),
            new Vector2(74.964320f, 110.291400f),
            new Vector2(88.058240f, 110.291400f),
            new Vector2(101.152100f, 110.291400f),
            new Vector2(114.246000f, 110.291400f),
            new Vector2(121.809800f, 107.331800f),
            new Vector2(113.590300f, 97.393220f),
            new Vector2(102.802500f, 89.978760f),
            new Vector2(92.327380f, 82.122380f),
            new Vector2(82.894160f, 73.064700f),
            new Vector2(72.820370f, 64.745610f),
            new Vector2(62.675720f, 56.508210f),
            new Vector2(53.277570f, 47.419400f),
            new Vector2(42.382750f, 40.156250f),
            new Vector2(31.599910f, 32.756500f),
            new Vector2(22.341170f, 23.497650f),
            new Vector2(10.919280f, 17.239990f),
            new Vector2(0.755890f, 9.236267f),
            new Vector2(-8.502892f, -0.022522f),
            new Vector2(-18.183970f, -8.766205f),
            new Vector2(-29.078790f, -16.029360f),
            new Vector2(-39.578640f, -23.774320f),
            new Vector2(-48.927210f, -32.923610f),
            new Vector2(-59.745900f, -40.279660f),
            new Vector2(-69.004740f, -49.538410f),
            new Vector2(-79.670370f, -57.081130f),
            new Vector2(-89.172040f, -66.043690f),
            new Vector2(-99.518870f, -73.975300f),
            new Vector2(-108.028800f, -83.623380f),
            new Vector2(-117.646500f, -92.163710f),
            new Vector2(-126.217100f, -101.751100f),
            new Vector2(-121.046800f, -108.379900f),
            new Vector2(-108.624900f, -112.520500f),
            new Vector2(-96.202860f, -116.661100f),
            new Vector2(-83.123420f, -116.750300f),
            new Vector2(-70.029520f, -116.750300f),
            new Vector2(-56.935620f, -116.750300f),
            new Vector2(-43.841710f, -116.750300f),
            new Vector2(-30.747810f, -116.750300f),
            new Vector2(-17.653900f, -116.750300f),
            new Vector2(-5.114784f, -113.331400f),
            new Vector2(7.345451f, -109.426500f),
            new Vector2(20.439360f, -109.426500f),
            new Vector2(32.917950f, -105.593400f),
            new Vector2(45.620910f, -102.417700f),
            new Vector2(58.446980f, -99.789470f),
            new Vector2(71.407330f, -98.440580f),
            new Vector2(84.501240f, -98.440580f),
            new Vector2(97.595150f, -98.440580f),
            new Vector2(110.689100f, -98.440580f),
            new Vector2(123.782900f, -98.440580f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-117.234000f, 127.084400f),
            new Vector2(-105.887700f, 123.495000f),
            new Vector2(-94.281250f, 121.060500f),
            new Vector2(-82.375090f, 121.060500f),
            new Vector2(-70.468900f, 121.060500f),
            new Vector2(-58.562740f, 121.060500f),
            new Vector2(-46.656590f, 121.060500f),
            new Vector2(-34.750400f, 121.060500f),
            new Vector2(-22.844220f, 121.060500f),
            new Vector2(-10.938050f, 121.060500f),
            new Vector2(0.968094f, 121.060500f),
            new Vector2(12.874250f, 121.060500f),
            new Vector2(24.780410f, 121.060500f),
            new Vector2(36.686550f, 121.060500f),
            new Vector2(48.592730f, 121.060500f),
            new Vector2(60.498890f, 121.060500f),
            new Vector2(72.405040f, 121.060500f),
            new Vector2(84.311220f, 121.060500f),
            new Vector2(91.765230f, 119.216300f),
            new Vector2(83.129520f, 111.061800f),
            new Vector2(73.223010f, 104.457400f),
            new Vector2(63.794630f, 97.269810f),
            new Vector2(55.825970f, 88.481750f),
            new Vector2(48.611980f, 79.075040f),
            new Vector2(40.669710f, 70.265350f),
            new Vector2(33.876310f, 60.492580f),
            new Vector2(26.778760f, 50.935000f),
            new Vector2(19.859600f, 41.286560f),
            new Vector2(11.622470f, 32.718600f),
            new Vector2(5.018097f, 22.812100f),
            new Vector2(-1.586227f, 12.905550f),
            new Vector2(-8.850632f, 3.540161f),
            new Vector2(-16.742540f, -5.310852f),
            new Vector2(-23.346910f, -15.217380f),
            new Vector2(-29.951280f, -25.123890f),
            new Vector2(-35.846590f, -35.441880f),
            new Vector2(-43.107700f, -44.381550f),
            new Vector2(-51.328400f, -52.671690f),
            new Vector2(-57.932800f, -62.578220f),
            new Vector2(-64.537170f, -72.484730f),
            new Vector2(-72.546170f, -81.239730f),
            new Vector2(-79.693570f, -90.701080f),
            new Vector2(-88.902100f, -98.003450f),
            new Vector2(-95.914250f, -107.502200f),
            new Vector2(-91.239290f, -117.366000f),
            new Vector2(-79.645480f, -119.903500f),
            new Vector2(-67.778200f, -120.219300f),
            new Vector2(-56.203950f, -122.915500f),
            new Vector2(-44.297820f, -122.915500f),
            new Vector2(-32.391660f, -122.915500f),
            new Vector2(-20.485470f, -122.915500f),
            new Vector2(-8.583603f, -122.872200f),
            new Vector2(3.091339f, -120.537200f),
            new Vector2(14.934750f, -119.903500f),
            new Vector2(26.840910f, -119.903500f),
            new Vector2(38.747070f, -119.903500f),
            new Vector2(50.653230f, -119.903500f),
            new Vector2(62.559390f, -119.903500f),
            new Vector2(74.465560f, -119.903500f),
            new Vector2(86.371720f, -119.903500f),
            new Vector2(97.958630f, -117.310200f),
            new Vector2(109.813200f, -116.891400f),
            new Vector2(121.419700f, -114.456900f),
            new Vector2(132.766000f, -110.867300f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-106.033400f, 127.112900f),
            new Vector2(-94.766180f, 127.112900f),
            new Vector2(-83.788520f, 125.328600f),
            new Vector2(-72.965900f, 122.224400f),
            new Vector2(-61.928730f, 120.356100f),
            new Vector2(-50.661540f, 120.356100f),
            new Vector2(-39.394340f, 120.356100f),
            new Vector2(-28.127150f, 120.356100f),
            new Vector2(-16.859960f, 120.356100f),
            new Vector2(-5.592766f, 120.356100f),
            new Vector2(5.674423f, 120.356100f),
            new Vector2(16.941620f, 120.356100f),
            new Vector2(28.208810f, 120.356100f),
            new Vector2(39.476010f, 120.356100f),
            new Vector2(50.743190f, 120.356100f),
            new Vector2(62.010390f, 120.356100f),
            new Vector2(73.277580f, 120.356100f),
            new Vector2(84.544770f, 120.356100f),
            new Vector2(86.640560f, 115.447700f),
            new Vector2(79.630760f, 106.695800f),
            new Vector2(71.663560f, 98.728770f),
            new Vector2(63.696550f, 90.761570f),
            new Vector2(58.359660f, 80.898690f),
            new Vector2(51.688920f, 71.997210f),
            new Vector2(44.211540f, 63.628620f),
            new Vector2(37.961640f, 54.253740f),
            new Vector2(31.500550f, 45.051990f),
            new Vector2(23.533380f, 37.084940f),
            new Vector2(15.566300f, 29.117800f),
            new Vector2(9.293888f, 19.929210f),
            new Vector2(3.223896f, 10.761810f),
            new Vector2(-5.661308f, 4.022240f),
            new Vector2(-11.949710f, -5.325401f),
            new Vector2(-18.710080f, -14.339100f),
            new Vector2(-25.176200f, -23.561010f),
            new Vector2(-32.276610f, -32.238630f),
            new Vector2(-39.860490f, -40.519910f),
            new Vector2(-46.182950f, -49.843460f),
            new Vector2(-52.943280f, -58.857190f),
            new Vector2(-59.375500f, -68.103090f),
            new Vector2(-66.590050f, -76.687150f),
            new Vector2(-74.059750f, -85.062040f),
            new Vector2(-80.057120f, -94.583470f),
            new Vector2(-85.096020f, -104.661100f),
            new Vector2(-91.185570f, -114.129100f),
            new Vector2(-83.922460f, -118.279700f),
            new Vector2(-72.991680f, -121.012400f),
            new Vector2(-61.955280f, -122.887100f),
            new Vector2(-50.688080f, -122.887100f),
            new Vector2(-39.420890f, -122.887100f),
            new Vector2(-28.153690f, -122.887100f),
            new Vector2(-16.886510f, -122.887100f),
            new Vector2(-5.619316f, -122.887100f),
            new Vector2(5.647881f, -122.887100f),
            new Vector2(16.673720f, -120.926600f),
            new Vector2(27.604500f, -118.193900f),
            new Vector2(38.617660f, -116.130400f),
            new Vector2(49.858540f, -115.968300f),
            new Vector2(60.603810f, -112.752000f),
            new Vector2(71.870990f, -112.752000f),
            new Vector2(83.138190f, -112.752000f),
            new Vector2(94.405380f, -112.752000f),
            new Vector2(105.672600f, -112.752000f),
            new Vector2(116.939800f, -112.752000f),
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
