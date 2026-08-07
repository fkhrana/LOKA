using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture La.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class LaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.La;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-118.911300f, -125.379900f),
            new Vector2(-113.860800f, -116.533900f),
            new Vector2(-112.724600f, -102.905600f),
            new Vector2(-111.117500f, -89.372430f),
            new Vector2(-104.143200f, -77.783530f),
            new Vector2(-101.234500f, -64.455640f),
            new Vector2(-95.330850f, -53.785560f),
            new Vector2(-93.658730f, -40.094900f),
            new Vector2(-93.658730f, -26.198400f),
            new Vector2(-88.696680f, -14.357250f),
            new Vector2(-88.608240f, -0.497380f),
            new Vector2(-88.608240f, 13.399120f),
            new Vector2(-88.608240f, 27.295620f),
            new Vector2(-88.608240f, 41.192130f),
            new Vector2(-83.557720f, 53.559440f),
            new Vector2(-81.032460f, 67.046150f),
            new Vector2(-78.507270f, 80.346540f),
            new Vector2(-78.507270f, 94.243040f),
            new Vector2(-71.910910f, 104.933300f),
            new Vector2(-63.496530f, 114.096800f),
            new Vector2(-55.779990f, 122.323800f),
            new Vector2(-44.179800f, 124.620100f),
            new Vector2(-30.283300f, 124.620100f),
            new Vector2(-16.386800f, 124.620100f),
            new Vector2(-7.800232f, 119.310200f),
            new Vector2(-7.800232f, 105.413700f),
            new Vector2(-7.800232f, 91.517170f),
            new Vector2(-7.800232f, 77.620670f),
            new Vector2(-7.670593f, 63.754780f),
            new Vector2(-5.274940f, 50.423810f),
            new Vector2(-5.274940f, 36.527310f),
            new Vector2(-5.274940f, 22.630810f),
            new Vector2(-5.274940f, 8.734308f),
            new Vector2(-5.274940f, -5.162192f),
            new Vector2(-5.274940f, -19.058690f),
            new Vector2(-5.274940f, -32.955190f),
            new Vector2(-2.749695f, -46.255570f),
            new Vector2(2.300785f, -58.060090f),
            new Vector2(2.300785f, -71.956600f),
            new Vector2(4.826080f, -85.256960f),
            new Vector2(12.348370f, -95.882980f),
            new Vector2(22.602960f, -102.652600f),
            new Vector2(35.554990f, -106.653500f),
            new Vector2(46.082310f, -112.753700f),
            new Vector2(59.382690f, -115.278900f),
            new Vector2(73.279200f, -115.278900f),
            new Vector2(84.927650f, -111.641400f),
            new Vector2(88.159390f, -98.507840f),
            new Vector2(88.159390f, -84.611330f),
            new Vector2(91.100970f, -71.933270f),
            new Vector2(93.573490f, -58.996140f),
            new Vector2(95.735160f, -45.609950f),
            new Vector2(99.470540f, -33.260680f),
            new Vector2(100.785600f, -19.908930f),
            new Vector2(100.785600f, -6.012424f),
            new Vector2(104.789200f, 5.409736f),
            new Vector2(105.836100f, 18.659190f),
            new Vector2(105.836100f, 32.555700f),
            new Vector2(105.836100f, 46.452200f),
            new Vector2(106.274200f, 60.277610f),
            new Vector2(108.361400f, 73.835400f),
            new Vector2(108.361400f, 87.731900f),
            new Vector2(109.874900f, 101.382800f),
            new Vector2(113.411900f, 114.519000f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-118.867200f, -128.115000f),
            new Vector2(-118.780400f, -113.655700f),
            new Vector2(-115.373500f, -99.854460f),
            new Vector2(-111.615200f, -86.284700f),
            new Vector2(-109.343400f, -72.492740f),
            new Vector2(-108.257000f, -58.189230f),
            new Vector2(-106.484100f, -44.064300f),
            new Vector2(-102.200500f, -30.881460f),
            new Vector2(-102.200500f, -16.401650f),
            new Vector2(-102.200500f, -1.921832f),
            new Vector2(-102.200500f, 12.557980f),
            new Vector2(-95.713460f, 23.938010f),
            new Vector2(-94.475200f, 37.918000f),
            new Vector2(-91.586140f, 52.038060f),
            new Vector2(-89.006670f, 66.228350f),
            new Vector2(-87.914780f, 80.530970f),
            new Vector2(-83.961380f, 94.077510f),
            new Vector2(-78.391040f, 106.394000f),
            new Vector2(-72.620740f, 117.767200f),
            new Vector2(-61.108840f, 121.885000f),
            new Vector2(-46.629040f, 121.885000f),
            new Vector2(-32.149230f, 121.885000f),
            new Vector2(-17.669420f, 121.885000f),
            new Vector2(-3.189621f, 121.885000f),
            new Vector2(7.323303f, 117.918100f),
            new Vector2(7.323303f, 103.438200f),
            new Vector2(9.704254f, 89.520500f),
            new Vector2(9.704254f, 75.040690f),
            new Vector2(9.704254f, 60.560870f),
            new Vector2(9.704254f, 46.081060f),
            new Vector2(7.323303f, 32.163310f),
            new Vector2(4.942322f, 18.245580f),
            new Vector2(2.561371f, 4.327830f),
            new Vector2(2.561371f, -10.151980f),
            new Vector2(1.657211f, -24.257290f),
            new Vector2(-2.200500f, -37.139190f),
            new Vector2(-3.128662f, -51.399890f),
            new Vector2(-4.581512f, -65.536730f),
            new Vector2(-4.581512f, -80.016550f),
            new Vector2(-4.581512f, -94.496360f),
            new Vector2(-0.403946f, -106.394300f),
            new Vector2(13.046590f, -110.028800f),
            new Vector2(25.493650f, -115.771600f),
            new Vector2(38.705610f, -119.021200f),
            new Vector2(52.377290f, -120.972200f),
            new Vector2(66.857100f, -120.972200f),
            new Vector2(78.393890f, -116.210300f),
            new Vector2(85.542950f, -105.360700f),
            new Vector2(88.612620f, -92.063770f),
            new Vector2(91.580270f, -78.648640f),
            new Vector2(93.037540f, -64.512840f),
            new Vector2(97.799420f, -52.005460f),
            new Vector2(97.799420f, -37.525650f),
            new Vector2(100.066500f, -23.581020f),
            new Vector2(100.180400f, -9.128101f),
            new Vector2(101.304900f, 5.086266f),
            new Vector2(107.323200f, 16.326510f),
            new Vector2(107.323200f, 30.806320f),
            new Vector2(107.323200f, 45.286140f),
            new Vector2(107.323200f, 59.765950f),
            new Vector2(109.704100f, 73.683700f),
            new Vector2(109.704100f, 88.163510f),
            new Vector2(109.704100f, 102.643300f),
            new Vector2(109.704100f, 117.123000f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-86.582730f, -124.557500f),
            new Vector2(-86.582730f, -110.747000f),
            new Vector2(-86.582730f, -96.936510f),
            new Vector2(-86.582730f, -83.126010f),
            new Vector2(-86.582730f, -69.315510f),
            new Vector2(-86.582730f, -55.504990f),
            new Vector2(-86.582730f, -41.694480f),
            new Vector2(-86.582730f, -27.883970f),
            new Vector2(-86.582730f, -14.073470f),
            new Vector2(-86.582730f, -0.262962f),
            new Vector2(-86.582730f, 13.547550f),
            new Vector2(-86.582730f, 27.358060f),
            new Vector2(-86.582730f, 41.168570f),
            new Vector2(-86.582730f, 54.979080f),
            new Vector2(-89.270940f, 68.154990f),
            new Vector2(-89.270940f, 81.965500f),
            new Vector2(-89.270940f, 95.776010f),
            new Vector2(-89.026660f, 109.435600f),
            new Vector2(-76.106580f, 110.795800f),
            new Vector2(-62.646010f, 109.313400f),
            new Vector2(-48.835490f, 109.313400f),
            new Vector2(-35.024990f, 109.313400f),
            new Vector2(-23.441440f, 114.689800f),
            new Vector2(-14.002090f, 113.641400f),
            new Vector2(-14.002090f, 99.830860f),
            new Vector2(-14.002090f, 86.020350f),
            new Vector2(-14.002090f, 72.209830f),
            new Vector2(-14.002090f, 58.399320f),
            new Vector2(-14.002090f, 44.588810f),
            new Vector2(-14.002090f, 30.778300f),
            new Vector2(-14.002090f, 16.967790f),
            new Vector2(-14.002090f, 3.157288f),
            new Vector2(-11.313890f, -10.018620f),
            new Vector2(-11.313890f, -23.829120f),
            new Vector2(-11.313890f, -37.639630f),
            new Vector2(-11.313890f, -51.450140f),
            new Vector2(-11.313890f, -65.260650f),
            new Vector2(-11.313890f, -79.071150f),
            new Vector2(-11.313890f, -92.881670f),
            new Vector2(-2.297531f, -97.675810f),
            new Vector2(9.286026f, -103.052100f),
            new Vector2(20.597500f, -110.105100f),
            new Vector2(34.243860f, -111.116600f),
            new Vector2(48.054370f, -111.116600f),
            new Vector2(61.864880f, -111.116600f),
            new Vector2(75.675380f, -111.116600f),
            new Vector2(84.895170f, -104.747300f),
            new Vector2(88.432990f, -91.730290f),
            new Vector2(90.836560f, -78.487170f),
            new Vector2(96.212940f, -66.304500f),
            new Vector2(96.212940f, -52.494000f),
            new Vector2(96.212940f, -38.683490f),
            new Vector2(98.716670f, -25.464030f),
            new Vector2(98.901100f, -11.697060f),
            new Vector2(101.589200f, 1.782539f),
            new Vector2(101.589200f, 15.593050f),
            new Vector2(101.589200f, 29.403560f),
            new Vector2(101.589200f, 43.214070f),
            new Vector2(101.589200f, 57.024580f),
            new Vector2(101.589200f, 70.835090f),
            new Vector2(101.589200f, 84.645610f),
            new Vector2(102.744600f, 98.183360f),
            new Vector2(104.277400f, 111.632000f),
            new Vector2(104.277400f, 125.442500f),
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