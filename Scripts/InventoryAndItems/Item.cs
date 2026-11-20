using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    [TextArea] public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    public AudioClip useSound;
    public float useVolume;

    public int maxStackSize = 1;

    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary,
    }

    public Rarity rarity;


    [Header("Shadow Settings")]
    public Vector2 shadowOffset = new Vector2(0f, -0.3f); 
    public Vector2 shadowScale = new Vector2(1f, 0.5f);   

    public virtual void Use(GameObject player)
    {
        //Something
    }
}
