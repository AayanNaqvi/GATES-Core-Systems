using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class TorchBreak : MonoBehaviour
{
    public GameObject torchLight;
    private Animator anim;
    public GameObject fizzleEffect;
    public GameObject reigniteEffect;
    private AudioSource torchSound;
    public AudioClip breakSound;
    public AudioClip reigniteSound;
    public float breakVol;


    public bool broken = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        torchSound = GetComponentInChildren<AudioSource>();
    }
    public void TorchBreaker()
    {
        TorchLights tL = torchLight.GetComponent<TorchLights>();
        anim.SetBool("Break", true);
        torchSound.Stop();
        AudioManager.Instance.Play3D(breakSound, transform.position, breakVol);
        tL.GetComponent<Light2D>().intensity = 0f;
        
        
        if(tL.doFlicker)
        {
            GameObject fizzle = Instantiate(fizzleEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
            Destroy(fizzle, 1f);
            tL.doFlicker = false;
        }

        broken = true;
        
    }

    public void TorchReignite()
    {
        TorchLights tL = torchLight.GetComponent<TorchLights>();

        anim.SetBool("Break", false);
        torchSound.Play();
        AudioManager.Instance.Play3D(reigniteSound, transform.position, breakVol);
        tL.GetComponent<Light2D>().intensity = 0.35f;


        GameObject reIg = Instantiate(reigniteEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        Destroy(reIg, 1f);


        if (!tL.doFlicker)
        {
            tL.doFlicker = true;
        }

        broken = false;
    }
}
