using UnityEngine;
using Pathfinding;
using JetBrains.Annotations;



[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class ArcherAI : MonoBehaviour, IKnockbackable, IBuffed
{
    [Header("References")]
    public Transform player;
    public Animator archerAnimator;
    public SpriteRenderer spriteRender;
    public GameObject shadow;
    private Enemy enemyScript;
    private ArcherShootingAndDetection aSAD;

    [Header("Movement")]
    [SerializeField, Range(0.5f, 10f)] private float speed = 4f;
    private float ogSpeed;
    [SerializeField, Range(0.1f, 3f)] public float nextWaypointDistance = 0.5f;
    [SerializeField] public float reCalcPathTime;
    [SerializeField] private float stopDistance;
    public bool beStunnedWhenSpawning;
    [SerializeField] private float spawnStunTime;
    public float spawnStunTimer;
    public bool isSpawnStun = true;
    public bool stunlock = false;

    [Header("Line of Sight")]
    public LayerMask obstacleLayer;
    public float arrowWidth = 0.11f;

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

    [Header("SFX")]
    public AudioClip[] footsteps;
    public float footStepVol;
    public float footStepPitch;
    public float walkInterval;
    private float footstepTimer = 0f;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (archerAnimator == null)
            archerAnimator = GetComponent<Animator>();
        if (spriteRender == null)
            spriteRender = GetComponent<SpriteRenderer>();
        if (enemyScript == null)
            enemyScript = GetComponent<Enemy>();
        if (aSAD == null)
        {
            aSAD = GetComponent<ArcherShootingAndDetection>();
            stopDistance = aSAD.detectDistance * 0.95f;
        }

        ogSpeed = speed;

        InvokeRepeating(nameof(UpdatePath), 0f, reCalcPathTime); 
    }

    public void StunLock()
    {
        stunlock = true;
    }
    public void StunUnlock()
    {
        stunlock = false;
    }
    public void KnockbackAndStun(Vector2 force, float stunTime)
    {

        knockbackVelocity = force;
        knockbackTimer = knockbackDuration;
        isKnockedBack = true;


        stunTimer = stunTime;
        isStunned = true;
        archerAnimator.SetBool("IsStunned", true);
    }



    private void UpdatePath()
    {
        if ((beStunnedWhenSpawning && isSpawnStun) || isStunned || enemyScript.dead) return;
        seeker.StartPath(rb.position, player.position, OnPathComplete);
    }

    public void ApplySpeedBuff(float multipler)
    {
        speed *= multipler;
    }
    public void RemoveSpeedBuff()
    {
        speed = ogSpeed;
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

        if (spawnStunTimer >= spawnStunTime)
        {
            isSpawnStun = false;
        }
    }
    private void FixedUpdate()
    {

        if (beStunnedWhenSpawning && isSpawnStun)
        {
            SpawnTimer();
        }



        if (rb.linearVelocity.magnitude > 0.1f)
        {
            archerAnimator.SetBool("Moving", true);
        }
        else
        {
            archerAnimator.SetBool("Moving", false);
        }

        if (stunlock)
        {
            rb.linearVelocity = Vector2.zero;
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
                archerAnimator.SetBool("IsStunned", false);
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
            rb.linearVelocity = Vector2.zero;

            if (player.position.x < transform.position.x)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else
                transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {

            if (path == null || currentWaypoint >= path.vectorPath.Count) return;

            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            rb.linearVelocity = direction * speed;
            PlayFootStepSound();

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


    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, arrowWidth, direction, distanceToPlayer, obstacleLayer);

        if (hit.collider != null)
        {
            return false;
        }

        return true;
    }

    public void PlayFootStepSound()
    {
        if (!isStunned)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (footsteps != null && footsteps.Length > 0)
                {
                    int randomIndex = Random.Range(0, footsteps.Length);
                    AudioManager.Instance.Play3D(footsteps[randomIndex], transform.position, footStepVol, footStepPitch);
                }
                footstepTimer = walkInterval / archerAnimator.speed;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }
}
