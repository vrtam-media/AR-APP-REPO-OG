using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SelfOffForTime : MonoBehaviour
{
    [Min(0f)] public float offSeconds = 3f;
    public bool useUnscaledTime = false;

    // guard so our own re-enable doesn't immediately retrigger another OFF
    bool _reenableGuard = false;

    void OnEnable()
    {
        // If we were just re-enabled by the timer, do nothing.
        if (_reenableGuard) { _reenableGuard = false; return; }

        // Schedule re-enable, then go OFF now.
        OffRunner.Ensure().ReenableAfter(this, Mathf.Max(0f, offSeconds), useUnscaledTime);
        gameObject.SetActive(false);
    }

    // ------------ tiny runner that stays alive while we're disabled ------------
    sealed class OffRunner : MonoBehaviour
    {
        static OffRunner _inst;
        public static OffRunner Ensure()
        {
            if (_inst) return _inst;
            var go = new GameObject("__OffTimerRunner");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            _inst = go.AddComponent<OffRunner>();
            return _inst;
        }

        public void ReenableAfter(SelfOffForTime comp, float seconds, bool unscaled)
        {
            StartCoroutine(Co(comp, seconds, unscaled));
        }

        IEnumerator Co(SelfOffForTime comp, float seconds, bool unscaled)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            if (comp && comp.gameObject)
            {
                // set guard so OnEnable doesn't immediately turn it off again
                comp._reenableGuard = true;
                comp.gameObject.SetActive(true);
            }
        }
    }
}
