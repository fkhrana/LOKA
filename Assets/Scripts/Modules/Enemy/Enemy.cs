using UnityEngine;

[RequireComponent(typeof(EnemyGestureCommand))]
[RequireComponent(typeof(EnemyMovementBehavior))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private AksaraData aksaraData;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer aksaraIconRenderer;
    [SerializeField] private AksaraFragmentItem aksaraIconFragment;

    private EnemyGestureCommand gestureCommand;
    private EnemyMovementBehavior movementBehavior;
    private bool hasBeenDefeated;

    public EnemyData EnemyData => enemyData;
    public AksaraData AksaraData => aksaraData;

    private void Awake()
    {
        gestureCommand = GetComponent<EnemyGestureCommand>();
        movementBehavior = GetComponent<EnemyMovementBehavior>();
    }

    private void Start()
    {
        ApplyEnemyData();
    }

    public void Configure(EnemyData newEnemyData, AksaraData newAksaraData)
    {
        enemyData = newEnemyData;
        aksaraData = newAksaraData;
        ApplyEnemyData();
    }

    private void ApplyEnemyData()
    {
        if (enemyData == null)
        {
            Debug.LogWarning($"[Enemy] ApplyEnemyData() skipped because enemyData is null.");
            return;
        }

        // kalau shielded (requiredCorrectGestures > 1) dan ada shieldedSprite, pakai itu
        // kalau tidak, pakai enemySprite biasa
        if (bodyRenderer != null)
        {
            bool isShielded = enemyData.RequiredCorrectGestures > 1 && enemyData.ShieldedSprite != null;
            bodyRenderer.sprite = isShielded ? enemyData.ShieldedSprite : enemyData.EnemySprite;
        }

        if (aksaraData != null && aksaraIconRenderer != null)
            aksaraIconRenderer.sprite = aksaraData.IconSprite;

        if (movementBehavior != null)
        {
            movementBehavior.SetSpeedFromData(enemyData.MoveSpeed);
            movementBehavior.SetDamageFromData(enemyData.DamageOnContact);
            movementBehavior.SetActive(false);
        }

        if (gestureCommand != null && aksaraData != null)
        {
            gestureCommand.ConfigureChallenge(
                aksaraData.GestureShape,
                enemyData.RequiredCorrectGestures);
        }
        else if (gestureCommand != null)
        {
            Debug.LogWarning("[Enemy] gestureCommand exists but aksaraData is null; challenge was not configured.");
        }
    }

    // dipanggil dari EnemyGestureCommand saat kena hit tapi belum mati
    public void OnHit(int remainingGestures)
    {
        if (enemyData == null || bodyRenderer == null)
            return;

        // shield hilang, ganti ke sprite normal
        if (remainingGestures > 0 && enemyData.EnemySprite != null)
        {
            bodyRenderer.sprite = enemyData.EnemySprite;
            Debug.Log($"[Enemy] {name} shield broken, switching to normal sprite.");
        }
    }

    public void OnDefeated()
    {
        if (hasBeenDefeated)
            return;

        hasBeenDefeated = true;

        if (enemyData != null && enemyData.DropsAksaraFragment && aksaraData != null)
        {
            if (CollectedAksaraManager.Instance != null)
            {
                if (CollectedAksaraManager.Instance.TryRegisterDrop(aksaraData.GestureShape))
                {
                    if (aksaraIconFragment != null)
                    {
                        aksaraIconFragment.transform.SetParent(null);
                        aksaraIconFragment.Initialize(aksaraData, aksaraIconFragment.transform.position);
                        Debug.Log($"Enemy {name} defeated. Fragment for {aksaraData.AksaraName} dropped.");
                    }
                    else
                    {
                        Debug.LogWarning($"Enemy {name} fragment prefab null.");
                    }
                }
                else
                {
                    Debug.Log($"Enemy {name} fragment {aksaraData.AksaraName} already dropped this wave.");
                }
            }
            return;
        }

        if (enemyData != null && enemyData.DropsAksaraFragment && aksaraData == null)
            Debug.LogWarning($"Enemy {name} set to drop fragment but AksaraData null.");
    }
}