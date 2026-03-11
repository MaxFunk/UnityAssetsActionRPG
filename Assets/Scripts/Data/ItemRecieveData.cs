using UnityEngine;

[System.Serializable]
public class ItemRecieveData
{
    public ItemData.ItemType type = ItemData.ItemType.Material;
    public int id = -1;
    public int recieveAmount = 0;
    public float recieveChance = 1;

    public int GetFullId()
    {
        return type switch
        {
            ItemData.ItemType.Material => id,
            ItemData.ItemType.Consumable => id + 100,
            ItemData.ItemType.Ingredient => id + 200,
            ItemData.ItemType.Gear => id + 300,
            ItemData.ItemType.Keyitem => id + 400,
            _ => id,
        };
    }
}
