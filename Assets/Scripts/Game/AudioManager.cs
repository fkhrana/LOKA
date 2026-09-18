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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }
    }

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup aksaraVoiceMixerGroup;

    private const string MIXER_BGM = "MusicV";
    private const string MIXER_SFX = "SFXV";

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource hoverSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource loopedSfxSource;
    [SerializeField] private AudioSource aksaraVoiceSource;

    [Header("Default Volume")]
    [SerializeField] private float defaultBgmVolume = 1f;
    [SerializeField] private float defaultSfxVolume = 1f;

    [Header("BGM Volume Khusus")]
    [SerializeField] private BGMSetting[] bgmSettings;

    [Header("Aksara Voice Ducking")]
    [Range(0f, 1f)] [SerializeField] private float aksaraBgmVolume = 0.3f;

    [Header("SFX Ducking (BGM turun saat SFX main)")]
    [Tooltip("Faktor pengali volume BGM saat SFX diputar. 0.7 = BGM turun jadi 70%. 1 = tidak ada ducking.")]
    [Range(0f, 1f)] [SerializeField] private float sfxBgmVolume = 0.7f;

    [Tooltip("Durasi minimum ducking (detik). Clip SFX yang sangat pendek tetap duck selama ini.")]
    [SerializeField] private float sfxDuckMinDuration = 0.2f;

    [Tooltip("Durasi maksimum ducking (detik). Clip SFX panjang di-cap di sini.")]
    [SerializeField] private float sfxDuckMaxDuration = 1.5f;

    [Header("BGM Transition")]
    [SerializeField] private float defaultBgmFadeDuration = 1f;

    [Header("SFX")]
    [SerializeField] private float sfxDuplicateGuardWindow = 0.08f;
    [SerializeField] private float uiSFXCooldown = 0.8f;

    private string currentBgmName;
    private float currentBgmVolume;
    private float currentSfxVolume;
    private float activeBgmVolume;
    private float lastUISFXTime;
    private float previousBGMVolume;

    // Aksara voice ducking.
    private float preDuckBGMVolume = -1f;

    // SFX ducking.
    private float preSfxDuckBGMVolume = -1f;

    private Coroutine restoreBGMCoroutine;
    private Coroutine bgmFadeCoroutine;
    private Coroutine restoreSfxDuckCoroutine;

    private readonly Dictionary<string, AudioClip> sfxCache = new();
    private readonly Dictionary<AudioClip, float> lastSfxPlayTime = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource ??= CreateSource("BGM", true);
        sfxSource ??= CreateSource("SFX", false);
        hoverSource ??= CreateSource("HOVER", false);
        uiSource ??= CreateSource("UI", false);
        loopedSfxSource ??= CreateSource("LOOPED_SFX", true);
        aksaraVoiceSource ??= CreateSource("AKSARA_VOICE", false);

        aksaraVoiceSource.volume = 1f;
        if (aksaraVoiceMixerGroup != null)
            aksaraVoiceSource.outputAudioMixerGroup = aksaraVoiceMixerGroup;
    }

    private void Start()
    {
        SetBGMVolume(defaultBgmVolume);
        SetSFXVolume(defaultSfxVolume);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private AudioSource CreateSource(string name, bool loop)
    {
        var go = new GameObject(name + "_Source");
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        src.volume = 1f;
        return src;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance == null) return;

        // Reset semua state ducking sebelum ganti BGM.
        CancelAllDucking();

        switch (scene.name)
        {
            case "MainMenu": PlayBGMWithFade("Surat Ajaib Desa", defaultBgmFadeDuration, defaultBgmFadeDuration); break;
            case "CutScenee": FadeOutBGM(defaultBgmFadeDuration); break;
            case "MainGameplay(Drawing)":
            case "Level2": PlayBGMWithFade("Broken Festival Kite", defaultBgmFadeDuration, defaultBgmFadeDuration); break;
            case "Level3": PlayBGMWithFade("Boss Theme", defaultBgmFadeDuration, defaultBgmFadeDuration); break;
            case "Latihan":PlayBGMWithFade("Bgm_tutorial", defaultBgmFadeDuration, defaultBgmFadeDuration);break;
            default: FadeOutBGM(defaultBgmFadeDuration); break;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (currentBgmName == clip.name) return;

        CancelAllDucking();

        if (bgmFadeCoroutine != null) { StopCoroutine(bgmFadeCoroutine); bgmFadeCoroutine = null; }

        currentBgmName = clip.name;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        ApplyBGMVolume(clip.name);
        bgmSource.Play();
    }

    public void PlayBGM(string resourceName, bool forceRestart = false)
    {
        if (string.IsNullOrEmpty(resourceName)) return;
        if (!forceRestart && currentBgmName == resourceName) return;
        if (forceRestart) currentBgmName = null;

        var clip = Resources.Load<AudioClip>($"Audio/BGM/{resourceName}");
        if (clip != null) PlayBGM(clip);
        else Debug.LogWarning($"[Audio] BGM tidak ditemukan: Audio/BGM/{resourceName}");
    }

    public void StopBGM(float fadeDuration = 0f, System.Action onComplete = null)
    {
        if (bgmSource == null) { onComplete?.Invoke(); return; }

        CancelAllDucking();

        if (fadeDuration > 0f) { FadeOutBGM(fadeDuration, onComplete); return; }

        if (bgmFadeCoroutine != null) { StopCoroutine(bgmFadeCoroutine); bgmFadeCoroutine = null; }

        bgmSource.Stop();
        currentBgmName = null;
        onComplete?.Invoke();
    }

    public void FadeOutBGM(float duration = -1f, System.Action onComplete = null)
    {
        if (bgmSource == null) return;
        if (duration < 0f) duration = defaultBgmFadeDuration;

        CancelAllDucking();

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);

        if (!bgmSource.isPlaying) { currentBgmName = null; onComplete?.Invoke(); return; }

        bgmFadeCoroutine = StartCoroutine(FadeOutBGMCoroutine(duration, onComplete));
    }

    public IEnumerator FadeOutBGMAndWait(float duration = -1f)
    {
        bool done = false;
        FadeOutBGM(duration, () => done = true);
        while (!done) yield return null;
    }

    public void PlayBGMWithFade(string resourceName, float fadeOutDuration = -1f, float fadeInDuration = -1f)
    {
        if (string.IsNullOrEmpty(resourceName)) return;
        if (currentBgmName == resourceName && bgmSource.isPlaying) return;

        if (fadeOutDuration <= 0f) fadeOutDuration = defaultBgmFadeDuration;
        if (fadeInDuration < 0f) fadeInDuration = defaultBgmFadeDuration;

        CancelAllDucking();

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);

        bgmFadeCoroutine = StartCoroutine(CrossfadeBGMCoroutine(resourceName, fadeOutDuration, fadeInDuration));
    }

    private IEnumerator FadeOutBGMCoroutine(float duration, System.Action onComplete)
    {
        float start = activeBgmVolume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            ApplyRawBGMVolume(Mathf.Lerp(start, 0.0001f, t / duration));
            yield return null;
        }

        bgmSource.Stop();
        currentBgmName = null;
        bgmFadeCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator CrossfadeBGMCoroutine(string resourceName, float fadeOutDuration, float fadeInDuration)
    {
        if (bgmSource.isPlaying && fadeOutDuration > 0f)
        {
            float start = activeBgmVolume;
            float t = 0f;

            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                ApplyRawBGMVolume(Mathf.Lerp(start, 0.0001f, t / fadeOutDuration));
                yield return null;
            }
        }

        bgmSource.Stop();

        var clip = Resources.Load<AudioClip>($"Audio/BGM/{resourceName}");
        if (clip == null)
        {
            Debug.LogWarning($"[Audio] BGM tidak ditemukan: Audio/BGM/{resourceName}");
            bgmFadeCoroutine = null;
            yield break;
        }

        currentBgmName = clip.name;
        bgmSource.clip = clip;
        bgmSource.loop = true;

        float target = Mathf.Clamp(GetBGMVolume(clip.name), 0.0001f, 1f);
        ApplyRawBGMVolume(0.0001f);
        bgmSource.Play();

        if (fadeInDuration > 0f)
        {
            float t = 0f;

            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                ApplyRawBGMVolume(Mathf.Lerp(0.0001f, target, t / fadeInDuration));
                yield return null;
            }
        }

        ApplyRawBGMVolume(target);
        bgmFadeCoroutine = null;
    }

    private float GetBGMVolume(string bgmName)
    {
        if (bgmSettings != null)
            foreach (var s in bgmSettings)
                if (s != null && s.bgmName == bgmName)
                    return Mathf.Max(0f, s.volume);

        return currentBgmVolume;
    }

    private void ApplyBGMVolume(string bgmName)
    {
        ApplyRawBGMVolume(Mathf.Clamp(GetBGMVolume(bgmName), 0.0001f, 1f));
    }

    private void ApplyRawBGMVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        activeBgmVolume = volume;

        if (audioMixer != null) audioMixer.SetFloat(MIXER_BGM, Mathf.Log10(volume) * 20f);
        else if (bgmSource != null) bgmSource.volume = volume;
    }

    // Cancel semua state ducking + restore BGM ke nilai asli.
    private void CancelAllDucking()
    {
        CancelAksaraDucking();
        CancelSfxDucking();
    }

    private void CancelAksaraDucking()
    {
        if (restoreBGMCoroutine != null)
        {
            StopCoroutine(restoreBGMCoroutine);
            restoreBGMCoroutine = null;
        }

        preDuckBGMVolume = -1f;
    }

    private void CancelSfxDucking()
    {
        if (restoreSfxDuckCoroutine != null)
        {
            StopCoroutine(restoreSfxDuckCoroutine);
            restoreSfxDuckCoroutine = null;
        }

        if (preSfxDuckBGMVolume >= 0f)
        {
            ApplyRawBGMVolume(preSfxDuckBGMVolume);
            preSfxDuckBGMVolume = -1f;
        }
    }

    // Turunkan BGM sementara saat SFX main. Skip kalau aksara voice sedang duck.
    private void DuckBGMForSFX(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource == null || !bgmSource.isPlaying) return;
        if (currentBgmVolume <= 0.001f) return;
        if (sfxBgmVolume >= 1f) return;
        if (preDuckBGMVolume >= 0f) return;

        if (restoreSfxDuckCoroutine != null)
        {
            StopCoroutine(restoreSfxDuckCoroutine);
            restoreSfxDuckCoroutine = null;
        }

        if (preSfxDuckBGMVolume < 0f)
            preSfxDuckBGMVolume = activeBgmVolume;

        float ducked = Mathf.Clamp(preSfxDuckBGMVolume * Mathf.Clamp01(sfxBgmVolume), 0.0001f, 1f);
        ApplyRawBGMVolume(ducked);

        float duration = Mathf.Clamp(clip.length, sfxDuckMinDuration, sfxDuckMaxDuration);
        restoreSfxDuckCoroutine = StartCoroutine(RestoreSfxDuckAfter(duration));
    }

    private IEnumerator RestoreSfxDuckAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        if (preSfxDuckBGMVolume >= 0f)
        {
            ApplyRawBGMVolume(preSfxDuckBGMVolume);
            preSfxDuckBGMVolume = -1f;
        }

        restoreSfxDuckCoroutine = null;
    }

    public void PlayRewardBGM(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || bgmSource == null) return;

        CancelAllDucking();

        previousBGMVolume = activeBgmVolume;
        currentBgmName = null;
        PlayBGM(clip);
        ApplyRawBGMVolume(volume);
    }

    public void RestorePreviousBGM()
    {
        if (bgmSource == null) return;
        ApplyRawBGMVolume(previousBGMVolume);
    }

    private bool IsDuplicateSfx(AudioClip clip)
    {
        if (clip == null) return false;

        float now = Time.unscaledTime;

        if (lastSfxPlayTime.TryGetValue(clip, out float last))
            if (now - last < sfxDuplicateGuardWindow) return true;

        lastSfxPlayTime[clip] = now;
        return false;
    }

    public void PlaySFX(AudioClip clip) => PlaySFX(clip, 1f);

    public void PlaySFX(AudioClip clip, float volumeMultiplier)
    {
        if (clip == null || sfxSource == null) return;
        if (IsDuplicateSfx(clip)) return;

        StopHoverSFX();
        DuckBGMForSFX(clip);
        sfxSource.PlayOneShot(clip, Mathf.Clamp(volumeMultiplier, 0f, 2f));
    }

    public void PlayLoudSFX(AudioClip clip, float volumeMultiplier)
    {
        if (clip == null || sfxSource == null) return;
        if (IsDuplicateSfx(clip)) return;

        StopHoverSFX();
        DuckBGMForSFX(clip);
        sfxSource.PlayOneShot(clip, Mathf.Clamp(volumeMultiplier, 0f, 10f));
    }

    public void PlaySFX(string clipName) => PlaySFX(clipName, 1f);

    public void PlaySFX(string clipName, float volumeMultiplier)
    {
        if (string.IsNullOrEmpty(clipName)) return;
        PlaySFX(GetSFXClip(clipName), volumeMultiplier);
    }

    public void PlayCollectSFX(float volumeMultiplier = 1f) => PlaySFX("collect_item_cling", volumeMultiplier);
    public void PlayPowerUpSFX(float volumeMultiplier = 1f) => PlaySFX("powerup_katching", volumeMultiplier);

    // Slider tick: tanpa ducking BGM, tanpa cooldown UI besar.
    public void PlaySliderTickSFX(AudioClip clip, float volumeMultiplier = 0.5f)
    {
        if (clip == null || sfxSource == null) return;
        if (IsDuplicateSfx(clip)) return;

        // Sengaja tidak StopHoverSFX & tidak DuckBGMForSFX.
        sfxSource.PlayOneShot(clip, Mathf.Clamp(volumeMultiplier, 0f, 1f));
    }

    public void PlayLoopedSFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || loopedSfxSource == null) return;
        if (loopedSfxSource.isPlaying && loopedSfxSource.clip == clip) return;

        loopedSfxSource.clip = clip;
        loopedSfxSource.volume = currentSfxVolume * Mathf.Clamp(volumeMultiplier, 0f, 2f);
        loopedSfxSource.loop = true;
        loopedSfxSource.Play();
    }

    public void PlayLoopedSFX(string clipName, float volumeMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(clipName)) return;
        PlayLoopedSFX(GetSFXClip(clipName), volumeMultiplier);
    }

    public void StopLoopedSFX()
    {
        if (loopedSfxSource != null && loopedSfxSource.isPlaying) loopedSfxSource.Stop();
    }

    public void PlayAksaraVoice(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || aksaraVoiceSource == null) return;

        StopHoverSFX();

        // Cancel SFX ducking karena aksara voice prioritas.
        CancelSfxDucking();

        bool shouldDuck = bgmSource != null && bgmSource.isPlaying && currentBgmVolume > 0.001f;

        if (shouldDuck)
        {
            if (restoreBGMCoroutine != null) { StopCoroutine(restoreBGMCoroutine); restoreBGMCoroutine = null; }

            if (preDuckBGMVolume < 0f) preDuckBGMVolume = activeBgmVolume;

            float ducked = Mathf.Clamp(preDuckBGMVolume * Mathf.Clamp01(aksaraBgmVolume), 0.0001f, 1f);
            ApplyRawBGMVolume(ducked);
            restoreBGMCoroutine = StartCoroutine(RestoreBGMVolumeAfter(clip.length));
        }

        aksaraVoiceSource.PlayOneShot(clip, Mathf.Clamp01(volumeMultiplier));
    }

    private IEnumerator RestoreBGMVolumeAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        if (preDuckBGMVolume >= 0f) { ApplyRawBGMVolume(preDuckBGMVolume); preDuckBGMVolume = -1f; }

        restoreBGMCoroutine = null;
    }

    public void PlayHoverSFX(AudioClip clip) => PlayHoverSFX(clip, 1f);

    public void PlayHoverSFX(AudioClip clip, float volumeMultiplier)
    {
        if (clip == null || hoverSource == null) return;
        if (hoverSource.isPlaying) hoverSource.Stop();

        hoverSource.clip = clip;
        hoverSource.volume = currentSfxVolume * Mathf.Clamp01(volumeMultiplier);
        hoverSource.Play();
    }

    public void PlayHoverSFX(string clipName) => PlayHoverSFX(clipName, 1f);

    public void PlayHoverSFX(string clipName, float volumeMultiplier)
    {
        if (string.IsNullOrEmpty(clipName)) return;
        PlayHoverSFX(GetSFXClip(clipName), volumeMultiplier);
    }

    public void StopHoverSFX()
    {
        if (hoverSource != null && hoverSource.isPlaying) hoverSource.Stop();
    }

    public void PlayUISFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || uiSource == null) return;
        if (Time.unscaledTime - lastUISFXTime < uiSFXCooldown) return;

        lastUISFXTime = Time.unscaledTime;

        if (uiSource.isPlaying) uiSource.Stop();

        DuckBGMForSFX(clip);
        uiSource.volume = currentSfxVolume * Mathf.Clamp01(volumeMultiplier);
        uiSource.clip = clip;
        uiSource.Play();
    }

    public void PlayUISFX(string clipName, float volumeMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(clipName)) return;
        PlayUISFX(GetSFXClip(clipName), volumeMultiplier);
    }

    public void StopUISFX()
    {
        if (uiSource != null && uiSource.isPlaying) uiSource.Stop();
    }

    private AudioClip GetSFXClip(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return null;
        if (sfxCache.TryGetValue(clipName, out var cached)) return cached;

        var clip = Resources.Load<AudioClip>($"Audio/SFX/{clipName}");

        if (clip != null) sfxCache[clipName] = clip;
        else Debug.LogWarning($"[Audio] SFX tidak ditemukan: Audio/SFX/{clipName}");

        return clip;
    }

    public void SetBGMVolume(float sliderValue)
    {
        float value = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        currentBgmVolume = value;

        // Kalau sedang SFX ducking: update base, reapply ducking.
        if (preSfxDuckBGMVolume >= 0f)
        {
            preSfxDuckBGMVolume = value;
            float ducked = Mathf.Clamp(value * Mathf.Clamp01(sfxBgmVolume), 0.0001f, 1f);
            ApplyRawBGMVolume(ducked);
            return;
        }

        // Kalau sedang aksara ducking: update base, reapply ducking.
        if (preDuckBGMVolume >= 0f)
        {
            preDuckBGMVolume = value;
            float ducked = Mathf.Clamp(value * Mathf.Clamp01(aksaraBgmVolume), 0.0001f, 1f);
            ApplyRawBGMVolume(ducked);
            return;
        }

        // Normal.
        activeBgmVolume = value;
        ApplyRawBGMVolume(value);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float value = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        currentSfxVolume = value;

        if (audioMixer != null) { audioMixer.SetFloat(MIXER_SFX, Mathf.Log10(value) * 20f); return; }

        if (sfxSource != null) sfxSource.volume = value;
        if (hoverSource != null) hoverSource.volume = value;
        if (uiSource != null) uiSource.volume = value;
        if (loopedSfxSource != null) loopedSfxSource.volume = value;
    }

    public float GetCurrentBGMVolume() => currentBgmVolume;
    public float GetCurrentSFXVolume() => currentSfxVolume;
}