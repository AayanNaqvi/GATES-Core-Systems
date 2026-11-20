using UnityEngine;

[CreateAssetMenu(fileName = "Strength Potion", menuName = "Inventory/Strength Potion")]
public class StrengthPotion : Item
{
    public float duration;

    public float increaseDamageMult;

    [Header("Effects")]
    public GameObject shockWave;

    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        SwordCombat sc = player.GetComponentInChildren<SwordCombat>();

        if(sc != null )
        {
            sc.ApplyAttackBoost(increaseDamageMult, 1f, duration);
        }
        GameObject shockwaveObj = Instantiate(shockWave, player.transform.position, Quaternion.identity);
        Destroy(shockwaveObj, 1.4f);
    }
}
