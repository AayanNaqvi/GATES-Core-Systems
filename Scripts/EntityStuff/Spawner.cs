using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class Spawner : MonoBehaviour, IClearDuriingReset
{
    public static int activeSpawners;


    [Header("Behavior")]
    public bool alpha = false;
    [SerializeField] private float spawnRange;
    [SerializeField] private GameObject enemyToSpawn;
    [SerializeField] private float spawnTimer;
    [SerializeField, Range(1, 5)] private int minEnemiesSpawned;
    [SerializeField, Range(1, 15)] private int maxEnemiesSpawned;
    public float currentSpawnTime;
    [SerializeField] private float enemyClearanceRadius;
    public bool active = true;
    private bool wasActive = true;

    private int maxEnemiesInLevel = 30;
    

    [Header("References")]
    public Tilemap background;
    public Tilemap wall;
    public LayerMask wallLayer;
    public LayerMask playerLayer;
    private Collider2D bc2d;

    [Header("Effects")]
    public GameObject spawnEffect;
    public ParticleSystem emitter;
    private ParticleSystem activeEmitter;
    public GameObject spawnerLight;

    [Header("Nuggets")]
    public GameObject[] possibleNuggets;
    public GameObject[] nuggets;

    [Header("Audio")]
    public AudioClip spawnNoise;
    public AudioClip nuggetSound;
    public float vol;


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


    void Start()
    {
        activeSpawners++;
        
        if (background == null || wall == null)
        {
            background = GameObject.FindGameObjectWithTag("Background").GetComponent<Tilemap>();
            wall = GameObject.FindGameObjectWithTag("Wall").GetComponent<Tilemap>();
        }

        SetNuggets();



        currentSpawnTime = spawnTimer;
        bc2d = GetComponent<Collider2D>();
        Collider2D enemyCol = enemyToSpawn.GetComponent<Collider2D>();
        
        

        activeEmitter = Instantiate(emitter, transform.position, Quaternion.identity);
        SpawnEnemies();


        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (wasActive && !active)
        {
            wasActive = false; 

            if (activeEmitter != null)
            {
                activeEmitter.Stop();
                Destroy(activeEmitter.gameObject, 3f);

            }
            if(spawnerLight != null)
            {
                Destroy(spawnerLight);
            }
        }
        if (!active) return;


        currentSpawnTime -= Time.deltaTime * 1f;

        if(currentSpawnTime <= 0)
        {
            SpawnEnemies();
            currentSpawnTime = spawnTimer;
        }

    }

    public void Clear()
    {
        Destroy(gameObject);
    }

    public void SpawnEnemies()
    {
        if (Enemy.globalEnemyCount >= maxEnemiesInLevel)
            return;


        int numOfEnemies = UnityEngine.Random.Range(minEnemiesSpawned, maxEnemiesSpawned + 1);

        for(int i = 0; i < numOfEnemies; i++)
        {
            Vector2? randomPoint = CheckForValidSpawnPoint();
            

            if(randomPoint.HasValue)
            {
                Instantiate(enemyToSpawn, randomPoint.Value, Quaternion.identity);
                GameObject spawnParticle = Instantiate(spawnEffect, randomPoint.Value + new Vector2(0, -1f), Quaternion.Euler(-90f, 0f, 0f));
                GameObject spawnerSpawningParticle = Instantiate(spawnEffect, transform.position + new Vector3(0, -1f, 0), Quaternion.Euler(-90f, 0f, 0f));
                Destroy(spawnParticle, 0.75f);
                Destroy(spawnerSpawningParticle, 0.75f);
            }
                
        }

        AudioManager.Instance.Play3D(spawnNoise, transform.position, vol);
    }


    private Vector2? CheckForValidSpawnPoint()
    {

        bool hasFloor = false;
        bool hasWall = true;
        Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * spawnRange;
        

        int maxAttempts = 30; 

        for (int i = 0; i < maxAttempts; i++)
        {
            randomPoint = UnityEngine.Random.insideUnitCircle * spawnRange;
            randomPoint += new Vector2(transform.position.x, transform.position.y);

            Vector3Int cellPosition = background.WorldToCell(randomPoint);


            hasFloor = background.HasTile(cellPosition);
            hasWall = wall.HasTile(cellPosition);

            
            bool touchingSpawner = bc2d.OverlapPoint(randomPoint);

            Collider2D wallClip = Physics2D.OverlapCircle(randomPoint, enemyClearanceRadius, wallLayer);
            Collider2D playerClip = Physics2D.OverlapCircle(randomPoint, enemyClearanceRadius, playerLayer);
            bool tooCloseToWall = (wallClip != null || playerClip != null);


            if (hasFloor && !hasWall && !touchingSpawner && !tooCloseToWall)
            {
                return randomPoint;
            }


        }

        return null;
    }


    public void DropNuggets()
    {
        for (int i = nuggets.Length - 1; i >= 0; i--)
        {
            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f);
            Vector3 spawnPos = transform.position;
            spawnPos.z = 0f;

            GameObject droppedNugget = Instantiate(nuggets[i], spawnPos, Quaternion.identity);
            droppedNugget.GetComponent<Rigidbody2D>().AddForce(randomOffset * 7.5f, ForceMode2D.Impulse);

            nuggets[i] = null;
        }

        AudioManager.Instance.Play3D(nuggetSound, transform.position, vol);
    }

    public void SetNuggets()
    {
        int enemDiff = maxEnemiesSpawned - minEnemiesSpawned;
        if(alpha)
        {
            nuggets = new GameObject[10];
            int rand = UnityEngine.Random.Range(2, 4);
            for (int i = 0; i < rand; i++)
            {
                nuggets[i] = possibleNuggets[2]; //Add gold nuggets
            }
            for (int i = rand; i < nuggets.Length - 2; i++)
            {
                nuggets[i] = possibleNuggets[1]; //Add some silver nuggets
            }
            for (int i = nuggets.Length - 2; i < nuggets.Length; i++)
            {
                nuggets[i] = possibleNuggets[0]; //Add 2 bronze nuggets
            }
        }
        else if(enemDiff <= 2 || spawnTimer >= 25f)
        {
            nuggets = new GameObject[UnityEngine.Random.Range(2, 5)];
            for(int i = 0; i  < nuggets.Length; i++)
            {
                int rand = UnityEngine.Random.Range(0, 3);
                if(rand == 0 || rand == 1)
                {
                    nuggets[i] = possibleNuggets[0];
                }
                else
                {
                    nuggets[i] = possibleNuggets[1];
                }
            }
        }
        else
        {
            nuggets = new GameObject[7];
            int rand = UnityEngine.Random.Range(1, 3);
            for (int i = 0; i < rand; i++)
            {
                nuggets[i] = possibleNuggets[2]; //Add gold nuggets
            }
            for (int i = rand; i < nuggets.Length - 2; i++)
            {
                nuggets[i] = possibleNuggets[1]; //Add some silver nuggets
            }
            for (int i = nuggets.Length - 2; i < nuggets.Length; i++)
            {
                nuggets[i] = possibleNuggets[0]; //Add 2 bronze nuggets
            }

        }
        

    }
    private void OnDestroy()
    {
        if (activeEmitter != null)
        {
            Destroy(activeEmitter.gameObject);

        }
        if(spawnerLight != null)
        {
            Destroy(spawnerLight);
        }

       
        
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.green;

        
        Gizmos.DrawWireSphere(transform.position, spawnRange);
    }

}
