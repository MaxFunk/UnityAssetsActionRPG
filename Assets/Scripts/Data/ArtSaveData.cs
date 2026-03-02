using UnityEngine;

[System.Serializable]
public class ArtSaveData
{
    public int artId = -1;
    public int artLevel = 0; // 0 - 4
    public int artSlot = -1; // 0 - 4: arts, 5: ult, -1: not used
}
