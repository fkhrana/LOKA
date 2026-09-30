using System;
using System.Collections.Generic;
using AnalyticsEvent = Unity.Services.Analytics.Event;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UnityConsent;

public class AksaraAnalytics : MonoBehaviour
{
    private const string LOG_PREFIX = "<color=#45D6FF>[AksaraAnalytics]</color> ";

    private static AksaraAnalytics instance;
    private static bool isInitialized;
    private static bool hasConsentDecision;
    private static bool analyticsConsentGranted;
    private static readonly Queue<AnalyticsEvent> pendingEvents =
        new Queue<AnalyticsEvent>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
            return;

        new GameObject(nameof(AksaraAnalytics)).AddComponent<AksaraAnalytics>();
    }

    private async void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            const string environmentName = "development";
#else
            const string environmentName = "production";
#endif
            var initializationOptions = new InitializationOptions();
            initializationOptions.SetEnvironmentName(environmentName);
            Debug.Log(LOG_PREFIX + "Initializing Unity Services. Environment=" + environmentName);
            await UnityServices.InitializeAsync(initializationOptions);

            isInitialized = true;
            Debug.Log(LOG_PREFIX + "Unity Services initialized successfully. isInitialized=" + isInitialized);

            if (hasConsentDecision)
            {
                ApplyConsentState();
                if (analyticsConsentGranted)
                    FlushPendingEvents();
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Debug.LogError(LOG_PREFIX + "Unity Services initialization failed. Analytics will not send events until initialization succeeds.");
        }
    }

    public static void TrackGestureAttempt(
        string shapeName,
        bool isRecognized,
        bool isHitTarget)
    {
        TrackGestureAttempt(shapeName, string.Empty, isRecognized, isHitTarget);
    }

    public static void TrackGestureAttempt(
        string shapeName,
        string expectedShape,
        bool isRecognized,
        bool isHitTarget)
    {
        TrackGestureAttempt(
            shapeName,
            expectedShape,
            string.Empty,
            string.Empty,
            string.Empty,
            isRecognized,
            isHitTarget
        );
    }

    public static void TrackGestureAttempt(
        string shapeName,
        string expectedShape,
        string intendedTarget,
        string levelWave,
        string activeTargets,
        bool isSuccess,
        bool isHitTarget,
        bool isTutorial = false)
    {
        Debug.Log(
            LOG_PREFIX + "TrackGestureAttempt called => " +
            "shapeName=" + shapeName +
            ", expectedShape=" + expectedShape +
            ", intendedTarget=" + intendedTarget +
            ", levelWave=" + levelWave +
            ", activeTargets=" + activeTargets +
            ", isSuccess=" + isSuccess +
            ", isHitTarget=" + isHitTarget +
            ", isTutorial=" + isTutorial
        );

        Submit(new GestureAttemptEvent(
            shapeName,
            expectedShape,
            intendedTarget,
            levelWave,
            activeTargets,
            isSuccess,
            isHitTarget,
            isTutorial
        ));
    }

    public static void TrackEnemyDefeated(string aksaraName)
    {
        Debug.Log(LOG_PREFIX + "TrackEnemyDefeated called => aksaraName=" + aksaraName);
        Submit(new EnemyDefeatedEvent(aksaraName));
    }

    public static void SetAnalyticsConsent(bool granted)
    {
        hasConsentDecision = true;
        analyticsConsentGranted = granted;

        Debug.Log(LOG_PREFIX + "Consent updated => granted=" + granted + ", hasConsentDecision=" + hasConsentDecision);

        if (!granted)
        {
            pendingEvents.Clear();
            Debug.LogWarning(LOG_PREFIX + "Consent denied. Clearing pending analytics events.");
            if (isInitialized)
                ApplyConsentState();
            return;
        }

        if (isInitialized)
        {
            ApplyConsentState();
            FlushPendingEvents();
        }
    }

    private static void Submit(AnalyticsEvent analyticsEvent)
    {
        string eventName = GetEventName(analyticsEvent);

        if (!hasConsentDecision || !analyticsConsentGranted)
        {
            Debug.LogWarning(
                LOG_PREFIX + "Event rejected because consent has not been granted yet. " +
                "hasConsentDecision=" + hasConsentDecision + ", analyticsConsentGranted=" + analyticsConsentGranted +
                ", eventName=" + eventName
            );
            return;
        }

        if (!isInitialized)
        {
            pendingEvents.Enqueue(analyticsEvent);
            Debug.LogWarning(
                LOG_PREFIX + "Unity Services not initialized yet. Event queued and will be sent later. " +
                "QueuedEventType=" + eventName
            );
            return;
        }

        try
        {
            AnalyticsService.Instance.RecordEvent(analyticsEvent);
            Debug.Log(LOG_PREFIX + "Event sent successfully => " + eventName);
        }
        catch (Exception exception)
        {
            Debug.LogError(LOG_PREFIX + "Failed to send analytics event: " + eventName + "\n" + exception);
        }
    }

    private static void ApplyConsentState()
    {
        EndUserConsent.SetConsentState(new ConsentState
        {
            AnalyticsIntent = analyticsConsentGranted
                ? ConsentStatus.Granted
                : ConsentStatus.Denied
        });

        Debug.Log(LOG_PREFIX + "Consent state applied => AnalyticsIntent=" +
            (analyticsConsentGranted ? ConsentStatus.Granted : ConsentStatus.Denied));
    }

    private static void FlushPendingEvents()
    {
        Debug.Log(LOG_PREFIX + "Flushing pending analytics events. Count=" + pendingEvents.Count);

        while (pendingEvents.Count > 0)
        {
            var pendingEvent = pendingEvents.Dequeue();
            string eventName = GetEventName(pendingEvent);

            try
            {
                AnalyticsService.Instance.RecordEvent(pendingEvent);
                Debug.Log(LOG_PREFIX + "Pending event flushed successfully => " + eventName);
            }
            catch (Exception exception)
            {
                Debug.LogError(LOG_PREFIX + "Failed to flush pending event: " + eventName + "\n" + exception);
            }
        }
    }

    private static string GetEventName(AnalyticsEvent analyticsEvent)
    {
        if (analyticsEvent is GestureAttemptEvent gestureAttemptEvent)
            return gestureAttemptEvent.EventName;

        if (analyticsEvent is EnemyDefeatedEvent enemyDefeatedEvent)
            return enemyDefeatedEvent.EventName;

        return analyticsEvent.GetType().Name;
    }

    private sealed class GestureAttemptEvent : AnalyticsEvent
    {
        public string EventName { get; }

        public GestureAttemptEvent(
            string shapeName,
            string expectedShape,
            string intendedTarget,
            string levelWave,
            string activeTargets,
            bool isSuccess,
            bool isHitTarget,
            bool isTutorial) : base("aksara_gesture_attempt")
        {
            EventName = "aksara_gesture_attempt";
            SetParameter("shape_name", shapeName);
            SetParameter("expected_shape", expectedShape);
            SetParameter("intended_target", intendedTarget);
            SetParameter("level_wave", levelWave);
            SetParameter("active_targets", activeTargets);
            SetParameter("is_recognized", isSuccess);
            SetParameter("is_success", isSuccess);
            SetParameter("is_hit_target", isHitTarget);
            SetParameter("is_tutorial", isTutorial);
        }
    }

    private sealed class EnemyDefeatedEvent : AnalyticsEvent
    {
        public string EventName { get; }

        public EnemyDefeatedEvent(string aksaraName)
            : base("aksara_enemy_defeated")
        {
            EventName = "aksara_enemy_defeated";
            SetParameter("aksara_name", aksaraName);
        }
    }
}