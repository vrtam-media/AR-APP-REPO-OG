using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vuforia;

[AddComponentMenu("AR/Director/AR Timeline (Simple)")]
public class ARTimelineSimple : MonoBehaviour
{
    // -------------------- Global --------------------
    [Header("Global")]
    [Tooltip("ImageTarget / ObserverBehaviour this sequence listens to. If empty, will try to get from this GameObject.")]
    public ObserverBehaviour observer;

    [Tooltip("AR Camera used for billboarding. If empty, uses Camera.main.")]
    public Camera arCamera;

    [Tooltip("Pause texts and animators when tracking is lost, resume on found.")]
    public bool pauseOnTrackingLost = true;

    [Tooltip("For quick editor tests without scanning. If ON, sequence starts immediately on Enable.")]
    public bool runInEditorNoTracking = false;

    [Tooltip("Safety: content root is auto-deactivated in Awake so it can never appear during Intro.")]
    public bool autoDeactivateContentRoot = true;

    // -------------------- Intro --------------------
    [Serializable]
    public class TextBlock
    {
        [Header("Target (3D TMP)")]
        public TMP_Text text;

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

        [Header("Hold")]
        [Tooltip("Extra time to keep the full text on screen AFTER last word appears.")]
        public float minHoldAfterReveal = 0.5f;

        [Header("Billboard")]
        public bool billboard = true;
        public bool yawOnly = true;
        [Range(0f, 30f)] public float rotateLerp = 12f;
    }

    [Header("Intro (3D TMP)")]
    public TextBlock intro = new TextBlock
    {
        message = "Welcome!",
        wordsPerSecond = 3f,
        doFade = true,
        fadeIn = 0.35f,
        fadeOut = 0.35f,
        minHoldAfterReveal = 0.5f,
        billboard = true,
        yawOnly = true
    };

    // -------------------- Content --------------------
    public enum WaitMode { ClipsFinish, FixedDuration }

    [Serializable]
    public class ContentBlock
    {
        [Header("Root")]
        [Tooltip("Parent that contains the environment + all characters.")]
        public GameObject contentRoot;

        [Header("Fade")]
        public bool doFade = true;
        public float fadeIn = 0.35f;
        public float fadeOut = 0.35f;

        [Header("When to end this section")]
        public WaitMode waitMode = WaitMode.ClipsFinish;
        [Tooltip("Used only if WaitMode = FixedDuration.")]
        public float duration = 3f;

        [Header("Animators (only used if WaitMode = ClipsFinish)")]
        [Tooltip("All character Animator components to wait for. All must finish.")]
        public Animator[] animators;
        public int animLayer = 0;
        [Tooltip("Optional specific state name to wait for. Leave empty to accept whatever state is currently playing.")]
        public string requiredStateName = "";

        [Header("Post")]
        [Tooltip("Pause AFTER animations finish (or duration ends), before hiding content.")]
        public float postDelay = 0.25f;
    }

    [Header("Content (env + characters)")]
    public ContentBlock content = new ContentBlock();

    // -------------------- Outro --------------------
    [Header("Outro (3D TMP)")]
    public TextBlock outro = new TextBlock
    {
        message = "Thanks for watching.",
        wordsPerSecond = 3f,
        doFade = true,
        fadeIn = 0.35f,
        fadeOut = 0.35f,
        minHoldAfterReveal = 0.5f,
        billboard = true,
        yawOnly = true
    };

    // -------------------- Runtime state --------------------
    bool paused;
    Coroutine sequenceCo;

    readonly Dictionary<Animator, float> originalAnimatorSpeeds = new Dictionary<Animator, float>();
    static readonly int PROP_FACE = Shader.PropertyToID("_FaceColor");
    static readonly int PROP_BASECOL = Shader.PropertyToID("_BaseColor");
    static readonly int PROP_COLOR = Shader.PropertyToID("_Color");

    void Awake()
    {
        if (!observer) observer = GetComponent<ObserverBehaviour>();
        if (!arCamera) arCamera = Camera.main;

        // Safety: hide everything on load
        HideTextImmediate(intro);
        HideTextImmediate(outro);

        if (autoDeactivateContentRoot && content.contentRoot)
            content.contentRoot.SetActive(false);
        SetContentVisible(content.contentRoot, false, 0f); // also zero alpha on renderers
    }

    void OnEnable()
    {
        if (observer) observer.OnTargetStatusChanged += OnTargetStatusChanged;

        if (runInEditorNoTracking)
        {
            StartSequenceFresh();
        }
    }

    void OnDisable()
    {
        if (observer) observer.OnTargetStatusChanged -= OnTargetStatusChanged;
        if (sequenceCo != null) StopCoroutine(sequenceCo);
        sequenceCo = null;
        originalAnimatorSpeeds.Clear();
    }

