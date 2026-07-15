using UnityEngine;

public class EnemyGestureCommand : MonoBehaviour
{
    [SerializeField] private bool autoIssueOnStart = true;
    [SerializeField] private CommandMode commandMode = CommandMode.Fixed;
    [SerializeField] private GestureShape gestureToCommand = GestureShape.Circle;
    [SerializeField] private GestureChallengeManager challengeManager;

    private void Start()
    {
        if (autoIssueOnStart)
            IssueCommand();
    }

    public void IssueCommand()
    {
        if (challengeManager == null)
            challengeManager = GestureChallengeManager.Instance;

        if (challengeManager == null)
        {
            Debug.LogWarning("GestureChallengeManager belum tersedia di scene.");
            return;
        }

        if (commandMode == CommandMode.Random)
        {
            challengeManager.StartRandomChallengeMode();
            return;
        }

        challengeManager.SetChallenge(gestureToCommand);
    }

    public void ClearCommand()
    {
        if (challengeManager == null)
            challengeManager = GestureChallengeManager.Instance;

        if (challengeManager == null)
            return;

        challengeManager.ClearChallenge();
    }

    public void StopCommandMode()
    {
        if (challengeManager == null)
            challengeManager = GestureChallengeManager.Instance;

        if (challengeManager == null)
            return;

        challengeManager.StopRandomChallengeMode();
        challengeManager.ClearChallenge();
    }

    private enum CommandMode
    {
        Fixed,
        Random
    }
}