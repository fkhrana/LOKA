using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Qa (single stroke version).
/// Data ini diproses menjadi satu stroke averaged dari dua recording.
/// QA dapat dideteksi dengan 1 stroke atau 2 stroke - template ini handle 1 stroke version.
/// </summary>
public class QaSingleStrokeTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Qa;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-125.492200f, -67.466580f),
            new Vector2(-123.508400f, -57.547420f),
            new Vector2(-119.834200f, -48.177910f),
            new Vector2(-116.635400f, -38.581400f),
            new Vector2(-115.075500f, -28.718920f),
            new Vector2(-112.785400f, -18.929620f),
            new Vector2(-109.549000f, -9.856754f),
            new Vector2(-104.006200f, -1.672741f),
            new Vector2(-101.190800f, 7.752468f),
            new Vector2(-98.196990f, 17.408700f),
            new Vector2(-94.111270f, 26.545310f),
            new Vector2(-89.658200f, 35.497040f),
            new Vector2(-80.294520f, 38.262870f),
            new Vector2(-70.488770f, 40.172310f),
            new Vector2(-60.373160f, 40.172310f),
            new Vector2(-50.257550f, 40.172310f),
            new Vector2(-40.141950f, 40.172310f),
            new Vector2(-30.059670f, 39.835800f),
            new Vector2(-20.106030f, 38.040020f),
            new Vector2(-10.120130f, 36.700080f),
            new Vector2(-1.920803f, 34.041410f),
            new Vector2(-5.856853f, 25.235740f),
            new Vector2(-9.338117f, 15.742690f),
            new Vector2(-12.942170f, 6.294613f),
            new Vector2(-13.578970f, -3.717655f),
            new Vector2(-15.538490f, -13.560620f),
            new Vector2(-15.538490f, -23.676220f),
            new Vector2(-17.853300f, -33.461530f),
            new Vector2(-20.177940f, -43.200280f),
            new Vector2(-21.564060f, -53.135910f),
            new Vector2(-23.664900f, -62.910590f),
            new Vector2(-22.053780f, -60.392110f),
            new Vector2(-19.160940f, -50.706490f),
            new Vector2(-15.648140f, -41.284800f),
            new Vector2(-15.538490f, -31.182690f),
            new Vector2(-14.381070f, -21.254910f),
            new Vector2(-14.381070f, -11.139310f),
            new Vector2(-13.223690f, -1.211510f),
            new Vector2(-13.223690f, 8.904102f),
            new Vector2(-10.164130f, 18.513590f),
            new Vector2(-5.225835f, 27.128790f),
            new Vector2(2.580989f, 33.094840f),
            new Vector2(11.940740f, 36.700080f),
            new Vector2(21.680720f, 39.014860f),
            new Vector2(31.796320f, 39.014860f),
            new Vector2(41.911930f, 39.014860f),
            new Vector2(51.997350f, 38.530060f),
            new Vector2(61.959340f, 36.839440f),
            new Vector2(71.809050f, 34.539110f),
            new Vector2(81.726870f, 32.643740f),
            new Vector2(91.732040f, 31.167300f),
            new Vector2(101.684100f, 29.755630f),
            new Vector2(111.442200f, 27.552200f),
            new Vector2(119.878100f, 25.779230f),
            new Vector2(122.526600f, 17.847920f),
            new Vector2(124.507800f, 8.552929f),
            new Vector2(123.350400f, -1.374855f),
            new Vector2(121.035600f, -11.114830f),
            new Vector2(119.538200f, -21.032780f),
            new Vector2(116.624600f, -30.712350f),
            new Vector2(113.901500f, -40.447400f),
            new Vector2(112.094700f, -50.307710f),
            new Vector2(109.993900f, -60.082400f),
            new Vector2(107.146700f, -69.781320f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-133.120400f, -96.657300f),
            new Vector2(-131.534400f, -84.878200f),
            new Vector2(-129.497300f, -73.225590f),
            new Vector2(-126.199700f, -61.728150f),
            new Vector2(-125.210200f, -49.859620f),
            new Vector2(-124.062500f, -37.998920f),
            new Vector2(-121.777100f, -26.395440f),
            new Vector2(-118.920600f, -14.788560f),
            new Vector2(-114.427500f, -3.769575f),
            new Vector2(-111.842800f, 7.919106f),
            new Vector2(-108.338900f, 19.354050f),
            new Vector2(-105.661700f, 31.008560f),
            new Vector2(-100.077400f, 41.458230f),
            new Vector2(-91.610280f, 49.925370f),
            new Vector2(-82.160570f, 56.802730f),
            new Vector2(-71.562550f, 62.147030f),
            new Vector2(-59.982150f, 64.574620f),
            new Vector2(-48.007790f, 64.574620f),
            new Vector2(-36.498870f, 62.157870f),
            new Vector2(-25.630840f, 57.176800f),
            new Vector2(-14.124060f, 53.894660f),
            new Vector2(-7.955360f, 44.151840f),
            new Vector2(-4.981583f, 32.576100f),
            new Vector2(-0.385330f, 21.787150f),
            new Vector2(0.937569f, 9.922289f),
            new Vector2(2.749134f, -1.829054f),
            new Vector2(2.749134f, -13.803400f),
            new Vector2(2.749134f, -25.777760f),
            new Vector2(2.749134f, -37.752110f),
            new Vector2(2.749134f, -49.726460f),
            new Vector2(2.749134f, -61.700810f),
            new Vector2(0.850720f, -73.367090f),
            new Vector2(-0.874081f, -82.890740f),
            new Vector2(-0.874081f, -70.916380f),
            new Vector2(-0.874081f, -58.942030f),
            new Vector2(-0.874081f, -46.967680f),
            new Vector2(-0.874081f, -34.993330f),
            new Vector2(0.070248f, -23.172230f),
            new Vector2(2.748028f, -11.517900f),
            new Vector2(6.372356f, -0.640690f),
            new Vector2(6.722385f, 11.276860f),
            new Vector2(9.995487f, 22.720070f),
            new Vector2(12.397590f, 34.327730f),
            new Vector2(17.530170f, 44.935340f),
            new Vector2(27.906430f, 50.030600f),
            new Vector2(38.632630f, 55.342410f),
            new Vector2(49.790310f, 59.577670f),
            new Vector2(61.550620f, 61.228250f),
            new Vector2(73.275920f, 62.763000f),
            new Vector2(85.250270f, 62.763000f),
            new Vector2(97.117680f, 62.103940f),
            new Vector2(106.712200f, 58.142890f),
            new Vector2(109.616400f, 46.526070f),
            new Vector2(112.091800f, 34.812810f),
            new Vector2(115.524600f, 23.349930f),
            new Vector2(116.879600f, 11.595460f),
            new Vector2(116.879600f, -0.378893f),
            new Vector2(116.879600f, -12.353240f),
            new Vector2(116.879600f, -24.327600f),
            new Vector2(116.879600f, -36.301950f),
            new Vector2(116.750600f, -48.260420f),
            new Vector2(115.068100f, -60.027660f),
            new Vector2(115.068100f, -72.002010f),
            new Vector2(115.068100f, -83.976140f),
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
