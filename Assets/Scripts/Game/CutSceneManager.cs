using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using EasyTransition;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainGameplay(Drawing)";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0.5f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            Debug.LogWarning(
                "VideoPlayer not assigned in CutsceneManager!",
                this
            );
        }
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached +=
                OnVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -=
                OnVideoFinished;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Simpan scene cutscene yang sedang dibuka
        GameProgressManager.SaveLastScene(
            SceneManager.GetActiveScene().name
        );

        // Jalankan video
        videoPlayer?.Play();

        // Bersihkan transition lama
        CleanupStaleTransitions();
    }

    public void PauseVideo()
    {
        if (isTransitioning)
            return;

        PlayButtonClickSFX();

        videoPlayer?.Pause();
    }

    public void ResumeVideo()
    {
        if (isTransitioning)
            return;

        PlayButtonClickSFX();

        videoPlayer?.Play();
    }

    public void OnSkipClicked()
    {
        if (isTransitioning)
            return;

        PlayButtonClickSFX();

        Time.timeScale = 1f;

        videoPlayer?.Stop();

        LoadNextScene();
    }

    private void PlayButtonClickSFX()
    {
        AudioManager.Instance?.PlaySFX(
            "ButtonHover"
        );
    }

    private void OnVideoFinished(
        VideoPlayer vp
    )
    {
        if (isTransitioning)
            return;

        Time.timeScale = 1f;

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError(
                "nextSceneName is empty!",
                this
            );

            return;
        }

        // Tandai cutscene sudah selesai
        GameProgressManager.SetCutsceneCompleted();

        // Unlock Level 1
        PlayerPrefs.SetInt(
            "LevelUnlocked_0",
            1
        );

        // Simpan progress unlock
        PlayerPrefs.Save();

        // Simpan scene berikutnya
        GameProgressManager.SaveLastScene(
            nextSceneName
        );

        isTransitioning = true;

        // Bersihkan transition lama
        CleanupStaleTransitions();

        TransitionManager tm =
            TransitionManager.Instance();

        if (tm != null &&
            transitionSettings != null)
        {
            tm.Transition(
                nextSceneName,
                transitionSettings,
                loadDelay
            );
        }
        else
        {
            Debug.LogWarning(
                "TransitionManager or Settings missing, loading scene directly."
            );

            SceneManager.LoadScene(
                nextSceneName
            );

            isTransitioning = false;
        }
    }

    private void CleanupStaleTransitions()
    {
        EasyTransition.Transition[] oldTransitions =
            FindObjectsByType<EasyTransition.Transition>(
                FindObjectsSortMode.None
            );

        foreach (var t in oldTransitions)
        {
            if (t == null)
                continue;

            if (IsTransitionStillAnimating(t))
            {
                continue;
            }

            Debug.LogWarning(
                "[CutsceneManager] Menemukan Transition instance lama (selesai), menghapus: "
                + t.gameObject.name
            );

            Destroy(t.gameObject);
        }
    }

    private bool IsTransitionStillAnimating(
        EasyTransition.Transition t
    )
    {
        Transform[] panels =
        {
            t.transitionPanelIN,
            t.transitionPanelOUT
        };

        foreach (var panel in panels)
        {
            if (panel == null ||
                !panel.gameObject.activeInHierarchy)
            {
                continue;
            }

            Animator[] anims =
                panel.GetComponentsInChildren<Animator>(
                    true
                );

            foreach (var anim in anims)
            {
                if (anim == null ||
                    !anim.isActiveAndEnabled)
                {
                    continue;
                }

                var state =
                    anim.GetCurrentAnimatorStateInfo(0);

                if (state.normalizedTime < 1f &&
                    !anim.IsInTransition(0))
                {
                    return true;
                }
            }
        }

        return false;
    }
}