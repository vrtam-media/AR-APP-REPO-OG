using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vuforia;

[AddComponentMenu("AR/Director/AR Sequence Director (Vuforia + 3D Text WordReveal)")]
public class ARSequenceDirector : MonoBehaviour
{
    [Header("Vuforia / Camera")]
    [Tooltip("Observer (e.g., ImageTarget). If empty, will search on this GameObject.")]
    public ObserverBehaviour observer;
    [Tooltip("AR camera used for billboarding. If empty, Camera.main is used.")]
    public Camera arCamera;

    [Header("Behavior on Tracking")]
    public bool pauseOnTrackingLost = true;

    [Header("Sequence (in order)")]
    public List<Step> steps = new List<Step>()
    {
        // Default wiring placeholders: Intro -> Content -> Outro
        new Step(){ mode = Step.Mode.Text, textTitle="Intro Text", message="Welcome!", wordsPerSecond=3, fadeIn=0.35f, hold=1.5f, fadeOut=0.35f, billboard=true, yawOnly=true },
        new Step(){ mode = Step.Mode.Content, contentTitle="Environment + Models", contentWait = Step.ContentWait.AnimatorsComplete, doFade=true, fadeIn=0.35f, postDelay=0.25f, fadeOut=0.35f, deactivateGO = true },
        new Step(){ mode = Step.Mode.Text, textTitle="Outro Text", message="Thanks for watching.", wordsPerSecond=3, fadeIn=0.35f, hold=1.5f, fadeOut=0.35f, billboard=true, yawOnly=true },
    };

    // --- runtime state ---
    int _index = -1;
    Coroutine _runner;
    bool _paused;
    Step _activeStep;

    // cache animator original speeds so we can pause/resume
    readonly Dictionary<Animator, float> _animOriginalSpeed = new Dictionary<Animator, float>();

    // ===== Unity =====
    void Awake()
    {
        if (!observer) observer = GetComponent<ObserverBehaviour>();
        if (!arCamera) arCamera = Camera.main;
    }

    void OnEnable()
    {
        if (observer != null)
            observer.OnTargetStatusChanged += OnTargetStatusChanged;

        // Ensure everything starts hidden/inactive
        HideAllImmediate();
    }

    void OnDisable()
    {
        if (observer != null)
            observer.OnTargetStatusChanged -= OnTargetStatusChanged;

        if (_runner != null) { StopCoroutine(_runner); _runner = null; }
        _animOriginalSpeed.Clear();
        _activeStep = null;
    }

    void LateUpdate()
    {
        // billboard active text (if any)
        if (_activeStep != null && _activeStep.mode == Step.Mode.Text && _activeStep.billboard && _activeStep.text != null)
        {
            if (!arCamera) arCamera = Camera.main;
            if (!arCamera) return;

            Transform t = _activeStep.text.transform;
            Vector3 toCam = arCamera.transform.position - t.position;
            if (_activeStep.yawOnly) toCam.y = 0f;
            if (toCam.sqrMagnitude > 1e-6f)
            {
                Quaternion target = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
                float lerp = Mathf.Clamp(_activeStep.rotateLerp, 0f, 30f);
                t.rotation = (lerp <= 0f) ? target
                    : Quaternion.Slerp(t.rotation, target, 1f - Mathf.Exp(-lerp * Time.deltaTime));
            }
        }
    }

    // ===== Vuforia status =====
    void OnTargetStatusChanged(ObserverBehaviour beh, TargetStatus status)
    {
        bool isTracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (isTracked)
        {
            if (pauseOnTrackingLost) ResumeSequence();
            if (_runner == null) _runner = StartCoroutine(CoRunSequence(true)); // start/restart clean on first found
        }
        else
        {
            if (pauseOnTrackingLost) PauseSequence();
        }
    }

    [ContextMenu("Restart Sequence")]
    public void Restart()
    {
        if (_runner != null) StopCoroutine(_runner);
        _runner = StartCoroutine(CoRunSequence(true));
    }

