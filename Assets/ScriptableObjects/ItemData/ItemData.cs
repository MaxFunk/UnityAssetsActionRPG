using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData", order = 2)]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Material,
        Consumable,
        Ingredient,
        Gear,
        Keyitem
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare
    }

    public string itemName = string.Empty;
    public string description = string.Empty;
    public string shortDescription = string.Empty;
    public int sellValue = 0;
    public int buyValue = 0;
    public ItemType type = ItemType.Material;
    public ItemRarity rarity = ItemRarity.Common;
    public Texture2D icon = null;
    public ItemConsumeEffect consumeEffect = null;
    public ItemGearData gearData = null;
}
