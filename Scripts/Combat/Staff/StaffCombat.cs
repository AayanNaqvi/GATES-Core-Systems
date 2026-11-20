using UnityEngine;
using UnityEngine.InputSystem;

public class StaffCombat : MonoBehaviour
{

    [Header("Attack Logic")]
    public ElementType staffElementType;
    [Range(0.1f, 5f)] public float fireRate = 0.5f;

    private bool attackHeld;
    private float attackTimer;

    [Header("References")]
    public GameObject projectilePrefab;
    private Transform firePoint;
    private PlayerInput playerInput;
    private InputAction attackButton;

    [Header("Sound Effects")]
    public AudioClip[] fireSounds;
    public float fireVol;


    private void Start()
    {
        firePoint = gameObject.transform;
        playerInput = GetComponentInParent<PlayerInput>();

        attackButton = playerInput.actions["Attack"];

        attackButton.performed += ctx => attackHeld = true;
        attackButton.canceled += ctx => attackHeld = false;
    }


    void FixedUpdate()
    {
        attackTimer -= Time.fixedDeltaTime;

        if (attackHeld && attackTimer <= 0f)
        {
            Fire();
            attackTimer = fireRate;
        }
    }

    void Fire()
    {
        Vector2 direction = firePoint.up; 
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        int randInt = Random.Range(0, fireSounds.Length);
        AudioManager.Instance.Play2D(fireSounds[randInt], fireVol);

        StaffProjectile projScript = proj.GetComponent<StaffProjectile>();
        projScript.Initialize(direction, staffElementType);
    }

    
}