    // ===== Sequence =====
    IEnumerator CoRunSequence(bool forceRestart = false)
    {
        if (forceRestart) { _index = -1; HideAllImmediate(); }

        if (_index < 0) { _index = 0; HideAllImmediate(); }

        while (_index < steps.Count)
        {
            // If paused on tracking lost, wait here
            if (pauseOnTrackingLost)
                yield return WaitWhilePaused();

            Step s = steps[_index];
            _activeStep = s;

            switch (s.mode)
            {
                case Step.Mode.Text:
                    yield return CoRunText(s);
                    break;
                case Step.Mode.Content:
                    yield return CoRunContent(s);
                    break;
            }

            _index++;
        }

        _activeStep = null;
        _runner = null;
    }

    IEnumerator CoRunText(Step s)
    {
        if (s.text == null) yield break;

        var tmp = s.text;

        // prepare text
        if (!string.IsNullOrEmpty(s.message)) tmp.text = s.message;
        tmp.ForceMeshUpdate(true, true);
        tmp.maxVisibleCharacters = int.MaxValue;
        tmp.maxVisibleWords = 0;
        SetTMPAlpha(tmp, 0f, true);

        // pre-delay
        if (s.preDelay > 0f) yield return WaitFor(s.preDelay);

        // fade in
        if (s.fadeIn > 0f) yield return CoFadeTMP(tmp, 0f, 1f, s.fadeIn);
        else SetTMPAlpha(tmp, 1f, true);

        // word-by-word
        int total = Mathf.Max(0, tmp.textInfo.wordCount);
        if (total > 0)
        {
            float baseStep = 1f / Mathf.Max(0.05f, s.wordsPerSecond);
            for (int i = 1; i <= total; i++)
            {
                tmp.maxVisibleWords = i;

                float wait = baseStep;
                if (s.punctuationPauses)
                {
                    char t = TailPunct(tmp, i - 1);
                    if (t == ',' || t == ';') wait += s.pauseAfterComma;
                    else if (t == '.' || t == '!' || t == '?') wait += s.pauseAfterPeriod;
                    else if (t == ':' || t == ')' || t == ']' || t == '"' || t == '’' || t == '\'')
                        wait += s.pauseAfterOther;
                }

                yield return WaitFor(wait);
            }
        }

        // hold fully visible after full reveal
        if (s.hold > 0f) yield return WaitFor(s.hold);

        // fade out
        if (s.fadeOut > 0f) yield return CoFadeTMP(tmp, 1f, 0f, s.fadeOut);
        else SetTMPAlpha(tmp, 0f, true);
    }

    IEnumerator CoRunContent(Step s)
    {
        // CONTENT IS HARD-DISABLED BEFORE SHOW (prevents animators running during intro)
        if (s.contentRoot == null) { if (s.hold > 0f) yield return WaitFor(s.hold); yield break; }

        // ensure GO is active only now
        if (s.deactivateGO && !s.contentRoot.activeSelf) s.contentRoot.SetActive(true);

        // fade in (or snap in)
        if (s.doFade && s.fadeIn > 0f) yield return CoFadeContent(s.contentRoot, 0f, 1f, s.fadeIn);
        else SetContentVisible(s.contentRoot, true, 1f);

        // ===== WAIT POLICY =====
        switch (s.contentWait)
        {
            case Step.ContentWait.Duration:
                if (s.hold > 0f) yield return WaitFor(s.hold);
                break;

            case Step.ContentWait.AnimatorsComplete:
                if ((s.animators != null && s.animators.Length > 0) || s.animator != null)
                    yield return WaitForAnimatorsDone(s);
                else
                {
                    // no animators provided; fallback to duration if set
                    if (s.hold > 0f) yield return WaitFor(s.hold);
                }
                break;
        }

        // optional pause AFTER animations are complete (content still visible)
        if (s.postDelay > 0f) yield return WaitFor(s.postDelay);

        // fade out / hide
        if (s.doFade && s.fadeOut > 0f) yield return CoFadeContent(s.contentRoot, 1f, 0f, s.fadeOut);
        SetContentVisible(s.contentRoot, false, 0f);
        if (s.deactivateGO) s.contentRoot.SetActive(false);
    }

