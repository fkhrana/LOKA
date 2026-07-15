using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GestureChallengeManager : MonoBehaviour
{
    public static GestureChallengeManager Instance { get; private set; }

    [SerializeField] private float nextRandomChallengeDelay = 0.15f;
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private GestureRecognizer gestureRecognizer;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int damageOnFail = 10;
    [SerializeField] private int healOnSuccess = 0;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string idlePrompt = "Gambar bebas";

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

        if (gestureRecognizer == null)
            gestureRecognizer = GestureRecognizer.Instance != null ? GestureRecognizer.Instance : FindAnyObjectByType<GestureRecognizer>();

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (gestureDrawer != null)
            gestureDrawer.StrokeCompleted += HandleStrokeCompleted;
    }

    private void OnDisable()
    {
        if (gestureDrawer != null)
            gestureDrawer.StrokeCompleted -= HandleStrokeCompleted;
    }

    public void SetChallenge(GestureShape requiredGesture)
    {
        StopRandomChallengeMode();
        CurrentRequiredGesture = requiredGesture;
        UpdatePrompt();
    }

    public void ClearChallenge()
    {
        StopRandomChallengeMode();
        CurrentRequiredGesture = GestureShape.None;
        UpdatePrompt();
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

    private void HandleStrokeCompleted(List<Vector2> points)
    {
        if (gestureRecognizer == null)
        {
            Debug.LogWarning("GestureRecognizer belum tersedia di scene.");
            return;
        }

        var result = gestureRecognizer.Recognize(points, CurrentRequiredGesture);

        if (!HasActiveChallenge())
            return;

        if (result.MatchesExpected)
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

    private void UpdatePrompt()
    {
        if (promptText == null)
        {
            Debug.LogWarning("GestureChallengeManager: Prompt Text belum di-assign di Inspector.");
            return;
        }

        if (CurrentRequiredGesture == GestureShape.None)
        {
            promptText.text = idlePrompt;
            return;
        }

        promptText.text = $"Gambar: {GetGestureLabel(CurrentRequiredGesture)}";
    }

    private string GetGestureLabel(GestureShape gestureShape)
    {
        switch (gestureShape)
        {
            case GestureShape.Circle:
                return "LINGKARAN";
            case GestureShape.Square:
                return "KOTAK";
            default:
                return gestureShape.ToString();
        }
    }

    private System.Collections.IEnumerator DelayThenSetRandomChallenge()
    {
        if (nextRandomChallengeDelay > 0f)
            yield return new WaitForSeconds(nextRandomChallengeDelay);

        var nextShape = GetRandomShape(CurrentRequiredGesture);
        CurrentRequiredGesture = nextShape;
        UpdatePrompt();
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
        UpdatePrompt();
    }
}