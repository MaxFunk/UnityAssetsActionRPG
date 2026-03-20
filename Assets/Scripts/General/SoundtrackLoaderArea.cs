using UnityEngine;

public class SoundtrackLoaderArea : MonoBehaviour
{
    public int soundtrackAreaIndex;

    void Awake()
    {
        SoundtrackManager.Instance.LoadAreaSoundtrack(soundtrackAreaIndex);
        Destroy(gameObject);
    }
}
