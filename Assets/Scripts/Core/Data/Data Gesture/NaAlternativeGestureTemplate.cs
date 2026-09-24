using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template alternatif untuk gesture Na dengan arah stroke berbeda.
/// </summary>
public class NaAlternativeGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Na;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(121.999400f, -112.014300f),
                new Vector2(109.586700f, -112.014300f),
                new Vector2(97.272710f, -111.016400f),
                new Vector2(84.978290f, -109.821400f),
                new Vector2(72.565530f, -109.821400f),
                new Vector2(60.152780f, -109.821400f),
                new Vector2(47.740030f, -109.821400f),
                new Vector2(35.327260f, -109.821400f),
                new Vector2(22.914510f, -109.821400f),
                new Vector2(10.501760f, -109.821400f),
                new Vector2(-1.910995f, -109.821400f),
                new Vector2(-14.323750f, -109.821400f),
                new Vector2(-26.736500f, -109.821400f),
                new Vector2(-39.149260f, -109.821400f),
                new Vector2(-51.562010f, -109.821400f),
                new Vector2(-63.974770f, -109.821400f),
                new Vector2(-76.387520f, -109.821400f),
                new Vector2(-88.800280f, -109.821400f),
                new Vector2(-101.213000f, -109.821400f),
                new Vector2(-98.975750f, -107.284300f),
                new Vector2(-89.288170f, -99.617770f),
                new Vector2(-80.257130f, -91.150330f),
                new Vector2(-69.166560f, -85.824100f),
                new Vector2(-60.506000f, -77.820250f),
                new Vector2(-51.408660f, -69.833320f),
                new Vector2(-45.332160f, -59.184700f),
                new Vector2(-36.646450f, -50.881370f),
                new Vector2(-29.787140f, -40.587550f),
                new Vector2(-22.558040f, -30.694630f),
                new Vector2(-15.095180f, -21.105220f),
                new Vector2(-8.694000f, -10.841990f),
                new Vector2(2.128525f, -5.097401f),
                new Vector2(8.733475f, 4.726448f),
                new Vector2(19.061490f, 11.611820f),
                new Vector2(25.812390f, 21.229390f),
                new Vector2(35.202480f, 29.258880f),
                new Vector2(43.979640f, 38.036000f),
                new Vector2(53.824680f, 45.085280f),
                new Vector2(60.498570f, 54.554920f),
                new Vector2(70.034420f, 62.406590f),
                new Vector2(76.785290f, 72.024150f),
                new Vector2(86.095100f, 80.151530f),
                new Vector2(92.161610f, 90.603940f),
                new Vector2(102.538300f, 96.732570f),
                new Vector2(102.358600f, 105.471900f),
                new Vector2(91.055150f, 109.476800f),
                new Vector2(78.642400f, 109.476800f),
                new Vector2(66.229640f, 109.476800f),
                new Vector2(53.816890f, 109.476800f),
                new Vector2(41.404140f, 109.476800f),
                new Vector2(28.991380f, 109.476800f),
                new Vector2(16.578620f, 109.476800f),
                new Vector2(4.165871f, 109.476800f),
                new Vector2(-8.090225f, 110.749300f),
                new Vector2(-20.389640f, 111.669900f),
                new Vector2(-32.802400f, 111.669900f),
                new Vector2(-44.859280f, 113.862900f),
                new Vector2(-55.507140f, 118.123800f),
                new Vector2(-67.868100f, 118.248800f),
                new Vector2(-80.280850f, 118.248800f),
                new Vector2(-92.693610f, 118.248800f),
                new Vector2(-104.659000f, 115.312300f),
                new Vector2(-116.213700f, 110.984300f),
                new Vector2(-128.000600f, 107.283900f)
            }
        };
    }
}
