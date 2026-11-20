using UnityEngine;

public class HealerHealing : MonoBehaviour
{
    [SerializeField] private float healRadius;
    [SerializeField] private float healAmount;
    [SerializeField] private float healFireRate;
    public LayerMask enemyLayer;
    public float currentHealTimer;

    [Header("Effects")]
    public ParticleSystem healRing;
    public AudioClip healSound;
    public float healVol;

    void Start()
    {
        currentHealTimer = healFireRate;
    }

    // Update is called once per frame
    void Update()
    {
        currentHealTimer -= Time.deltaTime * 1f;

        if (currentHealTimer <= 0f)
        {
            CheckForEnemies();
            currentHealTimer = healFireRate;
        }
            

    }

    void CheckForEnemies()
    {
        Collider2D[] enemiesFound = Physics2D.OverlapCircleAll(transform.position, healRadius, enemyLayer);
        foreach (Collider2D enem in enemiesFound)
        {
            Enemy e = enem.GetComponent<Enemy>();
            if (e != null && e.currentHealth < e.maxHealth && !e.dead && e.gameObject != this.gameObject && enem.GetComponent<HealerAI>() == null)
            {
                HealEnemies(enem, e);
            }
        }
    }


    public void HealEnemies(Collider2D enemy, Enemy enemyScript)
    {
        enemyScript.HealSelf(healAmount);
        AudioManager.Instance.Play3D(healSound, transform.position, healVol);
        healRing.Play();
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;


        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}
