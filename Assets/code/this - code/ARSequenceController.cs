using System.Collections.Generic;
using UnityEngine;
using Vuforia;

namespace Arun.ARSequences
{
    public class ARSequenceSimple : MonoBehaviour
    {
        public enum EndMode { Stop, Loop }

        [Header("Vuforia")]
        public ObserverBehaviour vuforiaObserver; // Drag your ImageTarget (ObserverBehaviour)

        [Header("Intro Texts (in order)")]
        public List<GameObject> introTexts = new List<GameObject>();     // each has WordReveal

        [Header("Content Groups (in order)")]
        public List<GameObject> contentGroups = new List<GameObject>();  // each has AutoPlayOnActive

        [Header("Outro Texts (in order)")]
        public List<GameObject> outroTexts = new List<GameObject>();     // each has WordReveal

        [Header("Durations (seconds)")]
        [Min(0.1f)] public float introDuration = 8f;
        [Min(0.1f)] public float contentDuration = 20f;
        [Min(0.1f)] public float outroDuration = 8f;

        [Header("Behavior")]
        public EndMode endMode = EndMode.Stop;
        public bool stopLeavesLastVisible = true;

        // runtime
        enum Phase { Intro, Content, Outro, Done }
        Phase _phase = Phase.Intro;
        int _index = -1;
        float _elapsed = 0f;
        bool _running;
        bool _paused;

        void Reset()
        {
            vuforiaObserver = GetComponentInParent<ObserverBehaviour>() ?? GetComponent<ObserverBehaviour>();
        }

        void Awake()
        {
            ShowOnly(null); // off at start
        }

        void OnEnable()
        {
            if (!vuforiaObserver)
                vuforiaObserver = GetComponentInParent<ObserverBehaviour>() ?? GetComponent<ObserverBehaviour>();
            if (vuforiaObserver != null)
                vuforiaObserver.OnTargetStatusChanged += OnTargetStatusChanged;
        }

        void OnDisable()
        {
            if (vuforiaObserver != null)
                vuforiaObserver.OnTargetStatusChanged -= OnTargetStatusChanged;
        }

        void OnTargetStatusChanged(ObserverBehaviour _, TargetStatus status)
        {
            bool isTracked =
                status.Status == Status.TRACKED ||
                status.Status == Status.EXTENDED_TRACKED ||
                status.Status == Status.LIMITED;

            if (isTracked)
            {
                if (!_running) StartSequence();
                else if (_paused) ResumeSequence();
            }
            else
            {
                if (_running && !_paused) PauseSequence();
            }
        }

        void Update()
        {
            if (!_running || _paused) return;

            _elapsed += Time.deltaTime;

            switch (_phase)
            {
                case Phase.Intro: if (_elapsed >= introDuration) Next(); break;
                case Phase.Content: if (_elapsed >= contentDuration) Next(); break;
                case Phase.Outro: if (_elapsed >= outroDuration) Next(); break;
            }
        }

        // ---- Flow ----
        void StartSequence()
        {
            _running = true; _paused = false;
            _phase = Phase.Intro; _index = -1;
            Next();
        }

        void Next()
        {
            _elapsed = 0f;
            DeactivateCurrent();

            switch (_phase)
            {
                case Phase.Intro:
                    _index++;
                    if (_index < introTexts.Count) ActivateText(introTexts[_index]);
                    else { _phase = Phase.Content; _index = -1; Next(); }
                    break;

                case Phase.Content:
                    _index++;
                    if (_index < contentGroups.Count) ActivateContent(contentGroups[_index]);
                    else { _phase = Phase.Outro; _index = -1; Next(); }
                    break;

                case Phase.Outro:
                    _index++;
                    if (_index < outroTexts.Count) ActivateText(outroTexts[_index]);
                    else Finish();
                    break;
            }
        }

        void Finish()
        {
            if (endMode == EndMode.Loop)
            {
                _phase = Phase.Intro; _index = -1; Next();
            }
            else
            {
                _phase = Phase.Done; _running = false;
                if (stopLeavesLastVisible) KeepLastVisible();
                else ShowOnly(null);
            }
        }

        void ActivateText(GameObject go)
        {
            if (!go) { Next(); return; }
            ShowOnly(go);

            var wr = go.GetComponent<WordReveal>();
            if (wr)
            {
                wr.ResetReveal();
                wr.StartReveal(Mathf.Max(0.1f, wr.wordsPerSecond)); // per-text speed set on the component
            }
        }

        void ActivateContent(GameObject go)
        {
            if (!go) { Next(); return; }
            ShowOnly(go);
            // AutoPlayOnActive handles play on enable; stop/pause on disable
        }

        void DeactivateCurrent() { /* visibility toggle does the work */ }

        // ---- Pause / Resume ----
        void PauseSequence()
        {
            _paused = true;
            var current = CurrentGO();
            if (!current) return;

            if (_phase == Phase.Intro || _phase == Phase.Outro)
            {
                var wr = current.GetComponent<WordReveal>();
                if (wr) wr.Pause();
            }
            else if (_phase == Phase.Content)
            {
                var auto = current.GetComponent<AutoPlayOnActive>();
                if (auto) auto.PauseAll();
            }
        }

        void ResumeSequence()
        {
            _paused = false;
            var current = CurrentGO();
            if (!current) return;

            if (_phase == Phase.Intro || _phase == Phase.Outro)
            {
                var wr = current.GetComponent<WordReveal>();
                if (wr) wr.Resume();
            }
            else if (_phase == Phase.Content)
            {
                var auto = current.GetComponent<AutoPlayOnActive>();
                if (auto) auto.ResumeAll();
            }
        }

        GameObject CurrentGO()
        {
            switch (_phase)
            {
                case Phase.Intro: return (_index >= 0 && _index < introTexts.Count) ? introTexts[_index] : null;
                case Phase.Content: return (_index >= 0 && _index < contentGroups.Count) ? contentGroups[_index] : null;
                case Phase.Outro: return (_index >= 0 && _index < outroTexts.Count) ? outroTexts[_index] : null;
            }
            return null;
        }

        // ---- Visibility helpers ----
        void ShowOnly(GameObject active)
        {
            foreach (var t in introTexts) if (t) t.SetActive(t == active);
            foreach (var c in contentGroups) if (c) c.SetActive(c == active);
            foreach (var t in outroTexts) if (t) t.SetActive(t == active);
        }

        void KeepLastVisible()
        {
            GameObject last = null;
            if (outroTexts.Count > 0) last = outroTexts[outroTexts.Count - 1];
            else if (contentGroups.Count > 0) last = contentGroups[contentGroups.Count - 1];
            else if (introTexts.Count > 0) last = introTexts[introTexts.Count - 1];
            ShowOnly(last);
        }
    }
}
