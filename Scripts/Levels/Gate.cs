using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TextBoxInteracter))]
public class Gate : MonoBehaviour, IClearDuriingReset
{

    public static Gate activeTarget;

    [Header("Behavior")]
    public bool unlocked = false;
    private TextBoxInteracter tbI;
    private PlayerInput playerInput;
    private InputAction interactAction;
    [SerializeField] private LayerMask players;
    public GameObject player;

    [TextArea]
    [SerializeField] private string unlockMessage;
    [TextArea]
    [SerializeField] private string lockMessage;

    [Header("Animation")]
    public Animator gateAnim;
    


    public GameObject stageToGoTo;

    public GameObject myPromptPrefab;
    private GameObject myActivePrompt;
    private bool isPromptShowing = false;


    [Header("Audio")]
    public AudioClip gateOpen;
    public AudioClip gateLocked;
    public float gateVol = 0.3f;



    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }

        if (myActivePrompt != null) Destroy(myActivePrompt);
    }

    void Start()
    {
        tbI = GetComponent<TextBoxInteracter>();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if(gateAnim == null)
        {
            gateAnim = GetComponent<Animator>();
        }
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            interactAction = playerInput.actions["EnterDoorOrUnlockChest"];
        }
        

        tbI.stages = 1;


        if (tbI.messages == null || tbI.messages.Length == 0)
        {
            tbI.messages = new string[1];
        }

        unlocked = CheckForActiveSpawners();
        UpdateGateText();


        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    // Update is called once per frame
    

    private void Update()
    {
        bool isCurrentlyUnlocked = CheckForActiveSpawners();

        if (isCurrentlyUnlocked != unlocked)
        {
            unlocked = isCurrentlyUnlocked;
            AudioManager.Instance.FadeOutMusic();
            UpdateGateText();
        }

        if (player != null)
        {
            float distance = Vector2.Distance(player.transform.position, transform.position);

            if (distance <= tbI.interactRadius && unlocked && !isPromptShowing)
            {
                if (myPromptPrefab != null) myActivePrompt = PromptManager.Instance.ShowPrompt(myPromptPrefab);
                isPromptShowing = true;
            }

            else if ((distance > tbI.interactRadius || !unlocked) && isPromptShowing)
            {
                if (myActivePrompt != null) Destroy(myActivePrompt);
                isPromptShowing = false;
            }
        }

        unlocked = CheckForActiveSpawners();
        MyInput();
    }


    public void Clear()
    {
        Destroy(gameObject);
    }
    private void UpdateGateText()
    {
        if (stageToGoTo != null)
        {
            Stage targetStage = stageToGoTo.GetComponent<Stage>();
            if (targetStage != null && !targetStage.rewardStage)
            {
                return;
            }
        }


        if (!unlocked)
        {
            tbI.messages[0] = lockMessage;
        }
        else
        {
            tbI.messages[0] = unlockMessage;
        }
    }

    void MyInput()
    {
        if (interactAction == null) return;


        if (interactAction.WasPressedThisFrame())
        {
            if(unlocked && CheckForPlayer())
            {
                
                Debug.Log(GameManager.currentStage);

                gateAnim.SetBool("Open", true);
                AudioManager.Instance.Play2D(gateOpen, gateVol);

                if (myActivePrompt != null) Destroy(myActivePrompt);
                GameManager.Instance.StartTransition(stageToGoTo);
            }
            if(!unlocked)
            {
                AudioManager.Instance.Play2D(gateLocked, gateVol);
            }
        }
    }

    public bool CheckForActiveSpawners()
    {
        return Spawner.activeSpawners <= 0 && Enemy.globalEnemyCount <= 0;
    }

    public bool CheckForPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(gameObject.transform.position, tbI.interactRadius, players);
        return colliders.Length > 0;
    }


}
