using System.Collections.Generic;
using UnityEngine;

public class GestureChallengeManager : MonoBehaviour
{
    public static GestureChallengeManager Instance { get; private set; }

    [SerializeField] private float nextRandomChallengeDelay = 0.15f;
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int damageOnFail = 10;
    [SerializeField] private int healOnSuccess = 0;

    public GestureShape CurrentRequiredGesture { get; private set; } = GestureShape.None;
    public bool IsRandomChallengeModeActive { get; private set; }

    private Coroutine randomChallengeCoroutine;
    private readonly GestureShape[] randomShapes = { GestureShape.Circle, GestureShape.Square };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (gestureDrawer != null)
            gestureDrawer.GestureRecognized += HandleGestureRecognized;
    }

    private void OnDisable()
    {
        if (gestureDrawer != null)
            gestureDrawer.GestureRecognized -= HandleGestureRecognized;
    }

    public void SetChallenge(GestureShape requiredGesture)
    {
        StopRandomChallengeMode();
        CurrentRequiredGesture = requiredGesture;
    }

    public void ClearChallenge()
    {
        StopRandomChallengeMode();
        CurrentRequiredGesture = GestureShape.None;
    }

    public void StartRandomChallengeMode()
    {
        IsRandomChallengeModeActive = true;
        SetRandomPromptImmediately();
        SetNextRandomChallenge();
    }

    public void StopRandomChallengeMode()
    {
        IsRandomChallengeModeActive = false;

        if (randomChallengeCoroutine != null)
        {
            StopCoroutine(randomChallengeCoroutine);
            randomChallengeCoroutine = null;
        }
    }

    public void SetNextRandomChallenge()
    {
        if (randomChallengeCoroutine != null)
        {
            StopCoroutine(randomChallengeCoroutine);
            randomChallengeCoroutine = null;
        }

        if (!IsRandomChallengeModeActive)
            IsRandomChallengeModeActive = true;

        randomChallengeCoroutine = StartCoroutine(DelayThenSetRandomChallenge());
    }

    public bool HasActiveChallenge()
    {
        return CurrentRequiredGesture != GestureShape.None;
    }

    private void HandleGestureRecognized(List<Vector2> points, GestureRecognitionResult result)
    {
        if (EnemyGestureCommand.HasActiveEnemyChallenges())
            return;

        if (result.IsRecognized && EnemyGestureCommand.HasActiveEnemyMatchingGesture(result.DetectedShape))
            return;

        if (!HasActiveChallenge())
            return;

        if (!result.IsRecognized)
        {
            if (playerHealth != null && damageOnFail > 0)
                playerHealth.TakeDamage(damageOnFail);
            return;
        }

        if (result.DetectedShape == CurrentRequiredGesture)
        {
            Debug.Log($"Challenge sukses: {result.DetectedShape}");

            if (playerHealth != null && healOnSuccess > 0)
                playerHealth.Heal(healOnSuccess);

            if (IsRandomChallengeModeActive)
            {
                SetNextRandomChallenge();
            }
            else
            {
                ClearChallenge();
            }
        }
        else
        {
            Debug.Log($"Challenge gagal. Diminta {CurrentRequiredGesture}, terdeteksi {result.DetectedShape}");

            if (playerHealth != null && damageOnFail > 0)
                playerHealth.TakeDamage(damageOnFail);
        }
    }

    private System.Collections.IEnumerator DelayThenSetRandomChallenge()
    {
        if (nextRandomChallengeDelay > 0f)
            yield return new WaitForSeconds(nextRandomChallengeDelay);

        var nextShape = GetRandomShape(CurrentRequiredGesture);
        CurrentRequiredGesture = nextShape;
        randomChallengeCoroutine = null;
    }

    private GestureShape GetRandomShape(GestureShape excludeShape = GestureShape.None)
    {
        if (randomShapes.Length == 0)
            return GestureShape.Circle;

        if (randomShapes.Length == 1)
            return randomShapes[0];

        GestureShape nextShape;
        do
        {
            int index = Random.Range(0, randomShapes.Length);
            nextShape = randomShapes[index];
        }
        while (nextShape == excludeShape);

        return nextShape;
    }

    private void SetRandomPromptImmediately()
    {
        CurrentRequiredGesture = GetRandomShape();
    }
}