using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SettingsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;


    private void OnEnable()
    {
        float safeMusic = Mathf.Clamp(PlayerPrefs.GetFloat("MusicVolume", 0.5f), 0.0001f, 1f);
        float safeSFX = Mathf.Clamp(PlayerPrefs.GetFloat("SFXVolume", 0.5f), 0.0001f, 1f);

        if (musicSlider != null) musicSlider.value = safeMusic;
        if (sfxSlider != null) sfxSlider.value = safeSFX;
    }
    void Start()
    {
        float safeMusic = Mathf.Clamp(PlayerPrefs.GetFloat("MusicVolume", 0.5f), 0.0001f, 1f);
        float safeSFX = Mathf.Clamp(PlayerPrefs.GetFloat("SFXVolume", 0.5f), 0.0001f, 1f);
        
        if (musicSlider != null)
        {
            musicSlider.minValue = 0.0001f;
            musicSlider.maxValue = 1f;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        

        

    }

    public void SetMusicVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateMusicVolume(sliderValue);
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateSFXVolume(sliderValue);
        }
    }
}
