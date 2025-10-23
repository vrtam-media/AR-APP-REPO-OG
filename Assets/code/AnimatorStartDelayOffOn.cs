using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)] // run after parent OnEnable (so our OFF wins)
public class AnimatorStartDelayOffOn : MonoBehaviour
{
    [Tooltip("Keep the Animator OFF for this many seconds after this object is enabled.")]
    [Min(0f)] public float delaySeconds = 1f;

    [Tooltip("Optional. If empty, auto-finds an Animator on this object or its children.")]
    public Animator targetAnimator;

    Coroutine _routine;

    void Awake()
    {
        if (!targetAnimator)
            targetAnimator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
    }

    void OnEnable()
    {
        if (!targetAnimator) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Co_OffThenOn());
    }

    void OnDisable()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        // Safety: don't force anything here; when re-enabled we'll handle OFF/ON again.
    }

    IEnumerator Co_OffThenOn()
    {
        // Let any parent "ResumeAll()" finish first
        yield return null;

        if (!this || !enabled || !targetAnimator) yield break;

        // Force OFF during delay
        targetAnimator.enabled = false;

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        if (!this || !enabled || !targetAnimator) yield break;

        // Turn ON after delay
        targetAnimator.enabled = true;

        _routine = null;
    }
}
