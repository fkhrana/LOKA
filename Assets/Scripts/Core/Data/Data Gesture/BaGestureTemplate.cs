using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Ba.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class BaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ba;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-45.225250f, 133.671100f),
            new Vector2(-59.054140f, 131.418900f),
            new Vector2(-72.850710f, 128.967100f),
            new Vector2(-84.455840f, 121.467500f),
            new Vector2(-90.238830f, 109.022000f),
            new Vector2(-90.950230f, 94.915180f),
            new Vector2(-94.917760f, 81.297440f),
            new Vector2(-97.027040f, 67.362690f),
            new Vector2(-100.433200f, 53.632800f),
            new Vector2(-102.073000f, 39.704480f),
            new Vector2(-106.036100f, 26.153200f),
            new Vector2(-108.913600f, 12.425750f),
            new Vector2(-111.096600f, -1.436188f),
            new Vector2(-115.069900f, -15.052260f),
            new Vector2(-117.422000f, -28.864980f),
            new Vector2(-119.549600f, -42.714130f),
            new Vector2(-123.550400f, -56.259290f),
            new Vector2(-125.471700f, -70.197460f),
            new Vector2(-128.558600f, -84.011840f),
            new Vector2(-119.654200f, -89.301800f),
            new Vector2(-105.459700f, -89.301800f),
            new Vector2(-91.265320f, -89.301800f),
            new Vector2(-77.070920f, -89.301800f),
            new Vector2(-62.876500f, -89.301800f),
            new Vector2(-48.682070f, -89.301800f),
            new Vector2(-35.154300f, -93.409580f),
            new Vector2(-21.389740f, -96.058590f),
            new Vector2(-7.560852f, -98.310870f),
            new Vector2(6.283905f, -96.156140f),
            new Vector2(6.694305f, -82.191750f),
            new Vector2(8.995605f, -68.364260f),
            new Vector2(11.081090f, -54.426590f),
            new Vector2(13.333310f, -40.597670f),
            new Vector2(15.585570f, -26.768760f),
            new Vector2(15.585570f, -12.574360f),
            new Vector2(15.585570f, 1.620041f),
            new Vector2(15.585570f, 15.814450f),
            new Vector2(21.618190f, 18.726430f),
            new Vector2(26.445950f, 5.693649f),
            new Vector2(33.698940f, -6.063889f),
            new Vector2(41.869930f, -17.254090f),
            new Vector2(48.848660f, -29.181120f),
            new Vector2(55.827330f, -41.108190f),
            new Vector2(65.362700f, -51.468750f),
            new Vector2(72.914200f, -63.297130f),
            new Vector2(81.713490f, -74.348720f),
            new Vector2(91.618090f, -70.663830f),
            new Vector2(96.106690f, -57.197810f),
            new Vector2(96.666580f, -43.094260f),
            new Vector2(100.810700f, -29.572360f),
            new Vector2(103.162600f, -15.759610f),
            new Vector2(104.802400f, -1.831314f),
            new Vector2(107.154400f, 11.981410f),
            new Vector2(107.927900f, 26.050310f),
            new Vector2(108.757400f, 40.142590f),
            new Vector2(112.200100f, 53.913180f),
            new Vector2(115.883800f, 67.594790f),
            new Vector2(119.469100f, 81.296230f),
            new Vector2(121.441400f, 95.327420f),
            new Vector2(121.441400f, 109.521900f),
            new Vector2(120.135000f, 122.409900f),
            new Vector2(106.273800f, 120.357000f),
            new Vector2(92.477220f, 117.905300f),
            new Vector2(78.648590f, 115.653100f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-72.716600f, 110.522600f),
            new Vector2(-86.492280f, 111.836200f),
            new Vector2(-99.757490f, 106.844600f),
            new Vector2(-112.959800f, 101.701300f),
            new Vector2(-126.432400f, 97.210450f),
            new Vector2(-127.834700f, 84.019740f),
            new Vector2(-126.790300f, 69.987840f),
            new Vector2(-124.166900f, 56.212170f),
            new Vector2(-123.897700f, 42.054470f),
            new Vector2(-123.897700f, 27.853100f),
            new Vector2(-123.897700f, 13.651710f),
            new Vector2(-123.897700f, -0.549675f),
            new Vector2(-123.897700f, -14.751060f),
            new Vector2(-123.897700f, -28.952440f),
            new Vector2(-123.897700f, -43.153830f),
            new Vector2(-123.897700f, -57.355210f),
            new Vector2(-123.897700f, -71.556590f),
            new Vector2(-123.897700f, -85.757980f),
            new Vector2(-116.491100f, -94.201790f),
            new Vector2(-102.289700f, -94.201790f),
            new Vector2(-88.088290f, -94.201790f),
            new Vector2(-74.206340f, -92.233310f),
            new Vector2(-61.197020f, -88.296280f),
            new Vector2(-46.995640f, -88.296280f),
            new Vector2(-32.794250f, -88.296280f),
            new Vector2(-18.912310f, -90.264780f),
            new Vector2(-8.516006f, -82.702370f),
            new Vector2(-3.716309f, -70.271720f),
            new Vector2(0.774582f, -56.799110f),
            new Vector2(2.086540f, -42.810630f),
            new Vector2(4.130836f, -28.938020f),
            new Vector2(6.023521f, -14.969640f),
            new Vector2(9.960564f, -1.407150f),
            new Vector2(12.670910f, 12.354400f),
            new Vector2(19.338140f, 7.586769f),
            new Vector2(26.864990f, -4.401009f),
            new Vector2(35.188380f, -15.901810f),
            new Vector2(43.508040f, -27.355590f),
            new Vector2(49.993060f, -39.765000f),
            new Vector2(56.688960f, -51.831860f),
            new Vector2(65.242280f, -62.951130f),
            new Vector2(73.771510f, -74.185980f),
            new Vector2(82.001330f, -85.533940f),
            new Vector2(90.669200f, -89.036030f),
            new Vector2(90.669200f, -74.834650f),
            new Vector2(91.324580f, -60.739620f),
            new Vector2(94.606190f, -47.070760f),
            new Vector2(94.703850f, -32.885230f),
            new Vector2(98.543230f, -19.229780f),
            new Vector2(102.328800f, -5.922050f),
            new Vector2(106.417200f, 7.051590f),
            new Vector2(109.474100f, 20.723920f),
            new Vector2(112.322700f, 34.376720f),
            new Vector2(115.144000f, 48.120260f),
            new Vector2(118.848400f, 61.821910f),
            new Vector2(122.165300f, 75.614960f),
            new Vector2(122.165300f, 89.816350f),
            new Vector2(122.165300f, 104.017700f),
            new Vector2(122.165300f, 118.219100f),
            new Vector2(119.952400f, 130.207600f),
            new Vector2(106.172100f, 127.993900f),
            new Vector2(93.657680f, 123.329700f),
            new Vector2(80.548550f, 120.086900f),
            new Vector2(67.047330f, 118.396600f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-59.767040f, 94.666350f),
            new Vector2(-71.479000f, 99.336360f),
            new Vector2(-83.876010f, 101.842600f),
            new Vector2(-93.530630f, 99.086430f),
            new Vector2(-95.679210f, 86.603300f),
            new Vector2(-98.359990f, 74.257110f),
            new Vector2(-100.800500f, 61.853780f),
            new Vector2(-103.541800f, 49.407930f),
            new Vector2(-105.728300f, 36.929440f),
            new Vector2(-108.962800f, 24.701840f),
            new Vector2(-110.771400f, 12.191090f),
            new Vector2(-113.771500f, 0.021851f),
            new Vector2(-114.920500f, -12.584370f),
            new Vector2(-114.920500f, -25.332030f),
            new Vector2(-114.920500f, -38.079690f),
            new Vector2(-114.920500f, -50.827350f),
            new Vector2(-118.950000f, -62.797860f),
            new Vector2(-127.071200f, -71.617280f),
            new Vector2(-118.975200f, -76.877300f),
            new Vector2(-106.565000f, -79.777110f),
            new Vector2(-93.843100f, -79.986290f),
            new Vector2(-81.095440f, -79.986290f),
            new Vector2(-68.347760f, -79.986290f),
            new Vector2(-55.600110f, -79.986290f),
            new Vector2(-42.852440f, -79.986290f),
            new Vector2(-30.104780f, -79.986290f),
            new Vector2(-20.759580f, -75.628490f),
            new Vector2(-15.181940f, -64.814990f),
            new Vector2(-12.361710f, -52.673320f),
            new Vector2(-11.237360f, -40.108120f),
            new Vector2(-8.183712f, -27.746070f),
            new Vector2(-5.968040f, -15.313220f),
            new Vector2(-4.280586f, -2.818306f),
            new Vector2(0.929947f, 4.094574f),
            new Vector2(8.523613f, -5.797192f),
            new Vector2(17.874760f, -14.327670f),
            new Vector2(26.259550f, -23.865890f),
            new Vector2(36.016750f, -31.662900f),
            new Vector2(46.181690f, -39.086900f),
            new Vector2(56.325560f, -46.725200f),
            new Vector2(64.139670f, -56.722800f),
            new Vector2(70.839620f, -67.347230f),
            new Vector2(78.406070f, -77.547870f),
            new Vector2(85.477220f, -76.414150f),
            new Vector2(89.220870f, -64.295010f),
            new Vector2(91.197970f, -51.790740f),
            new Vector2(93.924180f, -39.457780f),
            new Vector2(96.865280f, -27.187390f),
            new Vector2(99.457280f, -14.815350f),
            new Vector2(102.443600f, -2.606647f),
            new Vector2(106.336500f, 9.269478f),
            new Vector2(110.691700f, 21.185840f),
            new Vector2(113.121400f, 33.605130f),
            new Vector2(116.480900f, 45.894220f),
            new Vector2(119.481700f, 58.182440f),
            new Vector2(122.876400f, 70.379240f),
            new Vector2(122.928900f, 83.118380f),
            new Vector2(122.928900f, 95.866040f),
            new Vector2(119.509200f, 106.156700f),
            new Vector2(107.263000f, 109.498600f),
            new Vector2(94.528250f, 109.603700f),
            new Vector2(81.780590f, 109.603700f),
            new Vector2(69.032930f, 109.603700f),
            new Vector2(56.285260f, 109.603700f),
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
