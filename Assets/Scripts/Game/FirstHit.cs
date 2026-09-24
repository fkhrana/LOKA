using System.Collections;
using UnityEngine;

public class FirstHitTutorialManager : MonoBehaviour
{
    private const string KEY_FIRST_HIT = "FirstHitTutorialShown";

    #region Inspector
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private LowHealthHelperController helperController;
    [SerializeField] private TutorialHintManager hintManager;

    [Header("Gesture Hint")]
    [Tooltip("Gesture yang ditampilkan saat first hit.")]
    [SerializeField] private GestureShape tutorialGesture = GestureShape.Love;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float delayBeforeDots = 0.3f;
    [SerializeField, Min(0f)] private float dotsDuration = 8f;

    [Header("Debug")]
    [SerializeField] private bool resetForTesting = false;
    #endregion

    #region Runtime
    private bool triggered;
    private Coroutine tutorialRoutine;
    #endregion

    private void Awake()
    {
#if UNITY_EDITOR
        if (resetForTesting)
        {
            PlayerPrefs.DeleteKey(KEY_FIRST_HIT);
            PlayerPrefs.Save();
            Debug.Log("[FirstHitTutorial] 🔄 Reset via checkbox.");
        }
#endif
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(KEY_FIRST_HIT, 0) == 1)
        {
            Debug.Log("[FirstHitTutorial] Sudah pernah ditampilkan — skip.");
            enabled = false;
            return;
        }

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("[FirstHitTutorial] PlayerHealth tidak ditemukan.");
            enabled = false;
            return;
        }

        playerHealth.DamageTaken += OnPlayerDamaged;
        Debug.Log("[FirstHitTutorial] ✅ Menunggu first hit...");
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.DamageTaken -= OnPlayerDamaged;

        if (tutorialRoutine != null)
            StopCoroutine(tutorialRoutine);
    }

    private void OnPlayerDamaged(int amount)
    {
        if (triggered) return;
        if (PlayerPrefs.GetInt(KEY_FIRST_HIT, 0) == 1) return;
        if (TutorialManager.IsTrainingMode) return;

        triggered = true;
        PlayerPrefs.SetInt(KEY_FIRST_HIT, 1);
        PlayerPrefs.Save();

        Debug.Log($"[FirstHitTutorial] 🎬 First hit → tampilkan gesture: {tutorialGesture}");

        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);
        tutorialRoutine = StartCoroutine(TutorialRoutine());
    }

    private IEnumerator TutorialRoutine()
    {
        // 1) Spawn helper (roh baik)
        if (helperController != null)
        {
            bool ok = helperController.TrySpawnHelperForTutorial();
            if (!ok)
                Debug.LogWarning("[FirstHitTutorial] Gagal spawn roh baik.");
        }
        else
        {
            Debug.LogWarning("[FirstHitTutorial] helperController belum di-assign!");
        }

        // 2) Delay sebelum dots
        if (delayBeforeDots > 0f)
            yield return new WaitForSecondsRealtime(delayBeforeDots);

        // 3) Tampilkan dots gesture
        if (hintManager != null)
        {
            Debug.Log($"[FirstHitTutorial] Menampilkan dots gesture: {tutorialGesture}");
            hintManager.ShowPath(tutorialGesture);

            if (dotsDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(dotsDuration);
                hintManager.HidePath();
            }
        }
        else
        {
            Debug.LogWarning("[FirstHitTutorial] hintManager null — dots tidak muncul.");
        }

        tutorialRoutine = null;
    }

#if UNITY_EDITOR
    [ContextMenu("Reset First Hit Progress")]
    public void ResetFirstHit()
    {
        PlayerPrefs.DeleteKey(KEY_FIRST_HIT);
        PlayerPrefs.Save();
        Debug.Log("[FirstHitTutorial] Progress di-reset.");
    }
#endif
}