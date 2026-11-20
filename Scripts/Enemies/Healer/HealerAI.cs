using UnityEngine;
using Pathfinding;



[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class HealerAI : MonoBehaviour, IKnockbackable
{
    [Header("References")]
    public Transform target;
    public Animator healerAnimator;
    public SpriteRenderer spriteRender;
    public GameObject shadow;
    private Enemy enemyScript;

    [Header("Movement")]
    [SerializeField, Range(0.5f, 10f)] private float speed = 4f;
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
    public LayerMask enemies;
    public float arrowWidth = 0.11f;
    public float enemyRadius;
    private float searchTimer = 0f;

    private Seeker seeker;
    private Rigidbody2D rb;
    private Path path;
    private int currentWaypoint = 0;
    private BoxCollider2D bc2d;

    private bool isKnockedBack = false;
    private bool isStunned = false;
    private float knockbackTimer = 0f;
    private float stunTimer = 0f;

    public float knockbackDuration = 0.15f;
    public float stunDuration = 0.3f; 
    private Vector2 knockbackVelocity;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        


        if (healerAnimator == null)
            healerAnimator = GetComponent<Animator>();
        if (spriteRender == null)
            spriteRender = GetComponent<SpriteRenderer>();
        if (enemyScript == null)
            enemyScript = GetComponent<Enemy>();
        if(bc2d == null)
            bc2d = GetComponent<BoxCollider2D>();
        
            
        

        if(target == null)
            CheckForEnemies();
        
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
        healerAnimator.SetBool("IsStunned", true);
    }

    public void CheckForEnemies()
    {
        Collider2D[] foundEnemies = Physics2D.OverlapCircleAll(transform.position, enemyRadius, enemies);
        float minDist = Mathf.Infinity;

        foreach (Collider2D colliders in foundEnemies)
        {
            if (colliders.gameObject == this.gameObject) continue;

            Enemy potentialEnemy = colliders.GetComponent<Enemy>();
            if (potentialEnemy != null && potentialEnemy.dead) continue;

            float distanceToPlayer = Vector2.Distance(transform.position, colliders.transform.position);

            if (distanceToPlayer < minDist && colliders.GetComponent<Spawner>() == null && colliders.GetComponent<TorchBreak>() == null && colliders.GetComponent<HealerAI>() == null)
            {
                minDist = distanceToPlayer;
                target = colliders.transform;
            }
        }
    }

    private void UpdatePath()
    {
        if ((beStunnedWhenSpawning && isSpawnStun) || isStunned || enemyScript.dead || target == null) return;

        seeker.StartPath(rb.position, target.position, OnPathComplete);
        
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
        if (!isSpawnStun) return;

        spawnStunTimer += Time.deltaTime * 1f;

        if (spawnStunTimer >= spawnStunTime)
        {
            isSpawnStun = false;

            if (target != null)
            {
                UpdatePath();
            }
        }
    }
    private void FixedUpdate()
    {
        
        
        
        if (beStunnedWhenSpawning && isSpawnStun)
        {
            SpawnTimer();
        }


        if (stunlock)
        {
            rb.linearVelocity = Vector2.zero;
            return; 
        }



        if (target != null)
        {
            Enemy targetEnemy = target.GetComponent<Enemy>();
            if (targetEnemy != null && targetEnemy.dead)
            {
                target = null;
            }
        }


        if (target == null)
        {
            searchTimer -= Time.fixedDeltaTime; 

            if (searchTimer <= 0f)
            {
                CheckForEnemies();
                searchTimer = 0.5f;


                if (target != null && !isSpawnStun && !isStunned)
                {
                    UpdatePath();
                }
            }

            if (target == null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }





        if (rb.linearVelocity.magnitude > 0.1f)
        {
            healerAnimator.SetBool("Walking", true);
        }
        else
        {
            healerAnimator.SetBool("Walking", false);
        }


        if (target == null) return;
        

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
                healerAnimator.SetBool("IsStunned", false);
                isStunned = false;
                UpdatePath();
            }
            return;
        }

        if (path == null) return;


        float distToPlayer = Vector2.Distance(rb.position, target.position);
        bool canSeePlayer = HasLineOfSight();

        if (distToPlayer <= stopDistance && canSeePlayer)
        {
            rb.linearVelocity = Vector2.zero;

            if (target.position.x < transform.position.x)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else
                transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {

            if (currentWaypoint >= path.vectorPath.Count)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

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


    private bool HasLineOfSight()
    {
        if (target == null) return false;

        Vector2 direction = (target.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);


        RaycastHit2D hit = Physics2D.CircleCast(transform.position, arrowWidth, direction, distanceToPlayer, obstacleLayer);

        if (hit.collider != null)
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyRadius);
    }
}

