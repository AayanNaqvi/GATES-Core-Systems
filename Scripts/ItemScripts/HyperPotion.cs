using UnityEngine;

[CreateAssetMenu(fileName = "Hyper Potion", menuName = "Inventory/Hyper Potion")]
public class HyperPotion : Item
{
    public float duration;

    public float increaseDamageMult;
    public float increaseAtkSpd;
    public float increasedSpeed;
    public float healAmount;

    public GameObject shockWave;

    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        SwordCombat sc = player.GetComponentInChildren<SwordCombat>();
        PlayerHealth ph = player.GetComponent<PlayerHealth>();

        if (pm != null)
        {
            pm.ApplySpeedBoost(increasedSpeed, duration);
        }
        if (sc != null)
        {
            sc.ApplyAttackBoost(increaseDamageMult, increaseAtkSpd, duration);
        }
        if(pm != null)
        {
            ph.HealHP(healAmount);
        }

        GameObject shockwaveObj = Instantiate(shockWave, player.transform.position, Quaternion.identity);
        Destroy(shockwaveObj, 1.4f);
    }


}