    void LateUpdate()
    {
        // Billboard any currently visible text
        if (intro.billboard) Billboard(intro);
        if (outro.billboard) Billboard(outro);
    }

    // -------------------- Tracking events --------------------
    void OnTargetStatusChanged(ObserverBehaviour beh, TargetStatus status)
    {
        bool isTracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (isTracked)
        {
            if (pauseOnTrackingLost) ResumeAll();
            if (sequenceCo == null && !runInEditorNoTracking)
                StartSequenceFresh();
        }
        else
        {
            if (pauseOnTrackingLost) PauseAll();
        }
    }

    void StartSequenceFresh()
    {
        if (sequenceCo != null) StopCoroutine(sequenceCo);
        sequenceCo = StartCoroutine(CoSequence());
    }

    // -------------------- Sequence --------------------
    IEnumerator CoSequence()
    {
        // INTRO
        if (intro.text)
        {
            yield return CoPlayText(intro);
        }

        // CONTENT (shows only now; anims start only now)
        if (content.contentRoot)
        {
            // turn on + fade in
            content.contentRoot.SetActive(true);
            if (content.doFade && content.fadeIn > 0f)
                yield return CoFadeContent(content.contentRoot, 0f, 1f, content.fadeIn);
            else
                SetContentVisible(content.contentRoot, true, 1f);

            // wait policy
            if (content.waitMode == WaitMode.ClipsFinish && content.animators != null && content.animators.Length > 0)
            {
                yield return CoWaitAnimatorsFinished(content.animators, content.animLayer, content.requiredStateName);
            }
            else if (content.waitMode == WaitMode.FixedDuration && content.duration > 0f)
            {
                yield return Wait(content.duration);
            }

            // post delay (content still visible)
            if (content.postDelay > 0f)
                yield return Wait(content.postDelay);

            // fade out + hide
            if (content.doFade && content.fadeOut > 0f)
                yield return CoFadeContent(content.contentRoot, 1f, 0f, content.fadeOut);

            SetContentVisible(content.contentRoot, false, 0f);
            content.contentRoot.SetActive(false);
        }

        // OUTRO
        if (outro.text)
        {
            yield return CoPlayText(outro);
        }

        sequenceCo = null;
    }

    // -------------------- Text helpers --------------------
    void HideTextImmediate(TextBlock t)
    {
        if (t == null || t.text == null) return;
        SetTMPAlpha(t.text, 0f, true);
        t.text.maxVisibleCharacters = int.MaxValue;
        t.text.maxVisibleWords = 0;
    }

    IEnumerator CoPlayText(TextBlock t)
    {
        if (!t.text) yield break;

        // Prepare content
        if (!string.IsNullOrEmpty(t.message))
            t.text.text = t.message;

        t.text.ForceMeshUpdate(true, true);
        t.text.maxVisibleCharacters = int.MaxValue;
        t.text.maxVisibleWords = 0;

        SetTMPAlpha(t.text, 0f, true);

        // Fade in
        if (t.doFade && t.fadeIn > 0f) yield return CoFadeTMP(t.text, 0f, 1f, t.fadeIn);
        else SetTMPAlpha(t.text, 1f, true);

        // Word-by-word reveal
        int totalWords = Mathf.Max(0, t.text.textInfo.wordCount);
        float baseStep = 1f / Mathf.Max(0.05f, t.wordsPerSecond);

        float revealElapsed = 0f;
        for (int i = 1; i <= totalWords; i++)
        {
            t.text.maxVisibleWords = i;

            float wait = baseStep;
            if (t.punctuationPauses)
            {
                char p = TailPunct(t.text, i - 1);
                if (p == ',' || p == ';') wait += t.pauseAfterComma;
                else if (p == '.' || p == '!' || p == '?') wait += t.pauseAfterPeriod;
                else if (p == ':' || p == ')' || p == ']' || p == '"' || p == '’' || p == '\'') wait += t.pauseAfterOther;
            }

            float w = 0f;
            while (w < wait)
            {
                if (!paused) { w += Time.deltaTime; revealElapsed += Time.deltaTime; }
                yield return null;
            }
        }

        // Extra hold after last word
        if (t.minHoldAfterReveal > 0f)
            yield return Wait(t.minHoldAfterReveal);

        // Fade out
        if (t.doFade && t.fadeOut > 0f) yield return CoFadeTMP(t.text, 1f, 0f, t.fadeOut);
        else SetTMPAlpha(t.text, 0f, true);
    }

