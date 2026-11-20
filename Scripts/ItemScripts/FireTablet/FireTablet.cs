using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Fire Tablet", menuName = "Inventory/Fire Tablet")]
public class FireTablet : Item
{
    public GameObject fireOrbParticle;
    
    
    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        AimControl am = player.GetComponentInChildren<AimControl>();

        if (am != null)
        {
            GameObject fOrbP = Instantiate(fireOrbParticle, player.transform.position, Quaternion.identity);
            fOrbP.GetComponent<FireTabletProjectile>().Initialize(am.lookDirection, player.transform.position);
        }
    }
}
