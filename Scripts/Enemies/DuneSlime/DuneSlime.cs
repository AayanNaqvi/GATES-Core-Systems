using UnityEngine;
using System.Collections;

public class DuneSlime : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private int maxStages = 3;
    [SerializeField] public int currStage = 1;
    [SerializeField] private int numOfSplitters = 2;
    [SerializeField] private float sizeMultiplier = 0.6f;

    [Header("Effects")]
    public GameObject slimeDeathParticle;

    public bool alpha;

    public void SlimeDeath()
    {

        GameObject deathPart = Instantiate(slimeDeathParticle, transform.position, Quaternion.identity);
        Destroy(deathPart, 0.85f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2 awayFromPlayer = Vector2.up; 

        if (player != null)
        {
            awayFromPlayer = (transform.position - player.transform.position).normalized;
        }

        if (currStage >= maxStages) return;

        

        for (int i = 0; i < numOfSplitters; i++)
        {
            GameObject newSlime = Instantiate(gameObject, transform.position, Quaternion.identity);

            DuneSlime ds = newSlime.GetComponent<DuneSlime>();
            Enemy es = newSlime.GetComponent<Enemy>();
            WalkerAI ai = newSlime.GetComponent<WalkerAI>();
            Rigidbody2D rb = newSlime.GetComponent<Rigidbody2D>();


            IBuffed buffComponent = newSlime.GetComponent<IBuffed>();
            if (buffComponent != null)
            {
                buffComponent.RemoveSpeedBuff();
            }

            if (es != null)
            {
               
                es.maxHealth = GetComponent<Enemy>().maxHealth / 2;
                es.currentHealth = es.maxHealth;
                es.damage = GetComponent<Enemy>().damage - 5f;
                es.dead = false;

                if (es.enemAnimator != null)
                {
                    es.enemAnimator.Rebind();
                    es.enemAnimator.SetBool("Dead", false);
                    es.enemAnimator.Play("Idle");
                }
            }


            if (ai != null)
            {
                ai.enabled = false;
            }

            ds.currStage = currStage + 1;
            newSlime.transform.localScale = transform.localScale * sizeMultiplier;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;

                float randomSpread = UnityEngine.Random.Range(-45f, 45f);
                Vector2 finalPopDirection = Quaternion.Euler(0, 0, randomSpread) * awayFromPlayer;


                rb.AddForce(finalPopDirection * 5f, ForceMode2D.Impulse);

                ds.StartCoroutine(ds.RecoverFromSplit(0.5f));
            }
        }
    }

    public IEnumerator RecoverFromSplit(float duration)
    {
        yield return new WaitForSeconds(duration);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        WalkerAI ai = GetComponent<WalkerAI>();
        if (ai != null)
        {
            ai.enabled = true;
        }
    }
}