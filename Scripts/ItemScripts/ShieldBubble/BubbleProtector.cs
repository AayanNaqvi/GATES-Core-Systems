using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BubbleProtector : MonoBehaviour
{
    private float duration;
    private Animator anim;
    private Coroutine bubbleCycle;

    private GameObject player;

    public GameObject durSlider;
    private GameObject currDurSlider;
    private Transform sliderContainer;

    private float knockBackForce;
    
    private System.Collections.Generic.Dictionary<Collider2D, float> pulseTimers = new System.Collections.Generic.Dictionary<Collider2D, float>();
    private float pulseInterval = 0.5f;

    [Header("Sound")]
    public AudioClip bounceSound;
    public float bounceVol;
    public AudioClip decaySound;

    public void Initialize(float d, float kbf)
    {
        anim = GetComponent<Animator>();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            sliderContainer = player.GetComponent<PlayerMovement>().sliderContainer;
        }
            
        duration = d;
        knockBackForce = kbf;

        if(bubbleCycle != null) StopCoroutine(bubbleCycle);

        bubbleCycle = StartCoroutine(BubbleCycler());
        
    }

    public IEnumerator BubbleCycler()
    {
        if (currDurSlider != null) Destroy(currDurSlider);

        if (sliderContainer != null && durSlider != null)
        {
            currDurSlider = Instantiate(durSlider, sliderContainer, false);

            RectTransform rt = currDurSlider.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;

                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
            }

            BuffBars bb = currDurSlider.GetComponent<BuffBars>();
            if (bb != null)
            {
                bb.Initialize(duration);
            }
        }

        yield return new WaitForSeconds(duration);

        anim.SetBool("End", true);
        AudioManager.Instance.Play2D(decaySound, bounceVol);
        Destroy(gameObject, 0.5f);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyProjectile"))
        {
            AudioManager.Instance.Play2D(bounceSound, bounceVol);
            
            ArrowProjectile ap = other.GetComponent<ArrowProjectile>();
            if (ap != null)
            {
                ap.StartDeathAnim();
            }
            return;
        }


        IKnockbackable knockable = other.GetComponent<IKnockbackable>();
        RusherAI rusher = other.GetComponent<RusherAI>();
        if (rusher != null)
        {
            if (rusher.isDashing)
            {
                AudioManager.Instance.Play2D(bounceSound, bounceVol);
                rusher.StopDash();
            }
        }
        if(knockable != null)
        {
            AudioManager.Instance.Play2D(bounceSound, bounceVol);
            Vector2 bounceForce = (other.transform.position - transform.position).normalized * knockBackForce;
            knockable.KnockbackAndStun(bounceForce, 0.5f);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {

        if (other.CompareTag("EnemyProjectile") || other.CompareTag("Player")) return;

        IKnockbackable knockable = other.GetComponent<IKnockbackable>();
        if (knockable != null)
        {
            if (!pulseTimers.ContainsKey(other))
            {
                pulseTimers.Add(other, 0f);
            }

            if (Time.time >= pulseTimers[other])
            {
                Vector2 pushDir = (other.transform.position - transform.position).normalized;
                knockable.KnockbackAndStun(pushDir * (knockBackForce * 0.5f), 0.5f);

                pulseTimers[other] = Time.time + pulseInterval;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (pulseTimers.ContainsKey(other))
        {
            pulseTimers.Remove(other);
        }
    }

    private void OnDestroy()
    {
        if (currDurSlider != null)
        {
            Destroy(currDurSlider);
        }
    }
}
