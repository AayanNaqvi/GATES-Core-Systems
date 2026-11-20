using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class NuggetCollector : MonoBehaviour
{
    public int nuggetCount = 0;
    public TextMeshProUGUI countText;

    private CanvasGroup collectorPanel;
    public GameObject collectorPanelObj;
    public float fadeDuration;
    public float stayDuration;

    private Coroutine fadeAnimation;

    [Header("Audio")]
    public AudioClip pickupNugget;
    public float pickupVol;

    private void Start()
    {
        collectorPanel = collectorPanelObj.GetComponent<CanvasGroup>();
        collectorPanel.alpha = 0f;
        UpdateText();
    }

    public void CollectNugget(int nuggetValue)
    {
        nuggetCount += nuggetValue;
        AudioManager.Instance.Play2D(pickupNugget, pickupVol);
        UpdateText();

        if (fadeAnimation != null) StopCoroutine(fadeAnimation);
        fadeAnimation = StartCoroutine(FadingAnimation());
    }

    public void UpdateText()
    {
        countText.text = nuggetCount.ToString();
    }

    public IEnumerator FadingAnimation()
    {
        collectorPanel.alpha = 1f;

        yield return new WaitForSeconds(stayDuration);


        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            collectorPanel.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        collectorPanel.alpha = 0f;
        fadeAnimation = null;
    }

    public void ResetNuggets()
    {
        nuggetCount = 0;
        UpdateText();
    }

}
