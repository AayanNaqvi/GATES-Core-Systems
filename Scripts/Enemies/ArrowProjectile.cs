using System.Collections;
using TMPro;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour, IClearDuriingReset
{

    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockBackForce;
    [SerializeField] private float stunDuration;
    private Enemy archerRef;
    private GameObject deathTip;


    [Header("References")]
    private Rigidbody2D rb;
    private Animator anim;
    

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
        anim = GetComponent<Animator>();

        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    public void Initialize(Vector2 direction, Enemy enem)
    {
        moveDirection = direction.normalized;
        archerRef = enem;
        deathTip = GameObject.FindGameObjectWithTag("DeathTip");

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        rb.linearVelocity = moveDirection * speed;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (destroyed) return;

       
        float distanceThisFrame = speed * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, distanceThisFrame + 0.1f);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Wall"))
            {

                transform.position = hit.point;

                StartDeathAnim();
            }
        }
    }

    public void Clear()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed) return;
        AudioManager.Instance.Play3D(hitSound, transform.position, hitVol);

        if (other.GetComponent<PlayerHealth>() != null && !destroyed)
        {

            PlayerHealth pHealth = other.GetComponent<PlayerHealth>();
            pHealth.TakeDamage(damage, transform.position, knockBackForce, stunDuration);
            if (pHealth.currHealth - damage <= 0)
            {
                int randInt = UnityEngine.Random.Range(0, archerRef.deathMessages.Length);
                if (deathTip != null)
                    deathTip.GetComponent<TextMeshProUGUI>().SetText(archerRef.deathMessages[randInt]);

            }



            StartDeathAnim();


        }
        else if (other.CompareTag("Wall"))
        {
            
            StartDeathAnim();

        }


    }

    public void StartDeathAnim()
    {
        if (destroyed) return;
        destroyed = true;
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        if (anim != null)
            anim.SetTrigger("Hit");

        yield return new WaitForSeconds(0.45f);
        Destroy(gameObject);



    }





}
