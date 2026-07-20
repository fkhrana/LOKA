using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyGestureCommand : MonoBehaviour
{
    [SerializeField] private bool autoIssueOnStart = true;
    [SerializeField] private GestureShape gestureToCommand = GestureShape.Circle;
    [SerializeField, Min(1)] private int requiredCorrectGestures = 1;
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private int healOnSuccess = 0;
    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private int remainingCorrectGestures;
    private bool isSubscribed;
    private bool challengeActive;
    private static List<Vector2> cachedStrokePoints;
    private static bool cachedStrokeHandled;
    private EnemyMovementBehavior movementBehavior;

    private static readonly List<EnemyGestureCommand> activeEnemies = new List<EnemyGestureCommand>();

    public static bool HasActiveEnemyChallenges()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null && activeEnemies[i].challengeActive)
                return true;
        }

        return false;
    }

    public static bool HasActiveEnemyMatchingGesture(GestureShape gestureShape)
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var enemy = activeEnemies[i];
            if (enemy != null && enemy.challengeActive && enemy.gestureToCommand == gestureShape)
                return true;
        }

        return false;
    }

    public static bool HasHandledStroke(List<Vector2> points)
    {
        return cachedStrokePoints == points && cachedStrokeHandled;
    }

    private void Awake()
    {
        remainingCorrectGestures = Mathf.Max(1, requiredCorrectGestures);

        if (gestureDrawer == null)
            gestureDrawer = FindAnyObjectByType<GestureDrawer>();

        if (promptText == null)
            promptText = GetComponentInChildren<TMP_Text>(true);

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        playerTransform = playerHealth != null ? playerHealth.transform : null;

        movementBehavior = GetComponent<EnemyMovementBehavior>();
        if (movementBehavior == null)
            movementBehavior = GetComponentInChildren<EnemyMovementBehavior>(true);

        if (movementBehavior == null)
            movementBehavior = gameObject.AddComponent<EnemyMovementBehavior>();

        if (movementBehavior != null)
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            var enemyCollider = GetComponent<Collider2D>();
            if (enemyCollider == null)
                enemyCollider = GetComponentInChildren<Collider2D>(true);
            movementBehavior.Initialize(playerHealth, enemyCollider, spriteRenderer);
        }

        UpdatePrompt();
    }

    private void OnEnable()
    {
        activeEnemies.Add(this);
        SubscribeToGestureDrawer();
    }

    private void OnDisable()
    {
        activeEnemies.Remove(this);
        UnsubscribeFromGestureDrawer();
        if (movementBehavior != null)
            movementBehavior.SetActive(false);
    }

    private void Start()
    {
        if (autoIssueOnStart)
            IssueCommand();
    }

    private void Update()
    {
        if (!challengeActive)
            return;

        Debug.Log($"EnemyGestureCommand.Update challengeActive={challengeActive} hasMovement={(movementBehavior != null)}");
    }

    private void LateUpdate()
    {
        if (!challengeActive)
            return;

        if (movementBehavior != null)
            movementBehavior.Tick();
    }

    public void SetAutoIssueOnStart(bool shouldAutoIssue)
    {
        autoIssueOnStart = shouldAutoIssue;
    }

    public void IssueCommand()
    {
        if (gestureDrawer == null)
            Debug.LogWarning("EnemyGestureCommand: GestureDrawer belum tersedia di scene.");

        remainingCorrectGestures = Mathf.Max(1, requiredCorrectGestures);
        challengeActive = true;
        Debug.Log($"EnemyGestureCommand.IssueCommand active={challengeActive} movementBehavior={(movementBehavior != null)}");
        if (movementBehavior != null)
            movementBehavior.SetActive(true);
        else
            Debug.LogWarning("EnemyGestureCommand: EnemyMovementBehavior tidak tersedia.");

        UpdatePrompt();
    }

    public void ClearCommand()
    {
        challengeActive = false;
        remainingCorrectGestures = Mathf.Max(1, requiredCorrectGestures);
        if (movementBehavior != null)
            movementBehavior.SetActive(false);
        UpdatePrompt();
    }

    public void StopCommandMode()
    {
        ClearCommand();
    }

    public void ConfigureChallenge(GestureShape gestureShape, int correctGestureCount = 1)
    {
        gestureToCommand = gestureShape;
        requiredCorrectGestures = Mathf.Max(1, correctGestureCount);
        remainingCorrectGestures = requiredCorrectGestures;
        challengeActive = true;
        if (movementBehavior != null)
            movementBehavior.SetActive(true);
        UpdatePrompt();
    }

    private void HandleGestureRecognized(List<Vector2> points, GestureRecognitionResult result)
    {
        if (!challengeActive)
            return;

        if (cachedStrokePoints != points)
        {
            cachedStrokePoints = points;
            cachedStrokeHandled = false;
        }
        else if (cachedStrokeHandled)
        {
            return;
        }

        if (!result.IsRecognized)
        {
            return;
        }

        var target = FindNearestActiveEnemyMatchingGesture(points, result.DetectedShape);
        if (target == null)
        {
            return;
        }

        if (target != this)
            return;

        cachedStrokeHandled = true;

        if (playerHealth != null && healOnSuccess > 0)
            playerHealth.Heal(healOnSuccess);

        remainingCorrectGestures--;

        if (movementBehavior != null)
        {
            float knockbackDuration = movementBehavior.PlayKnockback(remainingCorrectGestures <= 0);
            if (remainingCorrectGestures <= 0)
            {
                challengeActive = false;
                UpdatePrompt();
                StartCoroutine(DestroyAfter(knockbackDuration));
                return;
            }
        }

        if (remainingCorrectGestures > 0)
        {
            UpdatePrompt();
            return;
        }

        challengeActive = false;
        if (movementBehavior != null)
            movementBehavior.SetActive(false);
        UpdatePrompt();
        Destroy(gameObject);
    }

    private void SubscribeToGestureDrawer()
    {
        if (gestureDrawer == null || isSubscribed)
            return;

        gestureDrawer.GestureRecognized += HandleGestureRecognized;
        isSubscribed = true;
    }

    private static EnemyGestureCommand FindNearestActiveEnemyMatchingGesture(List<Vector2> points, GestureShape detectedGesture)
    {
        var activeList = activeEnemies.FindAll(enemy => enemy != null && enemy.challengeActive && enemy.gestureToCommand == detectedGesture);
        if (activeList.Count == 0)
            return null;

        if (activeList.Count == 1)
            return activeList[0];

        Vector2 strokeCenter = GetStrokeCenter(points);
        EnemyGestureCommand nearest = null;
        float nearestSqr = float.MaxValue;

        foreach (var enemy in activeList)
        {
            var delta = (Vector2)enemy.transform.position - strokeCenter;
            float sqr = delta.sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private static Vector2 GetStrokeCenter(List<Vector2> points)
    {
        var center = Vector2.zero;
        foreach (var point in points)
            center += point;

        return center / Mathf.Max(1, points.Count);
    }

    private void UnsubscribeFromGestureDrawer()
    {
        if (gestureDrawer == null || !isSubscribed)
            return;
        gestureDrawer.GestureRecognized -= HandleGestureRecognized;
        isSubscribed = false;
    }

    private IEnumerator DestroyAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    private void UpdatePrompt()
    {
        if (promptText == null)
            return;

        if (!challengeActive)
        {
            promptText.text = string.Empty;
            return;
        }

        string gestureLabel = GetGestureLabel(gestureToCommand);
        if (remainingCorrectGestures > 1)
            promptText.text = $"{gestureLabel} x{remainingCorrectGestures}";
        else
            promptText.text = $"{gestureLabel}";
    }

    private string GetGestureLabel(GestureShape gestureShape)
    {
        switch (gestureShape)
        {
            case GestureShape.Circle:
                return "LINGKARAN";
            case GestureShape.Square:
                return "KOTAK";
            case GestureShape.Na:
                return "NA";
            case GestureShape.Ka:
                return "KA";
            default:
                return gestureShape.ToString();
        }
    }
}