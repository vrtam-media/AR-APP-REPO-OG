using System;
using System.Collections;
using TMPro;
using UnityEngine;

[AddComponentMenu("AR/Director/AR Timed Sequence (Intro → Content → Outro, Word-by-Word)")]
public class ARTimedSequence : MonoBehaviour
{
    // ---------- INTRO ----------
    [Header("Intro Text (3D TMP)")]
    public TMP_Text introText;                    // 3D TextMeshPro (MeshRenderer)
    [TextArea] public string introMessage = "Intro text";
    public float introPreDelay = 0f;              // wait before intro starts
    public float introShowTime = 2f;              // minimum time to remain after reveal finishes
    public bool introFade = true;
    public float introFadeIn = 0.35f;
    public float introFadeOut = 0.35f;

    [Header("Intro Word-by-Word")]
    [Min(0.05f)] public float introWordsPerSecond = 3f;
    public bool introPunctuationPauses = true;
    public float introPauseComma = 0.12f;
    public float introPausePeriod = 0.22f;
    public float introPauseOther = 0.08f;

    // ---------- CONTENT ----------
    [Header("Content (env + 3D models)")]
    public GameObject contentRoot;                // parent of environment + characters
    public float contentPreDelay = 0f;            // delay AFTER intro fades out
    public float contentShowTime = 3f;            // how long content stays visible
    public bool contentFade = true;
    public float contentFadeIn = 0.35f;
    public float contentFadeOut = 0.35f;

    // ---------- OUTRO ----------
    [Header("Outro Text (3D TMP)")]
    public TMP_Text outroText;                    // 3D TextMeshPro (MeshRenderer)
    [TextArea] public string outroMessage = "Outro text";
    public float outroPreDelay = 0f;              // delay AFTER content turns off
    public float outroShowTime = 2f;              // minimum time to remain after reveal finishes
    public bool outroFade = true;
    public float outroFadeIn = 0.35f;
    public float outroFadeOut = 0.35f;

    [Header("Outro Word-by-Word")]
    [Min(0.05f)] public float outroWordsPerSecond = 3f;
    public bool outroPunctuationPauses = true;
    public float outroPauseComma = 0.12f;
    public float outroPausePeriod = 0.22f;
    public float outroPauseOther = 0.08f;

    // ---------- General ----------
    [Header("General")]
    public bool startOnEnable = true;
    public bool useUnscaledTime = false;          // ignore Time.timeScale if true
    public bool billboardTexts = false;           // make texts face the camera
    public bool yawOnly = true;
    public float billboardLerp = 12f;
    public Camera targetCamera;                   // leave empty to use Camera.main

    // shader props used for best-effort fading on various materials
    static readonly int PROP_BASECOLOR = Shader.PropertyToID("_BaseColor");
    static readonly int PROP_COLOR = Shader.PropertyToID("_Color");

