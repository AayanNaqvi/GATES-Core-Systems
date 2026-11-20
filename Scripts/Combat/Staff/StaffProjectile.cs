using System.Collections;
using UnityEngine;

public class StaffProjectile : MonoBehaviour, IClearDuriingReset
{

    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float blastRadius;
    

    [Header("References")]
    private Rigidbody2D rb;
    private Animator anim;
    public ElementType elementType; 
    public ParticleSystem fireTrail;
    public GameObject explosion;
    public GameObject ring;

    private Vector2 moveDirection;
    private bool destroyed = false;


    public AudioClip impactSound;
    public float impactVol;


    

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
        anim = GetComponent<Animator>();


        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    public void Initialize(Vector2 direction, ElementType element)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        elementType = element;
        Destroy(gameObject, lifetime); 
    }

    void FixedUpdate()
    {
        if(!destroyed)
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    public void Clear()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed) return;

        if (other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {
            StartExplosion();
        }

    }

    private void StartExplosion()
    {
        if (destroyed) return;
        destroyed = true;

        rb.linearVelocity = Vector2.zero;
        AudioManager.Instance.Play3D(impactSound, transform.position, impactVol);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);

        foreach (Collider2D hitCollider in hits)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                IDamagable dmg = hitCollider.GetComponent<IDamagable>();
                if(hitCollider.GetComponent<DuneSlime>() != null && hitCollider.GetComponent<DuneSlime>().alpha)
                {
                    hitCollider.GetComponent<Enemy>().HealSelf(damage);
                }
                else if (dmg != null)
                {
                    dmg.TakeDamage(damage);
                }
            }

            TorchBreak tor = hitCollider.GetComponent<TorchBreak>();

            if (tor != null && tor.broken)
            {
                tor.TorchReignite();
            }
        }

        StartCoroutine(ExplodeAnimation());
    }





    private IEnumerator ExplodeAnimation()
    {
        if(anim != null)
            anim.SetTrigger("Explode");

        if (fireTrail != null)
            fireTrail.Stop();
        
        GameObject explodeObj = Instantiate(explosion, transform.position, Quaternion.identity);
        GameObject ringObj = Instantiate(ring, transform.position, Quaternion.identity);
        Destroy(ringObj, 1.4f);
        yield return new WaitForSeconds(0.8f);
        Destroy(explodeObj);
        Destroy(gameObject);
        


    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
