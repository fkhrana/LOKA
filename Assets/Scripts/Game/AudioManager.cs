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

    [Header("Default Volume")]
    [Range(0.0001f, 1f)] [SerializeField] private float defaultBgmVolume = 0.75f;
    [Range(0.0001f, 1f)] [SerializeField] private float defaultSfxVolume = 0.75f;

    private string currentBgmName;
    private readonly Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();
    private float currentBgmVolume;
    private float currentSfxVolume;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto‑buat AudioSource kalau belum di‑assign di Inspector
        if (bgmSource == null) bgmSource = CreateAudioSource("BGM", true);
        if (sfxSource == null) sfxSource = CreateAudioSource("SFX", false);
        if (hoverSource == null) hoverSource = CreateAudioSource("HOVER", false);
    }

    private AudioSource CreateAudioSource(string name, bool loop)
    {
        GameObject go = new GameObject(name + "_Source");
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        src.volume = 1f;
        return src;
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
        if (Instance == null) return;

        switch (scene.name)
        {
            case "MainMenu":
                PlayBGM("Surat Ajaib Desa");
                break;
            case "CutScenee":
                PlayBGM("Surat Ajaib Desa", true);
                break;
            case "MainGameplay(Drawing)":
                PlayBGM("Broken Festival Kite");
                break;
            case "Level2":
                PlayBGM("Broken Festival Kite");
                break;
            default:
                StopBGM();
                break;
        }
    }

    // ========== BGM ==========
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null || currentBgmName == clip.name) return;
        currentBgmName = clip.name;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayBGM(string resourceName, bool forceRestart = false)
    {
        if (string.IsNullOrEmpty(resourceName)) return;
        if (!forceRestart && currentBgmName == resourceName) return;
        if (forceRestart) currentBgmName = null;

        AudioClip clip = Resources.Load<AudioClip>($"Audio/BGM/{resourceName}");
        if (clip != null) PlayBGM(clip);
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
        currentBgmName = null;
    }

    // ========== SFX (One‑Shot) ==========
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return;

        if (!sfxCache.TryGetValue(clipName, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>($"Audio/SFX/{clipName}");
            if (clip != null) sfxCache[clipName] = clip;
        }
        PlaySFX(clip);
    }

    // ========== HOVER SFX (bisa di‑stop) ==========
    public void PlayHoverSFX(AudioClip clip)
    {
        if (clip == null || hoverSource == null) return;
        hoverSource.clip = clip;
        hoverSource.Play();
    }

    public void StopHoverSFX()
    {
        if (hoverSource != null)
            hoverSource.Stop();
    }

    // ========== VOLUME ==========
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
            if (sfxSource != null) sfxSource.volume = value;
            if (hoverSource != null) hoverSource.volume = value;
        }
    }

    public float GetCurrentBGMVolume() => currentBgmVolume;
    public float GetCurrentSFXVolume() => currentSfxVolume;
}