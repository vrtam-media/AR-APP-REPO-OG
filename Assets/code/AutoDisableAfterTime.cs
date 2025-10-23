using System.Collections;
using UnityEngine;
using Vuforia; // optional: used only if you enable pauseWhileUntracked

[DisallowMultipleComponent]
public class AutoDisableAfterTime : MonoBehaviour
{
    [Header("Timer")]
    [Tooltip("How long after enable before this object turns itself OFF.")]
    [Min(0f)] public float lifetimeSeconds = 5f;

    [Tooltip("Ignore Time.timeScale when counting down.")]
    public bool useUnscaledTime = false;

    [Header("Tracking (optional)")]
    [Tooltip("If ON, the countdown pauses whenever the ImageTarget is not tracked.")]
    public bool pauseWhileUntracked = false;

    [Tooltip("Observer on the ImageTarget. If empty, tries to auto-find on parents.")]
    public ObserverBehaviour vuforiaObserver;

    Coroutine _routine;
    bool _isTracked = true; // default true; only matters if pauseWhileUntracked is ON

    void Reset()
    {
        // try to auto-find
        vuforiaObserver = GetComponentInParent<ObserverBehaviour>();
    }

    void OnEnable()
    {
        if (pauseWhileUntracked && !vuforiaObserver)
            vuforiaObserver = GetComponentInParent<ObserverBehaviour>();

        if (pauseWhileUntracked && vuforiaObserver)
            vuforiaObserver.OnTargetStatusChanged += OnTargetStatusChanged;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Co_CountdownThenDisable());
    }

    void OnDisable()
    {
        if (pauseWhileUntracked && vuforiaObserver)
            vuforiaObserver.OnTargetStatusChanged -= OnTargetStatusChanged;

        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
    }

    void OnTargetStatusChanged(ObserverBehaviour _, TargetStatus status)
    {
        // Treat TRACKED / EXTENDED_TRACKED / LIMITED as "tracked"
        _isTracked = status.Status == Status.TRACKED
                 || status.Status == Status.EXTENDED_TRACKED
                 || status.Status == Status.LIMITED;
    }

    IEnumerator Co_CountdownThenDisable()
    {
        float goal = Mathf.Max(0f, lifetimeSeconds);
        float elapsed = 0f;

        while (elapsed < goal)
        {
            if (!pauseWhileUntracked || _isTracked)
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            yield return null;
        }

        // Turn this model OFF
        gameObject.SetActive(false);
        _routine = null;
    }
}
