using System;
using System.Collections;
using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer bodyRenderer;

    [Header("Animator States")]
    [SerializeField] private string idleStateName = "bossIdle";
    [SerializeField] private string hitStateName = "bossHit";
    [SerializeField] private string defeatStateName = "bossDie";

    [Header("Defeat Animation")]
    [SerializeField, Min(0.01f)] private float defeatBlinkInterval = 0.1f;

    private Coroutine hitAnimationCoroutine;
    private EnemyMovementBehavior observedMovementBehavior;

    public SpriteRenderer BodyRenderer
    {
        get
        {
            ResolveReferences();
            return bodyRenderer;
        }
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnDisable()
    {
        BindTo(null);
    }

    public void BindTo(EnemyMovementBehavior movementBehavior)
    {
        if (observedMovementBehavior != null)
            observedMovementBehavior.PlayerDamagedByContact -= PlayHit;

        observedMovementBehavior = movementBehavior;
        if (observedMovementBehavior != null)
            observedMovementBehavior.PlayerDamagedByContact += PlayHit;
    }

    public void PlayHit()
    {
        ResolveReferences();
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        int hitStateHash = Animator.StringToHash(hitStateName);
        if (!animator.HasState(0, hitStateHash))
            return;

        if (hitAnimationCoroutine != null)
            StopCoroutine(hitAnimationCoroutine);

        animator.Play(hitStateHash, 0, 0f);
        hitAnimationCoroutine = StartCoroutine(ReturnToIdleAfterHit(hitStateHash));
    }

    public IEnumerator BlinkWhile(Func<bool> isComplete)
    {
        ResolveReferences();
        bool isVisible = true;
        float interval = Mathf.Max(0.01f, defeatBlinkInterval);

        while (isComplete != null && !isComplete())
        {
            yield return new WaitForSeconds(interval);
            isVisible = !isVisible;

            if (bodyRenderer != null)
                bodyRenderer.enabled = isVisible;
        }

        if (bodyRenderer != null)
            bodyRenderer.enabled = true;
    }

    public IEnumerator PlayDefeatSequence()
    {
        ResolveReferences();
        PlayDefeat();
        yield return new WaitForSeconds(GetDefeatAnimationDuration());
    }

    private IEnumerator ReturnToIdleAfterHit(int hitStateHash)
    {
        yield return null;

        while (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != hitStateHash || stateInfo.normalizedTime >= 1f)
                break;

            yield return null;
        }

        int idleStateHash = Animator.StringToHash(idleStateName);
        if (animator != null && animator.runtimeAnimatorController != null &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash == hitStateHash &&
            animator.HasState(0, idleStateHash))
        {
            animator.Play(idleStateHash, 0, 0f);
        }

        hitAnimationCoroutine = null;
    }

    private void PlayDefeat()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        int stateHash = Animator.StringToHash(defeatStateName);
        if (animator.HasState(0, stateHash))
            animator.Play(stateHash, 0, 0f);
    }

    private float GetDefeatAnimationDuration()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0f;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == defeatStateName)
                return clips[i].length;
        }

        return 0f;
    }

    private void ResolveReferences()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }
}