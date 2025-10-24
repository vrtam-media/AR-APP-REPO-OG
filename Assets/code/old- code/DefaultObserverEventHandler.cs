using System.Collections;
using UnityEngine;

/// Attach anywhere (e.g., on the ImageTarget). 
/// In Vuforia's DefaultObserverEventHandler events:
///  - OnTargetFound  -> ARVuforiaDelayedSequence.OnTargetFound
///  - OnTargetLost   -> ARVuforiaDelayedSequence.OnTargetLost
[DisallowMultipleComponent]
public class ARVuforiaDelayedSequence : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator on Hanuman")]
    public Animator hanumanAnimator;
    [Tooltip("AudioSource for narration/voice-over")]
    public AudioSource narration;

    [Header("Animation")]
    [Tooltip("Exact name of the walking state in the Animator")]
    public string walkStateName = "Walk";
    [Range(0f, 1f)] public float crossFadeSeconds = 0.1f;
    [Tooltip("Delay after tracking before animation starts (seconds)")]
    [Min(0f)] public float animationDelay = 0.8f;

    [Header("Audio")]
    [Tooltip("Delay after tracking before audio starts (seconds)")]
    [Min(0f)] public float audioDelay = 0.8f;

    Coroutine _animCo, _audioCo;

    void Reset()
    {
        if (narration)
        {
            narration.playOnAwake = false;
            narration.spatialBlend = 0f; // 2D voice-over
        }
    }

    // --------- Vuforia hooks ----------
    public void OnTargetFound()
    {
        // cancel any previous runs then schedule fresh ones
        StopAllRunning();
        if (hanumanAnimator)
        {
            hanumanAnimator.enabled = true;
            _animCo = StartCoroutine(StartAnimAfterDelay(animationDelay));
        }
        if (narration)
            _audioCo = StartCoroutine(StartAudioAfterDelay(audioDelay));
    }

    public void OnTargetLost()
    {
        // hard stop & reset both
        StopAllRunning();

        if (narration)
            narration.Stop();

        if (hanumanAnimator)
        {
            // rewind and disable so next detection starts clean
            if (!string.IsNullOrEmpty(walkStateName))
                hanumanAnimator.Play(walkStateName, 0, 0f);
            hanumanAnimator.Update(0f);   // apply rewind immediately
            hanumanAnimator.enabled = false;
        }
    }

    // --------- Internals ----------
    IEnumerator StartAnimAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (!hanumanAnimator) yield break;

        int stateHash = Animator.StringToHash(walkStateName);
        if (crossFadeSeconds > 0f && hanumanAnimator.HasState(0, stateHash))
            hanumanAnimator.CrossFadeInFixedTime(stateHash, crossFadeSeconds, 0, 0f);
        else
            hanumanAnimator.Play(stateHash, 0, 0f);

        hanumanAnimator.speed = 1f;
    }

    IEnumerator StartAudioAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (narration && !narration.isPlaying)
            narration.Play();
    }

    void StopAllRunning()
    {
        if (_animCo != null) { StopCoroutine(_animCo); _animCo = null; }
        if (_audioCo != null) { StopCoroutine(_audioCo); _audioCo = null; }
    }
}
