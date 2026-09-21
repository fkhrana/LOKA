using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using EasyTransition;

public class LevelBossSettings : MonoBehaviour
{
    [Header("Win VFX")]
    [SerializeField] private GameObject winVfx;
    [SerializeField] private EfekConfetti[] winConfettiEffects;
    [SerializeField] private AudioClip confettiSfx;
    [SerializeField, Range(0f, 1f)] private float confettiSfxVolume = 1f;

    [Header("Cutscene in same scene")]
    [SerializeField] private GameObject cutsceneHolder;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipButton;

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField, Min(0f)] private float winVfxDisplayDelay = 0.8f;

    [Header("Win Panel")]
    [SerializeField] private GameObject winPanel;

    private TransitionManager transitionManager;
    private bool sequenceStarted;

    private float GetWinVfxDisplayDelay()
    {
        float delay = winVfxDisplayDelay;

        if (confettiSfx != null)
            delay = Mathf.Max(delay, confettiSfx.length + 0.15f);

        return delay;
    }

    public void TriggerWinSequence()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;

        if (winVfx != null)
        {
            Canvas parentCanvas = winVfx.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
                parentCanvas.gameObject.SetActive(true);

            winVfx.SetActive(true);

            if (winConfettiEffects != null)
            {
                foreach (EfekConfetti confettiEffect in winConfettiEffects)
                {
                    if (confettiEffect == null)
                        continue;

                    if (!confettiEffect.gameObject.activeSelf)
                        confettiEffect.gameObject.SetActive(true);

                    if (confettiSfx != null && AudioManager.Instance != null)
                        AudioManager.Instance.PlayUISFX(confettiSfx, confettiSfxVolume);

                    confettiEffect.MuntahkanConfetti();
                }
            }

            ParticleSystem[] particleSystems = winVfx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                particleSystem.Clear();
                particleSystem.Play(true);
            }

            Debug.Log("[LevelBossSettings] TriggerWinSequence -> winVfx activated.");
        }
        else
        {
            Debug.LogWarning("[LevelBossSettings] TriggerWinSequence dipanggil tapi winVfx null.");
        }

        StartCoroutine(TriggerWinSequenceRoutine());
    }

    private IEnumerator TriggerWinSequenceRoutine()
    {
        float delay = GetWinVfxDisplayDelay();

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        transitionManager = TransitionManager.Instance();

        if (transitionManager != null && transitionSettings != null)
        {
            transitionManager.onTransitionCutPointReached += StartCutscene;
            transitionManager.Transition(transitionSettings, transitionDelay);
        }
        else
        {
            StartCutscene();
        }
    }

    private void StartCutscene()
    {
        if (cutsceneHolder != null)
            cutsceneHolder.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.Play();
        }

        if (skipButton != null)
            skipButton.SetActive(true);

        if (transitionManager != null)
            transitionManager.onTransitionCutPointReached -= StartCutscene;
    }

    public void SkipCutscene()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (cutsceneHolder != null)
            cutsceneHolder.SetActive(false);

        if (skipButton != null)
            skipButton.SetActive(false);

        if (transitionManager != null)
            transitionManager.onTransitionCutPointReached -= StartCutscene;

        ShowWinPanel();
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    private void BindSkipButton()
    {
        if (skipButton == null)
            return;

        Button button = skipButton.GetComponent<Button>();
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SkipCutscene);
    }

    private void Start()
    {
        BindSkipButton();

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.OnReachedLevelComplete.AddListener(TriggerWinSequence);
    }

    private void OnEnable()
    {
        BindSkipButton();

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.OnReachedLevelComplete.AddListener(TriggerWinSequence);
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.OnReachedLevelComplete.RemoveListener(TriggerWinSequence);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (cutsceneHolder != null)
            cutsceneHolder.SetActive(false);

        if (skipButton != null)
            skipButton.SetActive(false);

        ShowWinPanel();
    }
}
