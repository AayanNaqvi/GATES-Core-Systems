using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TypewriterBox : MonoBehaviour, IClearDuriingReset
{
    [Header("References")]
    [Tooltip("Optional - will auto-find the first child TextMeshProUGUI if left empty")]
    [SerializeField] private TextMeshProUGUI tmp;

    [Header("Typewriter Settings")]
    [TextArea]public string fullText = "";
    [SerializeField, Min(0.001f)] private float typingSpeed = 0.03f; 

    [Header("Intro animation (before typing)")]
    [SerializeField] private float introDuration = 0.45f;
    [SerializeField] private float introMoveDistance = 16f; 
    [SerializeField] private bool autoPlayOnStart = true;

    private CanvasGroup canvasGroup;
    private RectTransform rect;
    private Vector2 originalAnchoredPos;
    private Vector2 startAnchoredPos;
    private Coroutine typingCoroutine;

    public bool isTyping = false;

    [Header("Audio")]
    public AudioClip tickSound;
    public float tickVolume = 0.3f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (tmp == null)
            tmp = GetComponentInChildren<TextMeshProUGUI>(true);

        originalAnchoredPos = rect.anchoredPosition;
        startAnchoredPos = originalAnchoredPos - new Vector2(0f, introMoveDistance);

        if (tmp != null)
        {
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.TopLeft;
        }
    }

    public void Clear()
    {
        Destroy(gameObject);
    }

    

    void OnDestroy()
    {
        if (GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Remove(this);
        }
    }
    private void Start()
    {
        if (tmp != null)
            tmp.text = "";

        canvasGroup.alpha = 0f;
        rect.anchoredPosition = startAnchoredPos;

        if (autoPlayOnStart)
            StartCoroutine(IntroThenType());


        if (!GameManager.resettableObjects.Contains(this))
        {
            GameManager.resettableObjects.Add(this);
        }
    }

    
    public void Play()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        StartCoroutine(IntroThenType());
    }

    
    public void PlayText(string text)
    {
        fullText = text;
        Play();
    }

    private IEnumerator IntroThenType()
    {
        isTyping = true;

        yield return StartCoroutine(DoIntroAnimation());

        
        Canvas.ForceUpdateCanvases();
        yield return null;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        
        if (tmp != null) tmp.textWrappingMode = TextWrappingModes.NoWrap;

        typingCoroutine = StartCoroutine(TypeRoutine());
    }

    private IEnumerator DoIntroAnimation()
    {
        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / introDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);
            rect.anchoredPosition = Vector2.Lerp(startAnchoredPos, originalAnchoredPos, eased);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        rect.anchoredPosition = originalAnchoredPos;
    }

    public IEnumerator TypeRoutine()
    {
        isTyping = true;

        if (tmp == null)
        {
            Debug.LogWarning("TypewriterBox: no TextMeshProUGUI assigned or found.", this);
            yield break;
        }



        tmp.text = "";
        StringBuilder committed = new StringBuilder();
        StringBuilder currentLine = new StringBuilder();

        Canvas.ForceUpdateCanvases();
        yield return null;

        float rectWidth = tmp.rectTransform.rect.width;
        if (rectWidth <= 0.01f)
        {
            Canvas.ForceUpdateCanvases();
            yield return null;
            rectWidth = tmp.rectTransform.rect.width;
        }

        var matches = Regex.Matches(fullText, @"\S+\s*|\s+");
        var tokens = matches.Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value).ToArray();

        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            bool tokenIsWhitespace = token.All(char.IsWhiteSpace);

            if (!tokenIsWhitespace)
            {
                string candidateLine = currentLine.ToString() + token;
                Vector2 pref = tmp.GetPreferredValues(candidateLine, Mathf.Infinity, Mathf.Infinity);

                bool wouldOverflow = pref.x > rectWidth;

                if (wouldOverflow && currentLine.Length > 0)
                {
                    committed.Append('\n');
                    currentLine.Clear();
                    tmp.text = committed.ToString();
                }
            }

            StringBuilder typingSoFar = new StringBuilder();
            for (int c = 0; c < token.Length; c++)
            {
                char ch = token[c];
                typingSoFar.Append(ch);
                tmp.text = committed.ToString() + typingSoFar.ToString();

                if (tickSound != null && !char.IsWhiteSpace(ch))
                {
                    float randomPitch = UnityEngine.Random.Range(0.95f, 1.05f);

                    AudioManager.Instance.Play2D(tickSound, tickVolume, randomPitch);
                }


                yield return new WaitForSeconds(typingSpeed);
            }

            committed.Append(typingSoFar.ToString());

            string committedStr = committed.ToString();
            int lastNl = committedStr.LastIndexOf('\n');
            if (lastNl >= 0)
            {
                currentLine.Clear();
                currentLine.Append(committedStr.Substring(lastNl + 1));
            }
            else
            {
                currentLine.Clear();
                currentLine.Append(committedStr);
            }
        }

        
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.text = fullText;
        isTyping = false;
        typingCoroutine = null;
    }
    public void FinishTypingEarly()
    {
        StopAllCoroutines();

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (rect != null) rect.anchoredPosition = originalAnchoredPos;

        if (tmp != null)
        {
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.text = fullText;
            
        }

        isTyping = false; 
        typingCoroutine = null; 
    }

    public void Hide()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        StartCoroutine(DoOutroAnimation());
    }

    private IEnumerator DoOutroAnimation()
    {
        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / introDuration);
            float eased = t * t * t;

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);
            rect.anchoredPosition = Vector2.Lerp(originalAnchoredPos, startAnchoredPos, eased);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        rect.anchoredPosition = startAnchoredPos;
    }
}
