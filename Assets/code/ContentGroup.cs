using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("AR/Blocks/Content Group (Show/Hide + Animators)")]
public class ContentGroup : MonoBehaviour
{
    [Header("Root")]
    public GameObject contentRoot;                // Parent of environment + characters

    [Header("Fade (best-effort via material color)")]
    public bool doFade = true;
    public float fadeIn = 0.5f;
    public float fadeOut = 0.5f;

    [Header("Animators (optional)")]
    [Tooltip("All character animators that should start when content shows.")]
    public Animator[] animators;
    public int animLayer = 0;
    [Tooltip("Optional state name to wait for; leave empty to wait current state. Non-looping recommended.")]
    public string requiredStateName = "";
    public bool waitForAllAnimatorsToFinish = true;

    bool paused;
    readonly Dictionary<Animator, float> originalSpeeds = new();

    static readonly int PROP_BASECOLOR = Shader.PropertyToID("_BaseColor");
    static readonly int PROP_COLOR = Shader.PropertyToID("_Color");

    void Reset()
    {
        contentRoot = gameObject;
    }

    public void HideImmediate()
    {
        if (contentRoot) contentRoot.SetActive(false);
    }

    public IEnumerator Show(MonoBehaviour runner)
    {
        if (!contentRoot) yield break;

        contentRoot.SetActive(true);

        // Optionally start animators (they will play whatever state is configured)
        if (animators != null)
        {
            foreach (var a in animators) if (a) a.Update(0f); // nudge to current state
        }

        if (doFade && fadeIn > 0f)
            yield return runner.StartCoroutine(CoFade(contentRoot, 0f, 1f, fadeIn));
        else
            SetVisible(contentRoot, true, 1f);
    }

    public IEnumerator WaitUntilFinished()
    {
        if (!waitForAllAnimatorsToFinish || animators == null || animators.Length == 0)
            yield break;

        // Wait until every listed animator reports finished
        bool allDone = false;
        while (!allDone)
        {
            if (!paused)
            {
                allDone = true;
                foreach (var a in animators)
                {
                    if (!a) continue;
                    var st = a.GetCurrentAnimatorStateInfo(animLayer);
                    bool rightState = string.IsNullOrEmpty(requiredStateName) || st.IsName(requiredStateName);
                    // If loop is on, this will never "finish"; ensure clips are non-looping or route through a non-loop state.
                    if (!(rightState && st.normalizedTime >= 1f))
                    {
                        allDone = false; break;
                    }
                }
            }
            yield return null;
        }
    }

    public IEnumerator Hide(MonoBehaviour runner)
    {
        if (!contentRoot) yield break;

        if (doFade && fadeOut > 0f)
            yield return runner.StartCoroutine(CoFade(contentRoot, 1f, 0f, fadeOut));

        contentRoot.SetActive(false);
    }

    public void Pause()
    {
        paused = true;
        if (animators != null)
        {
            foreach (var a in animators)
            {
                if (!a) continue;
                if (!originalSpeeds.ContainsKey(a)) originalSpeeds[a] = a.speed;
                a.speed = 0f;
            }
        }
    }

    public void Resume()
    {
        paused = false;
        foreach (var kv in originalSpeeds)
        {
            if (kv.Key) kv.Key.speed = kv.Value;
        }
        originalSpeeds.Clear();
    }

    // Fades
    IEnumerator CoFade(GameObject root, float from, float to, float dur)
    {
        if (!root || dur <= 0f) { SetVisible(root, to > 0f, to); yield break; }

        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            if (r) r.enabled = true;

        float t = 0f;
        while (t < dur)
        {
            if (!paused)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (!r) continue;
                    if (r.material.HasProperty(PROP_BASECOLOR))
                    { var c = r.material.GetColor(PROP_BASECOLOR); c.a = a; r.material.SetColor(PROP_BASECOLOR, c); }
                    else if (r.material.HasProperty(PROP_COLOR))
                    { var c = r.material.GetColor(PROP_COLOR); c.a = a; r.material.SetColor(PROP_COLOR, c); }
                }
            }
            yield return null;
        }
        SetVisible(root, to > 0f, to);
    }

    void SetVisible(GameObject root, bool visible, float alphaIfVisible)
    {
        if (!root) return;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!r) continue;
            r.enabled = visible;
            if (r.material.HasProperty(PROP_BASECOLOR))
            { var c = r.material.GetColor(PROP_BASECOLOR); c.a = visible ? alphaIfVisible : 0f; r.material.SetColor(PROP_BASECOLOR, c); }
            else if (r.material.HasProperty(PROP_COLOR))
            { var c = r.material.GetColor(PROP_COLOR); c.a = visible ? alphaIfVisible : 0f; r.material.SetColor(PROP_COLOR, c); }
        }
    }
}
