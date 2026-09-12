using System.Collections;
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
    [SerializeField] private Animator animator;

    [Header("Enemy Defeat Blink")]
    [SerializeField, Min(0f)] private float defeatBlinkDuration = 0.6f;
    [SerializeField, Min(0.01f)] private float defeatBlinkInterval = 0.1f;
    [SerializeField, Min(0f)] private float defeatShrinkDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] private float defeatShrinkTargetScale = 0.1f;
    [SerializeField] private string defeatAnimationStateName = "enemyDieBlubub";

    [Header("Enemy Defeat SFX")]
    [SerializeField] private bool useEnemyDefeatSFX = true;
    [SerializeField] private string enemyDefeatSFXName = "EnemyDefeat";
    [Range(0f, 1f)]
    [SerializeField] private float enemyDefeatSFXVolume = 1f;

    [Header("Aksara Drop SFX")]
    [SerializeField] private bool useAksaraDropSFX = true;
    [SerializeField] private string aksaraDropSFXName = "AksaraDrop";
    [Range(0f, 1f)]
    [SerializeField] private float aksaraDropSFXVolume = 1f;

    private EnemyGestureCommand gestureCommand;
    private EnemyMovementBehavior movementBehavior;
    private bool hasBeenDefeated;

    public EnemyData EnemyData => enemyData;
    public AksaraData AksaraData => aksaraData;
    public float DefeatBlinkDuration => defeatBlinkDuration;
    public float DefeatSequenceDuration =>
        defeatBlinkDuration + defeatShrinkDuration + GetDefeatAnimationDuration();

    private void Awake()
    {
        gestureCommand = GetComponent<EnemyGestureCommand>();
        movementBehavior = GetComponent<EnemyMovementBehavior>();

        if (animator == null)
            animator = GetComponent<Animator>();
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
            bool isShielded =
                enemyData.RequiredCorrectGestures > 1 &&
                enemyData.ShieldedSprite != null;

            bodyRenderer.sprite = isShielded
                ? enemyData.ShieldedSprite
                : enemyData.EnemySprite;
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
                enemyData.RequiredCorrectGestures
            );
        }
        else if (gestureCommand != null)
        {
            Debug.LogWarning(
                "[Enemy] gestureCommand exists but aksaraData is null; challenge was not configured."
            );
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

            Debug.Log(
                $"[Enemy] {name} shield broken, switching to normal sprite."
            );
        }
    }

    public void OnDefeated()
    {
        if (hasBeenDefeated)
            return;

        hasBeenDefeated = true;

        if (aksaraIconRenderer != null)
            aksaraIconRenderer.enabled = false;

        // ========================================
        // PLAY ENEMY DEFEAT SFX
        // ========================================
        PlayEnemyDefeatSFX();

        if (enemyData != null &&
            enemyData.DropsAksaraFragment &&
            aksaraData != null)
        {
            bool registeredDrop =
                CollectedAksaraManager.Instance != null
                && CollectedAksaraManager.Instance.TryRegisterDrop(
                    aksaraData.GestureShape
                );

            if (registeredDrop)
            {
                if (aksaraIconFragment != null)
                {
                    aksaraIconFragment.transform.SetParent(null);

                    aksaraIconFragment.Initialize(
                        aksaraData,
                        aksaraIconFragment.transform.position
                    );

                    if (aksaraIconRenderer != null)
                        aksaraIconRenderer.enabled = true;

                    Debug.Log(
                        $"Enemy {name} defeated. Fragment for {aksaraData.AksaraName} dropped."
                    );
                }
                else
                {
                    Debug.LogWarning(
                        $"Enemy {name} fragment prefab null."
                    );
                }

                PlayAksaraDropSFX();
                LevelProgressManager.Instance?.CompletePendingProgress();
            }

            if (!registeredDrop)
            {
                PlayNonCollectibleItemVfx();

                Debug.Log(
                    $"Enemy {name} item is non-collectible because {aksaraData.AksaraName} was already dropped."
                );
            }

            return;
        }

        if (enemyData != null &&
            enemyData.DropsAksaraFragment &&
            aksaraData == null)
        {
            Debug.LogWarning(
                $"Enemy {name} set to drop fragment but AksaraData null."
            );
        }

        PlayNonCollectibleItemVfx();
    }

    public void StartDefeatBlink()
    {
        StartCoroutine(DefeatBlinkRoutine());
    }

    private IEnumerator DefeatBlinkRoutine()
    {
        float elapsed = 0f;
        bool isVisible = true;
        float interval = Mathf.Max(0.01f, defeatBlinkInterval);

        while (elapsed < defeatBlinkDuration)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;
            isVisible = !isVisible;
            if (bodyRenderer != null)
                bodyRenderer.enabled = isVisible;
        }

        if (bodyRenderer != null)
            bodyRenderer.enabled = true;

        yield return ShrinkBodyRoutine();

        if (bodyRenderer != null)
            bodyRenderer.enabled = false;

        PlayDefeatAnimation();
    }

    private IEnumerator ShrinkBodyRoutine()
    {
        if (bodyRenderer == null || defeatShrinkDuration <= 0f)
            yield break;

        Transform bodyTransform = bodyRenderer.transform;
        Vector3 initialScale = bodyTransform.localScale;
        Vector3 targetScale = initialScale * defeatShrinkTargetScale;
        float elapsed = 0f;

        while (elapsed < defeatShrinkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / defeatShrinkDuration);
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            bodyTransform.localScale = Vector3.Lerp(
                initialScale,
                targetScale,
                progress
            );
            yield return null;
        }

        bodyTransform.localScale = targetScale;
    }

    private void PlayDefeatAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        int stateHash = Animator.StringToHash(defeatAnimationStateName);
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
            if (clips[i] != null && clips[i].name == defeatAnimationStateName)
                return clips[i].length;
        }

        return 0f;
    }

    private void PlayEnemyDefeatSFX()
    {
        if (useEnemyDefeatSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                enemyDefeatSFXName,
                enemyDefeatSFXVolume
            );
        }
    }

    private void PlayAksaraDropSFX()
    {
        if (useAksaraDropSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                aksaraDropSFXName,
                aksaraDropSFXVolume
            );
        }
    }

    private void PlayNonCollectibleItemVfx()
    {
        if (aksaraIconRenderer != null)
        {
            LevelProgressManager.Instance?.PlayNonCollectibleItemVfx(
                aksaraIconRenderer.transform.position
            );
        }
    }
}