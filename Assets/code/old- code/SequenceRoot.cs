using System.Collections;
using System.Linq;
using UnityEngine;
using Vuforia;
using TMPro;

public class ARSequenceController : MonoBehaviour
{
    [Header("Vuforia")]
    [Tooltip("Leave empty to auto-find on parent ImageTarget")]
    public ObserverBehaviour observer;

    [Header("Stage Objects")]
    public GameObject title3D;
    public GameObject environmentGroup;
    public GameObject moral3D;

    [Header("Optional: Text Bindings")]
    public TMP_Text titleText;
    public string titleString = "Book Title";
    public TMP_Text moralText;
    public string moralString = "Moral of the story goes here.";

    [Header("Characters")]
    [Tooltip("All character animators that should play during Stage 2")]
    public Animator[] characterAnimators;

    [Header("Timings")]
    [Tooltip("How long to show the title before Stage 2")]
    public float titleDuration = 2f;
    [Tooltip("Extra wait after ALL animations are finished, before showing Moral")]
    public float postAnimationDelay = 2f;

    [Header("Behavior")]
    [Tooltip("Show only one stage at a time (hide previous stage automatically)")]
    public bool showOneAtATime = true;

    // State
    private bool _started;
    private bool _completed;
    private bool _paused;
    private Coroutine _runner;

    // Cache
    private AudioSource[] _audioSourcesInSequence;
    private ParticleSystem[] _particlesInSequence;

    void Awake()
    {
        if (!observer) observer = GetComponentInParent<ObserverBehaviour>();

        // Optional text injection
        if (titleText) titleText.text = titleString;
        if (moralText) moralText.text = moralString;

        // Cache audio & particles under the sequence
        _audioSourcesInSequence = GetComponentsInChildren<AudioSource>(includeInactive: true);
        _particlesInSequence = GetComponentsInChildren<ParticleSystem>(includeInactive: true);

        // Ensure clean initial state for prefab drops
        SafeSetActive(title3D, false);
        SafeSetActive(environmentGroup, false);
        SafeSetActive(moral3D, false);
    }

    void OnEnable()
    {
        if (observer != null)
            observer.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    void OnDisable()
    {
        if (observer != null)
            observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour ob, TargetStatus status)
    {
        // Version-agnostic: consider anything that's NOT the "lost" state as found.
        // Works across Vuforia variants without referencing missing enum members.
        var stateName = status.Status.ToString(); // e.g., "TRACKED", "EXTENDED_TRACKED", "LIMITED", "NO_POSE", "NOT_OBSERVED"
        bool isFound = stateName != "NO_POSE" && stateName != "NOT_OBSERVED" && stateName != "UNKNOWN";

        if (isFound) OnFound();
        else OnLost();
    }


    private void OnFound()
    {
        if (_completed) { Resume(); return; }    // if you want restart on re-find, flip to Restart()
        if (!_started)
        {
            _started = true;
            _runner = StartCoroutine(RunSequence());
        }
        else
        {
            Resume();
        }
    }

    private void OnLost()
    {
        Pause();
    }

    private IEnumerator RunSequence()
    {
        // --- Stage 1: Title ---
        ShowOnly(title3D);
        yield return WaitForSecondsPausable(titleDuration);

        // --- Stage 2: Environment + Characters ---
        ShowOnly(environmentGroup);
        PlayAllAnimators();
        ResumeAudioAndParticles(); // if you have ambient in env
        // Wait until all character animators finish their current state
        yield return WaitUntilAllAnimatorsFinished();

        // Extra hold after animations
        yield return WaitForSecondsPausable(postAnimationDelay);

        // --- Stage 3: Moral ---
        ShowOnly(moral3D);

        _completed = true;
        _runner = null;
    }

    // ---------- Pause/Resume ----------
    public void Pause()
    {
        _paused = true;
        SetAnimatorsSpeed(0f);
        PauseAudioAndParticles();
    }

    public void Resume()
    {
        _paused = false;
        SetAnimatorsSpeed(1f);
        ResumeAudioAndParticles();
    }

    public void Restart()
    {
        if (_runner != null) StopCoroutine(_runner);
        _started = false; _completed = false; _paused = false;
        SafeSetActive(title3D, false);
        SafeSetActive(environmentGroup, false);
        SafeSetActive(moral3D, false);
        _runner = StartCoroutine(RunSequence());
    }

    // ---------- Helpers ----------
    private void ShowOnly(GameObject go)
    {
        if (!showOneAtATime)
        {
            SafeSetActive(go, true);
            return;
        }

        SafeSetActive(title3D, go == title3D);
        SafeSetActive(environmentGroup, go == environmentGroup);
        SafeSetActive(moral3D, go == moral3D);
    }

    private void SafeSetActive(GameObject g, bool on)
    {
        if (g && g.activeSelf != on) g.SetActive(on);
    }

    private void PlayAllAnimators()
    {
        foreach (var a in characterAnimators)
        {
            if (!a) continue;
            a.speed = 1f;
            a.Update(0f);      // force state update
            a.Play(0, 0, 0f);  // play from beginning (layer 0)
        }
    }

    private void SetAnimatorsSpeed(float s)
    {
        foreach (var a in characterAnimators)
            if (a) a.speed = s;
    }

    private IEnumerator WaitUntilAllAnimatorsFinished()
    {
        while (true)
        {
            if (!_paused)
            {
                bool allDone = characterAnimators
                    .Where(a => a != null)
                    .All(AnimatorFinishedOnce);
                if (allDone) yield break;
            }
            yield return null;
        }
    }

    private bool AnimatorFinishedOnce(Animator a)
    {
        if (!a) return true;
        // Check base layer; ensure not in transition and normalized time >= 1
        var st = a.GetCurrentAnimatorStateInfo(0);
        return !a.IsInTransition(0) && st.normalizedTime >= 0.99f;
    }

    private IEnumerator WaitForSecondsPausable(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!_paused) t += Time.deltaTime;
            yield return null;
        }
    }

    private void PauseAudioAndParticles()
    {
        foreach (var au in _audioSourcesInSequence) if (au) au.Pause();
        foreach (var ps in _particlesInSequence) if (ps) ps.Pause();
    }

    private void ResumeAudioAndParticles()
    {
        foreach (var au in _audioSourcesInSequence) if (au) au.UnPause();
        foreach (var ps in _particlesInSequence) if (ps) ps.Play();
    }
}
