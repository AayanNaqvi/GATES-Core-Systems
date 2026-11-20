using UnityEngine;

public class Nugget : MonoBehaviour, IClearDuriingReset
{
    public int value;

    public float startTimer;
    private float currentStartTimer;

    public GameObject parent;


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
    }


    private void Start()
    {
        currentStartTimer = startTimer;
    }

    private void Update()
    {
        currentStartTimer -= Time.deltaTime;
    }

    public void Clear()
    {
        Destroy(parent);
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentStartTimer <= 0)
        {
            collision.gameObject.GetComponent<NuggetCollector>().CollectNugget(value);
            Destroy(parent);
        }
    }


}