    // ===== Pause/Resume =====
    void PauseSequence()
    {
        _paused = true;

        // pause only the animators we know about (from Content steps)
        foreach (var s in steps)
        {
            if (s.mode != Step.Mode.Content || s.contentRoot == null) continue;
            foreach (var a in EnumerateAnimators(s))
            {
                if (!a) continue;
                if (!_animOriginalSpeed.ContainsKey(a)) _animOriginalSpeed[a] = a.speed;
                a.speed = 0f;
            }
        }
    }

    void ResumeSequence()
    {
        _paused = false;
        foreach (var kv in _animOriginalSpeed)
            if (kv.Key) kv.Key.speed = kv.Value;
        _animOriginalSpeed.Clear();
    }

    IEnumerator WaitWhilePaused()
    {
        while (pauseOnTrackingLost && _paused) yield return null;
    }

    IEnumerator WaitFor(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!pauseOnTrackingLost || !_paused)
                t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator WaitForAnimatorsDone(Step s)
    {
        // prefer explicit list; fallback to single animator field
        var list = new List<Animator>();
        if (s.animators != null && s.animators.Length > 0) list.AddRange(s.animators);
        if (s.animator != null) list.Add(s.animator);

        // let one frame settle
        yield return null;

        bool allDone = false;
        while (!allDone)
        {
            if (!pauseOnTrackingLost || !_paused)
            {
                allDone = true;
                foreach (var a in list)
                {
                    if (!a) continue;

                    AnimatorStateInfo st = a.GetCurrentAnimatorStateInfo(s.animLayer);
                    bool rightState = string.IsNullOrEmpty(s.requiredStateName) || st.IsName(s.requiredStateName);

                    // If clip loops, this never completes; ensure your clips end (no loop) or route through a non-loop state.
                    if (!(rightState && st.normalizedTime >= 1f && !a.IsInTransition(s.animLayer)))
                    {
                        allDone = false;
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerable<Animator> EnumerateAnimators(Step s)
    {
        if (s.animators != null) foreach (var a in s.animators) yield return a;
        if (s.animator != null) yield return s.animator;
    }

    // ===== Visibility helpers =====
    void HideAllImmediate()
    {
        foreach (var s in steps)
        {
            if (s.mode == Step.Mode.Text && s.text != null)
            {
                s.text.maxVisibleWords = 0;
                SetTMPAlpha(s.text, 0f, true);
            }
            else if (s.mode == Step.Mode.Content && s.contentRoot != null)
            {
                // HARD disable content so animators cannot run during intro
                if (s.deactivateGO) s.contentRoot.SetActive(false);
                SetContentVisible(s.contentRoot, false, 0f);
            }
        }
    }

    // --- Text (3D TMP) alpha & fades ---
    static void SetTMPAlpha(TMP_Text tmp, float a, bool enableRenderer)
    {
        if (!tmp) return;

        var rend = tmp.GetComponent<Renderer>();
        if (rend)
        {
            rend.enabled = enableRenderer;
            if (rend.material.HasProperty("_FaceColor"))
            {
                Color c = rend.material.GetColor("_FaceColor"); c.a = a;
                rend.material.SetColor("_FaceColor", c);
            }
            else if (rend.material.HasProperty("_Color"))
            {
                Color c = rend.material.color; c.a = a; rend.material.color = c;
            }
        }
    }

    IEnumerator CoFadeTMP(TMP_Text tmp, float from, float to, float dur)
    {
        SetTMPAlpha(tmp, from, true);
        if (dur <= 0f) { SetTMPAlpha(tmp, to, true); yield break; }

        float t = 0f;
        while (t < dur)
        {
            if (!pauseOnTrackingLost || !_paused)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                SetTMPAlpha(tmp, a, true);
            }
            yield return null;
        }
        SetTMPAlpha(tmp, to, true);
    }

    // --- Content (env/model) visibility & fades ---
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
            {
                Color c = r.material.GetColor(PROP_BASECOLOR);
                c.a = visible ? alphaIfVisible : 0f;
                r.material.SetColor(PROP_BASECOLOR, c);
            }
            else if (r.material.HasProperty(PROP_COLOR))
            {
                Color c = r.material.GetColor(PROP_COLOR);
                c.a = visible ? alphaIfVisible : 0f;
                r.material.SetColor(PROP_COLOR, c);
            }
        }
    }

    IEnumerator CoFadeContent(GameObject root, float from, float to, float dur)
    {
        if (!root || dur <= 0f) { SetContentVisible(root, to > 0f, to); yield break; }

        // make sure gameObject is active for the fade
        if (!root.activeSelf) root.SetActive(true);

        foreach (var r0 in root.GetComponentsInChildren<Renderer>(true)) if (r0) r0.enabled = true;

        float t = 0f;
        while (t < dur)
        {
            if (!pauseOnTrackingLost || !_paused)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (!r) continue;
                    if (r.material.HasProperty(PROP_BASECOLOR))
                    {
                        var c = r.material.GetColor(PROP_BASECOLOR); c.a = a; r.material.SetColor(PROP_BASECOLOR, c);
                    }
                    else if (r.material.HasProperty(PROP_COLOR))
                    {
                        var c = r.material.GetColor(PROP_COLOR); c.a = a; r.material.SetColor(PROP_COLOR, c);
                    }
                }
            }
            yield return null;
        }
        SetContentVisible(root, to > 0f, to);
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

    // ===== Data =====
    [Serializable]
    public class Step
    {
        public enum Mode { Text, Content }
        public Mode mode = Mode.Text;

        [Header("Common")]
        [Tooltip("Delay before this step starts (seconds).")]
        public float preDelay = 0f;

        [Tooltip("Fade-in time (seconds). For Text: alpha; For Content: material colors if supported.")]
        public float fadeIn = 0.35f;

        [Tooltip("Time to hold when fully shown (seconds). For Text: AFTER reveal; For Content: used only if Wait=Duration.")]
        public float hold = 1.5f;

        [Tooltip("Fade-out time (seconds).")]
        public float fadeOut = 0.35f;

        // ---- TEXT ----
        [Header("Text Step")]
        public string textTitle;
        [Tooltip("3D TMP (TextMeshPro - Text).")]
        public TMP_Text text;
        [TextArea] public string message = "Sample text";
        [Min(0.05f)] public float wordsPerSecond = 3f;

        public bool punctuationPauses = true;
        public float pauseAfterComma = 0.12f, pauseAfterPeriod = 0.22f, pauseAfterOther = 0.08f;

        [Header("Billboard")]
        public bool billboard = true;
        public bool yawOnly = true;
        [Range(0f, 30f)] public float rotateLerp = 12f;

        // ---- CONTENT ----
        [Header("Content Step")]
        public string contentTitle;
        [Tooltip("Root object of your environment + model(s).")]
        public GameObject contentRoot;

        [Tooltip("If true, the GO will be SetActive(false) at start and only enabled during this step.")]
        public bool deactivateGO = true;

        public bool doFade = true;

        public enum ContentWait { Duration, AnimatorsComplete }
        public ContentWait contentWait = ContentWait.AnimatorsComplete;

        [Tooltip("Animators to wait for when ContentWait = AnimatorsComplete (all must finish).")]
        public Animator[] animators;

        // Back-compat single animator (optional)
        public Animator animator;

        [Tooltip("Animator layer index to poll for completion.")]
        public int animLayer = 0;

        [Tooltip("Optional specific state to wait for. Leave empty to wait the current state's completion.")]
        public string requiredStateName = "";

        [Tooltip("Extra pause AFTER animations finish / duration elapses, before content fades out.")]
        public float postDelay = 0f;
    }
}
