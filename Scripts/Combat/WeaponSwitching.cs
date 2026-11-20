using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponSwitching : MonoBehaviour
{
    [Header("Both Weapon Anchors")]
    [SerializeField] private GameObject swordAnchor;
    [SerializeField] private GameObject staffAnchor;

    private PlayerInput playerInput;
    private InputAction switchWeapon;
    private bool swordEquipped = true;


    [Header("UI")]
    public Image weaponImage;
    public Sprite sword;
    public Sprite staff;


    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();

        switchWeapon = playerInput.actions["SwitchWeapon"];
    }

    private void OnEnable()
    {
        switchWeapon.performed += OnSwitchWeapon;
    }

    private void OnDisable()
    {
        switchWeapon.performed -= OnSwitchWeapon;
    }

    private void Start()
    {
        EquipSword();
    }

    private void OnSwitchWeapon(InputAction.CallbackContext ctx)
    {
        if (swordEquipped)
            EquipStaff();
        else
            EquipSword();
    }

    private void EquipSword()
    {
        swordAnchor.SetActive(true);
        staffAnchor.SetActive(false);
        swordEquipped = true;
        weaponImage.enabled = true;
        weaponImage.sprite = sword;
    }

    private void EquipStaff()
    {
        swordAnchor.SetActive(false);
        staffAnchor.SetActive(true);
        swordEquipped = false;
        weaponImage.enabled = true;
        weaponImage.sprite = staff;
    }
}