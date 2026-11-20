using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UpgradeWheelAnimator : MonoBehaviour
{
    public enum UpgradeType { Sword, Staff, Health}
    public UpgradeType typeOfUpgrade;

    public Sprite[] sprites;
    private Image img;
    public int stage = 0;

    public Button button;

    public GameObject player;
    private UpgradeStuff us;

    public InputActionReference buyInputReference;
    private InputAction buyInput;

    private void Start()
    {
        img = GetComponent<Image>();
        us = player.GetComponent<UpgradeStuff>();
        buyInput = buyInputReference.ToInputAction();
    }

    public void AttemptUpgrade()
    {
        if (stage >= 9) return;

        bool upgradeSuccessful = false;

        if (typeOfUpgrade == UpgradeType.Sword)
        {
            upgradeSuccessful = us.UpgradeSword();
        }
        else if (typeOfUpgrade == UpgradeType.Staff)
        {
            upgradeSuccessful = us.UpgradeStaff();
        }
        else if(typeOfUpgrade == UpgradeType.Health)
        {
            upgradeSuccessful = us.UpgradeHealth();
        }

        if (upgradeSuccessful)
        {
            stage++;
            img.sprite = sprites[stage];
        }
    }


    private void Update()
    {
        if (stage >= 9)
        {
            button.interactable = false;
            return;
        }

        if (typeOfUpgrade == UpgradeType.Sword)
        {
            button.interactable = us.CanAffordSword();
        }
        else if (typeOfUpgrade == UpgradeType.Staff)
        {
            button.interactable = us.CanAffordStaff();
        }
        else if(typeOfUpgrade == UpgradeType.Health)
        {
            button.interactable = us.CanAffordHealth();
        }

        MyInput();

    }

    public void MyInput()
    {
        if(buyInputReference != null && buyInput.WasPressedThisFrame())
        {
            if (button.interactable)
            {
                AttemptUpgrade();
            }
        }
    }

    public void ClearStuff()
    {
        stage = 0;
        if (img == null)
        {
            img = GetComponentInChildren<Image>(true);
        }
        if (img == null) return;

        if (sprites == null || sprites.Length == 0) return;

        img.sprite = sprites[0];
    }

}
