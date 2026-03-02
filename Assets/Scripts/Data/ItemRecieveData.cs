using UnityEngine;

[System.Serializable]
public class ItemRecieveData
{
    public ItemData.ItemType type = ItemData.ItemType.Material;
    public int id = -1;
    public int recieveAmount = 0;
    public float recieveChance = 1;
}
