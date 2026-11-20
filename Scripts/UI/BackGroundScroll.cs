using UnityEngine;
using UnityEngine.UI;

public class BackGroundScroll : MonoBehaviour
{
    private RawImage bgImage;

    [Header("Scroll Speed")]
    public float scrollSpeedX = 0.05f;
    public float scrollSpeedY = 0f;

    void Start()
    {
        bgImage = GetComponent<RawImage>();
    }

    void Update()
    {
        Rect currentRect = bgImage.uvRect;

        currentRect.x += scrollSpeedX * Time.unscaledDeltaTime;
        currentRect.y += scrollSpeedY * Time.unscaledDeltaTime;

        bgImage.uvRect = currentRect;
    }
}
