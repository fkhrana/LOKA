using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template alternatif untuk gesture Ba dengan arah stroke berbeda.
/// </summary>
public class BaAlternativeGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ba;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(52.324210f, 137.740400f),
                new Vector2(67.397170f, 137.740400f),
                new Vector2(82.470140f, 137.740400f),
                new Vector2(97.543110f, 137.740400f),
                new Vector2(111.295900f, 133.560900f),
                new Vector2(113.525400f, 118.771200f),
                new Vector2(115.332300f, 103.991500f),
                new Vector2(116.536200f, 89.037740f),
                new Vector2(117.364800f, 74.046820f),
                new Vector2(117.364800f, 58.973850f),
                new Vector2(117.364800f, 43.900890f),
                new Vector2(119.397400f, 29.157780f),
                new Vector2(119.397400f, 14.084820f),
                new Vector2(121.939200f, -0.218552f),
                new Vector2(123.462400f, -14.830320f),
                new Vector2(123.462400f, -29.903290f),
                new Vector2(123.462400f, -44.976260f),
                new Vector2(123.261500f, -60.016620f),
                new Vector2(120.423200f, -74.628980f),
                new Vector2(117.178100f, -88.795720f),
                new Vector2(105.020400f, -95.999230f),
                new Vector2(89.947480f, -95.999230f),
                new Vector2(78.316220f, -89.470950f),
                new Vector2(67.658040f, -78.812750f),
                new Vector2(62.486820f, -65.369740f),
                new Vector2(53.603390f, -53.842200f),
                new Vector2(45.691010f, -41.084600f),
                new Vector2(36.845930f, -29.239230f),
                new Vector2(29.641300f, -16.080520f),
                new Vector2(19.059620f, -5.824120f),
                new Vector2(7.977856f, 4.148720f),
                new Vector2(-4.238491f, 12.285800f),
                new Vector2(-12.716420f, 4.566200f),
                new Vector2(-14.748950f, -10.176930f),
                new Vector2(-20.772710f, -22.754780f),
                new Vector2(-22.879030f, -37.467340f),
                new Vector2(-25.258900f, -51.978490f),
                new Vector2(-26.944090f, -66.653630f),
                new Vector2(-26.944090f, -81.726590f),
                new Vector2(-26.944090f, -96.799560f),
                new Vector2(-32.654770f, -106.161800f),
                new Vector2(-46.496930f, -102.096800f),
                new Vector2(-61.569890f, -102.096800f),
                new Vector2(-76.433380f, -100.395200f),
                new Vector2(-90.234830f, -95.999230f),
                new Vector2(-104.893700f, -93.609800f),
                new Vector2(-118.743300f, -87.757200f),
                new Vector2(-124.505000f, -76.837030f),
                new Vector2(-126.537500f, -62.014280f),
                new Vector2(-126.537500f, -46.941310f),
                new Vector2(-126.537500f, -31.868350f),
                new Vector2(-126.537500f, -16.795380f),
                new Vector2(-126.537500f, -1.722420f),
                new Vector2(-126.537500f, 13.350550f),
                new Vector2(-126.537500f, 28.423520f),
                new Vector2(-126.537500f, 43.496480f),
                new Vector2(-124.578600f, 58.251550f),
                new Vector2(-124.505000f, 73.312570f),
                new Vector2(-124.505000f, 88.385540f),
                new Vector2(-124.505000f, 103.458500f),
                new Vector2(-124.505000f, 118.531500f),
                new Vector2(-121.809800f, 132.968100f),
                new Vector2(-109.046400f, 139.505800f),
                new Vector2(-94.017190f, 139.773000f)
            }
        };
    }
}
