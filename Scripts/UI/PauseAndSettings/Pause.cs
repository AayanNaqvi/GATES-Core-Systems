using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
public class Pause : MonoBehaviour
{
    [Header("References")]
    public GameObject pauseMenuMain;
    public GameObject pauseMenuMainStuff;
    public GameObject outline;
    public GameObject settingsOutline;
    public GameObject settingsMenu;
    public GameObject firstPauseButton;
    public GameObject firstSettingsButton;

    private PlayerInput pInp;
    private Inventory inventory;
    private InputAction pauseButton;
    private InputAction unpauseButton;
    private InputAction exitOut;

    [Header("UI Settings")]
    public Vector2 outlinePadding = new Vector2(20f, 20f);
    public Vector2 sliderHandleSize = new Vector2(40f, 40f);

    [Header("Audio")]
    public AudioClip pauseSound;
    public float pauseVol = 0.3f;

    private bool paused = false;
    private bool settingsOpen = false;




    void Start()
    {
        pInp = GetComponent<PlayerInput>();
        inventory = GetComponent<Inventory>();
        if(pInp != null)
        {
            pauseButton = pInp.actions.FindAction("Pause");
            unpauseButton = pInp.actions.FindAction("Unpause");
            exitOut = pInp.actions.FindAction("ExitOut");
        }
    }

    
    void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        if (pauseButton.WasPressedThisFrame())
        {
            PauseGame();
        }
        if (unpauseButton.WasPressedThisFrame() && !inventory.inventoryOpen)
        {
            UnPauseGame();
        }


        if(exitOut.WasPressedThisFrame() && !inventory.inventoryOpen)
        {
            if(paused && !settingsOpen)
            {
                UnPauseGame();
            }
            else if(paused && settingsOpen)
            {
                CloseSettings();
            }
        }

        bool gamepad = IsGamepad();
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        outline.SetActive(false);
        settingsOutline.SetActive(false);

        if (paused && gamepad && selectedObj != null)
        {
            RectTransform selectedRect = selectedObj.GetComponent<RectTransform>();

            if (selectedRect != null)
            {
                if (!settingsOpen)
                {
                    outline.SetActive(true);
                    outline.transform.position = selectedRect.position;
                }
                else
                {
                    settingsOutline.SetActive(true);

                    if (selectedObj.GetComponent<Slider>() != null)
                    {
                        Transform handle = selectedObj.transform.Find("Handle Slide Area/Handle");

                        if (handle != null)
                        {
                            RectTransform handleRect = handle.GetComponent<RectTransform>();
                            settingsOutline.transform.position = handleRect.position;

                            RectTransform outlineRect = settingsOutline.GetComponent<RectTransform>();
                            if (outlineRect != null)
                            {
                                outlineRect.sizeDelta = sliderHandleSize;
                            }
                        }
                    }
                    else
                    {
                        settingsOutline.transform.position = selectedRect.position;

                        RectTransform outlineRect = settingsOutline.GetComponent<RectTransform>();
                        if (outlineRect != null)
                        {
                            outlineRect.sizeDelta = selectedRect.rect.size + outlinePadding;
                        }
                    }
                }
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pInp.SwitchCurrentActionMap("UI");

        if (inventory.inventoryOpen) inventory.ToggleInventory(false);

        PlaySound();

        paused = true;
        pauseMenuMain.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPauseButton);
    }
    public void UnPauseGame()
    {
        if(settingsOpen)
        {
            CloseSettings();
            PlaySound();
            return;
        }
        
        
        
        Time.timeScale = 1f;
        
        pInp.SwitchCurrentActionMap("Player");
        PlaySound();

        EventSystem.current.SetSelectedGameObject(null);

        paused = false;
        pauseMenuMain.SetActive(false);
    }


    private bool IsGamepad()
    {
        return pInp.currentControlScheme == "Gamepad";
    }

    public void OpenSettings()
    {
        pauseMenuMainStuff.SetActive(false);
        settingsMenu.SetActive(true);
        settingsOpen = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSettingsButton);
    }
    public void CloseSettings()
    {
        pauseMenuMainStuff.SetActive(true);
        settingsMenu.SetActive(false);
        settingsOpen = false;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPauseButton);
    }

    public void PlaySound()
    {
        AudioManager.Instance.Play2D(pauseSound, pauseVol);
    }

    public void TempQuit()
    {
        Application.Quit();
    }
}
