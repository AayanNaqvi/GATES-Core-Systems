using UnityEngine;



[RequireComponent(typeof(SpriteRenderer))]
public class AfterImage : MonoBehaviour, IClearDuriingReset
{
    [SerializeField] private float fadeSpeed;
    private SpriteRenderer sr;



    

    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    public void Setup(Sprite currentSprite, Transform targetTransform, Color tintColor)
    {
        
        

        sr.sprite = currentSprite;

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        transform.localScale = targetTransform.localScale;

        sr.color = tintColor;
        sr.sortingOrder = 2;

    }

    // Update is called once per frame
    void Update()
    {
        Color c = sr.color;
        c.a -= fadeSpeed * Time.deltaTime;
        sr.color = c;

        if (c.a <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void Clear()
    {
        Destroy(gameObject);
    }
}
