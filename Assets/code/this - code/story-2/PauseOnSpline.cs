using UnityEngine;
using UnityEngine.Splines;

public class PauseOnSplineSeconds : MonoBehaviour
{
    [SerializeField] SplineAnimate anim;
    [Tooltip("Pause at this second of the SplineAnimate run (0..Duration)")]
    public float pauseAtSeconds = 3f;
    [Tooltip("How long to wait before continuing")]
    public float pauseSeconds = 2f;

    Coroutine runner;

    void Awake()
    {
        if (!anim) anim = GetComponent<SplineAnimate>();
    }

    void OnEnable()
    {
        if (runner != null) StopCoroutine(runner);
        runner = StartCoroutine(RunWithPause());
    }

    System.Collections.IEnumerator RunWithPause()
    {
        if (anim == null) yield break;

        // Ensure the animation is playing
        anim.Play();

        // Guard values
        float dur = Mathf.Max(anim.Duration, 0.0001f);
        float target = Mathf.Clamp(pauseAtSeconds, 0f, dur - 0.0001f); // avoid pausing exactly at the end

        // Wait until the elapsed time reaches the target
        while (anim.ElapsedTime < target)
            yield return null;

        anim.Pause();

        // Wait for the pause duration (real time, independent of Time.timeScale)
        float end = Time.realtimeSinceStartup + Mathf.Max(0f, pauseSeconds);
        while (Time.realtimeSinceStartup < end)
            yield return null;

        // Nudge past the pause point so we don't instantly re-trigger in edge cases
        // (Some versions don't allow setting ElapsedTime; that's fine—we just resume)
        anim.Play();
    }
}
