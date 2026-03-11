using UnityEngine;

[CreateAssetMenu(fileName = "SoundtrackFile", menuName = "Scriptable Objects/SoundtrackFile")]
public class SoundtrackFile : ScriptableObject
{
    public enum SoundtrackLayer
    {
        Area = 0,
        SubArea = 1,
        Battle = 2,
        Other = 3,
    }

    public string trackName = string.Empty;
    public AudioClip intro;
    public AudioClip main;
    public bool withoutIntro = false;
    public bool loopMain = true;
    public SoundtrackLayer layer;
}
