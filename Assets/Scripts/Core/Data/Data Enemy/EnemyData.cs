using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "LOKA/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyName = "Enemy";
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private int damageOnContact = 10;
    [SerializeField] private float contactCooldown = 0.35f;
    [Min(1)] [SerializeField] private int requiredCorrectGestures = 1;
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private bool dropsAksaraFragment = true;
    [SerializeField] private float dropForce = 2f;

    [Header("Shield")]
    [SerializeField] private Sprite shieldedSprite;

    public string EnemyName => enemyName;
    public float MoveSpeed => moveSpeed;
    public int DamageOnContact => damageOnContact;
    public float ContactCooldown => contactCooldown;
    public int RequiredCorrectGestures => Mathf.Max(1, requiredCorrectGestures);
    public Sprite EnemySprite => enemySprite;
    public bool DropsAksaraFragment => dropsAksaraFragment;
    public float DropForce => dropForce;
    public Sprite ShieldedSprite => shieldedSprite;
}