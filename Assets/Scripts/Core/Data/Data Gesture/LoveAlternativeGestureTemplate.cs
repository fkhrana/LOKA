using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template alternatif untuk gesture Love dengan arah awal stroke berbeda.
/// </summary>
public class LoveAlternativeGestureTemplate : IGestureTemplateProvider
{
    public GestureShape Shape => GestureShape.Love;

    public List<List<Vector2>> GetStrokes()
    {
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                new Vector2(-5.254545f, -9.015038f),
                new Vector2(-8.502806f, 4.087002f),
                new Vector2(-13.791930f, 16.780870f),
                new Vector2(-19.359880f, 29.354700f),
                new Vector2(-26.768030f, 40.798350f),
                new Vector2(-37.313480f, 49.575040f),
                new Vector2(-48.975600f, 56.686350f),
                new Vector2(-59.127820f, 65.604290f),
                new Vector2(-69.240940f, 74.791780f),
                new Vector2(-81.052280f, 80.932630f),
                new Vector2(-92.476060f, 88.044950f),
                new Vector2(-104.948300f, 88.104250f),
                new Vector2(-115.078900f, 78.934980f),
                new Vector2(-120.222800f, 66.909280f),
                new Vector2(-121.991600f, 53.444620f),
                new Vector2(-121.991600f, 39.692930f),
                new Vector2(-118.136100f, 27.009140f),
                new Vector2(-114.904600f, 13.948650f),
                new Vector2(-110.032300f, 1.094440f),
                new Vector2(-105.343600f, -11.166530f),
                new Vector2(-99.569560f, -23.599920f),
                new Vector2(-93.025020f, -35.661800f),
                new Vector2(-88.606660f, -47.706150f),
                new Vector2(-80.642190f, -58.776020f),
                new Vector2(-73.293350f, -70.132990f),
                new Vector2(-65.512980f, -81.412310f),
                new Vector2(-57.602470f, -92.032590f),
                new Vector2(-47.508470f, -101.283500f),
                new Vector2(-37.985920f, -110.494200f),
                new Vector2(-27.060480f, -118.096000f),
                new Vector2(-14.569130f, -122.214600f),
                new Vector2(-0.817436f, -122.214600f),
                new Vector2(12.934260f, -122.214600f),
                new Vector2(26.074510f, -120.195100f),
                new Vector2(37.486400f, -112.684200f),
                new Vector2(49.388180f, -106.484100f),
                new Vector2(59.112090f, -96.760210f),
                new Vector2(67.444180f, -85.949590f),
                new Vector2(74.586940f, -74.210380f),
                new Vector2(84.310840f, -64.486450f),
                new Vector2(91.848560f, -53.411390f),
                new Vector2(95.998140f, -40.418010f),
                new Vector2(105.057500f, -30.149280f),
                new Vector2(111.276400f, -18.064850f),
                new Vector2(117.136500f, -5.855240f),
                new Vector2(122.574200f, 6.572674f),
                new Vector2(123.863600f, 20.115140f),
                new Vector2(128.008500f, 32.326800f),
                new Vector2(125.161400f, 44.989240f),
                new Vector2(117.959100f, 56.454660f),
                new Vector2(109.066800f, 66.077370f),
                new Vector2(98.469530f, 74.179060f),
                new Vector2(87.595940f, 82.375660f),
                new Vector2(76.374050f, 89.525740f),
                new Vector2(64.150290f, 93.572050f),
                new Vector2(50.573730f, 95.340760f),
                new Vector2(38.412800f, 91.029970f),
                new Vector2(30.394770f, 80.718170f),
                new Vector2(23.418520f, 69.182860f),
                new Vector2(18.818960f, 56.819440f),
                new Vector2(13.767740f, 44.080660f),
                new Vector2(8.895403f, 31.958530f),
                new Vector2(6.957956f, 18.441280f),
                new Vector2(3.589180f, 5.135056f)
            }
        };
    }
}
