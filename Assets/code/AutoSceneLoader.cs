using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimer : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds to wait in THIS scene before moving on.")]
    public float delaySeconds = 5f;

    [Header("Routing")]
    [Tooltip("Name of the NEXT scene to load. Leave empty on the final scene.")]
    public string nextSceneName = "";

    [Header("Behavior")]
    [Tooltip("If OFF, the scene will not auto-advance (useful for testing).")]
    public bool autoAdvance = true;

    [Tooltip("If ON, the countdown only starts after you call Begin(). " +
             "Leave OFF to start automatically when the scene loads.")]
    public bool waitForExternalBegin = false;

    private bool _begun;

    void Start()
    {
        if (!waitForExternalBegin) Begin();
    }

    /// <summary>
    /// Call this from your image-tracking event if you want the timer to start
    /// only after the target is found. Safe to call once.
    /// </summary>
    public void Begin()
    {
        if (_begun) return;
        _begun = true;
        if (autoAdvance && !string.IsNullOrEmpty(nextSceneName))
            StartCoroutine(RunTimer());
    }

    private IEnumerator RunTimer()
    {
        var wait = Mathf.Max(0f, delaySeconds);
        if (wait > 0f) yield return new WaitForSeconds(wait);
        // Avoid reloading same scene by mistake
        if (SceneManager.GetActiveScene().name != nextSceneName)
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
