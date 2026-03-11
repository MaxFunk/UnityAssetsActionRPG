using UnityEngine;

public class SoundtrackAutoPlayer : MonoBehaviour
{
    public SoundtrackFile SoundtrackFile;

    void Awake()
    {
        SoundtrackManager.Instance.PlaySoundtrack(SoundtrackFile);
        Destroy(gameObject);
    }
}
