using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class TrackedAudioPlaylist : MonoBehaviour
{
    [System.Serializable]
    public class ClipEntry
    {
        public AudioClip clip;
        [Tooltip("Silence (seconds) after this clip finishes, before the next starts.")]
        [Min(0f)] public float delayAfter = 0f;
    }

    [Header("Vuforia")]
    [Tooltip("ObserverBehaviour on this ImageTarget. If left empty, auto-finds on this GameObject.")]
    public ObserverBehaviour vuforiaObserver;

    [Header("Playlist (played in order)")]
    public List<ClipEntry> playlist = new List<ClipEntry>();

    [Header("Playback Options")]
    [Tooltip("Optional silence before the very first clip starts (seconds).")]
    [Min(0f)] public float initialDelay = 0f;
    public bool loopPlaylist = false;

    [Header("Audio Source Settings")]
    public AudioSource audioSource;          // auto-filled
    [Range(0f, 1f)] public float baseVolume = 1f;
    [Tooltip("Extra loudness in decibels (e.g., 0 = none, 3 = ~1.41x, 6 = ~2x, 10 = ~3.16x).")]
    public float boostDb = 0f;
    [Range(0f, 1f)] public float spatialBlend = 1f; // 1 = 3D, 0 = 2D
    public float minDistance = 1f;
    public float maxDistance = 20f;

    [Header("Fades")]
    public bool fadeInEachClip = true;
    [Min(0f)] public float fadeInSeconds = 0.25f;
    public bool fadeInOnResume = true;
    [Min(0f)] public float resumeFadeSeconds = 0.15f;

    // runtime
    Coroutine _runner;
    bool _isTracked;
    bool _isRunning;
    int _clipIndex = 0;

    void Reset()
    {
        vuforiaObserver = GetComponent<ObserverBehaviour>();
    }

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!vuforiaObserver) vuforiaObserver = GetComponent<ObserverBehaviour>();

        // sensible defaults
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = 0f; // we’ll set target when playing
    }

    void OnEnable()
    {
        if (vuforiaObserver) vuforiaObserver.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    void OnDisable()
    {
        if (vuforiaObserver) vuforiaObserver.OnTargetStatusChanged -= OnTargetStatusChanged;
        StopAllCoroutines();
        _runner = null;
        _isRunning = false;
    }

    void OnValidate()
    {
        if (audioSource)
        {
            audioSource.spatialBlend = spatialBlend;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
        }
    }

    void OnTargetStatusChanged(ObserverBehaviour _, TargetStatus status)
    {
        bool nowTracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (nowTracked)
        {
            _isTracked = true;

            // Start playlist if not running
            if (!_isRunning)
            {
                _runner = StartCoroutine(Co_Playlist());
                return;
            }

            // If we were paused mid-clip, unpause and optionally fade up
            if (audioSource && !audioSource.isPlaying && audioSource.clip && audioSource.time > 0f)
            {
                audioSource.UnPause();
                if (fadeInOnResume && resumeFadeSeconds > 0f)
                    StartCoroutine(Co_FadeTo(TargetVolume(), resumeFadeSeconds));
                else
                    audioSource.volume = TargetVolume();
            }
        }
        else
        {
            _isTracked = false;
            if (audioSource && audioSource.isPlaying)
                audioSource.Pause();
        }
    }

    IEnumerator Co_Playlist()
    {
        _isRunning = true;

        // pre-roll delay
        if (initialDelay > 0f)
            yield return WaitTracked(initialDelay);

        while (true)
        {
            if (playlist == null || playlist.Count == 0) break;

            if (_clipIndex < 0 || _clipIndex >= playlist.Count)
                _clipIndex = 0;

            var entry = playlist[_clipIndex];
            if (entry != null && entry.clip)
            {
                // start / resume
                if (!audioSource.isPlaying || audioSource.clip != entry.clip)
                {
                    audioSource.clip = entry.clip;
                    audioSource.time = 0f;
                    audioSource.volume = (fadeInEachClip && fadeInSeconds > 0f) ? 0f : TargetVolume();
                    audioSource.Play();

                    if (fadeInEachClip && fadeInSeconds > 0f)
                        StartCoroutine(Co_FadeTo(TargetVolume(), fadeInSeconds));
                }

                // wait for clip end (pause-aware)
                float remaining = entry.clip.length - audioSource.time;
                if (remaining > 0f)
                    yield return WaitTracked(remaining);

                // inter-clip delay (pause-aware)
                if (entry.delayAfter > 0f)
                    yield return WaitTracked(entry.delayAfter);

                _clipIndex++;
                if (_clipIndex >= playlist.Count)
                {
                    if (loopPlaylist) _clipIndex = 0;
                    else break;
                }
            }
            else
            {
                _clipIndex++;
                if (_clipIndex >= playlist.Count)
                {
                    if (loopPlaylist) _clipIndex = 0; else break;
                }
            }
        }

        _isRunning = false;
        _runner = null;
    }

    // === helpers ===
    float TargetVolume()
    {
        // baseVolume * linear gain from dB
        float linear = Mathf.Pow(10f, boostDb / 20f);
        return Mathf.Clamp01(baseVolume * linear);
    }

    IEnumerator Co_FadeTo(float target, float seconds)
    {
        if (seconds <= 0f || !audioSource) { if (audioSource) audioSource.volume = target; yield break; }

        float start = audioSource.volume;
        float t = 0f;
        while (t < seconds)
        {
            if (_isTracked) // only advance while tracked
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                audioSource.volume = Mathf.Lerp(start, target, u);
            }
            yield return null;
        }
        audioSource.volume = target;
    }

    IEnumerator WaitTracked(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (_isTracked) elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
