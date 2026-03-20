using System;
using System.Linq;
using UnityEngine;
using static ItemData;

[CreateAssetMenu(fileName = "ItemPreload", menuName = "Scriptable Objects/ItemPreload", order = 3)]
public class ItemPreload : ScriptableObject
{
    public ItemData[] itemsMaterial = new ItemData[] { };
    public ItemData[] itemsConsumable = new ItemData[] { };
    public ItemData[] itemsIngredient = new ItemData[] { };
    public ItemData[] itemsGear = new ItemData[] { };
    public ItemData[] itemsKeyitem = new ItemData[] { };


    public ItemData GetGear(int index)
    {
        if (index < 0 || index >= itemsGear.Length)
            return null;
        return itemsGear[index];
    }

    public ItemData GetItem(ItemType itemType, int index)
    {
        switch (itemType)
        {
            case ItemType.Material:
                if (index >= 0 && index < itemsMaterial.Length)
                    return itemsMaterial[index];
                break;
            case ItemType.Consumable:
                if (index >= 0 && index < itemsConsumable.Length)
                    return itemsConsumable[index];
                break;
            case ItemType.Ingredient:
                if (index >= 0 && index < itemsIngredient.Length)
                    return itemsIngredient[index];
                break;
            case ItemType.Gear:
                if (index >= 0 && index < itemsGear.Length)
                    return itemsGear[index];
                break;
            case ItemType.Keyitem:
                if (index >= 0 && index < itemsKeyitem.Length)
                    return itemsKeyitem[index];
                break;
            default:
                break;
        }

        return null;
    }

    public int FindItemId(ItemData itemData)
    {
        switch (itemData.type)
        {
            case ItemType.Material:
                return Array.IndexOf(itemsMaterial, itemData);
            case ItemType.Consumable:
                return Array.IndexOf(itemsConsumable, itemData);
            case ItemType.Ingredient:
                return Array.IndexOf(itemsIngredient, itemData);
            case ItemType.Gear:
                return Array.IndexOf(itemsGear, itemData);
            case ItemType.Keyitem:
                return Array.IndexOf(itemsKeyitem, itemData);
            default:
                break;
        }

        return -1;
    }
}
