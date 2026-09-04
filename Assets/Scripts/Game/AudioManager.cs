using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    [Header("Default Volume")]
    [Range(0.0001f, 1f)]
    [SerializeField] private float defaultBgmVolume = 0.75f;

    [Range(0.0001f, 1f)]
    [SerializeField] private float defaultSfxVolume = 0.75f;

    private string currentBgmName;

    private readonly Dictionary<string, AudioClip> sfxCache =
        new Dictionary<string, AudioClip>();

    private float currentBgmVolume;
    private float currentSfxVolume;

    private float lastUISFXTime;

    [SerializeField] private float uiSFXCooldown = 0.05f;

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
            bgmSource = CreateAudioSource("BGM", true);

        if (sfxSource == null)
            sfxSource = CreateAudioSource("SFX", false);

        if (hoverSource == null)
            hoverSource = CreateAudioSource("HOVER", false);

        if (uiSource == null)
            uiSource = CreateAudioSource("UI", false);
    }

    private AudioSource CreateAudioSource(string sourceName, bool loop)
    {
        GameObject go = new GameObject(sourceName + "_Source");
        go.transform.SetParent(transform);

        AudioSource source = go.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = 1f;

        return source;
    }

    private void Start()
    {
        SetBGMVolume(defaultBgmVolume);
        SetSFXVolume(defaultSfxVolume);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance == null)
            return;

        switch (scene.name)
        {
            case "MainMenu":
                PlayBGM("Surat Ajaib Desa");
                break;

            case "CutScenee":
                StopBGM();
                break;

            case "MainGameplay(Drawing)":
            case "Level2":
                PlayBGM("Broken Festival Kite");
                break;

            default:
                StopBGM();
                break;
        }
    }

    // BGM
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
            return;

        if (currentBgmName == clip.name)
            return;

        currentBgmName = clip.name;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayBGM(string resourceName, bool forceRestart = false)
    {
        if (string.IsNullOrEmpty(resourceName))
            return;

        if (!forceRestart && currentBgmName == resourceName)
            return;

        if (forceRestart)
            currentBgmName = null;

        AudioClip clip =
            Resources.Load<AudioClip>($"Audio/BGM/{resourceName}");

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

    // SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        StopHoverSFX();
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier)
    {
        if (clip == null || sfxSource == null)
            return;

        StopHoverSFX();

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volumeMultiplier)
        );
    }

    public void PlaySFX(string clipName)
    {
        if (string.IsNullOrEmpty(clipName))
            return;

        PlaySFX(GetSFXClip(clipName));
    }

    public void PlaySFX(string clipName, float volumeMultiplier)
    {
        if (string.IsNullOrEmpty(clipName))
            return;

        PlaySFX(
            GetSFXClip(clipName),
            volumeMultiplier
        );
    }

    // Hover
    public void PlayHoverSFX(AudioClip clip)
    {
        if (clip == null || hoverSource == null)
            return;

        if (hoverSource.isPlaying)
            hoverSource.Stop();

        hoverSource.clip = clip;
        hoverSource.volume = currentSfxVolume;
        hoverSource.Play();
    }

    public void PlayHoverSFX(string clipName)
    {
        if (string.IsNullOrEmpty(clipName))
            return;

        PlayHoverSFX(GetSFXClip(clipName));
    }

    public void StopHoverSFX()
    {
        if (hoverSource != null && hoverSource.isPlaying)
            hoverSource.Stop();
    }

    // UI SFX
    public void PlayUISFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || uiSource == null)
            return;

        if (Time.unscaledTime - lastUISFXTime < uiSFXCooldown)
            return;

        lastUISFXTime = Time.unscaledTime;

        if (uiSource.isPlaying)
            uiSource.Stop();

        uiSource.volume =
            currentSfxVolume * Mathf.Clamp01(volumeMultiplier);

        uiSource.clip = clip;
        uiSource.Play();
    }

    public void PlayUISFX(string clipName, float volumeMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(clipName))
            return;

        PlayUISFX(
            GetSFXClip(clipName),
            volumeMultiplier
        );
    }

    public void StopUISFX()
    {
        if (uiSource != null && uiSource.isPlaying)
            uiSource.Stop();
    }

    private AudioClip GetSFXClip(string clipName)
    {
        if (string.IsNullOrEmpty(clipName))
            return null;

        if (sfxCache.TryGetValue(clipName, out AudioClip cachedClip))
            return cachedClip;

        AudioClip clip =
            Resources.Load<AudioClip>($"Audio/SFX/{clipName}");

        if (clip != null)
        {
            sfxCache[clipName] = clip;
        }
        else
        {
            Debug.LogWarning(
                $"SFX tidak ditemukan: Audio/SFX/{clipName}"
            );
        }

        return clip;
    }

    // Volume
    public void SetBGMVolume(float sliderValue)
    {
        float value = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        currentBgmVolume = value;

        if (audioMixer != null)
        {
            float dB = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(MIXER_BGM, dB);
        }
        else if (bgmSource != null)
        {
            bgmSource.volume = value;
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        float value = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        currentSfxVolume = value;

        if (audioMixer != null)
        {
            float dB = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(MIXER_SFX, dB);
        }
        else
        {
            if (sfxSource != null)
                sfxSource.volume = value;

            if (hoverSource != null)
                hoverSource.volume = value;

            if (uiSource != null)
                uiSource.volume = value;
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