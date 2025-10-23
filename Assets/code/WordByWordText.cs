using System;
using System.Collections;
using TMPro;
using UnityEngine;

[AddComponentMenu("AR/Blocks/Word By Word Text (3D TMP)")]
public class WordByWordText : MonoBehaviour
{
    [Header("Target (3D TMP)")]
    public TMP_Text text;                          // Assign a 3D TextMeshPro (MeshRenderer)

    [Header("Message")]
    [TextArea] public string message = "Sample text";

    [Header("Reveal")]
    [Min(0.05f)] public float wordsPerSecond = 3f;
    public bool punctuationPauses = true;
    public float pauseAfterComma = 0.12f;
    public float pauseAfterPeriod = 0.22f;
    public float pauseAfterOther = 0.08f;

    [Header("Fade")]
    public bool doFade = true;
    public float fadeIn = 0.35f;
    public float fadeOut = 0.35f;

    [Header("Billboard (optional)")]
    public bool billboard = true;
    public bool yawOnly = true;
    [Range(0f, 30f)] public float rotateLerp = 12f;
    public Camera targetCamera;                    // Leave empty to use Camera.main

    bool paused;
    Coroutine playing;

    static readonly int PROP_FACE = Shader.PropertyToID("_FaceColor");
    static readonly int PROP_COL = Shader.PropertyToID("_Color");

    void Reset()
    {
        text = GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
        if (!targetCamera) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (!billboard || !text) return;
        if (!targetCamera) targetCamera = Camera.main;
        if (!targetCamera) return;

        var tr = text.transform;
        Vector3 toCam = targetCamera.transform.position - tr.position;
        if (yawOnly) toCam.y = 0f;
        if (toCam.sqrMagnitude < 1e-6f) return;

        var look = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
        if (rotateLerp <= 0f) tr.rotation = look;
        else tr.rotation = Quaternion.Slerp(tr.rotation, look, 1f - Mathf.Exp(-rotateLerp * Time.deltaTime));
    }

    // Public API
    public void SetMessage(string msg)
    {
        message = msg;
        if (text) text.text = message;
    }

    public void HideImmediate()
    {
        if (!text) return;
        text.maxVisibleCharacters = int.MaxValue;
        text.maxVisibleWords = 0;
        SetAlpha(0f, false);
    }

    public Coroutine Play(MonoBehaviour runner, float minOnScreenAfterReveal, Action onDone = null, Action<float> onRevealTime = null)
    {
        if (playing != null) runner.StopCoroutine(playing);
        playing = runner.StartCoroutine(CoPlay(minOnScreenAfterReveal, onDone, onRevealTime));
        return playing;
    }

    public void Stop(MonoBehaviour runner)
    {
        if (playing != null) runner.StopCoroutine(playing);
        playing = null;
        HideImmediate();
    }

    public void Pause() { paused = true; }
    public void Resume() { paused = false; }

    // Internals
    IEnumerator CoPlay(float minHold, Action onDone, Action<float> onRevealTime)
    {
        if (!text) yield break;

        // Prep
        if (!string.IsNullOrEmpty(message)) text.text = message;
        text.ForceMeshUpdate(true, true);
        text.maxVisibleCharacters = int.MaxValue;
        text.maxVisibleWords = 0;
        SetAlpha(0f, true);

        // Fade in
        if (doFade && fadeIn > 0f) yield return CoFade(0f, 1f, fadeIn);
        else SetAlpha(1f, true);

        // Reveal words
        int total = Mathf.Max(0, text.textInfo.wordCount);
        float baseStep = 1f / Mathf.Max(0.05f, wordsPerSecond);
        float revealElapsed = 0f;

        for (int i = 1; i <= total; i++)
        {
            text.maxVisibleWords = i;
            float wait = baseStep;

            if (punctuationPauses)
            {
                char p = TailPunct(text, i - 1);
                if (p == ',' || p == ';') wait += pauseAfterComma;
                else if (p == '.' || p == '!' || p == '?') wait += pauseAfterPeriod;
                else if (p == ':' || p == ')' || p == ']' || p == '"' || p == '’' || p == '\'') wait += pauseAfterOther;
            }

            float t = 0f;
            while (t < wait)
            {
                if (!paused) { t += Time.deltaTime; revealElapsed += Time.deltaTime; }
                yield return null;
            }
        }

        onRevealTime?.Invoke(revealElapsed);

        // Ensure it stays on-screen at least minHold (AFTER reveal)
        float extra = Mathf.Max(0f, minHold);
        if (extra > 0f)
        {
            float t = 0f;
            while (t < extra)
            {
                if (!paused) t += Time.deltaTime;
                yield return null;
            }
        }

        // Fade out
        if (doFade && fadeOut > 0f) yield return CoFade(1f, 0f, fadeOut);
        else SetAlpha(0f, true);

        onDone?.Invoke();
        playing = null;
    }

    IEnumerator CoFade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            if (!paused)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                SetAlpha(a, true);
            }
            yield return null;
        }
        SetAlpha(to, true);
    }

    void SetAlpha(float a, bool enableRenderer)
    {
        if (!text) return;
        var r = text.GetComponent<Renderer>();
        if (r)
        {
            r.enabled = enableRenderer;
            if (r.material.HasProperty(PROP_FACE))
            {
                var c = r.material.GetColor(PROP_FACE); c.a = a; r.material.SetColor(PROP_FACE, c);
            }
            else if (r.material.HasProperty(PROP_COL))
            {
                var c = r.material.GetColor(PROP_COL); c.a = a; r.material.SetColor(PROP_COL, c);
            }
        }
    }

    static char TailPunct(TMP_Text tmp, int wordIndex)
    {
        var ti = tmp.textInfo;
        if (wordIndex < 0 || wordIndex >= ti.wordCount) return '\0';
        var wi = ti.wordInfo[wordIndex];
        if (wi.characterCount <= 0) return '\0';
        string src = tmp.text;
        int last = Mathf.Min(src.Length - 1, wi.firstCharacterIndex + wi.characterCount - 1);
        char c = src[last];
        if (c == '>' && last > 0) c = src[last - 1];
        return c;
    }
}
