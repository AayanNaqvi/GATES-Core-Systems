using UnityEngine;

[CreateAssetMenu(fileName = "Wind Tablet", menuName = "Inventory/Wind Tablet")]
public class WindTablet : Item
{

    public GameObject windParticleOrb;

    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        AimControl am = player.GetComponentInChildren<AimControl>();

        if (am != null)
        {
            GameObject wOrbP = Instantiate(windParticleOrb, player.transform.position, Quaternion.identity);
            wOrbP.GetComponent<WindParticleOrb>().Initialize(am.lookDirection, player.transform.position, player);
        }
    }
}
