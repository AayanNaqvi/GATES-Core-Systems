using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThunderSpearProjectile : MonoBehaviour, IClearDuriingReset
{

    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 10;
    public GameObject ring;
    public float stunDuration = 1f;
    private int currentBounce = 0;
    public int maxBounces = 3;
    public float checkRadius;


    [Header("References")]
    private Rigidbody2D rb;
    public LayerMask whatIsEnemy;
    private List<Enemy> hitEnemies = new List<Enemy>();

    private Vector2 moveDirection;
    private bool destroyed = false;

    [Header("Sound")]
    public AudioClip hitSound;
    public float hitVol;



    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();


        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        Destroy(gameObject, lifetime); 
    }

    void FixedUpdate()
    {
        if (!destroyed)
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    public void Clear()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed) return;

        
        
        if (other.CompareTag("Wall"))
        {
            AudioManager.Instance.Play2D(hitSound, hitVol);
            ExplodeOnWall();
        }
        if(other.CompareTag("Enemy"))
        {
            Enemy e = other.GetComponent<Enemy>();

            if (e != null && !e.dead && !hitEnemies.Contains(e))
            {
                if (other.GetComponent<TorchBreak>() != null) return;

                hitEnemies.Add(e);

                IKnockbackable ikb = e.GetComponent<IKnockbackable>();
                if (ikb != null)
                {
                    ikb.KnockbackAndStun(Vector2.zero, stunDuration);
                }
                e.TakeDamage(damage);
                AudioManager.Instance.Play2D(hitSound, hitVol, 1f + (currentBounce * 0.25f));

                ExplodeOnEnemy();
            }
        }

    }

    private void ExplodeOnEnemy()
    {
        if (destroyed) return;

        if(currentBounce >= maxBounces)
        {
            ExplodeOnWall();
            return;
        }

        currentBounce++;

        GameObject ringEffect = Instantiate(ring, transform.position, Quaternion.identity);
        Destroy(ringEffect, 1f);

        Collider2D[] enemiesFound = Physics2D.OverlapCircleAll(transform.position, checkRadius, whatIsEnemy);

        Transform nextTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D potentialCollider in enemiesFound)
        {
            Enemy potentialEnemy = potentialCollider.GetComponent<Enemy>();

            if (potentialEnemy == null || potentialEnemy.dead || hitEnemies.Contains(potentialEnemy)) continue;

            if (potentialCollider.GetComponent<TorchBreak>() != null) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, potentialCollider.transform.position);

            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                nextTarget = potentialCollider.transform;
            }
        }

        if (nextTarget == null)
        {
            ExplodeOnWall();
        }
        else
        {
            Vector2 directionToNext = (nextTarget.position - transform.position).normalized;
            moveDirection = directionToNext;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

    }

    private void ExplodeOnWall()
    {
        if (destroyed) return;
        destroyed = true;

        GameObject ringEffect = Instantiate(ring, transform.position, Quaternion.identity);
        Destroy(ringEffect, 1f);

        Destroy(gameObject);


    }
    







    

    


}
