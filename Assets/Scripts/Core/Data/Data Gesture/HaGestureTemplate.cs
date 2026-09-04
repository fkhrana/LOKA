using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Ha.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class HaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ha;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-132.031300f, 13.942410f),
            new Vector2(-122.070400f, 9.597292f),
            new Vector2(-111.006600f, 7.252022f),
            new Vector2(-99.743190f, 6.589449f),
            new Vector2(-88.424930f, 6.589449f),
            new Vector2(-77.106670f, 6.589449f),
            new Vector2(-65.793160f, 6.564766f),
            new Vector2(-55.036540f, 3.648312f),
            new Vector2(-43.834030f, 2.479422f),
            new Vector2(-32.545640f, 2.177702f),
            new Vector2(-35.544620f, -1.934575f),
            new Vector2(-45.672650f, -6.747293f),
            new Vector2(-54.422070f, -12.694130f),
            new Vector2(-64.062660f, -18.536930f),
            new Vector2(-72.628670f, -25.867560f),
            new Vector2(-82.588420f, -31.036810f),
            new Vector2(-91.690580f, -37.316530f),
            new Vector2(-100.146600f, -44.703030f),
            new Vector2(-109.919000f, -50.255390f),
            new Vector2(-120.066100f, -55.041530f),
            new Vector2(-118.903400f, -58.116400f),
            new Vector2(-107.585100f, -58.116400f),
            new Vector2(-96.315920f, -58.806820f),
            new Vector2(-85.053100f, -59.586970f),
            new Vector2(-73.734840f, -59.586970f),
            new Vector2(-62.416580f, -59.586970f),
            new Vector2(-51.098310f, -59.586970f),
            new Vector2(-40.075600f, -61.987670f),
            new Vector2(-28.823870f, -62.528140f),
            new Vector2(-17.505610f, -62.528140f),
            new Vector2(-6.821710f, -60.428610f),
            new Vector2(-5.560767f, -49.314980f),
            new Vector2(-2.057237f, -38.782290f),
            new Vector2(2.383350f, -28.401660f),
            new Vector2(5.515680f, -17.534100f),
            new Vector2(10.165900f, -7.320490f),
            new Vector2(13.938470f, 3.322365f),
            new Vector2(17.778660f, 13.752420f),
            new Vector2(21.957410f, 24.015240f),
            new Vector2(26.269530f, 34.433270f),
            new Vector2(29.848640f, 45.170730f),
            new Vector2(34.692270f, 55.289740f),
            new Vector2(39.897240f, 65.022370f),
            new Vector2(43.811810f, 75.079710f),
            new Vector2(49.997320f, 84.206380f),
            new Vector2(58.744710f, 90.412990f),
            new Vector2(70.062970f, 90.412990f),
            new Vector2(81.381230f, 90.412990f),
            new Vector2(92.460850f, 88.942430f),
            new Vector2(103.084800f, 86.001250f),
            new Vector2(113.847900f, 83.649780f),
            new Vector2(117.968700f, 74.278820f),
            new Vector2(116.498000f, 63.199230f),
            new Vector2(116.498000f, 51.880970f),
            new Vector2(116.498000f, 40.562710f),
            new Vector2(116.498000f, 29.244440f),
            new Vector2(116.498000f, 17.926180f),
            new Vector2(116.498000f, 6.607924f),
            new Vector2(115.540700f, -4.592491f),
            new Vector2(114.935700f, -15.836270f),
            new Vector2(113.010700f, -26.855850f),
            new Vector2(110.125200f, -37.529140f),
            new Vector2(107.674500f, -48.507310f),
            new Vector2(106.203900f, -59.586950f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-130.300800f, 11.891060f),
            new Vector2(-118.023300f, 14.798020f),
            new Vector2(-105.274100f, 14.798020f),
            new Vector2(-92.524960f, 14.798020f),
            new Vector2(-79.775790f, 14.798020f),
            new Vector2(-67.026610f, 14.798020f),
            new Vector2(-54.574380f, 17.443300f),
            new Vector2(-41.851110f, 17.704990f),
            new Vector2(-29.101940f, 17.704990f),
            new Vector2(-16.352760f, 17.704990f),
            new Vector2(-12.038690f, 15.151030f),
            new Vector2(-21.680490f, 6.943562f),
            new Vector2(-30.004350f, -2.638096f),
            new Vector2(-39.864720f, -10.667080f),
            new Vector2(-49.465760f, -18.745960f),
            new Vector2(-59.580510f, -26.066540f),
            new Vector2(-69.610730f, -32.801410f),
            new Vector2(-79.559890f, -40.565730f),
            new Vector2(-89.884490f, -47.983340f),
            new Vector2(-101.082400f, -53.466400f),
            new Vector2(-111.708600f, -60.500990f),
            new Vector2(-122.827300f, -66.528650f),
            new Vector2(-128.973700f, -70.431140f),
            new Vector2(-116.310000f, -70.957770f),
            new Vector2(-103.560800f, -70.957770f),
            new Vector2(-90.811640f, -70.957770f),
            new Vector2(-78.062460f, -70.957770f),
            new Vector2(-65.313290f, -70.957770f),
            new Vector2(-52.564110f, -70.957770f),
            new Vector2(-39.814930f, -70.957770f),
            new Vector2(-27.065760f, -70.957770f),
            new Vector2(-14.649870f, -69.091610f),
            new Vector2(-4.229590f, -62.083560f),
            new Vector2(2.143684f, -51.070140f),
            new Vector2(7.928841f, -39.841740f),
            new Vector2(11.563850f, -27.631320f),
            new Vector2(16.446970f, -15.889070f),
            new Vector2(20.264940f, -3.837975f),
            new Vector2(22.315510f, 8.591316f),
            new Vector2(25.222440f, 20.654260f),
            new Vector2(28.348270f, 32.896170f),
            new Vector2(30.541330f, 45.289470f),
            new Vector2(35.384760f, 56.900720f),
            new Vector2(39.485490f, 68.943100f),
            new Vector2(44.285940f, 80.709430f),
            new Vector2(48.317620f, 92.804350f),
            new Vector2(59.173870f, 98.503880f),
            new Vector2(71.846180f, 99.072310f),
            new Vector2(84.286420f, 96.299160f),
            new Vector2(97.025120f, 96.193360f),
            new Vector2(109.595400f, 94.739850f),
            new Vector2(119.541900f, 90.851350f),
            new Vector2(119.699200f, 78.127710f),
            new Vector2(119.699200f, 65.378530f),
            new Vector2(119.699200f, 52.629350f),
            new Vector2(119.699200f, 39.880180f),
            new Vector2(119.699200f, 27.131000f),
            new Vector2(118.893200f, 14.481060f),
            new Vector2(116.792200f, 1.990536f),
            new Vector2(113.427800f, -10.287540f),
            new Vector2(109.508600f, -22.290200f),
            new Vector2(106.665200f, -34.431080f),
            new Vector2(103.710900f, -46.759640f),
            new Vector2(102.257400f, -59.329860f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-119.679500f, -2.110658f),
            new Vector2(-109.533800f, 1.512406f),
            new Vector2(-97.887340f, 1.512406f),
            new Vector2(-86.240910f, 1.512406f),
            new Vector2(-74.594460f, 1.512406f),
            new Vector2(-63.241980f, 3.323901f),
            new Vector2(-63.531970f, -3.931705f),
            new Vector2(-71.943740f, -11.951710f),
            new Vector2(-81.634160f, -18.412010f),
            new Vector2(-89.657360f, -26.820940f),
            new Vector2(-97.109800f, -35.697990f),
            new Vector2(-105.345100f, -43.933270f),
            new Vector2(-111.625600f, -53.502260f),
            new Vector2(-120.526200f, -60.925940f),
            new Vector2(-127.766100f, -69.977290f),
            new Vector2(-130.377400f, -78.194400f),
            new Vector2(-118.730900f, -78.194400f),
            new Vector2(-107.084500f, -78.194400f),
            new Vector2(-95.438060f, -78.194400f),
            new Vector2(-83.791630f, -78.194400f),
            new Vector2(-72.145190f, -78.194400f),
            new Vector2(-60.498740f, -78.194400f),
            new Vector2(-48.852300f, -78.194400f),
            new Vector2(-37.205860f, -78.194400f),
            new Vector2(-26.597480f, -75.688300f),
            new Vector2(-22.847520f, -64.860820f),
            new Vector2(-20.883150f, -53.533160f),
            new Vector2(-18.234460f, -42.316540f),
            new Vector2(-16.422920f, -30.964070f),
            new Vector2(-14.990040f, -19.550150f),
            new Vector2(-14.611440f, -7.965141f),
            new Vector2(-12.327920f, 3.400180f),
            new Vector2(-9.081147f, 14.575970f),
            new Vector2(-7.365348f, 26.011190f),
            new Vector2(-3.742324f, 36.802360f),
            new Vector2(-0.514929f, 47.820590f),
            new Vector2(3.319916f, 58.745610f),
            new Vector2(7.126789f, 69.272460f),
            new Vector2(9.672905f, 80.534500f),
            new Vector2(15.845320f, 89.937770f),
            new Vector2(26.600990f, 92.088310f),
            new Vector2(38.247430f, 92.088310f),
            new Vector2(49.893880f, 92.088310f),
            new Vector2(61.540320f, 92.088310f),
            new Vector2(73.186760f, 92.088310f),
            new Vector2(84.737180f, 90.928120f),
            new Vector2(96.329720f, 90.276820f),
            new Vector2(107.976200f, 90.276820f),
            new Vector2(119.622600f, 90.276820f),
            new Vector2(117.629400f, 84.980330f),
            new Vector2(117.629400f, 73.333890f),
            new Vector2(116.846500f, 61.783840f),
            new Vector2(114.365600f, 50.407820f),
            new Vector2(111.393400f, 39.170300f),
            new Vector2(110.047700f, 27.678340f),
            new Vector2(108.083400f, 16.350650f),
            new Vector2(106.760300f, 4.918914f),
            new Vector2(106.760300f, -6.727529f),
            new Vector2(106.760300f, -18.373970f),
            new Vector2(106.760300f, -30.020410f),
            new Vector2(106.760300f, -41.666850f),
            new Vector2(106.760300f, -53.313300f),
            new Vector2(106.455300f, -64.922200f),
            new Vector2(104.948800f, -76.382870f),
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
