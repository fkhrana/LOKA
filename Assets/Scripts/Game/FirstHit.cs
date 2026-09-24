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

    [Header("Dots Aksara")]
    [SerializeField] private AksaraData fallbackAksara;
    [SerializeField, Min(1f)] private float enemySearchRadius = 20f;

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

        AksaraData aksara = FindNearestEnemyAksara();

        if (aksara != null)
            Debug.Log($"[FirstHitTutorial] 🎬 First hit → aksara: {aksara.GestureShape}");
        else
            Debug.Log("[FirstHitTutorial] 🎬 First hit → tidak ada musuh, pakai fallback.");

        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);
        tutorialRoutine = StartCoroutine(TutorialRoutine(aksara));
    }

    private AksaraData FindNearestEnemyAksara()
    {
        if (playerHealth == null) return fallbackAksara;

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Vector3 playerPos = playerHealth.transform.position;

        Enemy closest = null;
        float closestDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.AksaraData == null) continue;

            float d = Vector3.Distance(playerPos, enemy.transform.position);
            if (d > enemySearchRadius) continue;

            if (d < closestDist)
            {
                closestDist = d;
                closest = enemy;
            }
        }

        if (closest != null && closest.AksaraData != null)
            return closest.AksaraData;

        return fallbackAksara;
    }

    private IEnumerator TutorialRoutine(AksaraData aksara)
    {
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

        if (delayBeforeDots > 0f)
            yield return new WaitForSecondsRealtime(delayBeforeDots);

        if (hintManager != null && aksara != null)
        {
            Debug.Log($"[FirstHitTutorial] Menampilkan dots: {aksara.GestureShape}");
            hintManager.ShowPath(aksara);

            if (dotsDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(dotsDuration);
                hintManager.HidePath();
            }
        }
        else
        {
            Debug.LogWarning("[FirstHitTutorial] aksara / hintManager null — dots tidak muncul.");
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