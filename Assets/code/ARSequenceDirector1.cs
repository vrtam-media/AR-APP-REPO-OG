using System.Collections;
using UnityEngine;
using Vuforia;

[AddComponentMenu("AR/Director/AR Sequence Director (Intro → Content → Outro)")]
public class ARSequenceDirector1 : MonoBehaviour
{
    [Header("Tracking")]
    public ObserverBehaviour observer;           // Drag your ImageTarget (ObserverBehaviour)
    public bool pauseOnTrackingLost = true;      // Pause/resume when tracking drops/returns

    [Header("Intro")]
    public WordByWordText intro;                 // Drag the WordByWordText on your Intro 3D TMP
    public float introMinOnScreen = 0.5f;        // Extra time to keep intro after reveal (seconds)

    [Header("Bridge")]
    public float delayAfterIntro = 0.0f;         // Optional beat between intro-off and content-on

    [Header("Content")]
    public ContentGroup content;                 // Drag the ContentGroup on your ContentRoot

    [Header("Outro")]
    public WordByWordText outro;                 // Drag the WordByWordText on your Outro 3D TMP
    public float outroMinOnScreen = 0.5f;

    Coroutine runner;
    bool paused;

    void Reset()
    {
        observer = GetComponent<ObserverBehaviour>();
    }

    void Awake()
    {
        if (!observer) observer = GetComponent<ObserverBehaviour>();
    }

    void OnEnable()
    {
        if (observer) observer.OnTargetStatusChanged += OnTargetStatusChanged;
        HideAllImmediate();
    }

    void OnDisable()
    {
        if (observer) observer.OnTargetStatusChanged -= OnTargetStatusChanged;
        if (runner != null) StopCoroutine(runner);
        runner = null;
    }

    void OnTargetStatusChanged(ObserverBehaviour beh, TargetStatus status)
    {
        bool isTracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (isTracked)
        {
            ResumeAll();
            if (runner == null) runner = StartCoroutine(CoRun());
        }
        else
        {
            if (pauseOnTrackingLost) PauseAll();
        }
    }

    void HideAllImmediate()
    {
        if (intro) intro.HideImmediate();
        if (outro) outro.HideImmediate();
        if (content) content.HideImmediate();
    }

    void PauseAll()
    {
        paused = true;
        if (intro) intro.Pause();
        if (outro) outro.Pause();
        if (content) content.Pause();
    }

    void ResumeAll()
    {
        paused = false;
        if (intro) intro.Resume();
        if (outro) outro.Resume();
        if (content) content.Resume();
    }

    IEnumerator CoRun()
    {
        // Intro (text only)
        if (intro)
        {
            float revealT = 0f;
            intro.Play(this, introMinOnScreen,
                onDone: null,
                onRevealTime: t => revealT = t);

            // wait until intro finishes (fade out handled inside)
            // crude way: poll until no current playing coroutine (we rely on alpha==0 after done)
            // Safer: yield on a small sentinel by wrapping Play; for simplicity we wait on fade duration + reveal guess.
            // Instead, we poll renderer visibility:
            yield return StartCoroutine(WaitUntilInvisible(intro));
        }

        // Optional beat before content
        if (delayAfterIntro > 0f) yield return Wait(delayAfterIntro);

        // Content (env + characters turn ON; animations must finish if configured)
        if (content)
        {
            yield return StartCoroutine(content.Show(this));
            // Wait for animators to finish if option is enabled
            yield return StartCoroutine(content.WaitUntilFinished());
            yield return StartCoroutine(content.Hide(this));
        }

        // Outro (text only)
        if (outro)
        {
            outro.Play(this, outroMinOnScreen);
            yield return StartCoroutine(WaitUntilInvisible(outro));
        }

        runner = null; // done
    }

    IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!pauseOnTrackingLost || !paused) t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator WaitUntilInvisible(WordByWordText w)
    {
        if (w == null) yield break;
        var tmp = w.text;
        if (!tmp) yield break;

        var rend = tmp.GetComponent<Renderer>();
        float safety = 0f, safetyCap = 120f; // 2 min cap

        while (safety < safetyCap)
        {
            bool vis = rend && rend.enabled && GetAlpha(rend) > 0.001f;
            if (!vis) break;

            if (!pauseOnTrackingLost || !paused) safety += Time.deltaTime;
            yield return null;
        }
    }

    float GetAlpha(Renderer r)
    {
        if (!r) return 0f;
        if (r.material.HasProperty("_FaceColor"))
            return r.material.GetColor("_FaceColor").a;
        if (r.material.HasProperty("_Color"))
            return r.material.GetColor("_Color").a;
        return 1f;
    }
}
