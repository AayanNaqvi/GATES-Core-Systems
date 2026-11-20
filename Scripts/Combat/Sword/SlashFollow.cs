using UnityEngine;

public class SlashFollow : MonoBehaviour
{
    

    public Transform followTarget;    
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;

        followTarget = GameObject.FindGameObjectWithTag("Weapon Holder").transform;
    }

    void LateUpdate()
    {
        if (followTarget != null)
        {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }
}
