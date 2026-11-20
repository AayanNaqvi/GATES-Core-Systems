using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SwordCombat : MonoBehaviour
{
    [Header("Attack Logic")]
    [SerializeField, Range(0.1f, 5f)] private float attackSpeed = 0.5f;
    private float ogatkSpd;
    [HideInInspector] public float ogatkdmg;
    public float attackDamage = 50f;
    [SerializeField] private float attackRadius = 2.5f;
    [SerializeField, Range(0.1f, 15f)] private float attackKnockBack = 4f;
    [SerializeField, Range(0.1f, 3f)] private float attackStun = 0.5f;
    [SerializeField, Range(1f, 180f)] private float attackArcAngle;
    public LayerMask whatIsEnemy;
    public ElementType type; 



    [Header("Effects")]
    [SerializeField] private GameObject slashEffect;
    private Transform weaponHolder;
    [SerializeField] private float hitStopDur;

    [Header("Sound")]
    public AudioClip[] attackSounds;
    public float attackVolume;

    [Header("References")]
    private PlayerInput playerInput;
    private InputAction attackButton;

    [Header("Buffs")]
    private Coroutine activeDMGBoost;
    public GameObject dmgSlider;
    private GameObject currDMGSlider;
    public Transform sliderContainer;
    public bool attackBoosted = false;



    private float attackTimer; 
    private bool attackHeld;   

    void Start()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        weaponHolder = GetComponent<Transform>();
        

        attackButton = playerInput.actions["Attack"]; 

        
        attackButton.performed += ctx => attackHeld = true;
        attackButton.canceled += ctx => attackHeld = false;

        ogatkdmg = attackDamage;
        ogatkSpd = attackSpeed;
    }

   

    void FixedUpdate()
    {
        attackTimer -= Time.fixedDeltaTime;

        if (attackHeld && attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackSpeed; 
        }
    }

    void Attack()
    {
        Vector2 playerCenter = playerInput.transform.position;
        Vector2 attackDirection = weaponHolder.up;

        AudioManager.Instance.Play2D(attackSounds[Random.Range(0,3)], attackVolume);

        Collider2D[] hits = Physics2D.OverlapCircleAll(playerCenter, attackRadius, whatIsEnemy);
        

        foreach (Collider2D hit in hits)
        {
            Vector2 closestEdge = hit.ClosestPoint(playerCenter);

            Vector2 directionToTarget = (closestEdge - playerCenter).normalized;

            float angle = Vector2.Angle(attackDirection, directionToTarget);
            float distance = Vector2.Distance(playerCenter, closestEdge);

            if (distance < 0.5f || angle <= attackArcAngle / 2f) 
            {
                Enemy e = hit.GetComponent<Enemy>();
                IKnockbackable knockableAI = hit.GetComponent<IKnockbackable>();
                TorchBreak tor = hit.GetComponent<TorchBreak>();
                if (e != null)
                {
                    
                    float mult = ElementChart.GetMultiplier(type, e.currType);
                    Vector2 knockDir = (e.transform.position - transform.position).normalized;

                    
                    if (knockableAI != null)
                    {
                        knockableAI.KnockbackAndStun(knockDir * attackKnockBack, attackStun);
                    }



                    e.TakeDamage(attackDamage * mult);
                }

                if (tor != null && !tor.broken)
                {
                    tor.TorchBreaker();
                }

            }
        }


        

        #region effects
        GameObject slash = Instantiate(slashEffect, weaponHolder.position, weaponHolder.rotation);
        SlashFollow sf = slash.GetComponent<SlashFollow>();
        if (sf == null) sf = slash.AddComponent<SlashFollow>();
        sf.followTarget = weaponHolder;
        Destroy(slash, 0.5f);
        #endregion


        
    }


    public void ApplyAttackBoost(float boostDMG, float boostSpd, float duration)
    {
        

        
        if (activeDMGBoost != null)
        {
            StopCoroutine(activeDMGBoost);
        }

        if (currDMGSlider != null) Destroy(currDMGSlider);

        if (sliderContainer != null && dmgSlider != null)
        {
            currDMGSlider = Instantiate(dmgSlider, sliderContainer, false);

            RectTransform rt = currDMGSlider.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;

                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
            }

            BuffBars bb = currDMGSlider.GetComponent<BuffBars>();
            if (bb != null)
            {
                bb.Initialize(duration);
            }
        }

        
        activeDMGBoost = StartCoroutine(AttackBoostRoutine(boostDMG, boostSpd, duration));
    }

    private IEnumerator AttackBoostRoutine(float boostDMG, float boostSpd, float duration)
    {
        
        if(!attackBoosted)
        {
            attackDamage *= boostDMG;
            attackSpeed *= boostSpd;
            attackBoosted = true;
        }
        

        yield return new WaitForSeconds(duration);

        attackDamage = ogatkdmg;
        attackSpeed = ogatkSpd;
        attackBoosted = false;

        activeDMGBoost = null;
    }

    public void ResetDamageAndBuffs()
    {
        if (activeDMGBoost != null)
        {
            StopCoroutine(activeDMGBoost);
        }


        attackDamage = ogatkdmg;
        attackSpeed = ogatkSpd;
        attackBoosted = false;

        activeDMGBoost = null;
    }


    public void UpgradeBaseStats(float newBaseDamage)
    {
        float currentMultiplier = attackBoosted ? (attackDamage / ogatkdmg) : 1f;

        ogatkdmg = newBaseDamage;

        attackDamage = ogatkdmg * currentMultiplier;
    }
}