using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;  // Mouse, Touchscreen
#endif

[DisallowMultipleComponent]
public class TapAnimationOverrideLite_InputSystem : MonoBehaviour
{
    public enum FireMode { AnimatorTrigger, CrossFadeToState }
    public enum EndDetection { ByClipLength, ByFixedSeconds, ByAnimationEvent }

    [Header("Animator")]
    public Animator animator;
    [Min(0)] public int layer = 0;

    [Header("Override (played on tap)")]
    public FireMode fireMode = FireMode.CrossFadeToState;
    public string triggerName = "";   // used if FireMode = AnimatorTrigger
    public string stateName = "";     // used if FireMode = CrossFadeToState
    public AnimationClip overrideClip;
    [Min(0f)] public float crossFadeToOverride = 0.12f;

    [Header("When to end the override")]
    public EndDetection endDetection = EndDetection.ByClipLength;
    [Min(0f)] public float fixedSeconds = 0.8f;
    [Min(0f)] public float earlyExitEpsilon = 0.05f;

    [Header("Resume (auto-previous)")]
    [Min(0f)] public float crossFadeBack = 0.12f;

    [Header("Input / Raycast")]
    public Camera raycastCamera;                       // defaults to Camera.main
    [Min(0f)] public float maxRaycastDistance = 200f;

    [Header("Tap Handling")]
    public bool ignoreWhileActive = true;
    [Min(0f)] public float tapCooldown = 0f;

    // --- internal
    bool _active;
    float _cooldownTimer;
    Coroutine _runner;
    int _snapHash;
    float _snapNormTime;
    float _snapSpeed;
    int _stateHash;
    bool _waitingForEvent;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!raycastCamera) raycastCamera = Camera.main;
        _stateHash = string.IsNullOrEmpty(stateName) ? 0 : Animator.StringToHash(stateName);

        if (!GetComponentInChildren<Collider>(true))
            Debug.LogWarning($"{name}: needs a Collider to detect taps.");
    }

    void OnValidate()
    {
        _stateHash = string.IsNullOrEmpty(stateName) ? 0 : Animator.StringToHash(stateName);
    }

    void Update()
    {
        if (!animator) return;
        if (tapCooldown > 0f && _cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

        Vector2? tapPos = ReadTapPositionOnce();
        if (tapPos.HasValue) TryTap(tapPos.Value);
    }

    // --- INPUT SYSTEM tap read (mouse or primary touch) ---
    Vector2? ReadTapPositionOnce()
    {
#if ENABLE_INPUT_SYSTEM
        // Touch first (mobile / editor touch simulation)
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
                return touch.position.ReadValue();
        }
        // Mouse fallback
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return Mouse.current.position.ReadValue();
        return null;
#else
        // If someone flips Player Settings to Old or Both, keep a safe fallback:
        if (Input.GetMouseButtonDown(0)) return (Vector2)Input.mousePosition;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            return Input.GetTouch(0).position;
        return null;
#endif
    }

    void TryTap(Vector2 screenPos)
    {
        if (_cooldownTimer > 0f) return;
        if (ignoreWhileActive && _active) return;
        var cam = raycastCamera ? raycastCamera : Camera.main;
        if (!cam) return;

        if (Physics.Raycast(cam.ScreenPointToRay(screenPos), out var hit, maxRaycastDistance))
        {
            if (hit.collider && hit.collider.transform.IsChildOf(transform))
            {
                _cooldownTimer = tapCooldown;
                if (_runner != null) StopCoroutine(_runner);
                _runner = StartCoroutine(Co_OverrideThenResume());
            }
        }
    }

    IEnumerator Co_OverrideThenResume()
    {
        _active = true;

        // Snapshot current state
        var info = animator.GetCurrentAnimatorStateInfo(layer);
        _snapHash = info.shortNameHash;
        _snapNormTime = info.normalizedTime;
        _snapSpeed = Mathf.Max(0.0001f, info.speed);

        // Fire override
        if (fireMode == FireMode.AnimatorTrigger)
        {
            if (!string.IsNullOrEmpty(triggerName))
            {
                animator.ResetTrigger(triggerName);
                animator.SetTrigger(triggerName);
            }
        }
        else
        {
            if (_stateHash == 0 && !string.IsNullOrEmpty(stateName))
                _stateHash = Animator.StringToHash(stateName);
            if (_stateHash != 0)
                animator.CrossFade(_stateHash, crossFadeToOverride, layer);
        }

        // Wait until override state is active (for CrossFade path)
        if (fireMode == FireMode.CrossFadeToState && _stateHash != 0)
        {
            yield return null;
            yield return new WaitUntil(() =>
            {
                var st = animator.GetCurrentAnimatorStateInfo(layer);
                return st.shortNameHash == _stateHash || st.fullPathHash == _stateHash;
            });
        }

        // Duration
        float wait = 0.25f;
        if (endDetection == EndDetection.ByFixedSeconds)
        {
            wait = fixedSeconds;
        }
        else if (endDetection == EndDetection.ByClipLength)
        {
            float len = overrideClip ? overrideClip.length
                                     : animator.GetCurrentAnimatorStateInfo(layer).length;
            if (len <= 0f) len = 0.25f;
            float spd = Mathf.Max(0.0001f, animator.GetCurrentAnimatorStateInfo(layer).speed);
            wait = Mathf.Max(0f, (len / spd) - earlyExitEpsilon);
        }
        else
        {
            _waitingForEvent = true;
            yield return new WaitUntil(() => _waitingForEvent == false);
        }

        if (endDetection != EndDetection.ByAnimationEvent)
        {
            float t = 0f; while (t < wait) { t += Time.deltaTime; yield return null; }
        }

        // Resume to previous snapshot
        if (_snapHash != 0)
            animator.CrossFade(_snapHash, crossFadeBack, layer, _snapNormTime % 1f);

        _active = false;
    }

    // Call from an Animation Event at end of override when using ByAnimationEvent
    public void SignalOverrideFinished() { _waitingForEvent = false; }
}
