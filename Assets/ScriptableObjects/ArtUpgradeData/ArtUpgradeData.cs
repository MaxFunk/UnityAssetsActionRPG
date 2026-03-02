using UnityEngine;

[CreateAssetMenu(fileName = "ArtUpgradeData", menuName = "Scriptable Objects/ArtUpgradeData")]
public class ArtUpgradeData : ScriptableObject
{
    public int upgradeToId = -1;
    public int itemId = -1;
    public int itemCost = 0;
}
