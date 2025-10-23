using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)] // run after parent OnEnable so our hold wins
public class AnimatorHoldThenDisableSimple : MonoBehaviour
{
    [Tooltip("How long to wait after enable before turning this GameObject OFF.")]
    [Min(0f)] public float delaySeconds = 1f;

    [Tooltip("If ON: keep Animator enabled but speed=0 during the hold. If OFF: disable Animator during the hold.")]
    public bool holdByFreezingSpeed = true;

    [Tooltip("Optional. If empty, auto-finds Animator on this object or its children.")]
    public Animator targetAnimator;

    Coroutine _routine;

    void Awake()
    {
        if (!targetAnimator)
            targetAnimator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
    }

    void OnEnable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Co_HoldThenDisable());
    }

    void OnDisable()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        // No cleanup needed; object is going inactive anyway.
    }

    IEnumerator Co_HoldThenDisable()
    {
        // Let parent scripts (e.g., ResumeAll) run first this frame
        yield return null;

        // Apply hold (if we have an Animator)
        if (targetAnimator)
        {
            if (holdByFreezingSpeed)
            {
                targetAnimator.enabled = true; // required to freeze by speed
                targetAnimator.speed = 0f;
            }
            else
            {
                targetAnimator.enabled = false;
            }
        }

        // Wait the custom time (scaled time)
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        // Turn the model OFF
        if (this && gameObject.activeSelf)
            gameObject.SetActive(false);

        _routine = null;
    }
}
