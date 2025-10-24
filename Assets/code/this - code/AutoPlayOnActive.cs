using UnityEngine;
using UnityEngine.Playables;

public class AutoPlayOnActive : MonoBehaviour
{
    [Header("What to control")]
    public bool controlAnimators = true;
    public bool controlAudio = true;
    public bool controlPlayableDirectors = true;

    [Header("On Disable")]
    public bool pauseInsteadOfStop = true;

    Animator[] _animators;
    AudioSource[] _audios;
    PlayableDirector[] _directors;

    void Awake()
    {
        if (controlAnimators) _animators = GetComponentsInChildren<Animator>(true);
        if (controlAudio) _audios = GetComponentsInChildren<AudioSource>(true);
        if (controlPlayableDirectors) _directors = GetComponentsInChildren<PlayableDirector>(true);
    }

    void OnEnable() { ResumeAll(); }
    void OnDisable() { if (pauseInsteadOfStop) PauseAll(); else StopAll(); }

    public void PauseAll()
    {
        if (controlAnimators && _animators != null)
            foreach (var a in _animators) if (a) a.speed = 0f;

        if (controlAudio && _audios != null)
            foreach (var au in _audios) if (au) au.Pause();

        if (controlPlayableDirectors && _directors != null)
            foreach (var d in _directors) if (d) d.Pause();
    }

    public void ResumeAll()
    {
        if (controlAnimators && _animators != null)
            foreach (var a in _animators) if (a) { a.enabled = true; a.speed = 1f; }

        if (controlAudio && _audios != null)
            foreach (var au in _audios) if (au) { if (!au.isPlaying) au.Play(); else au.UnPause(); }

        if (controlPlayableDirectors && _directors != null)
            foreach (var d in _directors) if (d) d.Play();
    }

    public void StopAll()
    {
        if (controlAnimators && _animators != null)
            foreach (var a in _animators) if (a) { a.Rebind(); a.Update(0f); a.enabled = false; }

        if (controlAudio && _audios != null)
            foreach (var au in _audios) if (au) au.Stop();

        if (controlPlayableDirectors && _directors != null)
            foreach (var d in _directors) if (d) d.Stop();
    }
}
