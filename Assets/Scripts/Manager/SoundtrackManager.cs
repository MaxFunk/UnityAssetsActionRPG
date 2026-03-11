using UnityEngine;
using System.Collections;

public class SoundtrackManager : MonoBehaviour
{
    public static SoundtrackManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    private AudioSource activeSource;
    private AudioSource idleSource;
    private Coroutine musicRoutine;

    private SoundtrackFile areaSoundtrack = null;
    private bool areaTrackPlaying = false;
    private float areaTrackTime = 0f;
    private readonly float fadeTime = 1f;


    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this);

        activeSource = sourceA;
        idleSource = sourceB;
    }

    public void PlaySoundtrack(SoundtrackFile soundtrackFile)
    {
        if (soundtrackFile == null)
            return;

        if (soundtrackFile.layer == SoundtrackFile.SoundtrackLayer.Area)
        {
            areaSoundtrack = soundtrackFile;
            areaTrackTime = 0f;
        }

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(PlayTrackRoutine(soundtrackFile, fadeTime));
    }

    public void ResumeAreaSoundtrack()
    {
        if (areaSoundtrack == null)
            return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(PlayTrackRoutine(areaSoundtrack, fadeTime, true));
    }

    public void StopSoundtrack()
    {
        StartCoroutine(FadeOut(activeSource, fadeTime));
    }


    private IEnumerator PlayTrackRoutine(SoundtrackFile soundtrack, float fadeTime, bool resumeAreaTrack = false)
    {
        areaTrackPlaying = soundtrack.layer == SoundtrackFile.SoundtrackLayer.Area;

        if (soundtrack.withoutIntro || soundtrack.intro == null || resumeAreaTrack)
        {
            yield return Crossfade(soundtrack.main, soundtrack.loopMain, fadeTime, areaTrackTime);
        }
        else
        {
            yield return Crossfade(soundtrack.intro, false, fadeTime, 0f);
            yield return new WaitForSeconds(soundtrack.intro.length - fadeTime);
            SwitchActiveSoundtrack(soundtrack.main, soundtrack.loopMain);
        }
    }

    private void SwitchActiveSoundtrack(AudioClip clip, bool loop)
    {
        activeSource.clip = clip;
        activeSource.loop = loop;
        activeSource.Play();
    }

    IEnumerator Crossfade(AudioClip clip, bool loop, float fadeTime, float startTime)
    {
        idleSource.clip = clip;
        idleSource.loop = loop;
        idleSource.time = startTime;
        idleSource.volume = 0f;
        idleSource.Play();

        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float v = t / fadeTime;

            idleSource.volume = v;
            activeSource.volume = 1f - v;

            yield return null;
        }

        if (!areaTrackPlaying)
            areaTrackTime = activeSource.time;
        activeSource.Stop();

        (activeSource, idleSource) = (idleSource, activeSource);
    }

    IEnumerator FadeOut(AudioSource src, float fadeTime)
    {
        float startVolume = src.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        src.Stop();
    }
}
