using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public class TitleMenuSettingsAndControls : MonoBehaviour
{
    [Header("Input & Outlines")]
    public PlayerInput pInp;

    public GameObject mainOutline;
    public GameObject settingsOutline;

    public Vector2 outlinePadding = new Vector2(20f, 20f);
    public Vector2 settingsOutlinePadding = new Vector2(40f, 40f);
    public Vector2 sliderHandleSize = new Vector2(40f, 40f);

    [Header("Menu Containers")]
    public GameObject mainMenuContainer;
    public GameObject settingsMenuContainer;

    [Header("First Selected Elements")]
    public GameObject firstMainButton;
    public GameObject firstSettingsItem;

    private bool isSettingsOpen = false;

    public TextMeshProUGUI maxRunText;



    public Slider sfxSlider;
    public Slider musicSlider;

    [Header("Music")]
    public AudioClip titleScreenMusic;




    private void Start()
    {

        sfxSlider.minValue = 0.0001f;
        sfxSlider.maxValue = 1f;

        musicSlider.minValue = 0.0001f;
        musicSlider.maxValue = 1f;

        float rawSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        float safeSFX = Mathf.Clamp(rawSFX, 0.0001f, 1f);

        float rawMusic = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float safeMusic = Mathf.Clamp(rawMusic, 0.0001f, 1f);

        
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        sfxSlider.value = safeSFX;
        musicSlider.value = safeMusic;

        SetSFXVolume(safeSFX);
        SetMusicVolume(safeMusic);

        int maxStage = PlayerPrefs.GetInt("MaxRunStage", 1);

        if (maxRunText != null)
        {
            maxRunText.text = "Farthest Run: Stage " + maxStage;
        }

        CloseSettings();


        if (AudioManager.Instance != null && titleScreenMusic != null)
        {
            AudioManager.Instance.PlayMusic(titleScreenMusic);
        }

    }

    void Update()
    {
        bool isGamepad = pInp != null && pInp.currentControlScheme == "Gamepad";
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        if (mainOutline != null) mainOutline.SetActive(false);
        if (settingsOutline != null) settingsOutline.SetActive(false);

        if (isGamepad && selectedObj != null)
        {
            GameObject activeOutline = isSettingsOpen ? settingsOutline : mainOutline;

            if (activeOutline != null)
            {
                activeOutline.SetActive(true);
                RectTransform outlineRect = activeOutline.GetComponent<RectTransform>();

                if (selectedObj.GetComponent<Slider>() != null)
                {
                    Transform handle = selectedObj.transform.Find("Handle Slide Area/Handle");

                    if (handle != null && outlineRect != null)
                    {
                        RectTransform handleRect = handle.GetComponent<RectTransform>();

                        activeOutline.transform.position = handleRect.position;
                        outlineRect.sizeDelta = sliderHandleSize;
                    }
                }
                else
                {
                    
                    
                    RectTransform selectedRect = selectedObj.GetComponent<RectTransform>();
                    
                    if (selectedRect != null && outlineRect != null)
                    {
                        if (isSettingsOpen)
                        {
                            activeOutline.transform.position = selectedRect.position;
                            outlineRect.sizeDelta = selectedRect.rect.size + settingsOutlinePadding;
                        }
                        else
                        {
                            activeOutline.transform.position = selectedRect.position;
                            outlineRect.sizeDelta = selectedRect.rect.size + outlinePadding;
                        }
                            
                    }
                }
            }
        }
    }

    public void OpenSettings()
    {
        isSettingsOpen = true;

        mainMenuContainer.SetActive(false);
        settingsMenuContainer.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSettingsItem);
    }

    public void CloseSettings()
    {
        isSettingsOpen = false; 

        settingsMenuContainer.SetActive(false);
        mainMenuContainer.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainButton);
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

    public void SetMusicVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateMusicVolume(sliderValue);
        }
    }

    public void MakeOthersUnInteract(Button b)
    {
        b.interactable = false;
    }
}