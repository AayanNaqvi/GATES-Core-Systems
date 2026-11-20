using UnityEngine;
using UnityEngine.InputSystem;

public class AimControl : MonoBehaviour
{

    [Header("References")]
    public Camera cam;
    private PlayerInput playerInput;
    private InputAction lookAction;

    public Vector3 lookDirection;

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
    }

    void Update()
    {
        Vector2 rawInput = lookAction.ReadValue<Vector2>();

        if (playerInput.currentControlScheme == "Gamepad")
        {
            if (rawInput.sqrMagnitude > 0.01f)
            {
                lookDirection = new Vector3(rawInput.x, rawInput.y, 0f).normalized;
            }
        }
        else
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(rawInput.x, rawInput.y, cam.nearClipPlane));

            lookDirection = (mouseWorldPos - transform.position).normalized;
            lookDirection.z = 0f; 
        }

        if (lookDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

}
