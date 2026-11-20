using System.Collections.Generic;
using UnityEngine;

public class InitiatorSpeedBoost : MonoBehaviour
{

    [Header("Behavior")]
    public float speedMultiplier;

    private HashSet<Collider2D> buffedObjects = new HashSet<Collider2D>();

  
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!buffedObjects.Contains(collision))
        {
            IBuffed ib = collision.GetComponent<IBuffed>();
            if (ib != null)
            {
                ib.ApplySpeedBuff(speedMultiplier);
                buffedObjects.Add(collision);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (buffedObjects.Contains(collision))
        {
            IBuffed ib = collision.GetComponent<IBuffed>();
            if (ib != null)
            {
                ib.RemoveSpeedBuff();
            }
            buffedObjects.Remove(collision);
        }
    }

    private void OnDestroy()
    {
        foreach (Collider2D col in buffedObjects)
        {
            if (col != null)
            {
                IBuffed ib = col.GetComponent<IBuffed>();
                if (ib != null) ib.RemoveSpeedBuff();
            }
        }
        buffedObjects.Clear();
    }
}
