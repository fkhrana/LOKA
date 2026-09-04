using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template gesture Ta untuk bentuk satu stroke.
/// Dua recording dirata-ratakan menjadi satu stroke.
/// </summary>
public class TaSingleStrokeTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ta;

    public List<List<Vector2>> GetStrokes()
    {
        var firstStroke = new List<Vector2>
        {
            new Vector2(-116.836300f, 21.342720f), new Vector2(-102.787600f, 19.730130f), new Vector2(-88.540440f, 19.730130f), new Vector2(-74.293250f, 19.730130f), new Vector2(-60.046070f, 19.730130f), new Vector2(-45.798880f, 19.730130f), new Vector2(-31.551690f, 19.730130f), new Vector2(-25.992330f, 14.360740f), new Vector2(-35.752560f, 4.060101f), new Vector2(-45.826870f, -6.014158f), new Vector2(-57.100360f, -14.721840f), new Vector2(-67.964900f, -23.834890f), new Vector2(-78.974330f, -32.711440f), new Vector2(-89.797610f, -41.922140f), new Vector2(-101.936600f, -48.916960f), new Vector2(-114.090400f, -56.220090f), new Vector2(-126.480600f, -63.027410f), new Vector2(-138.273200f, -70.731280f), new Vector2(-133.611400f, -75.411090f), new Vector2(-119.364200f, -75.411090f), new Vector2(-105.117000f, -75.411090f), new Vector2(-90.869830f, -75.411090f), new Vector2(-76.622650f, -75.411090f), new Vector2(-62.375470f, -75.411090f), new Vector2(-48.128280f, -75.411090f), new Vector2(-33.881090f, -75.411090f), new Vector2(-19.633900f, -75.411090f), new Vector2(-5.386710f, -75.411090f), new Vector2(2.493414f, -66.795360f), new Vector2(7.608265f, -53.616160f), new Vector2(12.113590f, -40.100080f), new Vector2(14.937980f, -26.333420f), new Vector2(20.049140f, -13.068290f), new Vector2(24.824250f, 0.345225f), new Vector2(30.313980f, 13.491210f), new Vector2(33.075300f, 27.395670f), new Vector2(36.956770f, 41.065840f), new Vector2(39.582360f, 55.030650f), new Vector2(41.514610f, 69.066290f), new Vector2(47.127190f, 82.131520f), new Vector2(51.702900f, 95.573340f), new Vector2(59.306320f, 107.555100f), new Vector2(64.647000f, 120.725100f), new Vector2(70.221000f, 131.900600f), new Vector2(68.940220f, 117.811100f), new Vector2(65.383350f, 104.128100f), new Vector2(63.032340f, 90.215730f), new Vector2(60.545620f, 76.253620f), new Vector2(57.014740f, 62.657800f), new Vector2(52.126680f, 49.300250f), new Vector2(46.943450f, 36.064960f), new Vector2(42.171290f, 22.659400f), new Vector2(40.742070f, 9.668196f), new Vector2(54.628010f, 10.232330f), new Vector2(68.642340f, 11.667330f), new Vector2(82.889530f, 11.667330f), new Vector2(97.136710f, 11.667330f), new Vector2(111.383900f, 11.667330f), new Vector2(111.726800f, -1.304111f), new Vector2(109.113300f, -15.173870f), new Vector2(106.320700f, -29.121290f), new Vector2(104.084900f, -43.062510f), new Vector2(100.100300f, -56.726250f), new Vector2(97.634600f, -70.573300f)
        };
        var secondStroke = new List<Vector2>
        {
            new Vector2(-127.622800f, 5.314779f), new Vector2(-115.587000f, 6.680956f), new Vector2(-103.383000f, 6.680956f), new Vector2(-91.178990f, 6.680956f), new Vector2(-78.974980f, 6.680956f), new Vector2(-66.770990f, 6.680956f), new Vector2(-54.566990f, 6.680956f), new Vector2(-43.319480f, 6.284764f), new Vector2(-50.394560f, -3.202248f), new Vector2(-60.933270f, -9.316328f), new Vector2(-70.538600f, -16.689540f), new Vector2(-80.368480f, -23.653670f), new Vector2(-90.111050f, -30.759060f), new Vector2(-99.289220f, -38.756000f), new Vector2(-109.063200f, -45.797740f), new Vector2(-118.308100f, -53.676560f), new Vector2(-109.024700f, -56.160590f), new Vector2(-96.820720f, -56.160590f), new Vector2(-84.616730f, -56.160590f), new Vector2(-72.412730f, -56.160590f), new Vector2(-60.208740f, -56.160590f), new Vector2(-48.004750f, -56.160590f), new Vector2(-35.800750f, -56.160590f), new Vector2(-23.596750f, -56.160590f), new Vector2(-11.392760f, -56.160590f), new Vector2(0.811241f, -56.160590f), new Vector2(13.015230f, -56.160590f), new Vector2(13.087630f, -44.028990f), new Vector2(15.616050f, -32.181780f), new Vector2(18.552150f, -20.367830f), new Vector2(18.552150f, -8.163835f), new Vector2(19.917420f, 3.943137f), new Vector2(22.024340f, 15.959210f), new Vector2(25.109620f, 27.719650f), new Vector2(27.343220f, 39.707410f), new Vector2(29.284340f, 51.751890f), new Vector2(31.454280f, 63.754110f), new Vector2(34.254420f, 75.629650f), new Vector2(35.725390f, 87.669210f), new Vector2(38.991380f, 99.419700f), new Vector2(41.287940f, 111.285400f), new Vector2(40.581270f, 103.165700f), new Vector2(38.998050f, 91.151360f), new Vector2(36.350700f, 79.241460f), new Vector2(33.688400f, 67.334920f), new Vector2(31.429900f, 55.459860f), new Vector2(29.188540f, 43.469630f), new Vector2(26.164400f, 31.665570f), new Vector2(24.016540f, 19.693090f), new Vector2(22.112140f, 7.798127f), new Vector2(26.550660f, -0.149667f), new Vector2(38.754650f, -0.149667f), new Vector2(50.958640f, -0.149667f), new Vector2(63.162640f, -0.149667f), new Vector2(75.366640f, -0.149667f), new Vector2(87.570630f, -0.149667f), new Vector2(99.774640f, -0.149667f), new Vector2(111.978600f, -0.149667f), new Vector2(122.377200f, -1.955090f), new Vector2(119.645000f, -13.514110f), new Vector2(115.212300f, -24.558250f), new Vector2(111.861000f, -36.003150f), new Vector2(106.901900f, -47.023560f), new Vector2(104.617700f, -58.892830f)
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
