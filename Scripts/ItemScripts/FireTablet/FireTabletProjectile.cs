using UnityEngine;
using UnityEngine.EventSystems;

public class FireTabletProjectile : MonoBehaviour, IClearDuriingReset 
{
    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 100f;
    public float maxDistance = 4f;


    [Header("References")]
    private Rigidbody2D rb;
    private Animator anim;

    private Vector2 moveDirection;
    private Vector2 startingPoint;
    private bool destroyed = false;

    [Header("Sounds")]
    public AudioClip explodeSound;
    public float explodeVol;


    public GameObject fireArea;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }
    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }

    public void Clear()
    {
        Destroy(gameObject);
    }
    public void Initialize(Vector2 direction, Vector2 startingPt)
    {
        moveDirection = direction.normalized;
        startingPoint = startingPt;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        rb.linearVelocity = moveDirection * speed;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if(Vector2.Distance(transform.position, startingPoint) >= maxDistance && !destroyed)
        {
            Explode();
        }
    }

    void Explode()
    {
        destroyed = true;
        GameObject fire = Instantiate(fireArea, transform.position, Quaternion.identity);
        rb.linearVelocity = Vector2.zero;

        AudioManager.Instance.Play3D(explodeSound, transform.position, explodeVol);
        anim.SetBool("Explode", true);
        Destroy(gameObject, 10f);
        

        
    }
}