    void Billboard(TextBlock t)
    {
        if (!t.text || !t.billboard) return;
        if (!arCamera) arCamera = Camera.main;
        if (!arCamera) return;

        var tr = t.text.transform;
        Vector3 toCam = arCamera.transform.position - tr.position;
        if (t.yawOnly) toCam.y = 0f;
        if (toCam.sqrMagnitude < 1e-6f) return;

        Quaternion target = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
        float lerp = Mathf.Max(0f, t.rotateLerp);
        tr.rotation = (lerp <= 0f) ? target
            : Quaternion.Slerp(tr.rotation, target, 1f - Mathf.Exp(-lerp * Time.deltaTime));
    }

    // -------------------- Animator wait --------------------
    IEnumerator CoWaitAnimatorsFinished(Animator[] anims, int layer, string requiredState)
    {
        // allow one frame to settle
        yield return null;

        bool allDone = false;
        while (!allDone)
        {
            if (!paused)
            {
                allDone = true;
                foreach (var a in anims)
                {
                    if (!a) continue;
                    var st = a.GetCurrentAnimatorStateInfo(layer);
                    bool rightState = string.IsNullOrEmpty(requiredState) || st.IsName(requiredState);
                    // If your clip loops, this never finishes — turn off Loop Time or exit into a non-loop state.
                    if (!(rightState && st.normalizedTime >= 1f && !a.IsInTransition(layer)))
                    {
                        allDone = false; break;
                    }
                }
            }
            yield return null;
        }
    }

    // -------------------- Pause / Resume --------------------
    void PauseAll()
    {
        paused = true;

        // freeze animators we know about
        if (content.animators != null)
        {
            foreach (var a in content.animators)
            {
                if (!a) continue;
                if (!originalAnimatorSpeeds.ContainsKey(a)) originalAnimatorSpeeds[a] = a.speed;
                a.speed = 0f;
            }
        }
    }

    void ResumeAll()
    {
        paused = false;

        foreach (var kv in originalAnimatorSpeeds)
        {
            if (kv.Key) kv.Key.speed = kv.Value;
        }
        originalAnimatorSpeeds.Clear();
    }

    // -------------------- Fading / Visibility --------------------
    IEnumerator CoFadeTMP(TMP_Text tmp, float from, float to, float dur)
    {
        SetTMPAlpha(tmp, from, true);
        if (dur <= 0f) { SetTMPAlpha(tmp, to, true); yield break; }

        float t = 0f;
        while (t < dur)
        {
            if (!paused)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                SetTMPAlpha(tmp, a, true);
            }
            yield return null;
        }
        SetTMPAlpha(tmp, to, true);
    }

    void SetTMPAlpha(TMP_Text tmp, float a, bool enableRenderer)
    {
        if (!tmp) return;
        var r = tmp.GetComponent<Renderer>();
        if (!r) return;

        r.enabled = enableRenderer;

        if (r.material.HasProperty(PROP_FACE))
        {
            var c = r.material.GetColor(PROP_FACE); c.a = a;
            r.material.SetColor(PROP_FACE, c);
        }
        else if (r.material.HasProperty(PROP_COLOR))
        {
            var c = r.material.GetColor(PROP_COLOR); c.a = a;
            r.material.SetColor(PROP_COLOR, c);
        }
        else
        {
            // last resort
            var col = r.material.color; col.a = a;
            r.material.color = col;
        }
    }

    IEnumerator CoFadeContent(GameObject root, float from, float to, float dur)
    {
        if (!root || dur <= 0f) { SetContentVisible(root, to > 0f, to); yield break; }

        // ensure renderers are enabled to be able to fade
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

                    if (r.material.HasProperty(PROP_BASECOL))
                    {
                        var c = r.material.GetColor(PROP_BASECOL); c.a = a;
                        r.material.SetColor(PROP_BASECOL, c);
                    }
                    else if (r.material.HasProperty(PROP_COLOR))
                    {
                        var c = r.material.GetColor(PROP_COLOR); c.a = a;
                        r.material.SetColor(PROP_COLOR, c);
                    }
                }
            }
            yield return null;
        }
        SetContentVisible(root, to > 0f, to);
    }

    void SetContentVisible(GameObject root, bool visible, float alphaIfVisible)
    {
        if (!root) return;

        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!r) continue;
            r.enabled = visible;

            if (r.material.HasProperty(PROP_BASECOL))
            {
                var c = r.material.GetColor(PROP_BASECOL); c.a = visible ? alphaIfVisible : 0f;
                r.material.SetColor(PROP_BASECOL, c);
            }
            else if (r.material.HasProperty(PROP_COLOR))
            {
                var c = r.material.GetColor(PROP_COLOR); c.a = visible ? alphaIfVisible : 0f;
                r.material.SetColor(PROP_COLOR, c);
            }
        }
    }

    // -------------------- Utility --------------------
    IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!paused) t += Time.deltaTime;
            yield return null;
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
        // handle TMP rich-text endings like ...</b>
        if (c == '>' && last > 0) c = src[last - 1];
        return c;
    }
}
