using UnityEngine;



[CreateAssetMenu(fileName = "Thunder Spear", menuName = "Inventory/Thunder Spear")]
public class ThunderSpear : Item
{
    public GameObject thunderSpearProj;


    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        AimControl am = player.GetComponentInChildren<AimControl>();

        if (am != null)
        {
            GameObject thSpObj = Instantiate(thunderSpearProj, player.transform.position, Quaternion.identity);
            thSpObj.GetComponent<ThunderSpearProjectile>().Initialize(am.lookDirection);
        }
    }
}