    void OnEnable()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (!startOnEnable) return;
        StopAllCoroutines();
        HideImmediate();
        StartCoroutine(Run());
    }

    void Update()
    {
        if (!billboardTexts) return;
        if (!targetCamera) targetCamera = Camera.main;
        if (!targetCamera) return;

        void Face(TMP_Text t)
        {
            if (!t) return;
            var tr = t.transform;
            Vector3 toCam = targetCamera.transform.position - tr.position;
            if (yawOnly) toCam.y = 0f;
            if (toCam.sqrMagnitude < 1e-6f) return;
            var target = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            tr.rotation = (billboardLerp <= 0f) ? target
                : Quaternion.Slerp(tr.rotation, target, 1f - Mathf.Exp(-billboardLerp * dt));
        }

        if (introText) Face(introText);
        if (outroText) Face(outroText);
    }

    // -------------------- MAIN SEQUENCE --------------------
    IEnumerator Run()
    {
        // ---------- INTRO ----------
        if (introText)
        {
            if (introPreDelay > 0f) yield return Wait(introPreDelay);

            PrepareTMP(introText, introMessage);
            if (introFade && introFadeIn > 0f) yield return FadeTMP(introText, 0f, 1f, introFadeIn);
            else SetTMPAlpha(introText, 1f, true);

            float introRevealTime = 0f;
            yield return StartCoroutine(WordByWord(introText, introWordsPerSecond,
                introPunctuationPauses, introPauseComma, introPausePeriod, introPauseOther,
                t => introRevealTime = t));

            float extraIntroHold = Mathf.Max(0f, introShowTime - introRevealTime);
            if (extraIntroHold > 0f) yield return Wait(extraIntroHold);

            if (introFade && introFadeOut > 0f) yield return FadeTMP(introText, 1f, 0f, introFadeOut);
            else SetTMPAlpha(introText, 0f, true);
        }

        // ---------- CONTENT ----------
        if (contentRoot)
        {
            if (contentPreDelay > 0f) yield return Wait(contentPreDelay);

            contentRoot.SetActive(true);
            if (contentFade && contentFadeIn > 0f) yield return FadeContent(contentRoot, 0f, 1f, contentFadeIn);
            else SetContentVisible(contentRoot, true, 1f);

            if (contentShowTime > 0f) yield return Wait(contentShowTime);

            if (contentFade && contentFadeOut > 0f) yield return FadeContent(contentRoot, 1f, 0f, contentFadeOut);
            contentRoot.SetActive(false);
        }

        // ---------- OUTRO ----------
        if (outroText)
        {
            if (outroPreDelay > 0f) yield return Wait(outroPreDelay);

            PrepareTMP(outroText, outroMessage);
            if (outroFade && outroFadeIn > 0f) yield return FadeTMP(outroText, 0f, 1f, outroFadeIn);
            else SetTMPAlpha(outroText, 1f, true);

            float outroRevealTime = 0f;
            yield return StartCoroutine(WordByWord(outroText, outroWordsPerSecond,
                outroPunctuationPauses, outroPauseComma, outroPausePeriod, outroPauseOther,
                t => outroRevealTime = t));

            float extraOutroHold = Mathf.Max(0f, outroShowTime - outroRevealTime);
            if (extraOutroHold > 0f) yield return Wait(extraOutroHold);

            if (outroFade && outroFadeOut > 0f) yield return FadeTMP(outroText, 1f, 0f, outroFadeOut);
            else SetTMPAlpha(outroText, 0f, true);
        }
    }

    // -------------------- WORD-BY-WORD --------------------
    // Uses a callback (onDone) to return the total reveal time.
    IEnumerator WordByWord(
        TMP_Text tmp,
        float wordsPerSecond,
        bool punctuationPauses,
        float pauseComma,
        float pausePeriod,
        float pauseOther,
        Action<float> onDone)
    {
        if (!tmp) { onDone?.Invoke(0f); yield break; }

        tmp.ForceMeshUpdate(true, true);
        tmp.maxVisibleCharacters = int.MaxValue;
        tmp.maxVisibleWords = 0;

        int totalWords = Mathf.Max(0, tmp.textInfo.wordCount);
        if (totalWords == 0) { onDone?.Invoke(0f); yield break; }

        float baseStep = 1f / Mathf.Max(0.05f, wordsPerSecond);
        float elapsed = 0f;

        for (int i = 1; i <= totalWords; i++)
        {
            tmp.maxVisibleWords = i;

            float wait = baseStep;
            if (punctuationPauses)
            {
                char p = TailPunct(tmp, i - 1);
                if (p == ',' || p == ';') wait += pauseComma;
                else if (p == '.' || p == '!' || p == '?') wait += pausePeriod;
                else if (p == ':' || p == ')' || p == ']' || p == '"' || p == '\'') wait += pauseOther;
            }

            // timed wait while accumulating elapsed
            float t = 0f;
            while (t < wait)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt; elapsed += dt;
                yield return null;
            }
        }

        onDone?.Invoke(elapsed);
    }

    // -------------------- HELPERS --------------------
    void HideImmediate()
    {
        if (introText) SetTMPAlpha(introText, 0f, false);
        if (outroText) SetTMPAlpha(outroText, 0f, false);
        if (contentRoot) contentRoot.SetActive(false);
    }

    void PrepareTMP(TMP_Text tmp, string message)
    {
        if (!tmp) return;
        if (!string.IsNullOrEmpty(message)) tmp.text = message;
        tmp.maxVisibleCharacters = int.MaxValue;
        tmp.maxVisibleWords = 0;
        SetTMPAlpha(tmp, 0f, true);
    }

    IEnumerator Wait(float seconds)
    {
        if (!useUnscaledTime) yield return new WaitForSeconds(seconds);
        else
        {
            float t = 0f;
            while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        }
    }

    static void SetTMPAlpha(TMP_Text tmp, float a, bool enableRenderer)
    {
        if (!tmp) return;
        var r = tmp.GetComponent<Renderer>();
        if (r)
        {
            r.enabled = enableRenderer;
            if (r.material.HasProperty("_FaceColor"))
            { var c = r.material.GetColor("_FaceColor"); c.a = a; r.material.SetColor("_FaceColor", c); }
            else if (r.material.HasProperty("_Color"))
            { var c = r.material.GetColor("_Color"); c.a = a; r.material.SetColor("_Color", c); }
        }
    }

    IEnumerator FadeTMP(TMP_Text tmp, float from, float to, float dur)
    {
        SetTMPAlpha(tmp, from, true);
        if (dur <= 0f) { SetTMPAlpha(tmp, to, true); yield break; }

        float t = 0f;
        while (t < dur)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
            SetTMPAlpha(tmp, a, true);
            yield return null;
        }
        SetTMPAlpha(tmp, to, true);
    }

    void SetContentVisible(GameObject root, bool visible, float alphaIfVisible)
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

    IEnumerator FadeContent(GameObject root, float from, float to, float dur)
    {
        if (!root || dur <= 0f) { SetContentVisible(root, to > 0f, to); yield break; }
        foreach (var r in root.GetComponentsInChildren<Renderer>(true)) if (r) r.enabled = true;

        float t = 0f;
        while (t < dur)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!r) continue;
                if (r.material.HasProperty(PROP_BASECOLOR))
                { var c = r.material.GetColor(PROP_BASECOLOR); c.a = a; r.material.SetColor(PROP_BASECOLOR, c); }
                else if (r.material.HasProperty(PROP_COLOR))
                { var c = r.material.GetColor(PROP_COLOR); c.a = a; r.material.SetColor(PROP_COLOR, c); }
            }
            yield return null;
        }
        SetContentVisible(root, to > 0f, to);
    }

    static char TailPunct(TMP_Text tmp, int wordIndex)
    {
        if (!tmp) return '\0';
        var ti = tmp.textInfo;
        if (wordIndex < 0 || wordIndex >= ti.wordCount) return '\0';
        var wi = ti.wordInfo[wordIndex];
        if (wi.characterCount <= 0) return '\0';
        string src = tmp.text;
        int idx = Mathf.Min(src.Length - 1, wi.firstCharacterIndex + wi.characterCount - 1);
        char c = src[idx];
        if (c == '>' && idx > 0) c = src[idx - 1]; // handles TMP rich text
        return c;
    }
}
