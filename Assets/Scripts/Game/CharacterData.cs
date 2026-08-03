using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    [TextArea] public string description;
    public Sprite icon;
}