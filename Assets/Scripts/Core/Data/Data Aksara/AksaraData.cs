using UnityEngine;

[CreateAssetMenu(fileName = "Aksara Data", menuName = "LOKA/Aksara Data")]
public class AksaraData : ScriptableObject
{
    [SerializeField] private GestureShape gestureShape = GestureShape.None;
    [SerializeField] private string aksaraName = "Aksara";
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private Sprite fragmentSprite;

    public GestureShape GestureShape => gestureShape;
    public string AksaraName => aksaraName;
    public Sprite IconSprite => iconSprite;
    public Sprite FragmentSprite => fragmentSprite;
}
