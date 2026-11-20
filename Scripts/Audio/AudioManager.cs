using UnityEngine;
using UnityEngine.Audio;
using System.Collections; 

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource source2D;

    [Header("Audio Mixer Routing")]
    public AudioMixer mainMixer;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup musicGroup;

    public AudioSource musicSource;

    private Coroutine currentFadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (mainMixer != null)
        {
            float safeSFX = Mathf.Clamp(PlayerPrefs.GetFloat("SFXVolume", 0.5f), 0.0001f, 1f);
            float safeMusic = Mathf.Clamp(PlayerPrefs.GetFloat("MusicVolume", 0.5f), 0.0001f, 1f);

            mainMixer.SetFloat("SFXVol", Mathf.Log10(safeSFX) * 20);
            mainMixer.SetFloat("MusicVol", Mathf.Log10(safeMusic) * 20);
        }
    }

    public void Play2D(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        GameObject tempAudioObj = new GameObject("TempAudio2D_" + clip.name);
        AudioSource audioSource = tempAudioObj.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxGroup;

        audioSource.clip = clip;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        audioSource.Play();
        Destroy(tempAudioObj, clip.length / Mathf.Abs(pitch));
    }

    public void Play3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        GameObject tempAudioObj = new GameObject("TempAudio_" + clip.name);
        float cameraZ = Camera.main.transform.position.z;
        tempAudioObj.transform.position = new Vector3(position.x, position.y, cameraZ);

        AudioSource audioSource = tempAudioObj.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxGroup;

        audioSource.clip = clip;
        audioSource.spatialBlend = 1f;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 15f;

        audioSource.Play();
        Destroy(tempAudioObj, clip.length / Mathf.Abs(pitch));
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource == null) return;

        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
        }

        musicSource.volume = 1f;

        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void FadeOutMusic(float fadeDuration = 1.5f)
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            if (currentFadeRoutine != null)
            {
                StopCoroutine(currentFadeRoutine);
            }

            currentFadeRoutine = StartCoroutine(FadeOutRoutine(fadeDuration));
        }
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = musicSource.volume;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        musicSource.Stop();

        musicSource.volume = 1f;
        currentFadeRoutine = null;
    }

    public void UpdateSFXVolume(float val)
    {
        if (mainMixer != null) mainMixer.SetFloat("SFXVol", Mathf.Log10(val) * 20);
    }

    public void UpdateMusicVolume(float val)
    {
        if (mainMixer != null) mainMixer.SetFloat("MusicVol", Mathf.Log10(val) * 20);
    }
}