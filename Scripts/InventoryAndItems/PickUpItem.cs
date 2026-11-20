using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PickUpItem : MonoBehaviour, IClearDuriingReset
{
    public Item itemData;
    public static PickUpItem activeTarget;

    [Header("Interaction")]
    public float interactRadius = 1.5f;
    private GameObject player;
    private PlayerInput playerInput;
    private InputAction interactAction;

    [Header("Effects")]
    private Material regularMat;
    public Material outlineMat;
    private SpriteRenderer sr;
    private bool isGlowing = false;
    public Light2D emitting;
    public GameObject shadowSprite;

    public Color[] rarityColors;

    public GameObject myPromptPrefab;
    private GameObject myActivePrompt;

    [Header("Sound")]
    public AudioClip pickupSound;
    public float pickupVol;


    private void OnEnable()
    {
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    private void OnDisable()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }
    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }

        if (activeTarget == this) activeTarget = null;
        if (myActivePrompt != null) Destroy(myActivePrompt);
    }

    public void Clear()
    {
        Destroy(gameObject);
    }


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            interactAction = playerInput.actions["EnterDoorOrUnlockChest"];
        }

        if (sr != null)
        {
            regularMat = sr.material;
        }

        if (itemData != null) Setup(itemData);


    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);
        if (distance <= interactRadius)
        {
            if (activeTarget == null || activeTarget == this || distance < Vector2.Distance(player.transform.position, activeTarget.transform.position))
            {
                activeTarget = this;
            }
        }
        else
        {
            if (activeTarget == this) activeTarget = null;
        }

        if (activeTarget == this)
        {
            HighLightEntity(true);
            MyInput();
        }
        else
        {
            HighLightEntity(false);
        }
    }

    private void MyInput()
    {
        if (interactAction == null) return;

        if (interactAction.WasPressedThisFrame())
        {
            float distance = Vector2.Distance(player.transform.position, transform.position);

            if (distance <= interactRadius)
            {
                PickUp();
            }
        }
    }

    public void PickUp()
    {
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            bool wasPickedUp = inventory.AddItem(itemData);
            
            
            


            if (wasPickedUp)
            {
                Destroy(gameObject);

                if (pickupSound != null) AudioManager.Instance.Play2D(pickupSound, pickupVol);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;


        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }



    void HighLightEntity(bool shouldHighlight)
    {
        if (sr == null || outlineMat == null || player == null) return;

        if (shouldHighlight && !isGlowing)
        {
            sr.material = outlineMat;

            if (sr.sprite != null)
            {
                sr.material.SetTexture("_MainTex", sr.sprite.texture);
            }

            if (myActivePrompt == null) myActivePrompt = PromptManager.Instance.ShowPrompt(myPromptPrefab);

            isGlowing = true;
        }
        else if (!shouldHighlight && isGlowing)
        {
            sr.material = regularMat;
            if (myActivePrompt != null) Destroy(myActivePrompt);
            isGlowing = false;
        }
    }

    public void Setup(Item droppedItemData)
    {
        itemData = droppedItemData;

        if (sr != null && itemData != null)
        {
            sr.sprite = itemData.icon;

            Color rarityColor = itemData.rarity switch
            {
                Item.Rarity.Common => rarityColors[0],
                Item.Rarity.Rare => rarityColors[1],
                Item.Rarity.Epic => rarityColors[2],
                Item.Rarity.Legendary => rarityColors[3],
                _ => rarityColors[0]
            };

            rarityColor.a = 1f;

            if (emitting != null)
            {
                emitting.color = rarityColor;
            }

            if (shadowSprite != null)
            {
                shadowSprite.transform.localPosition = new Vector3(itemData.shadowOffset.x, itemData.shadowOffset.y, 0f);

                shadowSprite.transform.localScale = new Vector3(itemData.shadowScale.x, itemData.shadowScale.y, 1f);
            }

        }

        


    }

}
