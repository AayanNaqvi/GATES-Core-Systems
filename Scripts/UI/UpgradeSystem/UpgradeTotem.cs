using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class UpgradeTotem : MonoBehaviour, IClearDuriingReset
{
    private GameObject player;
    public static UpgradeTotem activeTarget;

    [Header("Glow")]
    private SpriteRenderer sr;
    private Material regularMat;
    public Material outlineMat;


    public GameObject myPromptPrefab;
    private GameObject myActivePrompt;

    public bool menuOpen = false;
    public bool isGlowing;

    [Header("Input")]
    private PlayerInput playerInput;
    private InputAction openMenu;
    private InputAction closeMenu;
    public float interactRadius;
    public LayerMask players;

    [Header("Audio")]
    public AudioClip menuOpenSound;
    public float menuVol;

    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }

    public void Clear()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            regularMat = sr.material;
        }
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            openMenu = playerInput.actions.FindAction("Player/EnterDoorOrUnlockChest");
            closeMenu = playerInput.actions.FindAction("UI/CloseUpgrade");
        }
            



    }

    void Update()
    {
        if (player == null) return;



        HighLightEntity();
        MyInput();
    }

    void HighLightEntity()
    {
        if (sr == null || outlineMat == null || player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);

        if (distance <= interactRadius && !isGlowing && !menuOpen)
        {
            sr.material = outlineMat;

            if (sr.sprite != null)
            {
                sr.material = outlineMat;
            }

            if (myActivePrompt == null && myPromptPrefab != null)
            {
                myActivePrompt = PromptManager.Instance.ShowPrompt(myPromptPrefab);
            }

            isGlowing = true;
        }
        else if ((distance > interactRadius || menuOpen) && isGlowing)
        {
            sr.material = regularMat;
            if (myActivePrompt != null) Destroy(myActivePrompt);
            isGlowing = false;
        }
    }

    void MyInput()
    {
        if (openMenu != null && openMenu.WasPressedThisFrame() && !menuOpen)
        {
            if (CheckForPlayer())
            {
                menuOpen = true;
                player.GetComponent<UpgradeStuff>().menuOpen = true;
                AudioManager.Instance.Play2D(menuOpenSound, menuVol);
                playerInput.SwitchCurrentActionMap("UI");
                UpgradeManager.Instance.OpenUpgradeMenu();

                player.GetComponent<UpgradeStuff>().UpdateText();

                if (myActivePrompt != null) Destroy(myActivePrompt);
            }
        }

        if (closeMenu != null && closeMenu.WasPressedThisFrame() && menuOpen)
        {
            menuOpen = false;
            player.GetComponent<UpgradeStuff>().menuOpen = false;
            AudioManager.Instance.Play2D(menuOpenSound, menuVol);
            playerInput.SwitchCurrentActionMap("Player");
            player.GetComponent<UpgradeStuff>().UpdateText();
            UpgradeManager.Instance.CloseUpgradeMenu();

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



}



