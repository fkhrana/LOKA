using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Ba (single stroke).
/// </summary>
public class BaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Ba;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(-75.128720f, 114.462400f),
                new Vector2(-88.772590f, 119.318900f),
                new Vector2(-103.072800f, 121.412900f),
                new Vector2(-117.028100f, 118.270100f),
                new Vector2(-124.774100f, 106.589800f),
                new Vector2(-124.774100f, 91.979020f),
                new Vector2(-124.774100f, 77.368260f),
                new Vector2(-124.774100f, 62.757510f),
                new Vector2(-124.774100f, 48.146740f),
                new Vector2(-124.774100f, 33.536000f),
                new Vector2(-124.774100f, 18.925230f),
                new Vector2(-124.774100f, 4.314468f),
                new Vector2(-124.774100f, -10.296280f),
                new Vector2(-124.774100f, -24.907040f),
                new Vector2(-124.774100f, -39.517790f),
                new Vector2(-123.001100f, -53.840860f),
                new Vector2(-122.248900f, -68.377120f),
                new Vector2(-121.228100f, -82.886800f),
                new Vector2(-118.775800f, -96.530620f),
                new Vector2(-104.452800f, -98.303680f),
                new Vector2(-89.842030f, -98.303680f),
                new Vector2(-76.304950f, -101.849800f),
                new Vector2(-61.981930f, -100.076800f),
                new Vector2(-47.371180f, -100.076800f),
                new Vector2(-33.497700f, -96.105180f),
                new Vector2(-19.643330f, -91.628890f),
                new Vector2(-13.071960f, -80.123950f),
                new Vector2(-10.552310f, -65.922080f),
                new Vector2(-9.525834f, -51.477900f),
                new Vector2(-9.525834f, -36.867140f),
                new Vector2(-7.852777f, -22.462340f),
                new Vector2(-8.318002f, -7.955597f),
                new Vector2(-9.525834f, 6.459152f),
                new Vector2(-3.802366f, 18.157560f),
                new Vector2(7.511992f, 20.837020f),
                new Vector2(17.384180f, 10.615420f),
                new Vector2(25.935150f, -1.145035f),
                new Vector2(31.366630f, -13.534290f),
                new Vector2(39.448570f, -25.304920f),
                new Vector2(48.052800f, -37.087950f),
                new Vector2(56.919940f, -48.571170f),
                new Vector2(65.629590f, -59.585570f),
                new Vector2(74.089130f, -71.032610f),
                new Vector2(84.600270f, -80.650380f),
                new Vector2(94.976800f, -87.665410f),
                new Vector2(105.722500f, -83.319230f),
                new Vector2(109.362900f, -69.557170f),
                new Vector2(111.308800f, -55.216160f),
                new Vector2(114.587700f, -41.379470f),
                new Vector2(117.316500f, -27.104640f),
                new Vector2(118.133800f, -12.594480f),
                new Vector2(123.453000f, 0.654846f),
                new Vector2(123.453000f, 15.265590f),
                new Vector2(123.453000f, 29.876360f),
                new Vector2(123.453000f, 44.487110f),
                new Vector2(123.453000f, 59.097870f),
                new Vector2(125.225900f, 73.420930f),
                new Vector2(125.225900f, 88.031710f),
                new Vector2(125.225900f, 102.642500f),
                new Vector2(125.225900f, 117.253200f),
                new Vector2(122.549100f, 130.431400f),
                new Vector2(108.471700f, 132.192900f),
                new Vector2(94.603530f, 135.468400f),
                new Vector2(80.899720f, 139.285100f)
            }
        };
    }
}
