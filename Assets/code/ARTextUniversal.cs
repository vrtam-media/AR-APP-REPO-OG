using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("AR/Text/Canvas Intro Image → Word Reveal (No Flicker)")]
public class CanvasIntroImageThenWords : MonoBehaviour
{
    [Header("Targets (same Canvas)")]
    [SerializeField] TMP_Text label;                  // TextMeshProUGUI (UI text on your world-space canvas)
    [Tooltip("Any UI Graphics (Image / RawImage / Text, etc.) that should show before the text starts.")]
    [SerializeField] Graphic[] introGraphics;

    [Header("Text Content")]
    [TextArea] public string message = "Hello from AR world.";
    public bool playOnEnable = true;

    [Header("Timing")]
    [Tooltip("Hide everything for this long, then show the image(s).")]
    [Min(0f)] public float startDelay = 0f;
    [Tooltip("Extra delay after the image is visible, before text begins revealing.")]
    [Min(0f)] public float beforeTextDelay = 0f;

    [Header("Image Appearance")]
    public bool fadeInImage = true;
    [Min(0f)] public float imageFadeDuration = 0.35f;

    [Header("Word-by-Word")]
    [Min(0.05f)] public float wordsPerSecond = 3f;
    public bool punctuationPauses = true;
    [Min(0f)] public float pauseAfterComma = 0.12f; // , ;
    [Min(0f)] public float pauseAfterPeriod = 0.22f; // . ! ?
    [Min(0f)] public float pauseAfterOther = 0.08f; // : ) ] " ’ '

    [Header("Time Source")]
    public bool useUnscaledTime = false;             // true = ignores Time.timeScale

    Coroutine co;

    void Reset()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
        if (introGraphics == null || introGraphics.Length == 0)
            introGraphics = GetComponentsInChildren<Graphic>(true);
    }

    void OnEnable()
    {
        if (playOnEnable) Play();
    }

    [ContextMenu("Play")]
    public void Play()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoRun());
    }

    [ContextMenu("Skip To End")]
    public void SkipToEnd()
    {
        if (co != null) StopCoroutine(co);

        // Image(s) fully visible
        SetGraphicsVisible(true, 1f);

        // Text fully visible and fully revealed
        if (label)
        {
            label.text = message;
            label.maxVisibleCharacters = int.MaxValue;
            label.maxVisibleWords = int.MaxValue;
            label.ForceMeshUpdate(true, true);
            SetLabelAlpha(1f);
        }
        co = null;
    }

    IEnumerator CoRun()
    {
        // --- Prep: hide everything with alpha (avoid TMP one-frame flash) ---
        SetGraphicsVisible(false, 0f);

        if (label)
        {
            label.text = message;

            // Clamp to 0 words BEFORE any visible frame
            label.maxVisibleCharacters = int.MaxValue;
            label.maxVisibleWords = 0;

            // Build TMP geometry
            yield return null;
            label.ForceMeshUpdate(true, true);

            // Keep it enabled but invisible
            SetLabelAlpha(0f);
        }

        // --- Initial delay (everything hidden) ---
        if (startDelay > 0f) yield return Wait(startDelay);

        // --- Show image(s) ---
        if (fadeInImage && imageFadeDuration > 0f)
            yield return FadeGraphics(0f, 1f, imageFadeDuration);
        else
            SetGraphicsVisible(true, 1f);

        // --- Optional gap before text starts ---
        if (beforeTextDelay > 0f) yield return Wait(beforeTextDelay);

        // --- Word-by-word reveal ---
        if (label)
        {
            // Make label visible now; still clamped to 0 words
            SetLabelAlpha(1f);

            int total = Mathf.Max(0, label.textInfo.wordCount);
            if (total > 0)
            {
                float baseStep = 1f / Mathf.Max(0.05f, wordsPerSecond);
                for (int i = 1; i <= total; i++)
                {
                    label.maxVisibleWords = i;

                    float wait = baseStep;
                    if (punctuationPauses)
                    {
                        char t = TailPunct(i - 1);
                        if (t == ',' || t == ';') wait += pauseAfterComma;
                        else if (t == '.' || t == '!' || t == '?') wait += pauseAfterPeriod;
                        else if (t == ':' || t == ')' || t == ']' || t == '"' || t == '’' || t == '\'')
                            wait += pauseAfterOther;
                    }
                    yield return Wait(wait);
                }
            }
        }

        co = null;
    }

    // ---------- helpers ----------
    void SetGraphicsVisible(bool visible, float alphaIfVisible)
    {
        if (introGraphics == null) return;
        foreach (var g in introGraphics)
        {
            if (!g) continue;
            var c = g.color;
            c.a = visible ? alphaIfVisible : 0f;
            g.color = c;
            g.enabled = true; // keep enabled so fades/alpha work; alpha handles hiding
        }
    }

    IEnumerator FadeGraphics(float from, float to, float duration)
    {
        if (introGraphics == null || introGraphics.Length == 0 || duration <= 0f)
        {
            SetGraphicsVisible(true, to);
            yield break;
        }

        foreach (var g in introGraphics) if (g) g.enabled = true;

        float t0 = useUnscaledTime ? Time.unscaledTime : Time.time;
        float t;
        while (true)
        {
            t = ((useUnscaledTime ? Time.unscaledTime : Time.time) - t0) / duration;
            if (t >= 1f) break;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t));
            foreach (var g in introGraphics)
            {
                if (!g) continue;
                var c = g.color; c.a = a; g.color = c;
            }
            yield return null;
        }
        foreach (var g in introGraphics)
        {
            if (!g) continue;
            var c = g.color; c.a = to; g.color = c;
        }
    }

    IEnumerator Wait(float seconds)
    {
        if (!useUnscaledTime) { yield return new WaitForSeconds(seconds); yield break; }
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    char TailPunct(int wordIndex)
    {
        if (!label) return '\0';
        var ti = label.textInfo;
        if (wordIndex < 0 || wordIndex >= ti.wordCount) return '\0';
        var wi = ti.wordInfo[wordIndex];
        if (wi.characterCount <= 0) return '\0';
        string src = label.text;
        int last = Mathf.Min(src.Length - 1, wi.firstCharacterIndex + wi.characterCount - 1);
        char c = src[last];
        if (c == '>' && last > 0) c = src[last - 1]; // handles rich-text closing tag
        return c;
    }

    // UGUI TMP alpha (primary path). Also supports 3D TMP if ever needed.
    void SetLabelAlpha(float a)
    {
        if (!label) return;

        // UGUI (TextMeshProUGUI)
        if (label is TextMeshProUGUI ui)
        {
            var c = ui.color; c.a = a; ui.color = c;
            var cr = ui.canvasRenderer; if (cr) cr.SetAlpha(a);
            return;
        }

        // 3D TMP fallback
        var rend = label.GetComponent<Renderer>();
        if (rend && rend.material && rend.material.HasProperty("_FaceColor"))
        {
            var c = rend.material.GetColor("_FaceColor");
            c.a = a;
            rend.material.SetColor("_FaceColor", c);
        }
    }
}
