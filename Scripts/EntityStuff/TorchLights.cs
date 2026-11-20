using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;


[RequireComponent(typeof(Light2D))]
public class TorchLights : MonoBehaviour
{
    [SerializeField] private float flickerSpeed;
    public bool doFlicker = true;

    [Header("Light Stuff")]
    private Light2D lighter;
    private float intensityMem;
    [SerializeField, Range(0f, 2f)] private float flickerRange;

    private float randomOffset;

    void Start()
    {
        lighter = GetComponent<Light2D>();
        
        intensityMem = lighter.intensity;

        
        randomOffset = Random.Range(0f, 100f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!doFlicker) return;
        
        float noise = Mathf.PerlinNoise((Time.time * flickerSpeed) + randomOffset, 0f);
        float normalizedNoise = (noise * 2f) - 1f;

        lighter.intensity = intensityMem + (normalizedNoise * flickerRange);
    }

    
}
