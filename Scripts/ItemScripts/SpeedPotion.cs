using UnityEngine;


[CreateAssetMenu(fileName = "Speed Potion", menuName = "Inventory/Speed Potion")]
public class SpeedPotion : Item
{
    public float speedDuration;

    public float increasedSpeed;

    [Header("Effects")]
    public GameObject shockWave;

    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        PlayerMovement pm = player.GetComponent<PlayerMovement>();

        if (pm != null)
        {
            pm.ApplySpeedBoost(increasedSpeed, speedDuration);
        }

        GameObject shockwaveObj = Instantiate(shockWave, player.transform.position, Quaternion.identity);
        Destroy(shockwaveObj, 1.4f);
    }

   




}
