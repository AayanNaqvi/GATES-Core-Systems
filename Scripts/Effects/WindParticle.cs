using UnityEngine;
using System.Collections;

public class WindParticle : MonoBehaviour, IClearDuriingReset
{
    public float lifetime = 0.75f;
    public float driftSpeed = 3f;


    
    

    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }

    private void Start()
    {
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color startColor = sr.color;
        float elapsed = 0f;
        if (sr == null)
        {
            Destroy(gameObject);
            yield break; 
        }
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;


            float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);


            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            transform.Translate(Vector3.right * driftSpeed * Time.deltaTime, Space.Self);

            yield return null;
        }
        Destroy(gameObject);
    }

    public void Clear()
    {
        Destroy(gameObject);
    }
}
