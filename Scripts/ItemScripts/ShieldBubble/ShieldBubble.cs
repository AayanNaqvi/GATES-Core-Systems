using UnityEngine;

[CreateAssetMenu(fileName = "Shield Bubble", menuName = "Inventory/Shield Bubble")]
public class ShieldBubble : Item
{
    public float duration;
    public GameObject shieldBubble;
    public GameObject bubbleEffect;
    public float knockBackForce;

    public override void Use(GameObject player)
    {
        AudioManager.Instance.Play2D(useSound, useVolume);

        GameObject sb = Instantiate(shieldBubble, player.transform);
        sb.GetComponent<BubbleProtector>().Initialize(duration, knockBackForce);
        
        
        GameObject shockwaveObj = Instantiate(bubbleEffect, player.transform.position, Quaternion.identity);
        Destroy(shockwaveObj, 1.4f);
    }
}
