using UnityEngine;


[CreateAssetMenu(fileName = "Crux Trap", menuName = "Inventory/Crux Trap")]

public class CruxTrap : Item
{
    public GameObject trapObj;

    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);
        Instantiate(trapObj, player.transform.position, Quaternion.identity);
    }
    
}
