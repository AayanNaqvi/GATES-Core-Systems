using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour, IDamagable, IClearDuriingReset
{
    public static int globalEnemyCount = 0;


    [Header("Health")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;
    public ElementType currType;

    [Header("Damage Flash")]
    [SerializeField, Range(0.01f, 1f)] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;
    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashRoutine;

    [Header("Damage")]
    [SerializeField] public float damage;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float hitStop;
    public bool dead = false;
    public bool tookDamage = false;

    [Header("Effects")]
    public Slider healthSlider;
    public float healthBarShowTimer = 2f; 
    [SerializeField] private float healthBarFadeTime = 0.5f; 
    private CanvasGroup sliderCVgroup;
    private Coroutine healthFadeRoutine;
    public GameObject hitParticle;

    [Header("Death Tips")]
    [TextArea] public string[] deathMessages;
    private GameObject deathTip;

    public Animator enemAnimator;
    public Collider2D bc2d;


    [Header("Sound")]
    public AudioClip[] idleSounds;
    public AudioClip[] hitSounds;
    public AudioClip deathSound;
    public float idleVol;
    public float hitVol;
    public float deathVol;
    public float soundInterval;
    private float soundTimer = 0;



    

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

        if (deathTip == null)
            deathTip = GameObject.FindGameObjectWithTag("DeathTip");

        currentHealth = maxHealth;
        
        
        if(GetComponent<Spawner>() == null )
            globalEnemyCount++;

        sr = GetComponent<SpriteRenderer>();
        bc2d = GetComponent<Collider2D>();
        enemAnimator = GetComponent<Animator>();
        if (sr != null)
            originalColor = sr.color;

        if(healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            sliderCVgroup = healthSlider.GetComponent<CanvasGroup>();

            if (sliderCVgroup != null) sliderCVgroup.alpha = 0f;
        }
    }
    

    void Update()
    {
        if (soundTimer < soundInterval)
        {
            soundTimer += Time.deltaTime;
        }
        if (soundTimer >= soundInterval)
        {
            soundTimer = 0 - UnityEngine.Random.Range(0f, 1.5f);

            if (idleSounds != null && idleSounds.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, idleSounds.Length);
                AudioManager.Instance.Play3D(idleSounds[randomIndex], transform.position, idleVol);
            }
                

        }
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            
            if(transform.rotation == Quaternion.Euler(transform.rotation.x,180f,transform.rotation.z))
                healthSlider.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            else
                healthSlider.transform.rotation = Quaternion.Euler(0f, 0f, 0f);



            


        }







    }

    

    public void Clear()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float dmg)
    {
        if (dead) return;

        tookDamage = true;
        
        
        if (currentHealth - dmg <= 0.9f)
        {
            currentHealth = 0f;
            dead = true;
            if (GetComponent<DuneSlime>() == null)
            {
                PlayHitParticle();
            }

            if(deathSound != null) AudioManager.Instance.Play3D(deathSound, transform.position, deathVol);
            StartCoroutine(DeathAnim());
            
        }
        else
        {
            if(hitSounds != null && hitSounds.Length > 0) AudioManager.Instance.Play3D(hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)], transform.position, hitVol);


            currentHealth -= dmg;
            
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine); 
        }
            
        flashRoutine = StartCoroutine(DamageFlash());

        if (sliderCVgroup != null)
        {
            if (healthFadeRoutine != null)
            {
                StopCoroutine(healthFadeRoutine);
            }
            healthFadeRoutine = StartCoroutine(FadeHealthBar());
        }


    }

    public void PlayHitParticle()
    {
        
         GameObject hitPart = Instantiate(hitParticle, transform.position, Quaternion.identity);
         Destroy(hitPart, 1.2f);
        
    }


    public void HealSelf(float healAmount)
    {
        float potentialHP = currentHealth + healAmount;

        if(potentialHP >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += healAmount;
        }

        
    }

    private IEnumerator FadeHealthBar()
    {
        sliderCVgroup.alpha = 1f;

        yield return new WaitForSeconds(healthBarShowTimer);

        float elapsed = 0f;
        while (elapsed < healthBarFadeTime)
        {
            sliderCVgroup.alpha = Mathf.Lerp(1f, 0f, elapsed / healthBarFadeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        sliderCVgroup.alpha = 0f;
    }

    private IEnumerator DamageFlash()
    {
        float elapsed = 0f;
        if (enemAnimator != null && !dead) enemAnimator.enabled = false;

        while (elapsed < flashDuration / 2f)
        {
            sr.color = Color.Lerp(originalColor, flashColor, elapsed / (flashDuration / 2f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        sr.color = flashColor;

        elapsed = 0f;

        while (elapsed < flashDuration / 2f)
        {
            sr.color = Color.Lerp(flashColor, originalColor, elapsed / (flashDuration / 2f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        
        if (enemAnimator != null && !dead) enemAnimator.enabled = true;
    }



    

    public IEnumerator DeathAnim()
    {
        Spawner sp = GetComponent<Spawner>();
        

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        if (enemAnimator != null) enemAnimator.enabled = true;

        if(GetComponent<IKnockbackable>() != null)
        {
            GetComponent<IKnockbackable>().StunUnlock();
        }
        
        
        enemAnimator.SetBool("Dead", true);
        dead = true;
        

        if(GetComponent<DuneSlime>() != null)
        {
            GetComponent<DuneSlime>().SlimeDeath();
        }

        if (sp == null)
        {
            globalEnemyCount--;
        }
        else
        {
            AudioSource emitterAudio = GetComponentInChildren<AudioSource>();
            if(emitterAudio != null)
                emitterAudio.Stop();
        }


            yield return new WaitForSeconds(0.3f);
        
        


        if(sp == null)
            Destroy(gameObject);
        else
        {
            bc2d.enabled = false;
            sp.active = false;
            sp.DropNuggets();
            Spawner.activeSpawners--;
        }




        
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null && !dead)
        {
            ph.TakeDamage(damage, transform.position, knockbackForce);
            if (ph.currHealth <= 0)
            {
                if (deathTip != null && deathMessages != null && deathMessages.Length > 0)
                {
                    int general = UnityEngine.Random.Range(0, 3);
                    if (general != 0)
                    {
                        int randInt = UnityEngine.Random.Range(0, deathMessages.Length);
                        deathTip.GetComponent<TextMeshProUGUI>().SetText(deathMessages[randInt]);
                    }
                }
            }


            //HitStop.DoHitStop(hitStop);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerHealth ph = collision.collider.GetComponent<PlayerHealth>();

        if (ph != null && !dead)
        {
            ph.TakeDamage(damage, transform.position, knockbackForce);

            if (ph.currHealth <= 0)
            {
                if (deathTip != null && deathMessages != null && deathMessages.Length > 0)
                {
                    int general = UnityEngine.Random.Range(0, 3);
                    if (general != 0)
                    {
                        int randInt = UnityEngine.Random.Range(0, deathMessages.Length);
                        deathTip.GetComponent<TextMeshProUGUI>().SetText(deathMessages[randInt]);
                    }
                }
            }
        }
    }
}
