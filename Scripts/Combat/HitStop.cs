using UnityEngine;
using System.Collections;

public static class HitStop
{
    private static bool isHitStopping = false;
    private static MonoBehaviour coroutineHost;

    public static void Initialize(MonoBehaviour host)
    {
        coroutineHost = host;
    }

    public static void DoHitStop(float duration)
    {
        if (!isHitStopping && coroutineHost != null)
        {
            coroutineHost.StartCoroutine(HitStopCoroutine(duration));
        }
    }

    private static IEnumerator HitStopCoroutine(float duration)
    {
        isHitStopping = true;
        float originalTimeScale = Time.timeScale;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        isHitStopping = false;
    }
}
