using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Logic")]
    public float maxHealth = 100f;
    private float ogMaxHealth;
    public float currHealth;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    public Animator playerAnim;

    public bool isDead = false;

    [Header("UI References")]
    [SerializeField] private Slider fronthealthSlider;
    [SerializeField] private Slider backHealthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backFillImage;
    [SerializeField] private Animator healthbarAnimator;
    [SerializeField] private TextMeshProUGUI hPText;
    [SerializeField] private TextMeshProUGUI hPTextShadow;
    [SerializeField] private Image faderImage;

    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = new Color(1f, 0.64f, 0f);
    [SerializeField] private Color lowHealthColor = Color.red;

    [Header("UI Timing")]
    [SerializeField] private float frontLerpDuration = 0.2f;
    [SerializeField] private float backLerpDelay = 0.3f;
    [SerializeField] private float backLerpDuration = 0.6f;

    [Header("Vignette Settings")]
    [SerializeField] private Image damageVignette;
    [SerializeField, Range(0f, 1f)] private float damageFlashMaxAlpha = 0.8f;
    [SerializeField] private float damageFlashDuration = 0.4f;

    [Header("Low Health Pulse Settings")]
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float pulseMinAlpha = 0.1f;
    [SerializeField, Range(0f, 1f)] private float pulseMaxAlpha = 0.4f;

    private float currentVignetteAlpha = 0f;
    private float flashTimer = 0f;
    private bool hasPlayedPulseSound = false;

    [Header("Sound")]
    public AudioClip healSound;
    public AudioClip playerHit;
    public AudioClip playerDeath;
    public AudioClip lowHP;
    public float healVol;


    private void Start()
    {
        currHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

        fronthealthSlider.maxValue = maxHealth;
        backHealthSlider.maxValue = maxHealth;

        fronthealthSlider.value = currHealth;
        backHealthSlider.value = currHealth;

        ogMaxHealth = maxHealth;

        if (damageVignette != null)
        {
            Color c = damageVignette.color;
            c.a = 0f;
            damageVignette.color = c;
        }

        UpdateHealthUI();
    }

    private void Update()
    {
        if (damageVignette == null) return;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            float normalizedTime = flashTimer / damageFlashDuration;
            currentVignetteAlpha = Mathf.Lerp(0f, damageFlashMaxAlpha, normalizedTime);
        }
        else if (currHealth / maxHealth <= lowHealthThreshold && !isDead)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            currentVignetteAlpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, pulse);
            if (pulse > 0.95f && !hasPlayedPulseSound)
            {
                AudioManager.Instance.Play2D(lowHP, healVol);
                hasPlayedPulseSound = true; 
            }
            else if (pulse < 0.5f)
            {
                hasPlayedPulseSound = false;
            }

        }
        else
        {
            currentVignetteAlpha = Mathf.MoveTowards(currentVignetteAlpha, 0f, Time.deltaTime * 3f);
        }

        Color vColor = damageVignette.color;
        vColor.a = currentVignetteAlpha;
        damageVignette.color = vColor;
    }

    public void TakeDamage(float damage, Vector2 source, float knockbackForce, float stunDuration = 0.2f)
    {
        if (isDead) return;

        currHealth -= damage;
        AudioManager.Instance.Play2D(playerHit, healVol);

        flashTimer = damageFlashDuration;

        if (currHealth <= 0f)
        {
            currHealth = 0f;
            isDead = true;
            AudioManager.Instance.Play2D(playerDeath, healVol);
            if (playerAnim != null)
            {
                playerAnim.Play("Death");
            }

            GameManager.Instance.GameOver();

            StartCoroutine(LerpFrontBar());
            StartCoroutine(LerpBackBar());

            return;
        }

        if (healthbarAnimator != null)
            healthbarAnimator.SetTrigger("Hurt");

        Vector2 knockDir = ((Vector2)transform.position - source).normalized;
        rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);

        if (playerMovement != null)
            StartCoroutine(StunRoutine(stunDuration));

        StartCoroutine(LerpFrontBar());
        StartCoroutine(LerpBackBar());
    }

    public void HealHP(float healAmount)
    {
        if (isDead) return;

        if (currHealth + healAmount > maxHealth)
        {
            currHealth = maxHealth;
        }
        else
        {
            currHealth += healAmount;
        }

        AudioManager.Instance.Play2D(healSound, healVol);

        StartCoroutine(LerpFrontBar());
        StartCoroutine(LerpBackBar());
    }

    public void UpgradeMaxHealth(float newMaxHealth)
    {
        float healthPercent = currHealth / maxHealth;

        maxHealth = newMaxHealth;
        currHealth = maxHealth * healthPercent;

        fronthealthSlider.maxValue = maxHealth;
        backHealthSlider.maxValue = maxHealth;

        ResetSliders();
        UpdateHealthUI();
    }

    private IEnumerator StunRoutine(float duration)
    {
        playerMovement.isKnockedBack = true;
        yield return new WaitForSeconds(duration);
        playerMovement.isKnockedBack = false;
    }

    public void ResetSliders()
    {
        fronthealthSlider.value = currHealth;
        backHealthSlider.value = currHealth;
        UpdateHPText(currHealth);
    }

    private IEnumerator LerpFrontBar()
    {
        float elapsed = 0f;
        float startValue = fronthealthSlider.value;
        float targetValue = currHealth;

        StartCoroutine(LerpHPText(startValue, targetValue));

        while (elapsed < frontLerpDuration)
        {
            elapsed += Time.deltaTime;
            fronthealthSlider.value = Mathf.Lerp(startValue, targetValue, elapsed / frontLerpDuration);
            UpdateHealthUI();
            yield return null;
        }

        if (healthbarAnimator != null)
            healthbarAnimator.SetTrigger("Hurt");
        fronthealthSlider.value = targetValue;
        UpdateHealthUI();
    }

    private IEnumerator LerpBackBar()
    {
        yield return new WaitForSeconds(backLerpDelay);

        float elapsed = 0f;
        float startValue = backHealthSlider.value;
        float targetValue = currHealth;

        while (elapsed < backLerpDuration)
        {
            elapsed += Time.deltaTime;
            backHealthSlider.value = Mathf.Lerp(startValue, targetValue, elapsed / backLerpDuration);
            UpdateHealthUI();
            yield return null;
        }

        backHealthSlider.value = targetValue;
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        float healthPercent = currHealth / maxHealth;
        fillImage.color = HealthColorLerp(healthPercent);
    }

    private Color HealthColorLerp(float percent)
    {
        if (percent > 0.5f)
        {
            float t = (percent - 0.5f) / 0.5f;
            return Color.Lerp(midHealthColor, fullHealthColor, t);
        }
        else
        {
            float t = percent / 0.5f;
            return Color.Lerp(lowHealthColor, midHealthColor, t);
        }
    }

    private IEnumerator LerpHPText(float startValue, float targetValue)
    {
        float elapsed = 0f;

        while (elapsed < frontLerpDuration)
        {
            elapsed += Time.deltaTime;
            float currentValue = Mathf.Lerp(startValue, targetValue, elapsed / frontLerpDuration);
            UpdateHPText(currentValue);
            yield return null;
        }

        UpdateHPText(targetValue);
    }

    private void UpdateHPText(float value)
    {
        int hpInt = Mathf.CeilToInt(value);
        hPText.text = hpInt.ToString();
        hPTextShadow.text = hpInt.ToString();

        float percent = value / maxHealth;
        hPText.color = HealthColorLerp(percent);
    }
}