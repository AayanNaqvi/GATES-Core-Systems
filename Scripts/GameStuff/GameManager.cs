using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    private GameObject player;
    public GameObject buffBarHolder;
    public GameObject sword;
    private PlayerMovement playerMovementScript;
    public Camera mainCamera;
    public GameObject firstLevel;
    public LayerMask enemies;
    public LayerMask gates;

    public int easyLevel;
    public int mediumLevel;
    public int hardLevel;
    public int extremeLevel;

    public GameObject upgradeWheel1;
    public GameObject upgradeWheel2;
    public GameObject upgradeWheel3;

    public TextMeshProUGUI testText;

    [Header("Fading")]
    public Image fadeImage;
    public float fadeSpeed = 1f;
    public float GOfadeSpeed = 1f;

    public GameObject gameOverScreen;
    public Button tryAgainButton;
    private CanvasGroup gameOverCV;
    public TextMeshProUGUI tips;
    [TextArea] public string[] generalTips;

    public GameObject[] beginnerStages;
    public GameObject[] easyStages;
    public GameObject[] mediumStages;
    public GameObject[] hardStages;
    public GameObject[] extremeStages;

    public static int currentStage = 1;

    public bool battleStage = false;

    public int currentDiff = 0;
    
    public static List<IClearDuriingReset> resettableObjects = new List<IClearDuriingReset>();

    private PlayerInput playerInp;
    private InputAction tryAgain;
    private InputAction mainMenu;
    public bool deathScreenActive = false;


    public AudioClip diffAdvance;
    public AudioClip battleTheme;
    public float sfxVol = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Enemy.globalEnemyCount = 0;
        Spawner.activeSpawners = 0;
        resettableObjects.Clear();

    }

    private void Start()
    {
        if(gameOverCV == null)
        {
            gameOverCV = gameOverScreen.GetComponent<CanvasGroup>();
            tryAgainButton.interactable = false;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            playerInp = player.GetComponent<PlayerInput>();
            playerMovementScript = player.GetComponent<PlayerMovement>();

            tryAgain = playerInp.actions.FindAction("UI/TryAgain");
            mainMenu = playerInp.actions.FindAction("UI/MainMenuDeathScrn");


            playerInp.SwitchCurrentActionMap("Player");
        }

        
    }

    private void Update()
    {
        testText.SetText((Enemy.globalEnemyCount + Spawner.activeSpawners).ToString());
        MyInput();

    }

    private void MyInput()
    {
        if(tryAgain.WasPressedThisFrame() && deathScreenActive)
        {
            RestartGameAgain();
        }
    }

    public void StartTransition(GameObject nextStage)
    {
        StartCoroutine(TransitionSequence(nextStage));
    }


    public IEnumerator TransitionSequence(GameObject nextStage)
    {
        if (playerInp != null) playerInp.DeactivateInput();

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
        }

        


        Stage currentStageObj = FindFirstObjectByType<Stage>();
        battleStage = !battleStage;
        if (!battleStage)
        {
            currentStage++;
            CheckAndSaveMaxStage();
        }
        AdvanceDifficulty();

        StartCoroutine(Fade(1f));
        yield return new WaitForSeconds(fadeSpeed);

        if (currentStageObj != null)
        {
            Destroy(currentStageObj.gameObject);
        }
        DestroyAllThings();
        yield return null;


        GameObject newStageObj = Instantiate(nextStage, Vector3.zero, Quaternion.identity);
        Stage newStage = newStageObj.GetComponent<Stage>();

        if (newStage != null && newStage.spawnPoint != null)
        {
            player.transform.position = newStage.spawnPoint.position;
        }
        else
        {
            Debug.LogWarning("No spawn point found on the new stage!");
            player.transform.position = Vector3.zero;
        }

        mainCamera.transform.position = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            mainCamera.transform.position.z
        );



        if (AstarPath.active != null)
        {
            AstarPath.active.Scan();
        }

        StartCoroutine(Fade(0f));
        yield return new WaitForSeconds(fadeSpeed);
        if (playerInp != null) playerInp.ActivateInput();
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        if(battleStage)
        {
            AudioManager.Instance.PlayMusic(battleTheme);
        }

    }

    public void AdvanceDifficulty()
    {
        if (currentDiff == 0 && currentStage >= easyLevel)
        {
            currentDiff = 1;
            AudioManager.Instance.Play2D(diffAdvance, sfxVol);
        }
        else if (currentDiff == 1 && currentStage >= mediumLevel)
        {
            currentDiff = 2;
            AudioManager.Instance.Play2D(diffAdvance, sfxVol);
        }
        else if (currentDiff == 2 && currentStage >= hardLevel)
        {
            currentDiff = 3;
            AudioManager.Instance.Play2D(diffAdvance, sfxVol);
        }
        else if(currentDiff == 3 && currentStage >= extremeLevel)
        {
            currentDiff = 4;
            AudioManager.Instance.Play2D(diffAdvance, sfxVol);
        }
    }


    private IEnumerator Fade(float targetAlpha)
    {
        Color currentColor = fadeImage.color;

        
        while (!Mathf.Approximately(currentColor.a, targetAlpha))
        {
            
            currentColor.a = Mathf.MoveTowards(currentColor.a, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            fadeImage.color = currentColor;

            
            yield return null;
        }

    }

    private IEnumerator FadeGameOverScreen(float targetAlpha)
    {
        if (gameOverScreen != null && !gameOverScreen.activeSelf)
        {
            gameOverScreen.SetActive(true);
        }

        while (!Mathf.Approximately(gameOverCV.alpha, targetAlpha))
        {
            gameOverCV.alpha = Mathf.MoveTowards(gameOverCV.alpha, targetAlpha, GOfadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        gameOverCV.alpha = targetAlpha;

        if (targetAlpha == 0f && gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }
        else
        {
            tryAgainButton.interactable = true;
        }
    }

    public void GameOver()
    {
        CheckAndSaveMaxStage();

        if (generalTips != null && generalTips.Length > 0 && tips != null)
        {
            int rand = Random.Range(0, generalTips.Length);
            tips.SetText(generalTips[rand]);
        }


        AudioManager.Instance.FadeOutMusic();
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {

        if (playerInp != null) playerInp.DeactivateInput();

        if (playerMovementScript != null) playerMovementScript.enabled = false;




        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {

            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = Vector2.zero;
                enemyRb.bodyType = RigidbodyType2D.Static;
            }

            if (enemy.enemAnimator != null) enemy.enemAnimator.enabled = false;

            
            enemy.enabled = false;

            
            var ai = enemy.GetComponent<Pathfinding.AIPath>();
            if (ai != null) ai.enabled = false;
        }




        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        yield return new WaitForSeconds(0.15f);

        float failsafeTimer = 0f;


        while (rb.linearVelocity.magnitude > 0.1f && failsafeTimer < 1.0f)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 10f);
            failsafeTimer += Time.deltaTime;
            yield return null;
        }

        
        rb.linearVelocity = Vector3.zero;

        

        
        yield return new WaitForSeconds(1f);



        Time.timeScale = 0f;


        yield return StartCoroutine(Fade(1f));

        yield return new WaitForSecondsRealtime(1f);

        yield return StartCoroutine(FadeGameOverScreen(1f));

        playerInp.SwitchCurrentActionMap("UI");
        deathScreenActive = true;
        if (playerInp != null) playerInp.ActivateInput();
        if (gameOverCV != null) gameOverCV.blocksRaycasts = true;

    }

    public void RestartGameAgain()
    {
        if (gameOverCV != null) gameOverCV.blocksRaycasts = false;
        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return StartCoroutine(FadeGameOverScreen(0f));


        PlayerInput pInput = player.GetComponent<PlayerInput>();

        currentStage = 1;
        currentDiff = 0;
        battleStage = false;
        Enemy.globalEnemyCount = 0;
        Spawner.activeSpawners = 0;
        player.GetComponent<Inventory>().ClearAllSlots();
        player.GetComponent<UpgradeStuff>().ClearBuffs();
        player.GetComponent<NuggetCollector>().ResetNuggets();


        sword.GetComponent<SwordCombat>().ResetDamageAndBuffs();
        playerMovementScript.ClearSpeedBoost();


        foreach (Transform g in buffBarHolder.transform)
        {
            Destroy(g.gameObject);
        }



        if (upgradeWheel1 != null) upgradeWheel1.GetComponent<UpgradeWheelAnimator>()?.ClearStuff();
        if (upgradeWheel2 != null) upgradeWheel2.GetComponent<UpgradeWheelAnimator>()?.ClearStuff();
        if (upgradeWheel3 != null) upgradeWheel3.GetComponent<UpgradeWheelAnimator>()?.ClearStuff();


        PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
        if (pHealth != null)
        {
            pHealth.currHealth = pHealth.maxHealth;
            pHealth.isDead = false;
            pHealth.UpdateHealthUI();
            pHealth.ResetSliders();

            if (pHealth.playerAnim != null)
            {
                pHealth.playerAnim.Play("Idle");
            }

        }

        DestroyAllThings();
        Stage currentStageObj = FindFirstObjectByType<Stage>();
        if (currentStageObj != null)
        {
            Destroy(currentStageObj.gameObject);
        }
        yield return null;


        GameObject newStageObj = Instantiate(firstLevel, Vector3.zero, Quaternion.identity);
        Stage newStage = newStageObj.GetComponent<Stage>();


        if (newStage != null && newStage.spawnPoint != null)
        {
            player.transform.position = newStage.spawnPoint.position;
        }

        mainCamera.transform.position = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            mainCamera.transform.position.z
        );


        if (AstarPath.active != null)
        {
            AstarPath.active.Scan();
        }
        Time.timeScale = 1f;

        yield return StartCoroutine(Fade(0f));
        deathScreenActive = false;
        playerInp.SwitchCurrentActionMap("Player");

        if (playerInp != null) playerInp.ActivateInput();
        
        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }

    public void DestroyAllThings()
    {
        for (int i = resettableObjects.Count - 1; i >= 0; i--)
        {
            resettableObjects[i].Clear();
        }
        resettableObjects.Clear();


    }

    public void CheckAndSaveMaxStage()
    {
        int savedMaxStage = PlayerPrefs.GetInt("MaxRunStage", 1);

        if (currentStage > savedMaxStage)
        {
            PlayerPrefs.SetInt("MaxRunStage", currentStage);
            PlayerPrefs.Save();
        }
    }
}