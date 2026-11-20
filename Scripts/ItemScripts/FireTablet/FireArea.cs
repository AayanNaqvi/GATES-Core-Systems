using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireArea : MonoBehaviour, IClearDuriingReset
{

    [Header("Fire Stats")]
    public float damagePerTick = 5f;
    public float timeBetweenTicks = 1f; 
    public float lifetime = 5f;
    
    
    public ParticleSystem particles;
    public Light2D fireLight;
    public AudioSource ad;


    private Dictionary<Collider2D, float> burnTimers = new Dictionary<Collider2D, float>();


    

    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }



    void Start()
    {
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }

        StartCoroutine(FireLifecycle());
    }


    public void Clear()
    {
        Destroy(gameObject);
    }
    private IEnumerator FireLifecycle()
    {
        float fadeDuration = 2f;
        float normalBurnTime = lifetime - fadeDuration;

        yield return new WaitForSeconds(normalBurnTime);

        if (particles != null)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (fireLight != null)
        {
            float startIntensity = fireLight.intensity;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fireLight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / fadeDuration);
                ad.volume = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        Destroy(gameObject);
    }

    void Update()
    {
        List<Collider2D> currentTargets = new List<Collider2D>(burnTimers.Keys);
        
        foreach (Collider2D targetCollider in currentTargets)
        {
            if (targetCollider == null || !burnTimers.ContainsKey(targetCollider))
            {
                if (targetCollider == null && burnTimers.ContainsKey(targetCollider))
                {
                    burnTimers.Remove(targetCollider);
                }
                continue;
            }

            if (Time.time >= burnTimers[targetCollider])
            {
                IDamagable target = targetCollider.GetComponent<IDamagable>();
                if (target != null)
                {
                    target.TakeDamage(damagePerTick);

                    if (burnTimers.ContainsKey(targetCollider))
                    {
                        burnTimers[targetCollider] = Time.time + timeBetweenTicks;
                    }
                }
            }
        }
    }

   
    void OnTriggerEnter2D(Collider2D other)
    {
        

        IDamagable target = other.GetComponent<IDamagable>();

        if (target != null)
        {
            SpriteRenderer sr = other.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(1f, 0.5f, 0f);
            }

            
            if (!burnTimers.ContainsKey(other))
            {
                burnTimers.Add(other, 0f);
            }
        }
    }

    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (burnTimers.ContainsKey(other))
        {
            burnTimers.Remove(other);
        }

        IDamagable target = other.GetComponent<IDamagable>();
        if (target != null)
        {
            SpriteRenderer sr = other.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }
    }
}
