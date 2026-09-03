using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Fa.
/// Data ini diproses menjadi satu stroke averaged dari tiga recording.
/// </summary>
public class FaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Fa;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-121.122700f, 128.411800f),
            new Vector2(-110.394300f, 124.245200f),
            new Vector2(-99.802840f, 121.143100f),
            new Vector2(-88.958800f, 117.538200f),
            new Vector2(-78.469860f, 112.805400f),
            new Vector2(-67.111180f, 111.745200f),
            new Vector2(-55.580460f, 111.745200f),
            new Vector2(-44.049730f, 111.745200f),
            new Vector2(-32.519010f, 111.745200f),
            new Vector2(-25.289320f, 109.865500f),
            new Vector2(-30.835540f, 100.183600f),
            new Vector2(-34.003970f, 89.708980f),
            new Vector2(-39.440580f, 79.708020f),
            new Vector2(-44.307300f, 69.274500f),
            new Vector2(-50.216890f, 59.734250f),
            new Vector2(-53.903290f, 48.819980f),
            new Vector2(-59.566210f, 39.221020f),
            new Vector2(-62.848280f, 28.175930f),
            new Vector2(-67.124150f, 17.827010f),
            new Vector2(-71.179020f, 7.352974f),
            new Vector2(-74.966260f, -3.395256f),
            new Vector2(-79.602580f, -13.841260f),
            new Vector2(-82.399200f, -25.027700f),
            new Vector2(-83.697370f, -36.395690f),
            new Vector2(-87.343730f, -47.334680f),
            new Vector2(-88.223050f, -58.739700f),
            new Vector2(-91.368130f, -69.824660f),
            new Vector2(-93.038090f, -81.084400f),
            new Vector2(-96.067980f, -92.202900f),
            new Vector2(-86.648240f, -97.649700f),
            new Vector2(-75.218690f, -98.671470f),
            new Vector2(-63.687960f, -98.671470f),
            new Vector2(-52.157240f, -98.671470f),
            new Vector2(-40.626530f, -98.671470f),
            new Vector2(-29.095790f, -98.671470f),
            new Vector2(-17.565080f, -98.671470f),
            new Vector2(-6.034363f, -98.671470f),
            new Vector2(5.496353f, -98.671470f),
            new Vector2(17.027080f, -98.671470f),
            new Vector2(28.557810f, -98.671470f),
            new Vector2(40.088520f, -98.671470f),
            new Vector2(51.619250f, -98.671470f),
            new Vector2(63.149970f, -98.671470f),
            new Vector2(74.680690f, -98.671470f),
            new Vector2(84.800980f, -96.036030f),
            new Vector2(89.439160f, -85.590850f),
            new Vector2(91.377400f, -74.298740f),
            new Vector2(93.505870f, -63.028960f),
            new Vector2(95.544050f, -51.700050f),
            new Vector2(97.573830f, -40.498710f),
            new Vector2(97.627350f, -28.976680f),
            new Vector2(97.627350f, -17.445950f),
            new Vector2(97.627350f, -5.915237f),
            new Vector2(100.485200f, 5.200722f),
            new Vector2(102.971900f, 16.456830f),
            new Vector2(106.409900f, 27.451500f),
            new Vector2(110.425700f, 38.236760f),
            new Vector2(112.687000f, 49.543570f),
            new Vector2(114.948400f, 60.850380f),
            new Vector2(117.406800f, 72.112880f),
            new Vector2(118.687500f, 83.485920f),
            new Vector2(121.769800f, 94.589200f),
            new Vector2(124.052300f, 105.549300f),
            new Vector2(128.877300f, 115.911800f),
        };

        var secondStroke = new List<Vector2>
        {
            new Vector2(-118.709900f, 73.457430f),
            new Vector2(-108.394300f, 73.457430f),
            new Vector2(-98.078660f, 73.457430f),
            new Vector2(-87.763060f, 73.457430f),
            new Vector2(-77.447450f, 73.457430f),
            new Vector2(-67.131850f, 73.457430f),
            new Vector2(-56.928330f, 74.589250f),
            new Vector2(-46.669060f, 75.158100f),
            new Vector2(-36.390600f, 75.386980f),
            new Vector2(-26.313850f, 76.858800f),
            new Vector2(-15.998250f, 76.858800f),
            new Vector2(-5.958626f, 78.559450f),
            new Vector2(4.080994f, 80.260120f),
            new Vector2(4.912834f, 76.331830f),
            new Vector2(-2.381409f, 69.037630f),
            new Vector2(-10.317760f, 62.576260f),
            new Vector2(-16.039830f, 53.993180f),
            new Vector2(-21.452670f, 45.501280f),
            new Vector2(-26.934550f, 36.800700f),
            new Vector2(-32.656630f, 28.217610f),
            new Vector2(-38.378680f, 19.634510f),
            new Vector2(-42.818240f, 10.424150f),
            new Vector2(-47.532970f, 1.273788f),
            new Vector2(-52.162340f, -7.843735f),
            new Vector2(-56.981730f, -16.868320f),
            new Vector2(-62.124040f, -25.492490f),
            new Vector2(-66.848400f, -34.563550f),
            new Vector2(-72.669330f, -42.855830f),
            new Vector2(-78.294910f, -51.494920f),
            new Vector2(-83.295930f, -60.496450f),
            new Vector2(-88.200500f, -69.553970f),
            new Vector2(-88.409520f, -77.591200f),
            new Vector2(-78.223080f, -77.903070f),
            new Vector2(-67.907490f, -77.903070f),
            new Vector2(-57.591900f, -77.903070f),
            new Vector2(-47.276300f, -77.903070f),
            new Vector2(-36.960700f, -77.903070f),
            new Vector2(-26.645100f, -77.903070f),
            new Vector2(-16.329500f, -77.903070f),
            new Vector2(-6.013901f, -77.903070f),
            new Vector2(4.301697f, -77.903070f),
            new Vector2(14.617290f, -77.903070f),
            new Vector2(24.932890f, -77.903070f),
            new Vector2(35.248490f, -77.903070f),
            new Vector2(45.564090f, -77.903070f),
            new Vector2(55.879690f, -77.903070f),
            new Vector2(65.053830f, -74.321120f),
            new Vector2(69.285730f, -64.936600f),
            new Vector2(74.010090f, -55.865550f),
            new Vector2(78.143840f, -46.440370f),
            new Vector2(81.530460f, -36.705710f),
            new Vector2(86.103320f, -27.485960f),
            new Vector2(90.561850f, -18.291160f),
            new Vector2(95.636920f, -9.631470f),
            new Vector2(99.248700f, -0.114853f),
            new Vector2(102.140700f, 9.325134f),
            new Vector2(105.779800f, 18.777830f),
            new Vector2(107.394300f, 28.894670f),
            new Vector2(111.722800f, 38.154130f),
            new Vector2(114.837800f, 47.910050f),
            new Vector2(118.976300f, 57.333280f),
            new Vector2(122.411200f, 66.941730f),
            new Vector2(126.565800f, 76.291430f),
            new Vector2(131.290100f, 85.362180f),
        };

        var thirdStroke = new List<Vector2>
        {
            new Vector2(-129.213600f, 77.719700f),
            new Vector2(-118.692200f, 77.719700f),
            new Vector2(-108.170800f, 77.719700f),
            new Vector2(-97.649410f, 77.719700f),
            new Vector2(-87.128010f, 77.719700f),
            new Vector2(-76.606610f, 77.719700f),
            new Vector2(-66.085220f, 77.719700f),
            new Vector2(-55.563820f, 77.719700f),
            new Vector2(-45.483230f, 80.436040f),
            new Vector2(-35.054230f, 81.005420f),
            new Vector2(-24.943110f, 83.533690f),
            new Vector2(-14.421710f, 83.533690f),
            new Vector2(-3.900314f, 83.533690f),
            new Vector2(-1.949745f, 79.983480f),
            new Vector2(-8.273895f, 71.629150f),
            new Vector2(-15.231880f, 63.794440f),
            new Vector2(-19.007330f, 54.075670f),
            new Vector2(-24.843490f, 45.321300f),
            new Vector2(-30.763100f, 36.635330f),
            new Vector2(-37.463160f, 28.662450f),
            new Vector2(-42.207880f, 19.274700f),
            new Vector2(-48.044130f, 10.520390f),
            new Vector2(-53.832290f, 1.738167f),
            new Vector2(-58.537540f, -7.672478f),
            new Vector2(-65.344710f, -15.557430f),
            new Vector2(-69.052570f, -25.352700f),
            new Vector2(-73.981000f, -34.381890f),
            new Vector2(-73.981000f, -44.903300f),
            new Vector2(-76.886210f, -54.545070f),
            new Vector2(-81.463870f, -63.914960f),
            new Vector2(-84.791050f, -73.896420f),
            new Vector2(-79.006580f, -80.751590f),
            new Vector2(-68.912800f, -82.163960f),
            new Vector2(-58.391410f, -82.163960f),
            new Vector2(-47.870010f, -82.163960f),
            new Vector2(-37.348610f, -82.163960f),
            new Vector2(-26.827220f, -82.163960f),
            new Vector2(-16.305820f, -82.163960f),
            new Vector2(-5.784424f, -82.163960f),
            new Vector2(4.736977f, -82.163960f),
            new Vector2(15.258370f, -82.163960f),
            new Vector2(25.779760f, -82.163960f),
            new Vector2(36.301160f, -82.163960f),
            new Vector2(46.822560f, -82.163960f),
            new Vector2(57.343960f, -82.163960f),
            new Vector2(67.865350f, -82.163960f),
            new Vector2(77.182380f, -79.256390f),
            new Vector2(82.116130f, -70.267830f),
            new Vector2(85.443400f, -60.286400f),
            new Vector2(88.770470f, -50.304890f),
            new Vector2(92.008870f, -40.297570f),
            new Vector2(94.560710f, -30.090320f),
            new Vector2(97.112430f, -19.883030f),
            new Vector2(97.554740f, -9.417046f),
            new Vector2(100.437600f, 0.636528f),
            new Vector2(101.215100f, 11.062210f),
            new Vector2(103.895300f, 21.232080f),
            new Vector2(107.222400f, 31.213580f),
            new Vector2(109.158600f, 41.420770f),
            new Vector2(111.118900f, 51.624050f),
            new Vector2(113.891300f, 61.767070f),
            new Vector2(114.972400f, 72.155380f),
            new Vector2(117.459400f, 82.273180f),
            new Vector2(120.786400f, 92.254590f),
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
