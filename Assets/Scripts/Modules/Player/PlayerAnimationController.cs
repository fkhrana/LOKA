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
    [SerializeField] private string healStateName = "playerHeal";
    [SerializeField] private string powerUpStateName = "playerPowerUp";
    [SerializeField] private string winStateName = "playerWin";
    [SerializeField] private string loseStateName = "playerLose";

    private int idleStateHash;
    private int initiateAttackStateHash;
    private int attackUpStateHash;
    private int gotHitStateHash;
    private int healStateHash;
    private int powerUpStateHash;
    private int winStateHash;
    private int loseStateHash;
    private LevelProgressManager levelProgressManager;
    private bool winAnimationPlayed;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        levelProgressManager = FindAnyObjectByType<LevelProgressManager>();

        idleStateHash = Animator.StringToHash(idleStateName);
        initiateAttackStateHash = Animator.StringToHash(initiateAttackStateName);
        attackUpStateHash = Animator.StringToHash(attackUpStateName);
        gotHitStateHash = Animator.StringToHash(gotHitStateName);
        healStateHash = Animator.StringToHash(healStateName);
        powerUpStateHash = Animator.StringToHash(powerUpStateName);
        winStateHash = Animator.StringToHash(winStateName);
        loseStateHash = Animator.StringToHash(loseStateName);
    }

    private void OnEnable()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.DrawingStarted += PlayInitiateAttack;
            gestureDrawer.DrawingStopped += StopInitiateAttack;
        }

        if (playerHealth != null)
        {
            playerHealth.DamageTaken += HandleDamageTaken;
            playerHealth.Healed += PlayHeal;
            playerHealth.Died += PlayLose;
        }

        if (levelProgressManager != null)
            levelProgressManager.OnReachedLevelComplete.AddListener(PlayWin);

        EnemyGestureCommand.EnemyDefeated += PlayAttackUp;
        PowerManager.OnAnyPowerUpStarted += PlayPowerUp;
    }

    private void OnDisable()
    {
        if (gestureDrawer != null)
        {
            gestureDrawer.DrawingStarted -= PlayInitiateAttack;
            gestureDrawer.DrawingStopped -= StopInitiateAttack;
        }

        if (playerHealth != null)
        {
            playerHealth.DamageTaken -= HandleDamageTaken;
            playerHealth.Healed -= PlayHeal;
            playerHealth.Died -= PlayLose;
        }

        if (levelProgressManager != null)
            levelProgressManager.OnReachedLevelComplete.RemoveListener(PlayWin);

        EnemyGestureCommand.EnemyDefeated -= PlayAttackUp;
        PowerManager.OnAnyPowerUpStarted -= PlayPowerUp;
    }

    private void HandleDamageTaken(int amount)
    {
        PlayState(gotHitStateHash, gotHitStateName);
    }

    private void PlayInitiateAttack()
    {
        winAnimationPlayed = false;
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
        if (TutorialManager.IsTrainingMode) return;
        PlayState(attackUpStateHash, attackUpStateName);
    }

    private void PlayWin()
    {
        if (TutorialManager.IsTrainingMode) return;

        if (winAnimationPlayed)
            return;

        winAnimationPlayed = true;
        PlayState(winStateHash, winStateName);
    }

    private void PlayLose()
    {
        PlayState(loseStateHash, loseStateName);
    }

    private void PlayHeal()
    {
        PlayState(healStateHash, healStateName);
    }

    private void PlayPowerUp()
    {
        PlayState(powerUpStateHash, powerUpStateName);
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