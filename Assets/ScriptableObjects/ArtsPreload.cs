using UnityEngine;

[CreateAssetMenu(fileName = "ArtsPreload", menuName = "Scriptable Objects/ArtsPreload", order = 3)]
public class ArtsPreload : ScriptableObject
{
    public ArtData[] artDatas = new ArtData[] { };
}
