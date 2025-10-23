using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vuforia;

[AddComponentMenu("AR/Director/AR Sequence Simple (Vuforia + 3D TMP)")]
public class ARSequenceSimple : MonoBehaviour
{
    [Header("Vuforia / Camera")]
    public ObserverBehaviour observer;      // ImageTarget (ObserverBehaviour)
    public Camera arCamera;                 // ARCamera (or leave null => Camera.main)

    [Header("Tracking")]
    public bool pauseOnTrackingLost = true;

    // ---------------- TEXT (shared) ----------------
    [Header("Text (shared settings)")]
    [Min(0.05f)] public float wordsPerSecond = 3f;
    public bool punctuationPauses = true;
    public float pauseAfterComma = 0.12f, pauseAfterPeriod = 0.22f, pauseAfterOther = 0.08f;
    public float textFadeIn = 0.35f, textFadeOut = 0.35f;

    [Header("Billboard")]
    public bool billboard = true;
    public bool yawOnly = true;
    [Range(0f, 30f)] public float rotateLerp = 12f;

    [System.Serializable]
    public class TextItem
    {
        public TMP_Text text;             // 3D TMP (MeshRenderer)
        [TextArea] public string message = "Sample";
        public float preDelay = 0f;       // wait before this text starts
        public float hold = 1.5f;         // after reveal completes
    }

    [Header("Intro Texts (in order)")]
    public List<TextItem> introTexts = new List<TextItem>();

    // ---------------- CONTENT ----------------
    [Header("Content (environment + models)")]
    public GameObject contentRoot;        // parent of env + characters

    [Tooltip("If ON, SetActive(false) until content step; guarantees nothing shows during intro.")]
    public bool hardToggleContentActive = true;

    [Tooltip("Extra delay AFTER intro text fades out, BEFORE showing content.")]
    public float contentStartDelay = 0f;

    public bool contentFade = true;
    public float contentFadeIn = 0.5f, contentFadeOut = 0.5f;

    [Tooltip("Minimum time to keep content shown.")]
    public float contentMinDuration = 3f;

    [Tooltip("Also wait for ALL animators to finish current (or named) state. Leave OFF for looping clips.")]
    public bool waitForAnimators = false;
    public Animator[] animators;          // drag ALL character animators
    public int animLayer = 0;
    [Tooltip("Optional specific state to wait for (case-sensitive). Leave empty to wait current state.")]
    public string requiredStateName = "";

    [Header("Outro Texts (in order)")]
    public List<TextItem> outroTexts = new List<TextItem>();

    // ---------------- runtime ----------------
    bool paused;
    Coroutine runner;
    readonly Dictionary<Animator, float> animOriginalSpeed = new();

    void Awake()
    {
        if (!observer) observer = GetComponent<ObserverBehaviour>();
        if (!arCamera) arCamera = Camera.main;
    }

    void OnEnable()
    {
        if (observer) observer.OnTargetStatusChanged += OnTargetStatusChanged;
        HideAllImmediate(); // intro-only visible at start (actually off until first text)
    }

    void OnDisable()
    {
        if (observer) observer.OnTargetStatusChanged -= OnTargetStatusChanged;
        if (runner != null) StopCoroutine(runner);
        animOriginalSpeed.Clear();
    }

    void LateUpdate()
    {
        if (!billboard) return;
        if (!arCamera) arCamera = Camera.main;
        if (!arCamera) return;

        void Face(TMP_Text t)
        {
            if (!t || !t.gameObject.activeInHierarchy) return;
            var tr = t.transform;
            Vector3 toCam = arCamera.transform.position - tr.position;
            if (yawOnly) toCam.y = 0f;
            if (toCam.sqrMagnitude < 1e-6f) return;
            var target = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
            tr.rotation = (rotateLerp <= 0f) ? target
                : Quaternion.Slerp(tr.rotation, target, 1f - Mathf.Exp(-rotateLerp * Time.deltaTime));
        }

        foreach (var ti in introTexts) Face(ti?.text);
        foreach (var ti in outroTexts) Face(ti?.text);
    }

    // ---------------- Vuforia ----------------
    void OnTargetStatusChanged(ObserverBehaviour beh, TargetStatus status)
    {
        bool isTracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (isTracked)
        {
            if (pauseOnTrackingLost) ResumeAll();
            if (runner == null) runner = StartCoroutine(CoRun());
        }
        else
        {
            if (pauseOnTrackingLost) PauseAll();
        }
    }

    // ---------------- Sequence ----------------
    IEnumerator CoRun()
    {
        // Intro text(s)
        foreach (var ti in introTexts)
            yield return CoRunText(ti);

        // Content
        yield return CoRunContent();

        // Outro text(s)
        foreach (var ti in outroTexts)
            yield return CoRunText(ti);

        runner = null;
    }

