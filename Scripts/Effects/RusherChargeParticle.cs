using System.Linq;
using UnityEngine;

public class RusherChargeParticle : MonoBehaviour, IClearDuriingReset
{
    

    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }

    void Start()
    {
        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    public void Clear()
    {
        Destroy(gameObject);
    }
}
