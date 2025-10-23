using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[DisallowMultipleComponent]
public class VuforiaSequencePlayer : MonoBehaviour
{
    // --------- Data Model ---------
    public enum WaitMode { ClipLength, FixedDuration }

    [System.Serializable]
    public class Step
    {
        [Header("Animation")]
        public Animator animator;                    // Drag the target Animator
        public AnimationClip clip;                   // Drag the clip to play
        [Min(0f)] public float animationDelay = 0f;  // Delay before anim starts
        [Range(0f, 1f)] public float crossFade = 0.15f;
        public bool loopAnimation = false;

        [Header("Audio (paired with this step)")]
        public AudioSource audioSource;              // Drag a source (can reuse)
        public AudioClip audioClip;                  // Drag the clip for this step
        [Min(0f)] public float audioDelay = 0f;
        public bool loopAudio = false;

        [Header("Advance")]
        public WaitMode waitMode = WaitMode.ClipLength;
        [Tooltip("Only used when WaitMode = FixedDuration")]
        [Min(0f)] public float fixedDuration = 0f;
    }

    [Header("Sequence")]
    public List<Step> steps = new();

    [Header("Idle Audio (optional)")]
    public AudioSource idleSource;      // Drag if you want idle VO/ambience
    public AudioClip idleClip;
    public bool idleLoop = true;
    [Tooltip("Play idle between steps (during delays)")]
    public bool playIdleDuringGaps = true;
    [Tooltip("Play idle after the last step finishes")]
    public bool playIdleAfterSequence = true;

    [Header("Options")]
    [Tooltip("If true, the sequence auto-starts when OnTargetFound is fired. Wire Vuforia: Found → StartOrResume()")]
    public bool startOnFound = true;

    [Header("Startup Gating")]
    [Tooltip("Disable animators at Awake so default states can't auto-play.")]
    public bool disableAnimatorsAtStart = true;
    [Tooltip("Force all audio sources to not Play On Awake and stop them at Awake.")]
    public bool stopAudioAtStart = true;
    [Tooltip("When StopAll() is called, disable the animators (prevents any controller from auto-playing).")]
    public bool disableAnimatorOnStop = true;

    // --------- Runtime ----------
    private int _index = 0;                    // current step index
    private bool _running = false;             // sequencing coroutine running
    private Coroutine _runner;

    // One PlayableGraph per Animator (so you can drive clips directly)
    class GraphBundle
    {
        public PlayableGraph graph;
        public AnimationMixerPlayable mixer;   // we crossfade clips here
        public int activeInput = -1;           // current input index
    }
    private readonly Dictionary<Animator, GraphBundle> _graphs = new();

    // ---------- Startup gating ----------
    void Awake()
    {
        if (stopAudioAtStart)
        {
            foreach (var s in steps)
            {
                if (s.audioSource)
                {
                    s.audioSource.playOnAwake = false;
                    s.audioSource.Stop();
                }
            }
            if (idleSource)
            {
                idleSource.playOnAwake = false;
                idleSource.Stop();
            }
        }

        if (disableAnimatorsAtStart)
        {
            foreach (var s in steps)
                if (s.animator) s.animator.enabled = false;
        }
    }

    // ---------- Public API (Vuforia hooks) ----------
    // Wire this to DefaultObserverEventHandler.OnTargetFound
    public void StartOrResume()
    {
        if (!startOnFound) return;
        if (_running) return;
        _runner = StartCoroutine(SequenceCR());
    }

    // Wire this to DefaultObserverEventHandler.OnTargetLost
    // Pause (best for brief tracking flickers). Use StopAll() if you want hard stop.
    public void Pause()
    {
        if (_runner != null) { StopCoroutine(_runner); _runner = null; }
        _running = false;

        foreach (var kv in _graphs)
            if (kv.Value.graph.IsValid()) kv.Value.graph.Stop();

        foreach (var s in steps)
            if (s.audioSource) s.audioSource.Pause();

        if (idleSource) idleSource.Pause();
    }

    // Hard stop (no reset of index/step). Keep for your manual “Stop” button or if you wire it to Lost.
    public void StopAll()
    {
        if (_runner != null) { StopCoroutine(_runner); _runner = null; }
        _running = false;

        // Stop all step audios
        foreach (var s in steps)
        {
            if (s.audioSource) s.audioSource.Stop();
        }

        // Stop idle
        if (idleSource) idleSource.Stop();

        // Stop all graphs but keep state/index
        foreach (var kv in _graphs)
            if (kv.Value.graph.IsValid()) kv.Value.graph.Stop();

        if (disableAnimatorOnStop)
        {
            foreach (var s in steps)
                if (s.animator) s.animator.enabled = false;
        }
    }

    // Optional buttons for your UI later
    public void ResetSequence()
    {
        StopAll();
        _index = 0;
        foreach (var s in steps)
        {
            if (s.animator) s.animator.Rebind();
        }
    }

    public void Resume()
    {
        if (_running) return;
        _runner = StartCoroutine(SequenceCR());
    }

