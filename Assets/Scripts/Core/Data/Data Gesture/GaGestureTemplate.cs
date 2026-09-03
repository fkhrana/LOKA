using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Ga.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class GaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ga;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-119.012000f, 118.410300f),
            new Vector2(-106.369800f, 118.410300f),
            new Vector2(-93.727570f, 118.410300f),
            new Vector2(-81.085360f, 118.410300f),
            new Vector2(-68.443130f, 118.410300f),
            new Vector2(-55.800900f, 118.410300f),
            new Vector2(-43.158680f, 118.410300f),
            new Vector2(-30.516450f, 118.410300f),
            new Vector2(-17.874240f, 118.410300f),
            new Vector2(-5.255554f, 118.265200f),
            new Vector2(2.600845f, 111.285500f),
            new Vector2(-0.153946f, 99.090300f),
            new Vector2(-7.207474f, 88.985350f),
            new Vector2(-13.279430f, 78.006330f),
            new Vector2(-20.292080f, 67.487380f),
            new Vector2(-26.330460f, 56.491940f),
            new Vector2(-32.233030f, 45.430070f),
            new Vector2(-39.170560f, 34.867510f),
            new Vector2(-44.974150f, 23.652560f),
            new Vector2(-52.559460f, 13.538770f),
            new Vector2(-61.477780f, 4.581085f),
            new Vector2(-69.572340f, -5.050934f),
            new Vector2(-76.584930f, -15.569920f),
            new Vector2(-85.258360f, -24.727390f),
            new Vector2(-92.636080f, -34.631930f),
            new Vector2(-100.707400f, -43.514070f),
            new Vector2(-106.488900f, -54.249660f),
            new Vector2(-114.413500f, -64.020980f),
            new Vector2(-120.258500f, -72.300670f),
            new Vector2(-107.616300f, -72.300670f),
            new Vector2(-94.974090f, -72.300670f),
            new Vector2(-82.331880f, -72.300670f),
            new Vector2(-69.689650f, -72.300670f),
            new Vector2(-57.247060f, -73.922250f),
            new Vector2(-44.982320f, -76.988450f),
            new Vector2(-32.460740f, -78.002430f),
            new Vector2(-20.064010f, -80.481740f),
            new Vector2(-7.881287f, -83.356380f),
            new Vector2(4.760941f, -83.356380f),
            new Vector2(17.403170f, -83.356380f),
            new Vector2(30.045380f, -83.356380f),
            new Vector2(41.665300f, -87.686980f),
            new Vector2(54.024890f, -88.884170f),
            new Vector2(66.667110f, -88.884170f),
            new Vector2(79.309330f, -88.884170f),
            new Vector2(91.951550f, -88.884170f),
            new Vector2(104.271800f, -86.268860f),
            new Vector2(113.955700f, -80.165280f),
            new Vector2(115.921900f, -67.765110f),
            new Vector2(115.921900f, -55.122890f),
            new Vector2(115.921900f, -42.480680f),
            new Vector2(115.921900f, -29.838460f),
            new Vector2(117.220300f, -17.356090f),
            new Vector2(123.352600f, -6.827377f),
            new Vector2(126.986600f, 5.116486f),
            new Vector2(129.741500f, 17.311650f),
            new Vector2(122.851300f, 26.266360f),
            new Vector2(110.492000f, 27.200760f),
            new Vector2(97.894200f, 27.561770f),
            new Vector2(85.629450f, 30.627930f),
            new Vector2(73.245830f, 32.728550f),
            new Vector2(60.603610f, 32.728550f),
            new Vector2(47.961400f, 32.728550f),
            new Vector2(35.767900f, 35.492510f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-124.273600f, 78.135080f),
            new Vector2(-115.156200f, 84.059300f),
            new Vector2(-104.457700f, 86.653850f),
            new Vector2(-93.626140f, 88.844570f),
            new Vector2(-82.940120f, 92.171060f),
            new Vector2(-71.837330f, 92.945720f),
            new Vector2(-60.668420f, 92.534080f),
            new Vector2(-49.654820f, 90.582740f),
            new Vector2(-38.728770f, 88.213080f),
            new Vector2(-27.606310f, 86.932990f),
            new Vector2(-16.674800f, 84.503780f),
            new Vector2(-5.798604f, 81.840280f),
            new Vector2(5.197410f, 79.759930f),
            new Vector2(16.201350f, 78.135080f),
            new Vector2(8.002240f, 71.190640f),
            new Vector2(-1.106893f, 64.924550f),
            new Vector2(-6.915630f, 55.852810f),
            new Vector2(-13.363260f, 46.863010f),
            new Vector2(-22.362570f, 40.731670f),
            new Vector2(-28.888330f, 31.636490f),
            new Vector2(-35.316570f, 22.472310f),
            new Vector2(-41.302310f, 13.020790f),
            new Vector2(-47.696900f, 3.844196f),
            new Vector2(-53.856640f, -5.478905f),
            new Vector2(-58.633130f, -14.698570f),
            new Vector2(-65.423770f, -23.594890f),
            new Vector2(-72.591360f, -32.164280f),
            new Vector2(-80.008560f, -40.517180f),
            new Vector2(-87.141860f, -49.079020f),
            new Vector2(-95.031280f, -57.020990f),
            new Vector2(-101.955000f, -65.569400f),
            new Vector2(-91.319390f, -68.490500f),
            new Vector2(-80.121230f, -68.490500f),
            new Vector2(-68.923070f, -68.490500f),
            new Vector2(-57.724900f, -68.490500f),
            new Vector2(-46.526730f, -68.490500f),
            new Vector2(-35.328560f, -68.490500f),
            new Vector2(-24.130390f, -68.490500f),
            new Vector2(-12.932230f, -68.490500f),
            new Vector2(-1.734064f, -68.490500f),
            new Vector2(9.464103f, -68.490500f),
            new Vector2(20.662270f, -68.490500f),
            new Vector2(31.860440f, -68.490500f),
            new Vector2(43.058600f, -68.490500f),
            new Vector2(54.076470f, -66.802110f),
            new Vector2(65.067930f, -65.528370f),
            new Vector2(76.266100f, -65.528370f),
            new Vector2(87.464260f, -65.528370f),
            new Vector2(98.662420f, -65.528370f),
            new Vector2(105.335800f, -57.990740f),
            new Vector2(108.876900f, -47.367220f),
            new Vector2(113.324700f, -37.187100f),
            new Vector2(115.443300f, -26.403080f),
            new Vector2(117.668500f, -15.596550f),
            new Vector2(120.102300f, -4.972919f),
            new Vector2(123.590300f, 5.659225f),
            new Vector2(125.726400f, 16.510750f),
            new Vector2(115.777800f, 17.411350f),
            new Vector2(104.579600f, 17.411350f),
            new Vector2(93.381440f, 17.411350f),
            new Vector2(82.183270f, 17.411350f),
            new Vector2(70.985110f, 17.411350f),
            new Vector2(59.786940f, 17.411350f),
            new Vector2(49.011450f, 14.449240f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-112.882400f, 104.347200f),
            new Vector2(-103.523900f, 109.602800f),
            new Vector2(-92.696840f, 112.123300f),
            new Vector2(-81.559520f, 112.123300f),
            new Vector2(-70.737670f, 114.067200f),
            new Vector2(-59.840630f, 115.547800f),
            new Vector2(-48.778510f, 116.011200f),
            new Vector2(-37.641210f, 116.011200f),
            new Vector2(-26.503900f, 116.011200f),
            new Vector2(-15.782590f, 115.711300f),
            new Vector2(-18.913560f, 105.031700f),
            new Vector2(-23.127190f, 94.958710f),
            new Vector2(-26.062730f, 84.871050f),
            new Vector2(-29.063110f, 74.153400f),
            new Vector2(-33.952300f, 64.316530f),
            new Vector2(-39.010620f, 54.933780f),
            new Vector2(-42.404800f, 44.824150f),
            new Vector2(-48.582650f, 35.557340f),
            new Vector2(-54.722120f, 26.268260f),
            new Vector2(-60.003930f, 16.481450f),
            new Vector2(-66.181760f, 7.214638f),
            new Vector2(-69.857590f, -3.159012f),
            new Vector2(-73.797380f, -13.415870f),
            new Vector2(-77.256970f, -23.999810f),
            new Vector2(-82.450390f, -33.748090f),
            new Vector2(-86.350370f, -44.080210f),
            new Vector2(-92.508760f, -53.192920f),
            new Vector2(-98.462220f, -62.589940f),
            new Vector2(-104.640100f, -71.856740f),
            new Vector2(-95.727480f, -76.444110f),
            new Vector2(-84.590160f, -76.444110f),
            new Vector2(-73.452870f, -76.444110f),
            new Vector2(-62.315560f, -76.444110f),
            new Vector2(-51.178250f, -76.444110f),
            new Vector2(-40.040940f, -76.444110f),
            new Vector2(-28.903640f, -76.444110f),
            new Vector2(-17.766330f, -76.444110f),
            new Vector2(-6.629025f, -76.444110f),
            new Vector2(4.508282f, -76.444110f),
            new Vector2(15.641230f, -76.400110f),
            new Vector2(26.590410f, -74.500140f),
            new Vector2(37.412260f, -72.556180f),
            new Vector2(48.549560f, -72.556180f),
            new Vector2(59.686870f, -72.556180f),
            new Vector2(70.824180f, -72.556180f),
            new Vector2(81.961490f, -72.556180f),
            new Vector2(93.098790f, -72.556180f),
            new Vector2(101.621600f, -66.674080f),
            new Vector2(104.620900f, -55.956110f),
            new Vector2(110.224100f, -46.698820f),
            new Vector2(113.483800f, -36.056690f),
            new Vector2(118.270200f, -26.174390f),
            new Vector2(123.542100f, -16.464490f),
            new Vector2(128.735500f, -6.716232f),
            new Vector2(132.257500f, 3.849533f),
            new Vector2(137.117600f, 13.760820f),
            new Vector2(129.763400f, 18.811570f),
            new Vector2(118.941500f, 20.755540f),
            new Vector2(107.890000f, 20.058820f),
            new Vector2(96.906220f, 18.811570f),
            new Vector2(85.768920f, 18.811570f),
            new Vector2(74.631610f, 18.811570f),
            new Vector2(63.494300f, 18.811570f),
            new Vector2(52.357120f, 18.811570f),
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