    IEnumerator CoRunText(TextItem ti)
    {
        if (ti == null || !ti.text) yield break;

        if (ti.preDelay > 0f) yield return Wait(ti.preDelay);

        var tmp = ti.text;
        if (!string.IsNullOrEmpty(ti.message)) tmp.text = ti.message;

        tmp.ForceMeshUpdate(true, true);
        tmp.maxVisibleCharacters = int.MaxValue;
        tmp.maxVisibleWords = 0;
        SetTMPAlpha(tmp, 0f, true);

        if (textFadeIn > 0f) yield return CoFadeTMP(tmp, 0f, 1f, textFadeIn);
        else SetTMPAlpha(tmp, 1f, true);

        int total = Mathf.Max(0, tmp.textInfo.wordCount);
        if (total > 0)
        {
            float step = 1f / Mathf.Max(0.05f, wordsPerSecond);
            for (int i = 1; i <= total; i++)
            {
                tmp.maxVisibleWords = i;

                float wait = step;
                if (punctuationPauses)
                {
                    char p = TailPunct(tmp, i - 1);
                    if (p == ',' || p == ';') wait += pauseAfterComma;
                    else if (p == '.' || p == '!' || p == '?') wait += pauseAfterPeriod;
                    else if (p == ':' || p == ')' || p == ']' || p == '"' || p == '’' || p == '\'')
                        wait += pauseAfterOther;
                }
                yield return Wait(wait);
            }
        }

        if (ti.hold > 0f) yield return Wait(ti.hold);

        if (textFadeOut > 0f) yield return CoFadeTMP(tmp, 1f, 0f, textFadeOut);
        else SetTMPAlpha(tmp, 0f, true);
    }

    IEnumerator CoRunContent()
    {
        // ensure a beat AFTER intro is completely gone
        if (contentStartDelay > 0f) yield return Wait(contentStartDelay);

        // SHOW content (only now)
        if (contentRoot)
        {
            if (hardToggleContentActive)
            {
                contentRoot.SetActive(true);
                if (contentFade && contentFadeIn > 0f)
                    yield return CoFadeContent(contentRoot, 0f, 1f, contentFadeIn);
            }
            else
            {
                if (contentFade && contentFadeIn > 0f)
                    yield return CoFadeContent(contentRoot, 0f, 1f, contentFadeIn);
                else
                    SetContentVisible(contentRoot, true, 1f);
            }
        }

        // WAIT: by min duration AND (optional) all animators
        float elapsed = 0f;
        while (true)
        {
            if (!pauseOnTrackingLost || !paused) elapsed += Time.deltaTime;

            bool timeDone = elapsed >= contentMinDuration;
            bool animsDone = true;

            if (waitForAnimators && animators != null && animators.Length > 0)
            {
                for (int i = 0; i < animators.Length; i++)
                {
                    var a = animators[i];
                    if (!a) continue;
                    var st = a.GetCurrentAnimatorStateInfo(animLayer);
                    bool rightState = string.IsNullOrEmpty(requiredStateName) || st.IsName(requiredStateName);
                    if (!(rightState && st.normalizedTime >= 1f))
                    {
                        animsDone = false; break;
                    }
                }
            }

            if (timeDone && (!waitForAnimators || animsDone)) break;
            yield return null;
        }

        // HIDE content (before outro starts)
        if (contentRoot)
        {
            if (hardToggleContentActive)
            {
                if (contentFade && contentFadeOut > 0f)
                    yield return CoFadeContent(contentRoot, 1f, 0f, contentFadeOut);
                contentRoot.SetActive(false);
            }
            else
            {
                if (contentFade && contentFadeOut > 0f)
                    yield return CoFadeContent(contentRoot, 1f, 0f, contentFadeOut);
                else
                    SetContentVisible(contentRoot, false, 0f);
            }
        }
    }

    // ---------------- Pause / Resume ----------------
    void PauseAll()
    {
        paused = true;
        if (animators != null)
        {
            foreach (var a in animators)
            {
                if (!a) continue;
                if (!animOriginalSpeed.ContainsKey(a)) animOriginalSpeed[a] = a.speed;
                a.speed = 0f;
            }
        }
    }

    void ResumeAll()
    {
        paused = false;
        foreach (var kv in animOriginalSpeed)
        {
            if (kv.Key) kv.Key.speed = kv.Value;
        }
        animOriginalSpeed.Clear();
    }

    // ---------------- Helpers ----------------
    IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!pauseOnTrackingLost || !paused) t += Time.deltaTime;
            yield return null;
        }
    }

    void HideAllImmediate()
    {
        // texts hidden
        foreach (var ti in introTexts) if (ti?.text) { ti.text.maxVisibleWords = 0; SetTMPAlpha(ti.text, 0f, true); }
        foreach (var ti in outroTexts) if (ti?.text) { ti.text.maxVisibleWords = 0; SetTMPAlpha(ti.text, 0f, true); }

        // content hidden
        if (contentRoot)
        {
            if (hardToggleContentActive) contentRoot.SetActive(false);
            else SetContentVisible(contentRoot, false, 0f);
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

    // 3D TMP alpha
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

    IEnumerator CoFadeTMP(TMP_Text tmp, float from, float to, float dur)
    {
        SetTMPAlpha(tmp, from, true);
        if (dur <= 0f) { SetTMPAlpha(tmp, to, true); yield break; }
        float t = 0f;
        while (t < dur)
        {
            if (!pauseOnTrackingLost || !paused)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                SetTMPAlpha(tmp, a, true);
            }
            yield return null;
        }
        SetTMPAlpha(tmp, to, true);
    }

    // Content visibility/fade (best-effort via material color)
    static readonly int PROP_BASECOLOR = Shader.PropertyToID("_BaseColor");
    static readonly int PROP_COLOR = Shader.PropertyToID("_Color");

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

    IEnumerator CoFadeContent(GameObject root, float from, float to, float dur)
    {
        if (!root || dur <= 0f) { SetContentVisible(root, to > 0f, to); yield break; }
        foreach (var r in root.GetComponentsInChildren<Renderer>(true)) if (r) r.enabled = true;

        float t = 0f;
        while (t < dur)
        {
            if (!pauseOnTrackingLost || !paused)
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
        SetContentVisible(root, to > 0f, to);
    }
}
