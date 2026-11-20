using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Follow Logic")]
    [SerializeField, Range(0.5f, 20f)] private float followSpeed;
    [HideInInspector] public bool cameraFollow = true;

    
    private Transform target;

    private void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        camReset();
    }

    void Update()
    {
        
        
        if (cameraFollow)
        {
            Vector3 newPos = new Vector3(target.position.x, target.position.y, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
        }
    }

    public void CamLock()
    {
        gameObject.transform.position = new Vector3(target.position.x, target.position.y, -10f);
    }

    public void camReset()
    {
        transform.position = new Vector3(target.position.x, target.position.y, -10f);
    }
}
