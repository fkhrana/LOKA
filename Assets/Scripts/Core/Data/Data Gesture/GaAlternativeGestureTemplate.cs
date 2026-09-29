using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template alternatif untuk gesture Ga dengan arah stroke berbeda.
/// </summary>
public class GaAlternativeGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ga;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(6.322453f, -6.538690f),
                new Vector2(17.067690f, -11.058510f),
                new Vector2(29.319540f, -11.058510f),
                new Vector2(41.571390f, -11.058510f),
                new Vector2(53.823230f, -11.058510f),
                new Vector2(66.075070f, -11.058510f),
                new Vector2(77.810520f, -8.376984f),
                new Vector2(89.998490f, -8.045313f),
                new Vector2(102.250300f, -8.045313f),
                new Vector2(114.502200f, -8.045313f),
                new Vector2(126.754000f, -8.045313f),
                new Vector2(125.364900f, -20.016950f),
                new Vector2(120.264300f, -30.889590f),
                new Vector2(113.159300f, -40.865170f),
                new Vector2(106.259400f, -50.985900f),
                new Vector2(101.757500f, -61.764580f),
                new Vector2(92.323090f, -68.877110f),
                new Vector2(89.530230f, -80.492190f),
                new Vector2(82.630340f, -89.754970f),
                new Vector2(72.675150f, -96.888820f),
                new Vector2(60.438780f, -96.935260f),
                new Vector2(48.186940f, -96.935260f),
                new Vector2(35.935090f, -96.935260f),
                new Vector2(23.683250f, -96.935260f),
                new Vector2(11.431400f, -96.935260f),
                new Vector2(-0.820445f, -96.935260f),
                new Vector2(-13.072290f, -96.935260f),
                new Vector2(-25.079640f, -95.428640f),
                new Vector2(-37.331490f, -95.428640f),
                new Vector2(-49.583330f, -95.428640f),
                new Vector2(-61.835180f, -95.428640f),
                new Vector2(-73.903700f, -94.298970f),
                new Vector2(-85.660340f, -92.097060f),
                new Vector2(-87.004520f, -80.031100f),
                new Vector2(-81.896210f, -69.144920f),
                new Vector2(-76.423930f, -58.684270f),
                new Vector2(-70.735320f, -48.870860f),
                new Vector2(-67.501460f, -37.226080f),
                new Vector2(-62.467140f, -26.602320f),
                new Vector2(-56.215800f, -16.099130f),
                new Vector2(-51.244460f, -5.744774f),
                new Vector2(-45.845890f, 4.098839f),
                new Vector2(-40.647610f, 14.999810f),
                new Vector2(-35.980420f, 26.135740f),
                new Vector2(-29.404890f, 36.077720f),
                new Vector2(-22.863690f, 46.105100f),
                new Vector2(-17.033150f, 56.726400f),
                new Vector2(-13.263510f, 67.972810f),
                new Vector2(-9.280241f, 79.503200f),
                new Vector2(-7.203394f, 91.458290f),
                new Vector2(-0.870212f, 101.888900f),
                new Vector2(4.175173f, 112.934100f),
                new Vector2(6.091775f, 124.767000f),
                new Vector2(-4.103594f, 129.980400f),
                new Vector2(-15.722960f, 132.069400f),
                new Vector2(-26.965360f, 135.871500f),
                new Vector2(-39.128850f, 136.589200f),
                new Vector2(-51.051750f, 135.502800f),
                new Vector2(-62.720200f, 133.576000f),
                new Vector2(-74.972050f, 133.576000f),
                new Vector2(-87.223890f, 133.576000f),
                new Vector2(-99.475740f, 133.576000f),
                new Vector2(-111.622800f, 132.930500f),
                new Vector2(-123.246000f, 132.069400f)
            }
        };
    }
}
