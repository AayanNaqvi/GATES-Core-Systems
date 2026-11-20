using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;

    [Header("Weapon")]
    public Transform weaponHolder;

    [Header("Animation")]
    public Animator animator;
    private string currentState;
    public float boostAnimSpeed;
    private float ogAnimSpeed;

    [Header("SFX")]
    public AudioClip[] footsteps;
    public float volume;
    public float walkInterval;
    private bool walking = false;
    private float footstepTimer = 0f;


    [Header("Movement")]
    [Range(0.1f, 30f)] public float moveSpeed;
    [HideInInspector] public float ogMoveSpeed;
    public Vector2 moveDirection;
    public bool isKnockedBack = false;
    public Vector2 lastMoveDir; 

    [Header("Wind Tablet")]
    public GameObject windParticle;
    

    [Header("Buffs")]
    private Coroutine activeSpeedBoost;
    public GameObject speedSlider;
    private GameObject currSpeedSlider;
    public Transform sliderContainer;

    [Header("Debug")]
    [SerializeField] private InputActionMap actionMap;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        ogMoveSpeed = moveSpeed;
        ogAnimSpeed = animator.speed;
        
        moveAction = playerInput.actions["Move"];
    }

    private void FixedUpdate()
    {
        Move();
        actionMap = playerInput.currentActionMap;
    }

    

    

    

    public void ApplySpeedBoost(float boostSpeed, float duration)
    {
        if (activeSpeedBoost != null)
        {
            StopCoroutine(activeSpeedBoost);
        }

        if (currSpeedSlider != null) Destroy(currSpeedSlider);

        if (sliderContainer != null && speedSlider != null)
        {
            currSpeedSlider = Instantiate(speedSlider, sliderContainer, false);

            RectTransform rt = currSpeedSlider.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;

                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
            }

            BuffBars bb = currSpeedSlider.GetComponent<BuffBars>();
            if (bb != null)
            {
                bb.Initialize(duration);
            }
        }


        activeSpeedBoost = StartCoroutine(SpeedBoostRoutine(boostSpeed, duration));
    }

    private IEnumerator SpeedBoostRoutine(float boostSpeed, float duration)
    {
        moveSpeed = boostSpeed;
        animator.speed = boostAnimSpeed;

        yield return new WaitForSeconds(duration);

        moveSpeed = ogMoveSpeed;
        animator.speed = ogAnimSpeed;
        activeSpeedBoost = null;

        
    }
    
    public void ClearSpeedBoost()
    {
        moveSpeed = ogMoveSpeed;
        animator.speed = ogAnimSpeed;
        if (activeSpeedBoost != null)
        {
            StopCoroutine(activeSpeedBoost);
            activeSpeedBoost = null;
        }
    }
    void Update()
    {
        MyInput();
        UpdateAnimation();
        UpdateWalking();
        PlayFootStepSound();

    }

    public void UpdateWalking()
    {
        if(rb.linearVelocity.magnitude >= 0.1f)
        {
            walking = true;
        }
        else walking = false;
    }

    public void PlayFootStepSound()
    {
        if (walking)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (footsteps != null && footsteps.Length > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, footsteps.Length);
                    AudioManager.Instance.Play2D(footsteps[randomIndex], volume);
                }
                footstepTimer = walkInterval / animator.speed;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    public void UpdateAnimation()
    {
        if (moveDirection == Vector2.zero || rb.linearVelocity.magnitude <= 0.1f)
        {
            ChangeAnimationState("Idle");
            return;
        }
        

        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
        {
            
            
            if (moveDirection.x > 0)
                ChangeAnimationState("WalkRight");
            else
                ChangeAnimationState("WalkLeft");
        }
        else
        {
            
            if (moveDirection.y > 0)
                ChangeAnimationState("WalkUp");
            else
                ChangeAnimationState("WalkDown");
        }
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);

        currentState = newState;
    }

    void MyInput()
    {
        Vector2 input = new Vector2(0,0);  


        
        input = moveAction.ReadValue<Vector2>();
        
        
        moveDirection = input;

        
        if (input != Vector2.zero)
            lastMoveDir = input;
    }

    void Move()
    {
        if (isKnockedBack) return;
        
        rb.linearVelocity = Vector2.ClampMagnitude(moveDirection, 1f) * moveSpeed;

    }

    
}