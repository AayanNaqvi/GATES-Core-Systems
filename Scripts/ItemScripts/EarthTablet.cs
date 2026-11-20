using System;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "Earth Tablet", menuName = "Inventory/Earth Tablet")]
public class EarthTablet : Item
{
    public float attackRadius;
    public float stunDuration;
    public float attackDamage;
    public float knockback;

    [Header("Effects")]
    public GameObject shockWave;
    public GameObject quake;
    
    
    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, attackRadius);


        foreach (Collider2D collider in colliders)
        {
            Enemy e = collider.GetComponent<Enemy>();
            if (e != null)
            {
                Vector2 knockDir = (e.transform.position - player.transform.position).normalized;
                IKnockbackable knockableAI = collider.GetComponent<IKnockbackable>();
                if (knockableAI != null)
                {
                    knockableAI.KnockbackAndStun(knockDir * knockback, stunDuration);
                }



                e.TakeDamage(attackDamage);
            }
        }


        GameObject quakeObj = Instantiate(quake, player.transform.position, Quaternion.identity);
        GameObject shockwaveObj = Instantiate(shockWave, player.transform.position, Quaternion.identity);

        Destroy(quakeObj, 1f);
        Destroy(shockwaveObj, 1.4f);
        
    }
}
