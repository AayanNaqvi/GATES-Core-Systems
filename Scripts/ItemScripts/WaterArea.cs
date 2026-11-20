using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WaterArea : MonoBehaviour, IClearDuriingReset
{

    [Header("Heal Stats")]
    public float healingPerTick = 5f;
    public float timeBetweenTicks = 1f;
    public float lifetime = 5f;


    public ParticleSystem particles;
    public Light2D waterLight;
    public AudioSource ad;

    private Dictionary<Collider2D, float> healTimers = new Dictionary<Collider2D, float>();


    

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

        StartCoroutine(WaterLifecycle());
    }


    public void Clear()
    {
        Destroy(gameObject);
    }
    private IEnumerator WaterLifecycle()
    {
        float fadeDuration = 2f;
        float normalBurnTime = lifetime - fadeDuration;

        yield return new WaitForSeconds(normalBurnTime);

        if (particles != null)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (waterLight != null)
        {
            float startIntensity = waterLight.intensity;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                waterLight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / fadeDuration);
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
        List<Collider2D> currentTargets = new List<Collider2D>(healTimers.Keys);

        foreach (Collider2D targetCollider in currentTargets)
        {
            if (targetCollider == null || !healTimers.ContainsKey(targetCollider))
            {
                if (targetCollider == null && healTimers.ContainsKey(targetCollider))
                {
                    healTimers.Remove(targetCollider);
                }
                continue;
            }

            if (Time.time >= healTimers[targetCollider])
            {
                PlayerHealth target = targetCollider.GetComponent<PlayerHealth>();  
                if (target != null)
                {
                    target.HealHP(healingPerTick);

                    if (healTimers.ContainsKey(targetCollider))
                    {
                        healTimers[targetCollider] = Time.time + timeBetweenTicks;
                    }
                }
            }
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {


        PlayerHealth target = other.GetComponent<PlayerHealth>();

        if (target != null)
        {


            if (!healTimers.ContainsKey(other))
            {
                healTimers.Add(other, 0f);
            }
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {

        if (healTimers.ContainsKey(other))
        {
            healTimers.Remove(other);
        }

        PlayerHealth target = other.GetComponent<PlayerHealth>();
        
    }
}
