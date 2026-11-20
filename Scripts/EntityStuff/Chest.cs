using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class Chest : MonoBehaviour, IClearDuriingReset
{
    public static Chest activeTarget;

    [Header("Behavior")]
    private PlayerInput playerInput;
    private InputAction interactAction;
    [SerializeField] private LayerMask players;
    private GameObject player;
    public float interactRadius;

    [Header("Animation")]
    private Animator chestAnim;
    private Material regularMat;
    public Material outlineMat;
    private SpriteRenderer sr;
    public Light2D lightEmit;
    public GameObject chestOpenEffect;

    private bool isGlowing = false;
    private bool opened = false;

    [Header("Item Logic")]
    public ItemLootPool[] itemPools;
    public NuggetLootPool nuggetPool;

    private List<Item> itemsToDrop = new List<Item>();
    private List<GameObject> nuggetsToDrop = new List<GameObject>();

    public GameObject droppedItem;

    public GameObject myPromptPrefab;
    private GameObject myActivePrompt;

    [Header("Audio")]
    public AudioClip openAudio;
    public float openVol;



    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }

        if (activeTarget == this) activeTarget = null;
        if (myActivePrompt != null) Destroy(myActivePrompt);
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (chestAnim == null)
        {
            chestAnim = GetComponent<Animator>();
        }

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            regularMat = sr.material;
        }

        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            interactAction = playerInput.actions["EnterDoorOrUnlockChest"];
        }


       

        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }

        SetItems();
    }



    private void Update()
    { 
        if(!opened)
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


    public void Clear()
    {
        Destroy(gameObject);
    }
    
    void MyInput()
    {
        if (interactAction == null) return;


        if (interactAction.WasPressedThisFrame())
        {
            if (CheckForPlayer())
            {
                chestAnim.SetBool("Open", true);
                opened = true;
                sr.material = regularMat;
                AudioSource idleAud = GetComponentInChildren<AudioSource>();
                if (idleAud != null) idleAud.Stop();
                AudioManager.Instance.Play2D(openAudio, openVol);

                DropItems();
                Destroy(lightEmit.gameObject);
                GameObject effect = Instantiate(chestOpenEffect, transform.position, Quaternion.identity);

                if (activeTarget == this) activeTarget = null;
                if (myActivePrompt != null) Destroy(myActivePrompt);

                Destroy(effect, 1.2f);
            }
        }
    }

    

    public bool CheckForPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(gameObject.transform.position, interactRadius, players);
        return colliders.Length > 0;
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;


        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    public void DropItems()
    {

        foreach (Item item in itemsToDrop)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            Vector3 spawnPos = transform.position;
            spawnPos.z = 0f;

            GameObject droppedObj = Instantiate(droppedItem, spawnPos, Quaternion.identity);
            droppedObj.GetComponent<Rigidbody2D>().AddForce(randomOffset * 7.5f, ForceMode2D.Impulse);

            PickUpItem pickup = droppedObj.GetComponent<PickUpItem>();
            if (pickup != null)
            {
                pickup.Setup(item);
            }
        }

        foreach (GameObject nuggetObj in nuggetsToDrop)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            Vector3 spawnPos = transform.position;
            spawnPos.z = 0f;

            GameObject droppedNugget = Instantiate(nuggetObj, spawnPos, Quaternion.identity);
            droppedNugget.GetComponent<Rigidbody2D>().AddForce(randomOffset * 7.5f, ForceMode2D.Impulse);
        }

        itemsToDrop.Clear();
        nuggetsToDrop.Clear();

    }

    public void SetItems()
    {
        itemsToDrop.Clear();
        nuggetsToDrop.Clear();

        foreach (ItemLootPool pool in itemPools)
        {
            if (pool.possibleItems.Length > 0)
            {
                int dropCount = Random.Range(pool.minDrops, pool.maxDrops + 1);

                for (int i = 0; i < dropCount; i++)
                {
                    Item randomItem = pool.possibleItems[Random.Range(0, pool.possibleItems.Length)];
                    itemsToDrop.Add(randomItem);
                }
            }
        }

        if (nuggetPool.possibleNuggets.Length > 0)
        {
            int nuggetCount = Random.Range(nuggetPool.minDrops, nuggetPool.maxDrops + 1);
            for (int i = 0; i < nuggetCount; i++)
            {
                GameObject randomNugget = nuggetPool.possibleNuggets[Random.Range(0, nuggetPool.possibleNuggets.Length)];
                nuggetsToDrop.Add(randomNugget);
            }
        }
    }
}


[System.Serializable]
public class ItemLootPool
{
    public string poolName; 
    public Item[] possibleItems;
    public int minDrops;
    public int maxDrops;
}

[System.Serializable]
public class NuggetLootPool
{
    public GameObject[] possibleNuggets;
    public int minDrops;
    public int maxDrops;
}
