using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BGMSetting
{
    public string bgmName;
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MIXER_BGM = "MusicV";
    private const string MIXER_SFX = "SFXV";

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource hoverSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource loopedSfxSource;

    [Header("Default Volume")]
    [SerializeField] private float defaultBgmVolume = 1f;
    [SerializeField] private float defaultSfxVolume = 1f;

    [Header("BGM Volume Khusus")]
    [SerializeField] private BGMSetting[] bgmSettings;

    [Header("Aksara Voice Ducking")]
    [SerializeField] private float aksaraBgmVolume = 1f;

    private string currentBgmName;

    private readonly Dictionary<string, AudioClip> sfxCache =
        new Dictionary<string, AudioClip>();

    // Volume slider user
    private float currentBgmVolume;
    private float currentSfxVolume;

    // Volume BGM yang sedang aktif
    private float activeBgmVolume;

    private float lastUISFXTime;

    [SerializeField] private float uiSFXCooldown = 0.8f;

    private Coroutine restoreBGMCoroutine;

    // Volume BGM sebelum masuk ke BGM chest
    private float previousBGMVolume;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
            bgmSource = CreateAudioSource(
                "BGM",
                true
            );

        if (sfxSource == null)
            sfxSource = CreateAudioSource(
                "SFX",
                false
            );

        if (hoverSource == null)
            hoverSource = CreateAudioSource(
                "HOVER",
                false
            );

        if (uiSource == null)
            uiSource = CreateAudioSource(
                "UI",
                false
            );

        if (loopedSfxSource == null)
            loopedSfxSource = CreateAudioSource(
                "LOOPED_SFX",
                true
            );
    }

    private AudioSource CreateAudioSource(
        string sourceName,
        bool loop
    )
    {
        GameObject go =
            new GameObject(
                sourceName + "_Source"
            );

        go.transform.SetParent(transform);

        AudioSource source =
            go.AddComponent<AudioSource>();

        source.loop = loop;
        source.playOnAwake = false;
        source.volume = 1f;

        return source;
    }

    private void Start()
    {
        SetBGMVolume(
            defaultBgmVolume
        );

        SetSFXVolume(
            defaultSfxVolume
        );
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (Instance == null)
            return;

        if (restoreBGMCoroutine != null)
        {
            StopCoroutine(restoreBGMCoroutine);
            restoreBGMCoroutine = null;
        }

        switch (scene.name)
        {
            case "MainMenu":
                PlayBGM(
                    "Surat Ajaib Desa"
                );
                break;

            case "CutScenee":
                StopBGM();
                break;

            case "MainGameplay(Drawing)":
                PlayBGM(
                    "Broken Festival Kite"
                );
                break;

            case "Level2":
                PlayBGM(
                    "Broken Festival Kite"
                );
                break;

            case "Level3":
                PlayBGM(
                    "Boss Theme"
                );
                break;

            default:
                StopBGM();
                break;
        }
    }

    // =========================
    // BGM
    // =========================

    public void PlayBGM(
        AudioClip clip
    )
    {
        if (
            clip == null ||
            bgmSource == null
        )
            return;

        if (
            currentBgmName ==
            clip.name
        )
            return;

        currentBgmName =
            clip.name;

        bgmSource.clip =
            clip;

        bgmSource.loop =
            true;

        ApplyBGMVolume(
            clip.name
        );

        bgmSource.Play();
    }

    public void PlayBGM(
        string resourceName,
        bool forceRestart = false
    )
    {
        if (
            string.IsNullOrEmpty(
                resourceName
            )
        )
            return;

        if (
            !forceRestart &&
            currentBgmName ==
            resourceName
        )
            return;

        if (forceRestart)
            currentBgmName = null;

        AudioClip clip =
            Resources.Load<AudioClip>(
                $"Audio/BGM/{resourceName}"
            );

        if (clip != null)
        {
            PlayBGM(clip);
        }
        else
        {
            Debug.LogWarning(
                $"BGM tidak ditemukan: Audio/BGM/{resourceName}"
            );
        }
    }

    public void StopBGM()
    {
        if (bgmSource == null)
            return;

        bgmSource.Stop();

        currentBgmName = null;
    }

    // =========================
    // BGM VOLUME KHUSUS
    // =========================

    private float GetBGMVolume(
        string bgmName
    )
    {
        if (
            bgmSettings != null
        )
        {
            foreach (
                BGMSetting setting
                in bgmSettings
            )
            {
                if (
                    setting != null &&
                    setting.bgmName ==
                    bgmName
                )
                {
                    return Mathf.Max(
                        0f,
                        setting.volume
                    );
                }
            }
        }

        return currentBgmVolume;
    }

    private void ApplyBGMVolume(
        string bgmName
    )
    {
        float volume =
            GetBGMVolume(
                bgmName
            );

        volume =
            Mathf.Clamp(
                volume,
                0.0001f,
                1f
            );

        activeBgmVolume = volume;

        if (audioMixer != null)
        {
            float dB =
                Mathf.Log10(
                    volume
                ) * 20f;

            audioMixer.SetFloat(
                MIXER_BGM,
                dB
            );
        }
        else if (bgmSource != null)
        {
            bgmSource.volume =
                volume;
        }
    }

    private void ApplyRawBGMVolume(
        float volume
    )
    {
        volume =
            Mathf.Clamp(
                volume,
                0.0001f,
                1f
            );

        activeBgmVolume = volume;

        if (audioMixer != null)
        {
            float dB =
                Mathf.Log10(
                    volume
                ) * 20f;

            audioMixer.SetFloat(
                MIXER_BGM,
                dB
            );
        }
        else if (bgmSource != null)
        {
            bgmSource.volume =
                volume;
        }
    }

    // =========================
    // CHEST / REWARD BGM
    // =========================

    public void PlayRewardBGM(
        AudioClip clip,
        float volume = 0.5f
    )
    {
        if (
            clip == null ||
            bgmSource == null
        )
            return;

        if (restoreBGMCoroutine != null)
        {
            StopCoroutine(restoreBGMCoroutine);
            restoreBGMCoroutine = null;
        }

        previousBGMVolume =
            activeBgmVolume;

        currentBgmName = null;

        PlayBGM(clip);

        ApplyRawBGMVolume(
            volume
        );

        Debug.Log(
            "[AudioManager] Reward BGM dimainkan. Volume: "
            + volume
        );
    }

    public void RestorePreviousBGM()
    {
        if (bgmSource == null)
            return;

        ApplyRawBGMVolume(
            previousBGMVolume
        );

        Debug.Log(
            "[AudioManager] Volume BGM sebelumnya dikembalikan."
        );
    }

    // =========================
    // SFX
    // =========================

    public void PlaySFX(
        AudioClip clip
    )
    {
        if (
            clip == null ||
            sfxSource == null
        )
            return;

        StopHoverSFX();

        sfxSource.PlayOneShot(
            clip
        );
    }

    public void PlaySFX(
        AudioClip clip,
        float volumeMultiplier
    )
    {
        if (
            clip == null ||
            sfxSource == null
        )
            return;

        StopHoverSFX();

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp(
                volumeMultiplier,
                0f,
                2f
            )
        );
    }

    public void PlaySFX(
        string clipName
    )
    {
        if (
            string.IsNullOrEmpty(
                clipName
            )
        )
            return;

        PlaySFX(
            GetSFXClip(
                clipName
            )
        );
    }

    public void PlaySFX(
        string clipName,
        float volumeMultiplier
    )
    {
        if (
            string.IsNullOrEmpty(
                clipName
            )
        )
            return;

        PlaySFX(
            GetSFXClip(
                clipName
            ),
            volumeMultiplier
        );
    }

    // =========================
    // LOOPED SFX
    // =========================

    public void PlayLoopedSFX(
        AudioClip clip,
        float volumeMultiplier = 1f
    )
    {
        if (
            clip == null ||
            loopedSfxSource == null
        )
            return;

        if (
            loopedSfxSource.isPlaying &&
            loopedSfxSource.clip == clip
        )
            return;

        loopedSfxSource.clip =
            clip;

        loopedSfxSource.volume =
            currentSfxVolume *
            Mathf.Clamp(
                volumeMultiplier,
                0f,
                2f
            );

        loopedSfxSource.loop =
            true;

        loopedSfxSource.Play();
    }

    public void PlayLoopedSFX(
        string clipName,
        float volumeMultiplier = 1f
    )
    {
        if (
            string.IsNullOrEmpty(
                clipName
            )
        )
            return;

        PlayLoopedSFX(
            GetSFXClip(
                clipName
            ),
            volumeMultiplier
        );
    }

    public void StopLoopedSFX()
    {
        if (
            loopedSfxSource != null &&
            loopedSfxSource.isPlaying
        )
        {
            loopedSfxSource.Stop();
        }
    }

    // =========================
    // AKSARA VOICE
    // =========================

    public void PlayAksaraVoice(
        AudioClip clip,
        float volumeMultiplier = 1f
    )
    {
        if (
            clip == null ||
            sfxSource == null
        )
            return;

        StopHoverSFX();

        if (
            restoreBGMCoroutine != null
        )
        {
            StopCoroutine(
                restoreBGMCoroutine
            );

            restoreBGMCoroutine =
                null;
        }

        float originalVolume =
            activeBgmVolume;

        if (audioMixer != null)
        {
            float duckedVolume =
                Mathf.Clamp(
                    aksaraBgmVolume,
                    0.0001f,
                    1f
                );

            float dB =
                Mathf.Log10(
                    duckedVolume
                ) * 20f;

            audioMixer.SetFloat(
                MIXER_BGM,
                dB
            );

            activeBgmVolume =
                duckedVolume;
        }
        else if (
            bgmSource != null
        )
        {
            float duckedVolume =
                Mathf.Clamp(
                    aksaraBgmVolume,
                    0f,
                    1f
                );

            bgmSource.volume =
                duckedVolume;

            activeBgmVolume =
                duckedVolume;
        }

        // Volume Aksara bisa sampai 500x
        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp(
                volumeMultiplier,
                0f,
                500f
            )
        );

        restoreBGMCoroutine =
            StartCoroutine(
                RestoreBGMVolumeAfter(
                    clip.length,
                    originalVolume
                )
            );
    }

    private IEnumerator RestoreBGMVolumeAfter(
        float duration,
        float originalVolume
    )
    {
        yield return new WaitForSeconds(
            duration
        );

        ApplyRawBGMVolume(
            originalVolume
        );

        restoreBGMCoroutine =
            null;
    }

    // =========================
    // HOVER SFX
    // =========================

    public void PlayHoverSFX(
        AudioClip clip
    )
    {
        if (
            clip == null ||
            hoverSource == null
        )
            return;

        if (hoverSource.isPlaying)
            hoverSource.Stop();

        hoverSource.clip =
            clip;

        hoverSource.volume =
            currentSfxVolume;

        hoverSource.Play();
    }

    public void PlayHoverSFX(
        AudioClip clip,
        float volumeMultiplier
    )
    {
        if (
            clip == null ||
            hoverSource == null
        )
            return;

        if (hoverSource.isPlaying)
            hoverSource.Stop();

        hoverSource.clip =
            clip;

        hoverSource.volume =
            currentSfxVolume *
            Mathf.Clamp01(
                volumeMultiplier
            );

        hoverSource.Play();
    }

    public void PlayHoverSFX(
        string clipName
    )
    {
        if (
            string.IsNullOrEmpty(
                clipName
            )
        )
            return;

        PlayHoverSFX(
            GetSFXClip(
                clipName
            )
        );
    }

    public void PlayHoverSFX(
        string clipName,
        float volumeMultiplier
    )
    {
        if (
            string.IsNullOrEmpty(
                clipName
            )
        )
            return;

        PlayHoverSFX(
            GetSFXClip(
                clipName
            ),
            volumeMultiplier
        );
    }

    public void StopHoverSFX()
    {
        if (
            hoverSource != null &&
            hoverSource.isPlaying
        )
        {
            hoverSource.Stop();
        }
    }

    // =========================
    // UI SFX
    // =========================

    public void PlayUISFX(
        AudioClip clip,
        float volumeMultiplier = 1f
    )
    {
        if (
            clip == null ||
            uiSource == null
        )
            return;

        if (
            Time.unscaledTime -
            lastUISFXTime <
            uiSFXCooldown
        )
        {
            return;
        }

        lastUISFXTime =
            Time.unscaledTime;

        if (uiSource.isPlaying)
            uiSource.Stop();

        uiSource.volume =
            currentSfxVolume *
            Mathf.Clamp01(
                volumeMultiplier
            );

        uiSource.clip =
            clip;

        uiSource.Play();
    }

    public void PlayUISFX(
        string clipName,
        float volumeMultiplier = 1f
    )
    {
        if (
            string.IsNullOrEmpty(
                clipName
            )
        )
            return;

        PlayUISFX(
            GetSFXClip(
                clipName
            ),
            volumeMultiplier
        );
    }

    public void StopUISFX()
    {
        if (
            uiSource != null &&
            uiSource.isPlaying
        )
        {
            uiSource.Stop();
        }
    }

    // =========================
    // LOAD SFX
    // =========================

    private AudioClip GetSFXClip(
        string clipName
    )
    {
        if (
            string.IsNullOrEmpty(
                clipName
            )
        )
            return null;

        if (
            sfxCache.TryGetValue(
                clipName,
                out AudioClip cachedClip
            )
        )
        {
            return cachedClip;
        }

        AudioClip clip =
            Resources.Load<AudioClip>(
                $"Audio/SFX/{clipName}"
            );

        if (clip != null)
        {
            sfxCache[
                clipName
            ] = clip;
        }
        else
        {
            Debug.LogWarning(
                $"SFX tidak ditemukan: Audio/SFX/{clipName}"
            );
        }

        return clip;
    }

    // =========================
    // VOLUME
    // =========================

    public void SetBGMVolume(
        float sliderValue
    )
    {
        float value =
            Mathf.Clamp(
                sliderValue,
                0.0001f,
                1f
            );

        currentBgmVolume =
            value;

        activeBgmVolume =
            value;

        if (audioMixer != null)
        {
            float dB =
                Mathf.Log10(
                    value
                ) * 20f;

            audioMixer.SetFloat(
                MIXER_BGM,
                dB
            );
        }
        else if (
            bgmSource != null
        )
        {
            bgmSource.volume =
                value;
        }
    }

    public void SetSFXVolume(
        float sliderValue
    )
    {
        float value =
            Mathf.Clamp(
                sliderValue,
                0.0001f,
                1f
            );

        currentSfxVolume =
            value;

        if (audioMixer != null)
        {
            float dB =
                Mathf.Log10(
                    value
                ) * 20f;

            audioMixer.SetFloat(
                MIXER_SFX,
                dB
            );
        }
        else
        {
            if (sfxSource != null)
                sfxSource.volume =
                    value;

            if (hoverSource != null)
                hoverSource.volume =
                    value;

            if (uiSource != null)
                uiSource.volume =
                    value;

            if (loopedSfxSource != null)
                loopedSfxSource.volume =
                    value;
        }
    }

    public float GetCurrentBGMVolume()
    {
        return currentBgmVolume;
    }

    public float GetCurrentSFXVolume()
    {
        return currentSfxVolume;
    }
}