    // ---------- Sequence Core ----------
    IEnumerator SequenceCR()
    {
        _running = true;

        // Ensure graphs exist for all animators we will use
        foreach (var s in steps)
            if (s.animator && !_graphs.ContainsKey(s.animator))
                BuildGraphFor(s.animator);

        while (_index < steps.Count)
        {
            var step = steps[_index];

            // Start idle during the gap BEFORE this step (if enabled)
            if (playIdleDuringGaps && idleSource && idleClip)
            {
                if (!idleSource.isPlaying)
                {
                    idleSource.clip = idleClip;
                    idleSource.loop = idleLoop;
                    idleSource.Play();
                }
            }

            // Kick off the step with per-track delays
            Coroutine animCo = null, audioCo = null;

            if (step.clip && step.animator)
                animCo = StartCoroutine(PlayAnimAfterDelay(step));

            if (step.audioClip && step.audioSource)
                audioCo = StartCoroutine(PlayAudioAfterDelay(step));

            // As soon as either anim or audio starts, stop idle (gap is over)
            float waited = 0f;
            bool animStarted = (animCo == null);
            bool audioStarted = (audioCo == null);
            while (!animStarted || !audioStarted)
            {
                waited += Time.deltaTime;
                if (!animStarted && waited >= step.animationDelay) animStarted = true;
                if (!audioStarted && waited >= step.audioDelay) audioStarted = true;

                if ((animStarted || audioStarted) && idleSource && idleSource.isPlaying)
                    idleSource.Stop();

                yield return null;
            }

            // Wait until it’s time to advance (clip length or fixed duration)
            float waitTime = 0f;
            if (step.waitMode == WaitMode.ClipLength && step.clip)
                waitTime = Mathf.Max(0.01f, step.clip.length); // one cycle
            else if (step.waitMode == WaitMode.FixedDuration)
                waitTime = Mathf.Max(0f, step.fixedDuration);

            if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

            // Next step
            _index++;
        }

        // Sequence finished
        _running = false;

        if (playIdleAfterSequence && idleSource && idleClip)
        {
            idleSource.clip = idleClip;
            idleSource.loop = idleLoop;
            idleSource.Play();
        }
    }

    IEnumerator PlayAnimAfterDelay(Step s)
    {
        if (s.animationDelay > 0f) yield return new WaitForSeconds(s.animationDelay);

        // Enable animator right before playing so default controller can't auto-run earlier.
        if (!s.animator.enabled) s.animator.enabled = true;

        var gb = _graphs[s.animator];
        if (!gb.graph.IsValid()) yield break;

        var newPlayable = AnimationClipPlayable.Create(gb.graph, s.clip);
        newPlayable.SetApplyFootIK(false);
        newPlayable.SetApplyPlayableIK(false);
        newPlayable.SetTime(0);
        newPlayable.SetSpeed(1);
        newPlayable.SetPropagateSetTime(true);
        if (s.loopAnimation)
            newPlayable.SetDuration(double.PositiveInfinity);
        else
            newPlayable.SetDuration(s.clip.length);

        int newIndex = gb.mixer.GetInputCount();
        gb.mixer.SetInputCount(newIndex + 1);
        gb.graph.Connect(newPlayable, 0, gb.mixer, newIndex);
        gb.mixer.SetInputWeight(newIndex, 0f);

        // crossfade from previous to new
        float t = 0f;
        float dur = Mathf.Max(0f, s.crossFade);
        int prev = gb.activeInput;

        if (!gb.graph.IsPlaying()) gb.graph.Play();

        while (t < dur)
        {
            float w = (dur <= 0f) ? 1f : (t / dur);
            if (prev >= 0) gb.mixer.SetInputWeight(prev, 1f - w);
            gb.mixer.SetInputWeight(newIndex, w);
            t += Time.deltaTime;
            yield return null;
        }

        if (prev >= 0) gb.mixer.SetInputWeight(prev, 0f);
        gb.mixer.SetInputWeight(newIndex, 1f);

        // detach previous playable to keep mixer lean
        if (prev >= 0)
        {
            var prevPlayable = gb.mixer.GetInput(prev);
            gb.mixer.DisconnectInput(prev);
            prevPlayable.Destroy();
        }

        gb.activeInput = newIndex;
        _graphs[s.animator] = gb;
    }

    IEnumerator PlayAudioAfterDelay(Step s)
    {
        if (s.audioDelay > 0f) yield return new WaitForSeconds(s.audioDelay);
        if (!s.audioSource) yield break;

        s.audioSource.playOnAwake = false;
        s.audioSource.clip = s.audioClip;
        s.audioSource.loop = s.loopAudio;
        s.audioSource.Play();
    }

    void BuildGraphFor(Animator anim)
    {
        var graph = PlayableGraph.Create($"SequenceGraph-{anim.name}");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var output = AnimationPlayableOutput.Create(graph, "AnimOutput", anim);
        var mixer = AnimationMixerPlayable.Create(graph, 0); // no obsolete normalizeWeights arg
        output.SetSourcePlayable(mixer);

        _graphs[anim] = new GraphBundle
        {
            graph = graph,
            mixer = mixer,
            activeInput = -1
        };
    }

    void OnDisable()
    {
        // Ensure clean shutdown (but do not reset index)
        Pause();
    }

    void OnDestroy()
    {
        foreach (var kv in _graphs)
            if (kv.Value.graph.IsValid()) kv.Value.graph.Destroy();
        _graphs.Clear();
    }
}
