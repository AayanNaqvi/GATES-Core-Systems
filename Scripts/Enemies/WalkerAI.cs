using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;



[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class WalkerAI : MonoBehaviour, IKnockbackable, IBuffed
{
    [Header("References")]
    public Transform player;
    public Animator walkerAnimator;
    public SpriteRenderer spriteRender;
    public GameObject shadow;
    private Enemy enemyScript;

    [Header("Movement")]
    [SerializeField, Range(0.5f, 10f)] private float baseSpeed = 4f;
    [SerializeField] private float currentSpeed;
    [SerializeField, Range(0.1f, 3f)] public float nextWaypointDistance = 0.5f;
    [SerializeField] public float reCalcPathTime;
    public bool beStunnedWhenSpawning;
    [SerializeField] private float spawnStunTime;
    public float spawnStunTimer;
    public bool isSpawnStun = true;
    public bool stunlock = false;

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

        currentSpeed = baseSpeed;
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if(walkerAnimator == null)
            walkerAnimator = GetComponent<Animator>();
        if (spriteRender == null)
            spriteRender = GetComponent<SpriteRenderer>();
        if (enemyScript == null)
            enemyScript = GetComponent<Enemy>();


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


    public void ApplySpeedBuff(float multipler)
    {
        currentSpeed *= multipler;
    }
    public void RemoveSpeedBuff()
    {
        currentSpeed = baseSpeed;
    }


    public void KnockbackAndStun(Vector2 force, float stunTime)
    {
        knockbackVelocity = force;
        knockbackTimer = knockbackDuration;
        isKnockedBack = true;

        stunTimer = stunTime;
        isStunned = true;
        walkerAnimator.SetBool("IsStunned", true);
        walkerAnimator.SetBool("Walking", false);
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

        if (spawnStunTimer >= spawnStunTime)
        {
            isSpawnStun = false;
            walkerAnimator.SetBool("Walking", false);
        }
    }
    private void FixedUpdate()
    {
        PlayFootStepSound();

        if (beStunnedWhenSpawning && isSpawnStun)
        {
            walkerAnimator.SetBool("Walking", false);
            SpawnTimer();
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
                walkerAnimator.SetBool("IsStunned", false);
                isStunned = false;
                UpdatePath();
            }
                
            return; 
        }


        if (path == null) return;

        walkerAnimator.SetBool("Walking", true);

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 velocity = direction * currentSpeed;
        rb.linearVelocity = velocity;

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
                footstepTimer = walkInterval / walkerAnimator.speed;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }
}
