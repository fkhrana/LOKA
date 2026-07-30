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

        Debug.Log($"[Enemy] Configure() called on {name}. enemyData={(enemyData != null ? enemyData.name : "null")}, aksaraData={(aksaraData != null ? aksaraData.name : "null")}, bodyRenderer={(bodyRenderer != null)}, aksaraIconRenderer={(aksaraIconRenderer != null)}");

        ApplyEnemyData();
    }

    private void ApplyEnemyData()
    {
        Debug.Log($"[Enemy] ApplyEnemyData() called on {name}. enemyData={(enemyData != null ? enemyData.name : "null")}, aksaraData={(aksaraData != null ? aksaraData.name : "null")}, bodyRenderer={(bodyRenderer != null)}, aksaraIconRenderer={(aksaraIconRenderer != null)}");

        if (enemyData == null)
        {
            Debug.LogWarning($"[Enemy] ApplyEnemyData() skipped because enemyData is null.");
            return;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.sprite = enemyData.EnemySprite;
            Debug.Log($"[Enemy] bodyRenderer sprite set to {(enemyData.EnemySprite != null ? enemyData.EnemySprite.name : "null")}");
        }

        if (aksaraData != null && aksaraIconRenderer != null)
        {
            aksaraIconRenderer.sprite = aksaraData.IconSprite;
            Debug.Log($"[Enemy] aksaraIconRenderer sprite set to {(aksaraData.IconSprite != null ? aksaraData.IconSprite.name : "null")}");
        }

        if (movementBehavior != null)
        {
            movementBehavior.SetActive(false);
        }

        if (gestureCommand != null && aksaraData != null)
        {
            gestureCommand.ConfigureChallenge(
                aksaraData.GestureShape,
                enemyData.RequiredCorrectGestures);

            Debug.Log($"[Enemy] gestureCommand configured with gesture={aksaraData.GestureShape} and requiredGestures={enemyData.RequiredCorrectGestures}");
        }
        else if (gestureCommand != null)
        {
            Debug.LogWarning("[Enemy] gestureCommand exists but aksaraData is null; challenge was not configured.");
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
                        Debug.LogWarning($"Enemy {name} is configured to drop a fragment but aksaraIconFragment is null.");
                    }
                }
                else
                {
                    Debug.Log($"Enemy {name} defeated. Fragment for {aksaraData.AksaraName} already dropped this wave; skipping.");
                }
            }
            else
            {
                Debug.LogWarning("[Enemy] CollectedAksaraManager.Instance is null; cannot register drop.");
            }

            return;
        }

        if (enemyData != null && enemyData.DropsAksaraFragment && aksaraData == null)
        {
            Debug.LogWarning($"Enemy {name} is set to drop a fragment, but no AksaraData is assigned.");
        }
    }
}
