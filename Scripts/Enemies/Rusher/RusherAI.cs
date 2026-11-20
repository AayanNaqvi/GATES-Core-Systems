using UnityEngine;
using Pathfinding;
using System.Collections;



[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class RusherAI : MonoBehaviour, IKnockbackable, IBuffed
{
    [Header("References")]
    public Transform player;
    public Animator rusherAnimator;
    public SpriteRenderer spriteRender;
    public GameObject shadow;
    private Enemy enemyScript;


    [Header("Movement")]
    [SerializeField, Range(0.5f, 10f)] private float speed = 4f;
    private float ogSpeed;
    [SerializeField, Range(0.1f, 3f)] public float nextWaypointDistance = 0.5f;
    [SerializeField] public float reCalcPathTime;
    [SerializeField] private float stopDistance;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float chargeTime;
    public bool beStunnedWhenSpawning;
    public bool isDashing;
    public bool isCharging;
    private Vector2 dashDirection;
    
    [SerializeField] private float spawnStunTime;
    public float spawnStunTimer;
    public bool isSpawnStun = true;
    public bool stunlock = false;
    

    [Header("Line of Sight")]
    public LayerMask obstacleLayer;
    public float rushWidth = 0.11f;
    public Transform topPoint;
    public Transform bottomPoint;
    public bool alpha = false;


    //References
    private Seeker seeker;
    private Rigidbody2D rb;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;

    private bool isKnockedBack = false;
    private bool isStunned = false;
    private float knockbackTimer = 0f;
    private float stunTimer = 0f;

    public float knockbackDuration = 0.15f; 
    public float stunDuration = 0.3f;
    private Vector2 knockbackVelocity;

    [Header("Effects")]
    public GameObject afterImagePrefab;
    public float timeBetweenGhosts; 
    public Color ghostColor = new Color(1f, 1f, 1f, 0.7f);
    private float lastGhostTime;
    public GameObject chargeParticle;
    public GameObject rusherImpact;


    [Header("SFX")]
    public AudioClip[] footsteps;
    public AudioClip chargeSound;
    public AudioClip dashSound;
    public AudioClip impactSound;
    public float chargeVol = 0.3f;
    public float footStepVol;
    public float footStepPitch;
    public float walkInterval;
    private float footstepTimer = 0f;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void StunLock()
    {
        stunlock = true;
    }
    public void StunUnlock()
    {
        stunlock = false;
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (rusherAnimator == null)
            rusherAnimator = GetComponent<Animator>();
        if (spriteRender == null)
            spriteRender = GetComponent<SpriteRenderer>();
        if (enemyScript == null)
            enemyScript = GetComponent<Enemy>();

        ogSpeed = speed;
        
        
        InvokeRepeating(nameof(UpdatePath), 0f, reCalcPathTime); 
        
    }

    


    public void KnockbackAndStun(Vector2 force, float stunTime)
    {
        knockbackVelocity = force;
        knockbackTimer = knockbackDuration;
        isKnockedBack = true;

        stunTimer = stunTime;
        isStunned = true;
        rusherAnimator.SetBool("IsStunned", true);
        rusherAnimator.SetBool("Walking", false);
    }

    public void ApplySpeedBuff(float multipler)
    {
        speed *= multipler;
    }
    public void RemoveSpeedBuff()
    {
        speed = ogSpeed;
    }

    private void UpdatePath()
    {
        if ((beStunnedWhenSpawning && isSpawnStun) || isStunned || enemyScript.dead) return; 
        if (seeker.IsDone())
            seeker.StartPath(rb.position, player.position, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void SpawnTimer()
    {
        spawnStunTimer += Time.deltaTime * 1f;

        if(spawnStunTimer >= spawnStunTime)
        {
            isSpawnStun = false;
            rusherAnimator.SetBool("Walking", false);
        }
    }
    private void FixedUpdate()
    {
        PlayFootStepSound();

        if (beStunnedWhenSpawning && isSpawnStun)
        {
            rusherAnimator.SetBool("Walking", false);
            SpawnTimer();
        }


        if (stunlock)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isCharging)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {

            rb.linearVelocity = dashDirection * dashSpeed;


            if (Time.time >= lastGhostTime + timeBetweenGhosts)
            {
                
                GameObject ghost = Instantiate(afterImagePrefab, transform.position, Quaternion.identity);

                
                AfterImage aftI = ghost.GetComponent<AfterImage>();
                if (aftI != null)
                {
                    
                    aftI.Setup(spriteRender.sprite, transform, ghostColor);
                }

                
                lastGhostTime = Time.time;
            }



            return; 
        }

        if (isKnockedBack)
        {

            float t = 1f - (knockbackTimer / knockbackDuration);

            t = 1f - Mathf.Pow(1f - t, 3f);

            rb.linearVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, t);

            knockbackTimer -= Time.fixedDeltaTime;

            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.linearVelocity = Vector2.zero; 
            }
            return;
        }

        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f)
            {
                rusherAnimator.SetBool("IsStunned", false);

                isStunned = false;
                UpdatePath();
            }
            return;
        }

        if (path == null) return;


        float distToPlayer = Vector2.Distance(rb.position, player.position);
        bool canSeePlayer = HasLineOfSight();

        if (distToPlayer <= stopDistance && canSeePlayer)
        {
            if (!isCharging && !isDashing)
            {
                if (player.position.x < transform.position.x)
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                else
                    transform.rotation = Quaternion.Euler(0, 180, 0);

                StartCoroutine(RushAttack());
            }
        }
        else
        {

            if (path == null || currentWaypoint >= path.vectorPath.Count) return;

            rusherAnimator.SetBool("Walking", true);

            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            rb.linearVelocity = direction * speed;

            if (Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
            {
                currentWaypoint++;
            }

            if (rb.linearVelocity.x < -0.1f)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (rb.linearVelocity.x > 0.1f)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }


    }

    private IEnumerator RushAttack()
    {
        isCharging = true;

        rb.linearVelocity = Vector2.zero;
        rusherAnimator.SetBool("IsCharging", true);
        AudioManager.Instance.Play3D(chargeSound, transform.position, chargeVol);
        GameObject chargeParticleInst = Instantiate(chargeParticle, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        dashDirection = (player.position - transform.position).normalized;

        yield return new WaitForSeconds(chargeTime);
        rusherAnimator.SetBool("IsCharging", false);
        if(alpha)
        {
            dashDirection = (player.position - transform.position).normalized;
        }

        isCharging = false;
        isDashing = true;
        Destroy(chargeParticleInst);
        rusherAnimator.SetBool("Dashing", true);
        AudioManager.Instance.Play2D(dashSound, chargeVol);

    }

    public void PlayFootStepSound()
    {
        if (!isStunned && !isDashing && !isCharging)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (footsteps != null && footsteps.Length > 0)
                {
                    int randomIndex = Random.Range(0, footsteps.Length);
                    AudioManager.Instance.Play3D(footsteps[randomIndex], transform.position, footStepVol, footStepPitch);
                }
                footstepTimer = walkInterval / rusherAnimator.speed;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D topHit = Physics2D.Raycast(topPoint.position, direction, distanceToPlayer, obstacleLayer);
        RaycastHit2D botHit = Physics2D.Raycast(bottomPoint.position, direction, distanceToPlayer, obstacleLayer);

        Debug.DrawRay(topPoint.position, direction * distanceToPlayer, topHit ? Color.red : Color.green);
        Debug.DrawRay(bottomPoint.position, direction * distanceToPlayer, botHit ? Color.red : Color.green);

        if (topHit.collider != null || botHit.collider != null)
        {
            return false;
        }

        return true;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDashCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleDashCollision(collision);
    }

    private void HandleDashCollision(Collision2D collision)
    {
        if (!isDashing) return;

        
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<Enemy>() != null)
        {
            StopDash();
        }
        
        else if (collision.gameObject.CompareTag("Wall"))
        {

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector2 wallNormal = collision.GetContact(i).normal;
                float crashAngle = Vector2.Dot(dashDirection, wallNormal);

                
                if (crashAngle < -0.5f)
                {

                    AudioManager.Instance.Play3D(impactSound, transform.position, chargeVol);
                    GameObject impactEffect = Instantiate(rusherImpact, transform.position, Quaternion.identity);
                    Destroy(impactEffect, 0.8f);
                    StopDash();
                    return; 
                }
            }
        }
    }

    public void StopDash()
    {
        isDashing = false;
        rusherAnimator.SetBool("Dashing", false);
        rb.linearVelocity = Vector2.zero;
    }


    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stopDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, rushWidth);

            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}

