using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuffBars : MonoBehaviour
{
    private Slider slider;
    public float fadeDuration = 1;
    private CanvasGroup cv;
    

    public void Initialize(float maxValue)
    {
        slider = GetComponent<Slider>();
        
        slider.maxValue = maxValue;
        cv = GetComponent<CanvasGroup>();

        if (cv != null) cv.alpha = 0f;

        StartCoroutine(DrainBarRoutine(maxValue));

    }

    public IEnumerator DrainBarRoutine(float maxValue)
    {
        slider.value = maxValue;
        float elapsed = 0f;

        while (elapsed < maxValue)
        {
            elapsed += Time.deltaTime;
            if (slider != null)
            {
                slider.value = maxValue - elapsed;
            }

            if (cv != null)
            {
                if (fadeDuration <= 0f)
                {
                    cv.alpha = 1f;
                }
                else if (elapsed < fadeDuration)
                {
                    cv.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                }
                else if (maxValue - elapsed <= fadeDuration)
                {
                    float fadeOutElapsed = fadeDuration - (maxValue - elapsed);
                    cv.alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeDuration);
                }
                else
                {
                    cv.alpha = 1f;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
