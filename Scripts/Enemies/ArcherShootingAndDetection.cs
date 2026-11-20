using UnityEngine;
using System.Collections;

public class ArcherShootingAndDetection : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField, Range(0.1f, 3f)] private float fireRate;
    public float detectDistance;
    [SerializeField] private float checkInterval = 0.5f;
    public LayerMask obstacleLayer;
    [SerializeField] private float arrowWaitTime;

    private ArcherAI aAI;
    private Rigidbody2D rb;
    private bool isShooting = false;

    [Header("References")]
    public GameObject projectilePrefab;
    public Transform player;

    private float nextFireTime;

    [Header("Sound")]
    public AudioClip fireSound;
    public float fireVol;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (GetComponent<ArcherAI>() != null)
            aAI = GetComponent<ArcherAI>();
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(Check());
    }

    private IEnumerator Check()
    {
        while (true)
        {
            CheckForPlayer();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    public void CheckForPlayer()
    {
        if (player == null || aAI.isSpawnStun) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isInRange = (distanceToPlayer < detectDistance && HasLineOfSight());

        
        if (isInRange && !isShooting)
        {
            
            if (Time.time > nextFireTime && rb.linearVelocity.magnitude <= 0.1f)
            {
                
                StartCoroutine(Shoot());
            }
        }
    }

    bool HasLineOfSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distToPlayer, obstacleLayer);

        if (hit.collider == null)
        {
            return true;
        }
        return false;
    }

    private IEnumerator Shoot()
    {
        isShooting = true;

        
        yield return new WaitForSeconds(arrowWaitTime);

        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer >= detectDistance || !HasLineOfSight())
        {
            isShooting = false;
            yield break;
        }

        
        GameObject arrow = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        AudioManager.Instance.Play3D(fireSound, transform.position, fireVol);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        ArrowProjectile apj = arrow.GetComponent<ArrowProjectile>();
        if (apj != null)
        {
            apj.Initialize(directionToPlayer, GetComponent<Enemy>());
        }

        
        nextFireTime = Time.time + fireRate;

        isShooting = false;
    }
}