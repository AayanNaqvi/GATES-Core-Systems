using System.Linq;
using UnityEngine;
using System.Collections;

public class WindParticleOrb : MonoBehaviour, IClearDuriingReset
{
    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 100f;


    [Header("References")]
    private Rigidbody2D rb;

    private Vector2 moveDirection;
    private bool destroyed = false;

    private GameObject playerObj;
    public GameObject windParticle;
    public GameObject smokeBombEffect;

    [Header("Sound")]
    public AudioClip explodeSound;
    public float explodeVol;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }
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
    public void Initialize(Vector2 direction, Vector2 startingPt, GameObject player)
    {
        moveDirection = direction.normalized;
        playerObj = player;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        rb.linearVelocity = moveDirection * speed;
        StartCoroutine(SpawnDashTrail(lifetime));
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Wall"))
        {
            Explode();
        }
    }

    void Explode()
    {
        Vector3 safeTeleportSpot = transform.position - (Vector3)(moveDirection * 0.5f);
        playerObj.transform.position = safeTeleportSpot;

        AudioManager.Instance.Play3D(explodeSound, transform.position, explodeVol);

        GameObject smoke = Instantiate(smokeBombEffect, transform.position, Quaternion.identity);
        Destroy(smoke, 0.8f);

        Destroy(gameObject);
    }


    private IEnumerator SpawnDashTrail(float duration)
    {
        float elapsed = 0f;
        

        float spawnInterval = 0.05f;
        float coneSpread = 150f;

        while (elapsed < duration)
        {
            Vector2 backwardDir = -moveDirection;

            float randomTurn = UnityEngine.Random.Range(-coneSpread / 2f, coneSpread / 2f);

            Vector2 coneDirection = Quaternion.Euler(0f, 0f, randomTurn) * backwardDir;

            float randomDistance = UnityEngine.Random.Range(0.2f, 0.8f);
            Vector3 spawnPos = transform.position + (Vector3)(coneDirection * randomDistance);

            float angle = Mathf.Atan2(coneDirection.y, coneDirection.x) * Mathf.Rad2Deg;
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle);

            Instantiate(windParticle, spawnPos, spawnRotation);

            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }

    }




}
