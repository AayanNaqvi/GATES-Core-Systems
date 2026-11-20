using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TextBoxInteracter : MonoBehaviour, IClearDuriingReset
{
    public static TextBoxInteracter activeTarget;

    [Header("References")]
    private PlayerInput playerInput;
    private InputAction interactAction;
    public GameObject textObj;
    public GameObject player;
    public Canvas mainCanvas;
    private GameObject currentTextBox;
    private Material regularMat;
    public Material outlineMat;
    private SpriteRenderer sr;


    [Header("Behavior")]
    public int stages;
    [TextArea] public string[] messages;
    [Range(0.1f, 5f)] public float interactRadius;
    [SerializeField] private LayerMask players;
    [SerializeField] private float maxDistBeforeCollapse;

    private bool textScrolling = false;
    [SerializeField] private int currentStage = 0;

    private bool hintLoaded = false;
    private bool isGlowing = false;

    public GameObject myPromptPrefab;
    private GameObject myActivePrompt;


    [Header("Audio")]
    public AudioClip textSound;
    public float soundVol;


    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            interactAction = playerInput.actions["Interact"];
        }

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            regularMat = sr.material;
        }


        if (mainCanvas == null)
        {
            GameObject canvasObj = GameObject.FindGameObjectWithTag("Main Canvas");
            if (canvasObj != null)
            {
                mainCanvas = canvasObj.GetComponent<Canvas>();
            }
            
        }

        
    }

    private void OnDisable()
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

    // Update is called once per frame
    void Update()
    {
        if (!hintLoaded)
        {
            Gate g = GetComponent<Gate>();
            if (g != null && g.stageToGoTo != null)
            {
                InitializeTextSign(g);
                hintLoaded = true;
            }
        }

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
        }
        else
        {
            HighLightEntity(false);
        }

        if (activeTarget == this || textScrolling)
        {
            MyInput();
        }

        CollapseIfPlayerIsAway();
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>().isDead)
        {
            textScrolling = false;
            currentStage = 0;
            Destroy(currentTextBox, 0.25f);
        }
    }

    void CollapseIfPlayerIsAway()
    {
        if(Vector2.Distance(player.transform.position, transform.position) >= maxDistBeforeCollapse)
        {
            textScrolling = false;
            currentStage = 0;
            Destroy(currentTextBox, 0.25f);
        }
    }

    
    void MyInput()
    {
        if (interactAction == null) return;

        if (interactAction.WasPressedThisFrame())
        {
            if(!textScrolling) CheckForPlayer();
            else
            {
                TypewriterBox twb = currentTextBox.GetComponent<TypewriterBox>();

                if (twb != null)
                {
                    
                    if (twb.isTyping)
                    {
                        twb.FinishTypingEarly();
                        AudioManager.Instance.Play2D(textSound, soundVol);
                    }
                    else
                    { 
                        currentStage++;
                        EnterTextScroller(twb);
                        AudioManager.Instance.Play2D(textSound, soundVol);
                    }
                }
            }
        }

    }

    void CheckForPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(gameObject.transform.position, interactRadius, players);
        if(colliders.Length > 0)
        {
            
            
            TextManager.FindText();
            if(!TextManager.textOnScreen)
            {
                currentTextBox = Instantiate(textObj, mainCanvas.transform);
                TypewriterBox twb = currentTextBox.GetComponent<TypewriterBox>();

                if(twb != null)
                {
                    textScrolling = true;
                    EnterTextScroller(twb);
                    AudioManager.Instance.Play2D(textSound, soundVol);
                }
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

    void EnterTextScroller(TypewriterBox box)
    {
        
        if(currentStage < stages)
        {
            box.fullText = messages[currentStage];
            if (currentStage != 0)
            {
                StopAllCoroutines();
                StartCoroutine(box.TypeRoutine());
            }
            
        }
        else
        {
            box.Hide();
            textScrolling = false;
            currentStage = 0;
            Destroy(currentTextBox, 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;


        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    private void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }

        if (activeTarget == this) activeTarget = null;
        if (myActivePrompt != null) Destroy(myActivePrompt);


        Destroy(currentTextBox);
    }

    public void InitializeTextSign(Gate g)
    {
        Stage targetStage = g.stageToGoTo.GetComponent<Stage>();
        if (targetStage != null && !targetStage.rewardStage)
        {
            
            if (targetStage.gateMessages != null && targetStage.gateMessages.Length > 0)
            {
                int index = Random.Range(0, targetStage.gateMessages.Length);
                messages[0] = targetStage.gateMessages[index];
            }
        }
    }

}
