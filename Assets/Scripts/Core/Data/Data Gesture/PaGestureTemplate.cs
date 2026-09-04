using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Pa.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class PaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Pa;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-127.288800f, 88.573520f),
            new Vector2(-115.684700f, 87.161100f),
            new Vector2(-104.256900f, 84.662710f),
            new Vector2(-92.969040f, 81.511340f),
            new Vector2(-81.225110f, 81.511340f),
            new Vector2(-69.481190f, 81.511340f),
            new Vector2(-57.737270f, 81.511340f),
            new Vector2(-45.993340f, 81.511340f),
            new Vector2(-40.107540f, 76.105830f),
            new Vector2(-44.240880f, 65.118260f),
            new Vector2(-48.782030f, 54.319730f),
            new Vector2(-53.363330f, 43.854890f),
            new Vector2(-58.146870f, 33.354440f),
            new Vector2(-63.398930f, 22.850360f),
            new Vector2(-67.994780f, 12.218200f),
            new Vector2(-72.203990f, 1.554217f),
            new Vector2(-76.494900f, -9.098583f),
            new Vector2(-81.357380f, -19.394990f),
            new Vector2(-86.253340f, -29.958050f),
            new Vector2(-92.119140f, -40.098670f),
            new Vector2(-99.311630f, -49.246560f),
            new Vector2(-104.459700f, -59.501380f),
            new Vector2(-110.139600f, -69.756050f),
            new Vector2(-116.528000f, -79.607320f),
            new Vector2(-108.988200f, -85.155300f),
            new Vector2(-97.244280f, -85.155300f),
            new Vector2(-85.500370f, -85.155300f),
            new Vector2(-73.930310f, -86.567740f),
            new Vector2(-62.186390f, -86.567740f),
            new Vector2(-50.442470f, -86.567740f),
            new Vector2(-38.698550f, -86.567740f),
            new Vector2(-26.954620f, -86.567740f),
            new Vector2(-15.210700f, -86.567740f),
            new Vector2(-3.466778f, -86.567740f),
            new Vector2(8.277145f, -86.567740f),
            new Vector2(19.876600f, -87.883830f),
            new Vector2(31.435240f, -89.826140f),
            new Vector2(43.134760f, -90.805030f),
            new Vector2(54.878680f, -90.805030f),
            new Vector2(66.601640f, -90.675830f),
            new Vector2(73.649600f, -82.476550f),
            new Vector2(77.082670f, -71.341950f),
            new Vector2(81.971120f, -60.703360f),
            new Vector2(85.848540f, -50.053900f),
            new Vector2(88.775410f, -38.695370f),
            new Vector2(93.351170f, -28.056270f),
            new Vector2(97.440950f, -17.128500f),
            new Vector2(101.471100f, -6.327989f),
            new Vector2(104.310700f, 5.045400f),
            new Vector2(105.762100f, 16.556280f),
            new Vector2(108.930200f, 27.799510f),
            new Vector2(110.750800f, 39.319320f),
            new Vector2(112.824200f, 50.752680f),
            new Vector2(114.421800f, 62.292680f),
            new Vector2(116.795600f, 73.651370f),
            new Vector2(120.029000f, 84.478840f),
            new Vector2(122.711200f, 95.522920f),
            new Vector2(111.080000f, 95.635640f),
            new Vector2(99.359280f, 95.823940f),
            new Vector2(87.766060f, 97.048100f),
            new Vector2(76.022140f, 97.048100f),
            new Vector2(64.278210f, 97.048100f),
            new Vector2(52.534290f, 97.048100f),
            new Vector2(40.790370f, 97.048100f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-127.480700f, 82.314470f),
            new Vector2(-116.674200f, 81.345580f),
            new Vector2(-105.875400f, 80.057420f),
            new Vector2(-94.911790f, 80.057420f),
            new Vector2(-83.948140f, 80.057420f),
            new Vector2(-73.167630f, 78.928910f),
            new Vector2(-62.342920f, 77.800350f),
            new Vector2(-60.860400f, 68.299290f),
            new Vector2(-64.011540f, 58.028860f),
            new Vector2(-68.171230f, 47.955360f),
            new Vector2(-72.508710f, 38.124730f),
            new Vector2(-75.389080f, 27.552880f),
            new Vector2(-79.265700f, 17.363700f),
            new Vector2(-83.427220f, 7.327553f),
            new Vector2(-87.396820f, -2.827665f),
            new Vector2(-91.725430f, -12.785780f),
            new Vector2(-95.362570f, -22.989410f),
            new Vector2(-99.431360f, -33.124460f),
            new Vector2(-103.781400f, -42.786580f),
            new Vector2(-107.570600f, -52.788450f),
            new Vector2(-112.355500f, -62.585530f),
            new Vector2(-117.043600f, -72.438870f),
            new Vector2(-111.196200f, -78.341060f),
            new Vector2(-100.383400f, -80.055250f),
            new Vector2(-89.428440f, -80.194780f),
            new Vector2(-78.464800f, -80.194780f),
            new Vector2(-67.501160f, -80.194780f),
            new Vector2(-56.537510f, -80.194780f),
            new Vector2(-45.809950f, -78.531600f),
            new Vector2(-34.905110f, -77.937700f),
            new Vector2(-23.973650f, -77.292410f),
            new Vector2(-13.117200f, -75.805180f),
            new Vector2(-2.257270f, -74.552090f),
            new Vector2(8.706375f, -74.552090f),
            new Vector2(19.648660f, -74.294080f),
            new Vector2(30.249370f, -71.631020f),
            new Vector2(41.137630f, -71.166470f),
            new Vector2(52.088480f, -70.986420f),
            new Vector2(62.902050f, -69.203120f),
            new Vector2(73.653470f, -67.780870f),
            new Vector2(84.552680f, -67.130070f),
            new Vector2(92.583960f, -61.499730f),
            new Vector2(94.841050f, -51.068920f),
            new Vector2(97.098140f, -40.427350f),
            new Vector2(99.950360f, -29.882350f),
            new Vector2(102.067800f, -19.233490f),
            new Vector2(103.695100f, -8.491346f),
            new Vector2(106.091500f, 2.083412f),
            new Vector2(108.383500f, 12.719320f),
            new Vector2(110.640500f, 23.360910f),
            new Vector2(112.897700f, 34.002470f),
            new Vector2(115.061500f, 44.699740f),
            new Vector2(117.086600f, 55.382620f),
            new Vector2(118.540300f, 66.110340f),
            new Vector2(120.797400f, 76.751920f),
            new Vector2(122.519400f, 87.480350f),
            new Vector2(114.149300f, 91.342800f),
            new Vector2(103.368800f, 92.471310f),
            new Vector2(92.703890f, 90.630400f),
            new Vector2(81.839560f, 90.018390f),
            new Vector2(71.166200f, 87.957180f),
            new Vector2(60.202560f, 87.957180f),
            new Vector2(49.238910f, 87.957180f),
            new Vector2(38.414250f, 86.828640f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-104.720800f, 84.256110f),
            new Vector2(-93.753170f, 84.256110f),
            new Vector2(-82.785580f, 84.256110f),
            new Vector2(-71.817990f, 84.256110f),
            new Vector2(-60.967280f, 85.205650f),
            new Vector2(-50.017490f, 85.060970f),
            new Vector2(-39.303060f, 83.306570f),
            new Vector2(-39.017020f, 76.272710f),
            new Vector2(-43.838110f, 66.548290f),
            new Vector2(-50.451440f, 57.861680f),
            new Vector2(-55.594210f, 48.371990f),
            new Vector2(-60.694100f, 38.723570f),
            new Vector2(-66.397570f, 29.523060f),
            new Vector2(-70.815650f, 19.799590f),
            new Vector2(-74.922030f, 9.742184f),
            new Vector2(-79.495570f, -0.130090f),
            new Vector2(-83.981790f, -10.052080f),
            new Vector2(-88.861340f, -19.811170f),
            new Vector2(-93.400050f, -29.764230f),
            new Vector2(-99.604570f, -38.158400f),
            new Vector2(-105.231800f, -47.548890f),
            new Vector2(-110.346600f, -57.012990f),
            new Vector2(-116.785800f, -65.829370f),
            new Vector2(-123.681700f, -74.289160f),
            new Vector2(-112.903800f, -75.268880f),
            new Vector2(-101.936200f, -75.268880f),
            new Vector2(-90.968630f, -75.268880f),
            new Vector2(-80.001030f, -75.268880f),
            new Vector2(-69.033440f, -75.268880f),
            new Vector2(-58.065830f, -75.268880f),
            new Vector2(-47.098240f, -75.268880f),
            new Vector2(-36.130640f, -75.268880f),
            new Vector2(-25.163030f, -75.268880f),
            new Vector2(-14.195440f, -75.268880f),
            new Vector2(-3.303865f, -74.350220f),
            new Vector2(7.388954f, -72.407750f),
            new Vector2(18.014530f, -69.748220f),
            new Vector2(28.784280f, -68.116290f),
            new Vector2(39.560340f, -66.414890f),
            new Vector2(50.357950f, -64.823790f),
            new Vector2(61.325550f, -64.823790f),
            new Vector2(72.293150f, -64.823790f),
            new Vector2(83.116110f, -63.932480f),
            new Vector2(87.750650f, -54.292990f),
            new Vector2(89.786860f, -43.587330f),
            new Vector2(93.787900f, -33.410560f),
            new Vector2(96.548870f, -23.150510f),
            new Vector2(100.481900f, -12.953410f),
            new Vector2(103.327800f, -2.428794f),
            new Vector2(106.246900f, 8.009315f),
            new Vector2(109.188800f, 18.567370f),
            new Vector2(111.777400f, 29.152100f),
            new Vector2(115.064300f, 39.553970f),
            new Vector2(118.852600f, 49.550960f),
            new Vector2(121.644300f, 59.732180f),
            new Vector2(123.911500f, 70.331880f),
            new Vector2(126.318300f, 80.904290f),
            new Vector2(123.791700f, 86.155220f),
            new Vector2(113.286400f, 83.306570f),
            new Vector2(102.463100f, 82.417400f),
            new Vector2(91.505260f, 82.357030f),
            new Vector2(80.537650f, 82.357030f),
            new Vector2(69.570050f, 82.357030f),
            new Vector2(58.602450f, 82.357030f),
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
