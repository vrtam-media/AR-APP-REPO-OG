
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class WordReveal : MonoBehaviour
{
    [Header("Reveal")]
    [Min(0.1f)] public float wordsPerSecond = 4f;
    [Tooltip("Runtime multiplier—can be driven by a controller or animation.")]
    [Min(0f)] public float speedMultiplier = 1f;

    [Header("Face Camera (built-in billboard)")]
    public bool faceCamera = true;
    [Tooltip("Leave empty to use Camera.main")]
    public Camera targetCamera;

    TMP_Text _tmp;
    int _totalWords;
    float _accum;
    bool _running;
    bool _paused;

    void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
        Prepare();
    }

    void OnEnable() => Prepare();

    void Prepare()
    {
        if (!_tmp) _tmp = GetComponent<TMP_Text>();
        _tmp.ForceMeshUpdate();
        _totalWords = _tmp.textInfo.wordCount;
        _tmp.maxVisibleWords = 0;
        _accum = 0f;
        _running = false;
        _paused = false;
    }

    public void StartReveal(float wps)
    {
        wordsPerSecond = Mathf.Max(0.1f, wps);
        _accum = 0f;
        if (_tmp) _tmp.maxVisibleWords = 0;
        _running = true;
        _paused = false;
    }

    public void ResetReveal()
    {
        _running = false;
        _paused = false;
        _accum = 0f;
        if (_tmp) _tmp.maxVisibleWords = 0;
    }

    public void Pause() { _paused = true; }
    public void Resume() { _paused = false; }

    public bool IsFinished => _tmp && _tmp.maxVisibleWords >= _totalWords;

    void Update()
    {
        // Billboard (no extra script needed)
        if (faceCamera)
        {
            var cam = targetCamera ? targetCamera : Camera.main;
            if (cam)
            {
                Vector3 dir = transform.position - cam.transform.position;
                if (dir.sqrMagnitude > 1e-6f)
                    transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }

        if (!_running || _paused || !_tmp) return;

        float wps = wordsPerSecond * Mathf.Max(0f, speedMultiplier);
        _accum += Time.deltaTime * wps;

        int target = Mathf.Clamp(Mathf.FloorToInt(_accum), 0, _totalWords);
        if (target != _tmp.maxVisibleWords)
            _tmp.maxVisibleWords = target;

        if (_tmp.maxVisibleWords >= _totalWords)
            _running = false;
    }
}
