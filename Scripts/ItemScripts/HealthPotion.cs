using UnityEngine;


[CreateAssetMenu(fileName = "New Health Potion", menuName = "Inventory/Health Potion")]
public class HealthPotion : Item
{
    
    public float healAmount = 50f;

    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        ph.HealHP(healAmount);
    }
}
