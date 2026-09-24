using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template alternatif untuk gesture Ma dengan arah stroke berbeda.
/// </summary>
public class MaAlternativeGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ma;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(-127.366000f, 123.382600f),
                new Vector2(-112.168400f, 123.382600f),
                new Vector2(-96.970870f, 123.382600f),
                new Vector2(-81.773300f, 123.382600f),
                new Vector2(-66.575710f, 123.382600f),
                new Vector2(-51.378140f, 123.382600f),
                new Vector2(-36.180570f, 123.382600f),
                new Vector2(-20.982990f, 123.382600f),
                new Vector2(-5.785408f, 123.382600f),
                new Vector2(8.892063f, 120.177500f),
                new Vector2(21.973550f, 115.068800f),
                new Vector2(36.632030f, 113.767300f),
                new Vector2(51.829610f, 113.767300f),
                new Vector2(67.027180f, 113.767300f),
                new Vector2(82.224760f, 113.767300f),
                new Vector2(97.422330f, 113.767300f),
                new Vector2(112.619900f, 113.767300f),
                new Vector2(116.223700f, 102.173500f),
                new Vector2(112.842600f, 87.774110f),
                new Vector2(109.813600f, 73.291580f),
                new Vector2(103.822000f, 59.908120f),
                new Vector2(102.256200f, 45.312480f),
                new Vector2(100.198200f, 30.967360f),
                new Vector2(96.992970f, 16.526430f),
                new Vector2(90.582820f, 5.290526f),
                new Vector2(87.377640f, -9.386913f),
                new Vector2(84.273330f, -23.851670f),
                new Vector2(77.762210f, -35.063630f),
                new Vector2(75.094960f, -49.156410f),
                new Vector2(71.351960f, -62.803590f),
                new Vector2(68.967520f, -76.527520f),
                new Vector2(64.941800f, -89.237090f),
                new Vector2(61.736630f, -103.678000f),
                new Vector2(58.531450f, -118.355500f),
                new Vector2(48.390620f, -123.412200f),
                new Vector2(33.193040f, -123.412200f),
                new Vector2(17.995460f, -123.412200f),
                new Vector2(2.797882f, -123.412200f),
                new Vector2(-12.399690f, -123.412200f),
                new Vector2(-27.124870f, -126.323300f),
                new Vector2(-42.274730f, -126.617300f),
                new Vector2(-57.472300f, -126.617300f),
                new Vector2(-72.669880f, -126.617300f),
                new Vector2(-87.867460f, -126.617300f),
                new Vector2(-91.526080f, -120.207100f),
                new Vector2(-78.983700f, -113.796800f),
                new Vector2(-74.187160f, -102.284700f),
                new Vector2(-69.776490f, -88.361440f),
                new Vector2(-60.058310f, -78.477490f),
                new Vector2(-55.858220f, -65.222440f),
                new Vector2(-49.220750f, -53.660210f),
                new Vector2(-41.489430f, -40.740810f),
                new Vector2(-32.605750f, -28.651990f),
                new Vector2(-24.801910f, -17.993380f),
                new Vector2(-18.391600f, -9.206112f),
                new Vector2(-13.011380f, 3.762901f),
                new Vector2(-19.311950f, 11.203220f),
                new Vector2(-33.181990f, 14.408270f),
                new Vector2(-48.379570f, 14.408270f),
                new Vector2(-63.577140f, 14.408270f),
                new Vector2(-78.774720f, 14.408270f),
                new Vector2(-90.010560f, 7.998069f),
                new Vector2(-103.880500f, 4.792919f),
                new Vector2(-117.750500f, 1.587768f)
            }
        };
    }
}
