using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class CruxTrapObj : MonoBehaviour, IClearDuriingReset
{
    [SerializeField] private float trapTime;
    public bool setoff = false;

    public Transform trapPoint;

    private Animator anim;

    private Enemy enemTrapped;

    public GameObject effect;

    [Header("Sounds")]
    public AudioClip trapHit;
    public AudioClip trapOpen;
    public float trapVol;


    public void Clear()
    {
        AudioManager.Instance.Play3D(trapOpen, transform.position, trapVol);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }

        GameObject placeEffect = Instantiate(effect, transform.position, Quaternion.identity);
        Destroy(placeEffect, 1f);
    }

    private void Update()
    {
        if(enemTrapped != null && enemTrapped.dead)
        {
            GameObject placeEffect = Instantiate(effect, transform.position, Quaternion.identity);
            Destroy(placeEffect, 1f);

            Clear();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (setoff) return;
        
        
        
            

            if (collision.CompareTag("Enemy"))
            {
                IKnockbackable ikb = collision.gameObject.GetComponent<IKnockbackable>();
                enemTrapped = collision.gameObject.GetComponent<Enemy>();
                if (ikb != null)
                {
                    setoff = true;
                    StartCoroutine(TrapSequence(collision.gameObject, ikb));
                }
            }

        
    }

    public IEnumerator TrapSequence(GameObject e, IKnockbackable ikb)
    {
        ikb.StunLock();
        anim.SetBool("Setoff", true);
        AudioManager.Instance.Play2D(trapHit, trapVol);
        Collider2D enemyCol = e.GetComponent<Collider2D>();
        if (enemyCol != null)
        {
            float centerToFeetOffset = e.transform.position.y - enemyCol.bounds.min.y;
            e.transform.position = new Vector3(trapPoint.position.x, trapPoint.position.y + centerToFeetOffset, e.transform.position.z);
        }
        else
        {
            e.transform.position = trapPoint.position;
        }

        Rigidbody2D rb = e.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        yield return new WaitForSeconds(trapTime);

        ikb.StunUnlock();
        AudioManager.Instance.Play3D(trapOpen, transform.position, trapVol);
        GameObject placeEffect = Instantiate(effect, transform.position, Quaternion.identity);
        Destroy(placeEffect, 1f);

        Clear();
    }
}
