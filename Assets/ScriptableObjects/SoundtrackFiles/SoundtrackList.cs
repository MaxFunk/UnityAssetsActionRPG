using UnityEngine;

[CreateAssetMenu(fileName = "SoundtrackList", menuName = "Scriptable Objects/SoundtrackList")]
public class SoundtrackList : ScriptableObject
{
    public SoundtrackFile[] soundtracks = new SoundtrackFile[0];

    public SoundtrackFile GetSoundtrackFile(int index)
    {
        if (index < 0 || index >= soundtracks.Length)
            return null;

        return soundtracks[index];
    }
}
