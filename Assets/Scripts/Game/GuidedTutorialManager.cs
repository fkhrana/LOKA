using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GuidedTutorialManager : MonoBehaviour
{
    public enum Step
    {
        Idle,
        DrawAksara,
        HpHeal,
        Puzzle,
        Done
    }

    [Header("UI Overlay")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject guidedTutorialPanel;
    [SerializeField] private TMP_Text stepTitle;
    [SerializeField] private TMP_Text stepDescription;

    [Header("References")]
    [SerializeField] private GestureDrawer gestureDrawer;
    [SerializeField] private TutorialHintManager hintManager;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PuzzleManager puzzleManager;

    [Header("Wave Spawner")]
    [SerializeField] private EnemyWaveSpawner enemyWaveSpawner;

    [Header("Tutorial Content")]
    [SerializeField] private AksaraData tutorialAksara;
    [SerializeField] private GestureShape healGesture = GestureShape.Love;

    [Header("Tutorial Enemy Spawner")]
    [SerializeField] private TutorialEnemySpawner tutorialEnemySpawner;
    [SerializeField] private EnemyData tutorialEnemyData;
    [SerializeField] private float enemySpawnRadius = 1.5f;
    [SerializeField] private float drawPhaseEnemySpeed = 0.3f;
    [SerializeField] private float hpPhaseEnemySpeed = 1.5f;
    [SerializeField] private Vector3 enemySpawnCenter = Vector3.zero;
    [SerializeField] private bool spawnNearTargetKanan = true;

    [Header("Tutorial Damage")]
    [SerializeField, Min(1)] private int step2Damage = 20;

    [Header("Retry Settings")]
    [SerializeField, Min(1)] private int maxRetry = 4;
    [SerializeField, Min(0f)] private float retryCooldown = 1f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float delayBetweenSteps = 1.5f;
    [SerializeField, Min(0f)] private float delayBeforeHealHint = 1.0f;
    [SerializeField, Min(1f)] private float waitForCollectTimeout = 15f;
    [SerializeField, Min(1f)] private float waitForEnemyHitTimeout = 15f;
    [SerializeField, Min(0f)] private float waitAfterEnemyDeath = 1f;
    [SerializeField, Min(0f)] private float waitBeforePuzzle = 2f;
    [SerializeField, Min(0f)] private float waitAfterPuzzle = 1.5f;

    [Header("Puzzle Transition")]
    [SerializeField, Min(0f)] private float puzzlePanelAppearDelay = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private Step currentStep = Step.Idle;

    private bool gestureSubscribed;
    private bool healSubscribed;
    private bool damageSubscribed;

    private int retryCount = 0;

    private bool tutorialFinishConfirmed = false;

    public void BeginTutorial()
    {
        if (currentStep != Step.Idle)
            return;

        GameProgressManager.SetGuidedTutorialActive(true);

        Log("🎬 Mulai Guided Tutorial.");

        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
            Log($"✅ HP direset: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
        }

        var helper = FindFirstObjectByType<LowHealthHelperController>();

        if (helper != null)
        {
            helper.ResetForTutorial();
            Log("✅ Helper direset.");
        }

        EnemyMovementBehavior.SetAllMovementPaused(false);

        if (enemyWaveSpawner != null)
        {
            enemyWaveSpawner.StopAllCoroutines();
            enemyWaveSpawner.StopWaveSequence();
            enemyWaveSpawner.ClearSpawnedEnemies();
            enemyWaveSpawner.enabled = false;
        }

        CleanupLeakedEnemies();

        StartCoroutine(BeginTutorialDelayed());
    }

    private IEnumerator BeginTutorialDelayed()
    {
        yield return null;

        CleanupLeakedEnemies();

        if (overlay != null)
            overlay.SetActive(true);

        if (guidedTutorialPanel != null)
            guidedTutorialPanel.SetActive(true);

        SetGestureEnabled(false);

        if (puzzleManager != null)
            puzzleManager.SetTutorialMode(true);

        GoTo(Step.DrawAksara);
    }

    private void CleanupLeakedEnemies()
    {
        var allEnemies = FindObjectsByType<EnemyGestureCommand>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int removedCount = 0;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null)
                continue;

            if (enemy.CompareTag("TutorialEnemy"))
                continue;

            if (enemy.GetComponent<BossEnemy>() != null)
                continue;

            Destroy(enemy.gameObject);
            removedCount++;
        }

        if (removedCount > 0)
            Log($"🧹 {removedCount} musuh non-tutorial dibersihkan.");
    }

    private void GoTo(Step step)
    {
        Log($"➡️ Step: {step}");

        currentStep = step;

        switch (step)
        {
            case Step.DrawAksara:
                EnterDrawAksara();
                break;

            case Step.HpHeal:
                EnterHpHeal();
                break;

            case Step.Puzzle:
                EnterPuzzle();
                break;

            case Step.Done:
                EnterDone();
                break;
        }
    }

    // =========================================================
    // STEP 1
    // =========================================================

    private void EnterDrawAksara()
    {
        ShowGuidedTutorialUI();

        SetText(
            "Langkah 1",
            "Gambar aksara ini untuk mengalahkan musuh!"
        );

        TutorialManager.IsTrainingMode = true;

        retryCount = 0;

        SpawnTutorialEnemy();

        if (hintManager != null && tutorialAksara != null)
            hintManager.ShowPath(tutorialAksara);

        SubscribeGesture();

        SetGestureEnabled(true);
    }

    private void SpawnTutorialEnemy()
    {
        if (tutorialEnemySpawner == null)
        {
            StartCoroutine(NextStepRoutine(Step.HpHeal));
            return;
        }

        tutorialEnemySpawner.Spawn(
            1,
            GetEnemySpawnCenter(),
            enemySpawnRadius,
            tutorialEnemyData,
            tutorialAksara,
            true,
            false,
            null,
            OnEnemyCrashed
        );

        tutorialEnemySpawner.SetSpeed(drawPhaseEnemySpeed);

        tutorialEnemySpawner.ActivateAll();
    }

    private void OnEnemyCrashed()
    {
        if (currentStep != Step.DrawAksara)
            return;

        retryCount++;

        if (retryCount >= maxRetry)
        {
            hintManager?.HideAll();

            UnsubscribeGesture();

            SetGestureEnabled(false);

            tutorialEnemySpawner?.Clear();

            StartCoroutine(
                NextStepRoutine(Step.HpHeal)
            );

            return;
        }

        RetryEnemy();
    }

    private Vector3 GetEnemySpawnCenter()
    {
        if (
            spawnNearTargetKanan &&
            CameraIntroManager.Instance != null &&
            CameraIntroManager.Instance.targetKanan != null
        )
        {
            return CameraIntroManager.Instance.targetKanan.position;
        }

        return enemySpawnCenter;
    }

    private void SubscribeGesture()
    {
        if (gestureSubscribed || gestureDrawer == null)
            return;

        gestureDrawer.GestureRecognized += OnGestureRecognized;

        gestureSubscribed = true;
    }

    private void UnsubscribeGesture()
    {
        if (!gestureSubscribed || gestureDrawer == null)
            return;

        gestureDrawer.GestureRecognized -= OnGestureRecognized;

        gestureSubscribed = false;
    }

    private void OnGestureRecognized(
        List<List<Vector2>> strokes,
        GestureRecognitionResult result
    )
    {
        if (currentStep != Step.DrawAksara)
            return;

        if (!result.IsRecognized)
            return;

        if (
            tutorialAksara != null &&
            result.DetectedShape != tutorialAksara.GestureShape
        )
        {
            retryCount++;

            if (retryCount >= maxRetry)
            {
                hintManager?.HideAll();

                UnsubscribeGesture();

                SetGestureEnabled(false);

                tutorialEnemySpawner?.Clear();

                StartCoroutine(
                    NextStepRoutine(Step.HpHeal)
                );

                return;
            }

            RetryEnemy();

            return;
        }

        hintManager?.HideAll();

        UnsubscribeGesture();

        SetGestureEnabled(false);

        StartCoroutine(
            WaitForEnemyDeathAndCollectRoutine()
        );
    }

    private void RetryEnemy()
    {
        StartCoroutine(RespawnEnemyRoutine());
    }

    private IEnumerator RespawnEnemyRoutine()
    {
        tutorialEnemySpawner?.Clear();

        yield return null;

        yield return new WaitForSeconds(
            retryCooldown
        );

        if (currentStep == Step.DrawAksara)
        {
            SpawnTutorialEnemy();

            if (hintManager != null && tutorialAksara != null)
            {
                hintManager.HideAll();

                yield return new WaitForSeconds(0.3f);

                hintManager.ShowPath(tutorialAksara);
            }
        }
    }

    private IEnumerator WaitForEnemyDeathAndCollectRoutine()
    {
        float elapsed = 0f;

        while (elapsed < 5f)
        {
            if (
                tutorialEnemySpawner == null ||
                tutorialEnemySpawner.AliveCount == 0
            )
            {
                break;
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(
            waitAfterEnemyDeath
        );

        elapsed = 0f;

        while (elapsed < waitForCollectTimeout)
        {
            if (
                CollectedAksaraManager.Instance != null &&
                tutorialAksara != null &&
                CollectedAksaraManager.Instance.IsCollected(
                    tutorialAksara
                )
            )
            {
                break;
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        StartCoroutine(
            NextStepRoutine(Step.HpHeal)
        );
    }

    // =========================================================
    // STEP 2
    // =========================================================

    private void EnterHpHeal()
    {
        ShowGuidedTutorialUI();

        SetText(
            "Langkah 2",
            "Awas! Musuh datang. Kalau kena musuh, HP kamu berkurang."
        );

        TutorialManager.IsTrainingMode = false;

        SpawnChargingEnemy();

        SubscribeDamage();

        StartCoroutine(
            WaitForPlayerDamageRoutine()
        );
    }

    private void SpawnChargingEnemy()
    {
        if (tutorialEnemySpawner == null)
        {
            StartCoroutine(
                FallbackDamageRoutine()
            );

            return;
        }

        tutorialEnemySpawner.Spawn(
            1,
            GetEnemySpawnCenter(),
            enemySpawnRadius,
            tutorialEnemyData,
            null,
            false,
            true
        );

        tutorialEnemySpawner.SetSpeed(
            hpPhaseEnemySpeed
        );

        foreach (
            var enemy in tutorialEnemySpawner.SpawnedEnemies
        )
        {
            if (enemy == null)
                continue;

            var movement =
                enemy.GetComponent<EnemyMovementBehavior>()
                ??
                enemy.GetComponentInChildren<EnemyMovementBehavior>(
                    true
                );

            movement?.SetDamageFromData(
                step2Damage
            );
        }

        tutorialEnemySpawner.ActivateAll();
    }

    private void SubscribeDamage()
    {
        if (
            damageSubscribed ||
            playerHealth == null
        )
        {
            return;
        }

        playerHealth.DamageTaken += OnPlayerDamaged;

        damageSubscribed = true;
    }

    private void UnsubscribeDamage()
    {
        if (
            !damageSubscribed ||
            playerHealth == null
        )
        {
            return;
        }

        playerHealth.DamageTaken -= OnPlayerDamaged;

        damageSubscribed = false;
    }

    private void OnPlayerDamaged(int amount)
    {
        if (currentStep != Step.HpHeal)
            return;

        if (healSubscribed)
            return;

        UnsubscribeDamage();

        tutorialEnemySpawner?.Clear();

        StartCoroutine(
            GoToHealPhaseRoutine()
        );
    }

    private IEnumerator WaitForPlayerDamageRoutine()
    {
        float elapsed = 0f;

        while (
            elapsed < waitForEnemyHitTimeout &&
            currentStep == Step.HpHeal &&
            !healSubscribed
        )
        {
            elapsed += Time.deltaTime;

            yield return null;
        }

        if (
            currentStep == Step.HpHeal &&
            !healSubscribed
        )
        {
            if (playerHealth != null)
                playerHealth.TakeDamage(
                    step2Damage
                );

            tutorialEnemySpawner?.Clear();

            StartCoroutine(
                GoToHealPhaseRoutine()
            );
        }
    }

    private IEnumerator FallbackDamageRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (playerHealth != null)
            playerHealth.TakeDamage(
                step2Damage
            );

        yield return new WaitForSeconds(
            delayBeforeHealHint
        );

        EnterHealPhase();
    }

    private IEnumerator GoToHealPhaseRoutine()
    {
        yield return new WaitForSeconds(
            delayBeforeHealHint
        );

        EnterHealPhase();
    }

    private void EnterHealPhase()
    {
        ShowGuidedTutorialUI();

        SetText(
            "Langkah 3",
            "Sekarang gambar gesture Love untuk heal."
        );

        TutorialManager.IsTrainingMode = false;

        if (hintManager != null)
            hintManager.ShowPath(
                healGesture
            );

        SubscribeHeal();

        SetGestureEnabled(true);
    }

    private void SubscribeHeal()
    {
        if (
            healSubscribed ||
            playerHealth == null
        )
        {
            return;
        }

        playerHealth.Healed += OnPlayerHealed;

        healSubscribed = true;
    }

    private void UnsubscribeHeal()
    {
        if (
            !healSubscribed ||
            playerHealth == null
        )
        {
            return;
        }

        playerHealth.Healed -= OnPlayerHealed;

        healSubscribed = false;
    }

    private void OnPlayerHealed()
    {
        if (currentStep != Step.HpHeal)
            return;

        Log(
            $"✅ Player heal. HP: {playerHealth?.CurrentHealth}/{playerHealth?.MaxHealth}"
        );

        hintManager?.HideAll();

        UnsubscribeHeal();

        SetGestureEnabled(false);

        StartCoroutine(
            NextStepRoutine(Step.Puzzle)
        );
    }

    // =========================================================
    // STEP 3 / PUZZLE
    // =========================================================

    private void EnterPuzzle()
    {
        ShowGuidedTutorialUI();

        SetText(
            "Langkah 4",
            "Seret aksara ke slot yang benar!"
        );

        if (puzzleManager == null)
        {
            StartCoroutine(
                NextStepRoutine(Step.Done)
            );

            return;
        }

        StartCoroutine(
            ShowPuzzleWithDelayRoutine()
        );
    }

    private IEnumerator ShowPuzzleWithDelayRoutine()
    {
        yield return new WaitForSeconds(
            waitBeforePuzzle
        );

        puzzleManager.SetTutorialSlots();

        puzzleManager.ResetPuzzleForTutorial();

        puzzleManager.ShowPuzzlePanel();

        yield return new WaitForSeconds(
            puzzlePanelAppearDelay
        );

        Log("🎯 Puzzle panel muncul.");

        while (currentStep == Step.Puzzle)
        {
            if (
                puzzleManager != null &&
                puzzleManager.IsPuzzleCompleted()
            )
            {
                break;
            }

            yield return new WaitForSeconds(
                0.2f
            );
        }

        yield return new WaitForSeconds(
            waitAfterPuzzle
        );

        if (puzzleManager != null)
            puzzleManager.ShowFinishPanelForTutorial();

        StartCoroutine(
            NextStepRoutine(Step.Done)
        );
    }

    // =========================================================
    // FINISH
    // =========================================================

    private void EnterDone()
    {
        Log(
            "🎉 Guided Tutorial selesai (menunggu klik FinishPanel)."
        );

        TutorialManager.IsTrainingMode = false;

        HideGuidedTutorialUI();

        if (puzzleManager != null)
        {
            puzzleManager.SetTutorialMode(false);

            puzzleManager.ResetPuzzleStateForGameplay();

            puzzleManager.RestoreGameplaySlots();
        }

        GameProgressManager.MarkGuidedTutorialCompleted();

        currentStep = Step.Done;

        StartCoroutine(
            WaitForFinishConfirmationRoutine()
        );
    }

    private IEnumerator WaitForFinishConfirmationRoutine()
    {
        tutorialFinishConfirmed = false;

        yield return new WaitUntil(
            () => tutorialFinishConfirmed
        );

        StartCameraIntroThenWave();
    }

    public void OnTutorialFinishConfirmed()
    {
        Log(
            "✅ Tombol YA diklik. Membersihkan Guided Tutorial UI."
        );

        tutorialFinishConfirmed = true;

        GameProgressManager.SetGuidedTutorialActive(false);

        HideGuidedTutorialUI();

        if (puzzleManager != null)
            puzzleManager.HideFinishPanel();

        Log(
            "✅ Konfirmasi FinishPanel. Guided Tutorial disembunyikan."
        );
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void RestartTutorial()
    {
        Log(
            "🔄 Restart tutorial dari awal."
        );

        GameProgressManager.ResetGuidedTutorial();

        GameProgressManager.SetGuidedTutorialActive(true);

        if (playerHealth != null)
        {
            playerHealth.ResetHealth();

            Log(
                $"✅ HP direset ke full: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}"
            );
        }

        var helper =
            FindFirstObjectByType<LowHealthHelperController>();

        if (helper != null)
        {
            helper.ResetForTutorial();

            Log(
                "✅ LowHealthHelper direset."
            );
        }

        if (overlay != null)
            overlay.SetActive(false);

        if (guidedTutorialPanel != null)
            guidedTutorialPanel.SetActive(true);

        if (puzzleManager != null)
        {
            puzzleManager.HideAllPuzzlePanels();

            puzzleManager.SetTutorialMode(true);

            puzzleManager.ResetPuzzleForTutorial();
        }

        if (tutorialEnemySpawner != null)
            tutorialEnemySpawner.Clear();

        CleanupLeakedEnemies();

        currentStep = Step.Idle;

        tutorialFinishConfirmed = false;

        retryCount = 0;

        gestureSubscribed = false;

        healSubscribed = false;

        damageSubscribed = false;

        Time.timeScale = 1f;

        ClearStepText();

        BeginTutorial();

        Log(
            "✅ Restart selesai. Tutorial jalan dari Step 1."
        );
    }

    // =========================================================
    // GAMEPLAY
    // =========================================================

    private void StartCameraIntroThenWave()
    {
        Debug.Log(
            "=== [GuidedTutorial] StartCameraIntroThenWave ==="
        );

        ResumeWaveSpawner();

        if (CameraIntroManager.Instance == null)
        {
            Log(
                "⚠️ CameraIntroManager null — langsung mulai gameplay."
            );

            return;
        }

        Log(
            "🎥 Mulai camera intro setelah tutorial."
        );

        CameraIntroManager.Instance.StartIntroAfterTutorial(
            () =>
            {
                Log(
                    "✅ Camera intro selesai."
                );
            }
        );
    }

    private void ResumeWaveSpawner()
    {
        if (enemyWaveSpawner == null)
            return;

        enemyWaveSpawner.enabled = true;

        enemyWaveSpawner.StartWaveSequence();

        Log(
            "▶️ EnemyWaveSpawner di-resume."
        );
    }

    // =========================================================
    // UI
    // =========================================================

    private void ShowGuidedTutorialUI()
    {
        if (overlay != null)
            overlay.SetActive(true);

        if (guidedTutorialPanel != null)
            guidedTutorialPanel.SetActive(true);
    }

    private void HideGuidedTutorialUI()
    {
        if (overlay != null)
            overlay.SetActive(false);

        if (guidedTutorialPanel != null)
            guidedTutorialPanel.SetActive(false);

        ClearStepText();
    }

    private void ClearStepText()
    {
        if (stepTitle != null)
            stepTitle.text = "";

        if (stepDescription != null)
            stepDescription.text = "";
    }

    private void SetText(
        string title,
        string desc
    )
    {
        if (stepTitle != null)
            stepTitle.text = title;

        if (stepDescription != null)
            stepDescription.text = desc;
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void SetGestureEnabled(
        bool enabled
    )
    {
        if (gestureDrawer == null)
            return;

        if (!enabled)
            gestureDrawer.ResetGestureInput();

        gestureDrawer.enabled = enabled;
    }

    // =========================================================
    // STEP DELAY
    // =========================================================

    private IEnumerator NextStepRoutine(
        Step next
    )
    {
        yield return new WaitForSeconds(
            delayBetweenSteps
        );

        GoTo(next);
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        UnsubscribeGesture();

        UnsubscribeHeal();

        UnsubscribeDamage();
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void Log(
        string msg
    )
    {
        if (debugLog)
            Debug.Log(
                $"[GuidedTutorial] {msg}"
            );
    }
}
