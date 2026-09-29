using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Ga (single stroke).
/// </summary>
public class GaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ga;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(-119.952500f, 121.086200f),
                new Vector2(-107.726200f, 121.086200f),
                new Vector2(-95.499920f, 121.086200f),
                new Vector2(-83.273650f, 121.086200f),
                new Vector2(-71.047360f, 121.086200f),
                new Vector2(-58.821080f, 121.086200f),
                new Vector2(-46.594790f, 121.086200f),
                new Vector2(-34.368510f, 121.086200f),
                new Vector2(-22.142220f, 121.086200f),
                new Vector2(-9.915936f, 121.086200f),
                new Vector2(2.310349f, 121.086200f),
                new Vector2(8.990929f, 113.607100f),
                new Vector2(4.180279f, 104.532600f),
                new Vector2(0.065144f, 93.471650f),
                new Vector2(-7.044178f, 83.530410f),
                new Vector2(-14.572490f, 73.952560f),
                new Vector2(-20.742100f, 63.623490f),
                new Vector2(-27.775880f, 53.816700f),
                new Vector2(-34.552830f, 43.985520f),
                new Vector2(-42.231270f, 34.547600f),
                new Vector2(-48.694350f, 24.287360f),
                new Vector2(-54.741690f, 13.713700f),
                new Vector2(-60.807600f, 3.098301f),
                new Vector2(-66.699650f, -7.544777f),
                new Vector2(-73.613310f, -17.511760f),
                new Vector2(-79.903670f, -27.995730f),
                new Vector2(-88.089570f, -36.999690f),
                new Vector2(-93.143010f, -47.905820f),
                new Vector2(-98.541280f, -58.702010f),
                new Vector2(-103.670900f, -68.544800f),
                new Vector2(-100.699900f, -78.462510f),
                new Vector2(-88.730550f, -79.954500f),
                new Vector2(-76.504260f, -79.954500f),
                new Vector2(-64.277980f, -79.954500f),
                new Vector2(-52.051690f, -79.954500f),
                new Vector2(-40.050400f, -81.340970f),
                new Vector2(-27.824120f, -81.340970f),
                new Vector2(-15.597830f, -81.340970f),
                new Vector2(-3.371548f, -81.340970f),
                new Vector2(8.821648f, -81.938390f),
                new Vector2(21.004230f, -82.727480f),
                new Vector2(33.230510f, -82.727480f),
                new Vector2(45.435380f, -82.859410f),
                new Vector2(57.335690f, -85.108240f),
                new Vector2(69.396160f, -86.224690f),
                new Vector2(81.289980f, -88.273410f),
                new Vector2(93.516270f, -88.273410f),
                new Vector2(103.272100f, -85.802920f),
                new Vector2(110.013600f, -76.081340f),
                new Vector2(114.704400f, -64.885700f),
                new Vector2(116.913300f, -53.119280f),
                new Vector2(123.251500f, -42.768550f),
                new Vector2(126.042000f, -31.055450f),
                new Vector2(129.615300f, -19.613640f),
                new Vector2(130.047500f, -7.767241f),
                new Vector2(120.343800f, -1.069302f),
                new Vector2(108.907600f, 2.250715f),
                new Vector2(97.204240f, 1.848310f),
                new Vector2(84.977950f, 1.848310f),
                new Vector2(72.751670f, 1.848310f),
                new Vector2(60.696080f, 0.461811f),
                new Vector2(48.469790f, 0.461811f),
                new Vector2(36.243510f, 0.461811f),
                new Vector2(24.242230f, 1.848310f)
            }
        };
    }
}
