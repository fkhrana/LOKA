using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Animator States")]
    [SerializeField] private string idleStateName = "playerIdle";
    [SerializeField] private string initiateAttackStateName = "playerInitiateAttack";
    [SerializeField] private string attackUpStateName = "playerAttackUp";
    [SerializeField] private string gotHitStateName = "playerGotHit";

    private int idleStateHash;
    private int initiateAttackStateHash;
    private int attackUpStateHash;
    private int gotHitStateHash;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        idleStateHash = Animator.StringToHash(idleStateName);
        initiateAttackStateHash = Animator.StringToHash(initiateAttackStateName);
        attackUpStateHash = Animator.StringToHash(attackUpStateName);
        gotHitStateHash = Animator.StringToHash(gotHitStateName);
    }

    private void OnEnable()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.DrawingStarted += PlayInitiateAttack;
            gestureDrawer.DrawingStopped += StopInitiateAttack;
        }

        if (playerHealth != null)
            playerHealth.DamageTaken += HandleDamageTaken;

        EnemyGestureCommand.EnemyDefeated += PlayAttackUp;
    }

    private void OnDisable()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.DrawingStarted -= PlayInitiateAttack;
            gestureDrawer.DrawingStopped -= StopInitiateAttack;
        }

        if (playerHealth != null)
            playerHealth.DamageTaken -= HandleDamageTaken;

        EnemyGestureCommand.EnemyDefeated -= PlayAttackUp;
    }

    private void HandleDamageTaken(int amount)
    {
        PlayState(gotHitStateHash, gotHitStateName);
    }

    private void PlayInitiateAttack()
    {
        PlayState(initiateAttackStateHash, initiateAttackStateName);
    }

    private void StopInitiateAttack()
    {
        if (animator == null)
            return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.shortNameHash == initiateAttackStateHash)
            PlayState(idleStateHash, idleStateName);
    }

    private void PlayAttackUp()
    {
        PlayState(attackUpStateHash, attackUpStateName);
    }

    private void PlayState(int stateHash, string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning($"PlayerAnimationController: state '{stateName}' tidak ditemukan.", this);
            return;
        }

        animator.Play(stateHash, 0, 0f);
    }
}