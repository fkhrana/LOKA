using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Square.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class SquareGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Square;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-116.002700f, 69.332890f),
            new Vector2(-116.002700f, 56.284790f),
            new Vector2(-116.002700f, 43.236690f),
            new Vector2(-116.002700f, 30.188630f),
            new Vector2(-116.002700f, 17.140500f),
            new Vector2(-116.002700f, 4.092407f),
            new Vector2(-116.002700f, -8.955719f),
            new Vector2(-116.002700f, -22.003810f),
            new Vector2(-116.002700f, -35.051910f),
            new Vector2(-116.002700f, -48.100040f),
            new Vector2(-116.002700f, -61.148130f),
            new Vector2(-116.002700f, -74.196240f),
            new Vector2(-116.002700f, -87.244350f),
            new Vector2(-110.469800f, -96.304550f),
            new Vector2(-98.091250f, -94.236880f),
            new Vector2(-85.612580f, -90.454640f),
            new Vector2(-72.778870f, -88.713090f),
            new Vector2(-60.111600f, -86.366230f),
            new Vector2(-47.488830f, -83.078840f),
            new Vector2(-34.830310f, -79.914200f),
            new Vector2(-22.114030f, -77.218800f),
            new Vector2(-9.065926f, -77.218800f),
            new Vector2(3.982178f, -77.218800f),
            new Vector2(17.030280f, -77.218800f),
            new Vector2(30.078380f, -77.218800f),
            new Vector2(42.775970f, -75.058870f),
            new Vector2(55.708260f, -74.345230f),
            new Vector2(68.756360f, -74.345230f),
            new Vector2(81.804470f, -74.345230f),
            new Vector2(94.852570f, -74.345230f),
            new Vector2(107.737800f, -75.989900f),
            new Vector2(120.664200f, -77.218800f),
            new Vector2(125.376600f, -68.883090f),
            new Vector2(125.376600f, -55.834960f),
            new Vector2(125.376600f, -42.786870f),
            new Vector2(125.376600f, -29.738740f),
            new Vector2(123.456900f, -16.849520f),
            new Vector2(122.503200f, -3.880341f),
            new Vector2(122.503200f, 9.167755f),
            new Vector2(122.503200f, 22.215850f),
            new Vector2(119.646100f, 34.800350f),
            new Vector2(119.629500f, 47.845730f),
            new Vector2(119.629500f, 60.893830f),
            new Vector2(119.629500f, 73.941890f),
            new Vector2(117.680600f, 86.673740f),
            new Vector2(106.828600f, 91.433350f),
            new Vector2(93.868420f, 92.321440f),
            new Vector2(80.820320f, 92.321440f),
            new Vector2(67.772220f, 92.321440f),
            new Vector2(55.031880f, 89.821440f),
            new Vector2(42.030470f, 89.443570f),
            new Vector2(29.651960f, 85.317380f),
            new Vector2(16.866210f, 83.700710f),
            new Vector2(4.291298f, 81.243590f),
            new Vector2(-8.123230f, 77.953640f),
            new Vector2(-20.876570f, 76.137390f),
            new Vector2(-33.286810f, 72.206540f),
            new Vector2(-46.334910f, 72.206540f),
            new Vector2(-59.383010f, 72.206540f),
            new Vector2(-72.431110f, 72.206540f),
            new Vector2(-85.479200f, 72.206540f),
            new Vector2(-98.527300f, 72.206540f),
            new Vector2(-111.575400f, 72.206540f),
            new Vector2(-124.623400f, 72.206540f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-125.307200f, 85.654140f),
            new Vector2(-125.307200f, 72.238660f),
            new Vector2(-125.307200f, 58.823170f),
            new Vector2(-125.307200f, 45.407690f),
            new Vector2(-125.307200f, 31.992210f),
            new Vector2(-125.307200f, 18.576740f),
            new Vector2(-123.974900f, 5.325264f),
            new Vector2(-121.000800f, -7.752533f),
            new Vector2(-119.057100f, -20.975550f),
            new Vector2(-119.057100f, -34.391030f),
            new Vector2(-119.057100f, -47.806510f),
            new Vector2(-119.057100f, -61.221980f),
            new Vector2(-119.057100f, -74.637460f),
            new Vector2(-119.057100f, -88.052940f),
            new Vector2(-111.570700f, -97.461830f),
            new Vector2(-98.536450f, -98.720890f),
            new Vector2(-85.120970f, -98.720890f),
            new Vector2(-71.705490f, -98.720890f),
            new Vector2(-58.290020f, -98.720890f),
            new Vector2(-44.874540f, -98.720890f),
            new Vector2(-31.459060f, -98.720890f),
            new Vector2(-18.043590f, -98.720890f),
            new Vector2(-4.628105f, -98.720890f),
            new Vector2(8.787369f, -98.720890f),
            new Vector2(22.202850f, -98.720890f),
            new Vector2(35.618330f, -98.720890f),
            new Vector2(49.033810f, -98.720890f),
            new Vector2(62.449300f, -98.720890f),
            new Vector2(75.864780f, -98.720890f),
            new Vector2(89.280260f, -98.720890f),
            new Vector2(102.695700f, -98.720890f),
            new Vector2(115.318000f, -95.309810f),
            new Vector2(115.318000f, -81.894330f),
            new Vector2(115.318000f, -68.478840f),
            new Vector2(115.318000f, -55.063380f),
            new Vector2(115.318000f, -41.647900f),
            new Vector2(115.318000f, -28.232410f),
            new Vector2(115.318000f, -14.816940f),
            new Vector2(118.423100f, -1.905327f),
            new Vector2(121.567800f, 11.122280f),
            new Vector2(121.567800f, 24.537750f),
            new Vector2(122.125400f, 37.884580f),
            new Vector2(124.692800f, 50.983990f),
            new Vector2(124.692800f, 64.399470f),
            new Vector2(124.692800f, 77.814950f),
            new Vector2(122.929400f, 90.944260f),
            new Vector2(112.730100f, 97.238580f),
            new Vector2(99.921990f, 101.202700f),
            new Vector2(86.518900f, 101.279100f),
            new Vector2(73.103420f, 101.279100f),
            new Vector2(59.687940f, 101.279100f),
            new Vector2(46.272460f, 101.279100f),
            new Vector2(32.856980f, 101.279100f),
            new Vector2(19.441500f, 101.279100f),
            new Vector2(6.182106f, 100.317200f),
            new Vector2(-6.686050f, 96.559380f),
            new Vector2(-19.548490f, 92.782010f),
            new Vector2(-32.821520f, 91.904150f),
            new Vector2(-46.237000f, 91.904150f),
            new Vector2(-59.145320f, 88.779030f),
            new Vector2(-72.560810f, 88.779030f),
            new Vector2(-85.976290f, 88.779030f),
            new Vector2(-99.391750f, 88.779030f),
            new Vector2(-112.807200f, 88.779030f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-113.340700f, 93.025650f),
            new Vector2(-113.340700f, 79.315160f),
            new Vector2(-113.340700f, 65.604660f),
            new Vector2(-113.340700f, 51.894150f),
            new Vector2(-113.340700f, 38.183650f),
            new Vector2(-113.340700f, 24.473160f),
            new Vector2(-111.915200f, 10.863980f),
            new Vector2(-111.580100f, -2.822708f),
            new Vector2(-111.580100f, -16.533200f),
            new Vector2(-111.580100f, -30.243710f),
            new Vector2(-111.580100f, -43.954210f),
            new Vector2(-111.580100f, -57.664710f),
            new Vector2(-111.580100f, -71.375210f),
            new Vector2(-111.580100f, -85.085710f),
            new Vector2(-111.580100f, -98.796200f),
            new Vector2(-98.451610f, -102.396900f),
            new Vector2(-84.741110f, -102.396900f),
            new Vector2(-71.030620f, -102.396900f),
            new Vector2(-57.320110f, -102.396900f),
            new Vector2(-43.609620f, -102.396900f),
            new Vector2(-29.899120f, -102.396900f),
            new Vector2(-16.188610f, -102.396900f),
            new Vector2(-2.478119f, -102.396900f),
            new Vector2(11.232380f, -102.396900f),
            new Vector2(24.942880f, -102.396900f),
            new Vector2(38.653380f, -102.396900f),
            new Vector2(52.218180f, -104.157400f),
            new Vector2(65.928680f, -104.157400f),
            new Vector2(79.639180f, -104.157400f),
            new Vector2(93.349680f, -104.157400f),
            new Vector2(107.060200f, -104.157400f),
            new Vector2(117.293000f, -100.679800f),
            new Vector2(118.445800f, -87.191250f),
            new Vector2(122.574900f, -74.153630f),
            new Vector2(122.574900f, -60.443130f),
            new Vector2(122.574900f, -46.732640f),
            new Vector2(122.574900f, -33.022130f),
            new Vector2(122.574900f, -19.311640f),
            new Vector2(122.574900f, -5.601135f),
            new Vector2(122.574900f, 8.109367f),
            new Vector2(122.574900f, 21.819850f),
            new Vector2(122.574900f, 35.530360f),
            new Vector2(122.529900f, 49.236400f),
            new Vector2(119.610600f, 62.628850f),
            new Vector2(117.293000f, 76.054030f),
            new Vector2(116.172200f, 89.425170f),
            new Vector2(105.265000f, 96.912960f),
            new Vector2(91.726120f, 98.307340f),
            new Vector2(78.015630f, 98.307340f),
            new Vector2(64.305120f, 98.307340f),
            new Vector2(50.594620f, 98.307340f),
            new Vector2(36.884120f, 98.307340f),
            new Vector2(23.173620f, 98.307340f),
            new Vector2(9.463120f, 98.307340f),
            new Vector2(-4.247375f, 98.307340f),
            new Vector2(-17.957880f, 98.307340f),
            new Vector2(-31.668380f, 98.307340f),
            new Vector2(-45.378880f, 98.307340f),
            new Vector2(-59.089380f, 98.307340f),
            new Vector2(-72.799870f, 98.307340f),
            new Vector2(-86.365270f, 99.486020f),
            new Vector2(-100.004100f, 100.067900f),
            new Vector2(-113.714600f, 100.067900f),
            new Vector2(-127.425100f, 100.067900f),
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
