using System.Collections;
using TMPro;
using UnityEngine;

[AddComponentMenu("AR/UI/AR 3D Text – Billboard + Word Reveal")]
public class AR3DTextWordReveal : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] TMP_Text label;                 // Assign your TextMeshPro (3D or UGUI)
    [TextArea] public string message = "Hello from AR world.";

    [Header("Timing")]
    public bool playOnEnable = true;
    [Tooltip("Nothing is visible until this delay finishes.")]
    [Min(0f)] public float startDelay = 0f;
    [Min(0.05f)] public float wordsPerSecond = 3f;

    [Header("Natural Pauses")]
    public bool punctuationPauses = true;
    [Min(0f)] public float pauseAfterComma = 0.12f;
    [Min(0f)] public float pauseAfterPeriod = 0.22f;
    [Min(0f)] public float pauseAfterOther = 0.08f;

    [Header("Billboard")]
    public Camera cam;                 // auto = Camera.main
    public bool faceCamera = true;
    public bool yawOnly = true;        // keep upright
    [Range(0f, 30f)] public float rotateLerp = 12f;

    [Header("Auto-Scale (3D)")]
    public bool autoScale = true;
    [Tooltip("Meters where size looks right")]
    public float referenceDistance = 1.5f;
    [Tooltip("Local scale at referenceDistance. For 3D TMP use ~0.1; for UGUI world-canvas use ~0.001.")]
    public float scaleAtRef = 0.1f;
    public float minScale = 0.05f, maxScale = 0.5f;

    Renderer labelRenderer;  // for 3D TMP
    Coroutine co;

    void Reset() { label = GetComponent<TMP_Text>() ?? GetComponentInChildren<TMP_Text>(true); }
    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>() ?? GetComponentInChildren<TMP_Text>(true);
        labelRenderer = label ? label.GetComponent<Renderer>() : null;
    }

    void OnEnable()
    {
        if (label && !string.IsNullOrEmpty(message)) label.text = message;
        if (playOnEnable) Play();
    }

    public void Play(string newText = null)
    {
        if (!label) return;
        if (!string.IsNullOrEmpty(newText)) { message = newText; label.text = message; }
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoRun());
    }

    [ContextMenu("Skip To End")]
    public void SkipToEnd()
    {
        if (!label) return;
        if (co != null) StopCoroutine(co);
        ShowLabel(true);
        label.ForceMeshUpdate(true, true);
        label.maxVisibleCharacters = int.MaxValue;
        label.maxVisibleWords = int.MaxValue;
        co = null;
    }

    IEnumerator CoRun()
    {
        // Completely hide the label during the delay
        ShowLabel(false);

        // Build word info
        yield return null;                        // wait one frame for mesh
        label.ForceMeshUpdate(true, true);

        // Wait start delay
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        // Reveal begins now → make label visible
        ShowLabel(true);

        int total = Mathf.Max(0, label.textInfo.wordCount);
        label.maxVisibleCharacters = int.MaxValue;
        label.maxVisibleWords = 0;
        if (total == 0) { co = null; yield break; }

        float step = 1f / wordsPerSecond;
        for (int i = 1; i <= total; i++)
        {
            label.maxVisibleWords = i;

            float wait = step;
            if (punctuationPauses)
            {
                char t = TailPunct(i - 1);
                if (t == ',' || t == ';') wait += pauseAfterComma;
                else if (t == '.' || t == '!' || t == '?') wait += pauseAfterPeriod;
                else if (t == ':' || t == ')' || t == ']' || t == '"' || t == '’' || t == '\'') wait += pauseAfterOther;
            }
            yield return new WaitForSeconds(wait);
        }
        co = null;
    }

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;

        // Billboard
        if (faceCamera)
        {
            Vector3 toCam = cam.transform.position - transform.position;
            if (yawOnly) toCam.y = 0f;
            if (toCam.sqrMagnitude > 1e-6f)
            {
                var target = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
                transform.rotation = (rotateLerp <= 0f)
                    ? target
                    : Quaternion.Slerp(transform.rotation, target, 1f - Mathf.Exp(-rotateLerp * Time.deltaTime));
            }
        }

        // Auto-scale (keeps similar on-screen size)
        if (autoScale)
        {
            float d = Mathf.Max(0.01f, Vector3.Distance(transform.position, cam.transform.position));
            float s = Mathf.Clamp(scaleAtRef * (d / referenceDistance), minScale, maxScale);
            transform.localScale = Vector3.one * s;
        }
    }

    // Hide/show without disabling this script
    void ShowLabel(bool visible)
    {
        // 3D TMP uses MeshRenderer
        if (labelRenderer) { labelRenderer.enabled = visible; return; }
        // UGUI TMP: enable/disable the graphic component
        if (label) label.enabled = visible;
    }

    char TailPunct(int wordIndex)
    {
        var ti = label.textInfo;
        if (wordIndex < 0 || wordIndex >= ti.wordCount) return '\0';
        var wi = ti.wordInfo[wordIndex];
        if (wi.characterCount <= 0) return '\0';
        string src = label.text;
        int last = Mathf.Min(src.Length - 1, wi.firstCharacterIndex + wi.characterCount - 1);
        char c = src[last];
        if (c == '>' && last > 0) c = src[last - 1];
        return c;
    }
}
