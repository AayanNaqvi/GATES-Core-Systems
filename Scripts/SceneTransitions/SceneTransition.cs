using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneTransition : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed;

    public GameObject deathscreen;
    private CanvasGroup dsC;


    public AudioClip pauseSound;
    public float pauseVol;

    private void Start()
    {
        if (fadeImage != null)
        {
            Color currCol = fadeImage.color;
            currCol.a = 1f;
            fadeImage.color = currCol; 

            StartCoroutine(Fade(0f, fadeImage));
        
        }
        if(deathscreen != null)
        {
            dsC = deathscreen.gameObject.GetComponent<CanvasGroup>();
        }
    }



    public void LoadScene(int index)
    {
        if (index == 1)
            Time.timeScale = 1f;

        AudioManager.Instance.FadeOutMusic(1f);
        StartCoroutine(FadeToGame(index));
        
    }

    public IEnumerator FadeToGame(int index)
    {
        if (GameManager.Instance != null && deathscreen != null && GameManager.Instance.deathScreenActive)
        {
            StartCoroutine(FadeGameOverScreen(0f));
        }
        else
        {
            StartCoroutine(Fade(1f, fadeImage));
        }

        yield return new WaitForSecondsRealtime(2f);


        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);


        while (!asyncLoad.isDone)
        {
            yield return null;
        }

    }
    public void LeaveGame()
    {
        Application.Quit();
    }

    private IEnumerator Fade(float targetAlpha, Image fadeIMG)
    {
        Color currentColor = fadeIMG.color;



        while (!Mathf.Approximately(currentColor.a, targetAlpha))
        {

            currentColor.a = Mathf.MoveTowards(currentColor.a, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            fadeIMG.color = currentColor;


            yield return null;
        }

    }

    private IEnumerator FadeGameOverScreen(float targetAlpha)
    {
        if (deathscreen != null && !deathscreen.activeSelf)
        {
            deathscreen.SetActive(true);
        }

        while (!Mathf.Approximately(dsC.alpha, targetAlpha))
        {
            dsC.alpha = Mathf.MoveTowards(dsC.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        dsC.alpha = targetAlpha;

        
    }

    public void PlaySound()
    {
        AudioManager.Instance.Play2D(pauseSound, pauseVol);
    }
}
