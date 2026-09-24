using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template recorded untuk gesture Na (single stroke).
/// </summary>
public class NaGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Na;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(-106.606800f, 120.570500f),
                new Vector2(-94.854350f, 120.570500f),
                new Vector2(-83.101840f, 120.570500f),
                new Vector2(-71.349360f, 120.570500f),
                new Vector2(-59.596860f, 120.570500f),
                new Vector2(-47.844350f, 120.570500f),
                new Vector2(-36.091850f, 120.570500f),
                new Vector2(-24.339350f, 120.570500f),
                new Vector2(-12.586850f, 120.570500f),
                new Vector2(-0.834351f, 120.570500f),
                new Vector2(10.918150f, 120.570500f),
                new Vector2(22.670650f, 120.570500f),
                new Vector2(34.423160f, 120.570500f),
                new Vector2(46.175660f, 120.570500f),
                new Vector2(57.928160f, 120.570500f),
                new Vector2(69.680660f, 120.570500f),
                new Vector2(81.433170f, 120.570500f),
                new Vector2(91.591330f, 118.976200f),
                new Vector2(88.644150f, 107.919400f),
                new Vector2(79.967050f, 101.463500f),
                new Vector2(76.250600f, 90.314050f),
                new Vector2(67.961120f, 82.294550f),
                new Vector2(59.140790f, 74.606440f),
                new Vector2(50.877960f, 66.257280f),
                new Vector2(44.180210f, 56.604990f),
                new Vector2(35.487230f, 49.675180f),
                new Vector2(26.952550f, 42.756580f),
                new Vector2(18.692070f, 36.180710f),
                new Vector2(11.691800f, 28.622040f),
                new Vector2(7.187630f, 18.260360f),
                new Vector2(-0.663162f, 10.268710f),
                new Vector2(-7.780510f, 0.928463f),
                new Vector2(-16.090770f, -7.381832f),
                new Vector2(-23.206810f, -16.727870f),
                new Vector2(-32.142710f, -24.137870f),
                new Vector2(-39.996600f, -32.737490f),
                new Vector2(-48.980290f, -40.271330f),
                new Vector2(-58.127720f, -47.560440f),
                new Vector2(-63.487690f, -57.672980f),
                new Vector2(-70.935630f, -66.731170f),
                new Vector2(-79.982830f, -74.142550f),
                new Vector2(-86.368480f, -82.893560f),
                new Vector2(-91.039920f, -91.340030f),
                new Vector2(-97.136540f, -101.018400f),
                new Vector2(-96.678830f, -109.159200f),
                new Vector2(-84.926320f, -109.159200f),
                new Vector2(-73.173820f, -109.159200f),
                new Vector2(-61.421320f, -109.159200f),
                new Vector2(-49.668820f, -109.159200f),
                new Vector2(-37.916310f, -109.159200f),
                new Vector2(-26.163810f, -109.159200f),
                new Vector2(-14.411320f, -109.159200f),
                new Vector2(-2.658813f, -109.159200f),
                new Vector2(9.093689f, -109.159200f),
                new Vector2(20.846190f, -109.159200f),
                new Vector2(32.389820f, -110.446400f),
                new Vector2(43.725920f, -113.521700f),
                new Vector2(54.828900f, -115.189600f),
                new Vector2(64.982130f, -120.420500f),
                new Vector2(76.572410f, -121.420200f),
                new Vector2(87.756160f, -124.925000f),
                new Vector2(97.849400f, -128.930800f),
                new Vector2(109.395400f, -129.429500f),
                new Vector2(120.870600f, -127.177200f)
            }
        };
    }
}
