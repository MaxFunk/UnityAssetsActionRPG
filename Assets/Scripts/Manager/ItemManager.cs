using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemData;

public class ItemManager
{
    public struct Item
    {
        public int id;
        public int amount;
        public ItemData data;

        public Item(int id, int amount, ItemData data)
        {
            this.id = id;
            this.amount = amount;
            this.data = data;
        }

        public readonly bool IsUsable()
        {
            return data.type == ItemData.ItemType.Consumable && data.consumeEffect != null;
        }
    }

    [System.Serializable]
    public struct ItemSaveData
    {
        public int[] itemsMaterial;
        public int[] itemsConsumable;
        public int[] itemsIngredient;
        public int[] itemsGear;
        public int[] itemsKeyitem;
    }


    public ItemPreload itemPreload;
    public List<Item> itemsMaterial = new();
    public List<Item> itemsConsumable = new();
    public List<Item> itemsIngredient = new();
    public List<Item> itemsGear = new();
    public List<Item> itemsKeyitem = new();
    public List<Item> itemsFallback = new();

    public void LoadItems(ItemSaveData itemSaveData)
    {
        itemPreload = ScriptableManager.instance.itemPreload;
        itemsMaterial = CreateItemList(ref itemPreload.itemsMaterial, itemSaveData.itemsMaterial);
        itemsConsumable = CreateItemList(ref itemPreload.itemsConsumable, itemSaveData.itemsConsumable);
        itemsIngredient = CreateItemList(ref itemPreload.itemsIngredient, itemSaveData.itemsIngredient);
        itemsGear = CreateItemList(ref itemPreload.itemsGear, itemSaveData.itemsGear);
        itemsKeyitem = CreateItemList(ref itemPreload.itemsKeyitem, itemSaveData.itemsKeyitem);
    }

    public void RecieveItems(ItemRecieveData recieveData)
    {
        if (recieveData == null || recieveData.id < 0)
            return;

        var itemList = GetItemList(recieveData.type);
        if (itemList == itemsFallback || recieveData.id >= itemList.Count)
            return;

        var item = itemList[recieveData.id];
        item.amount = Mathf.Clamp(item.amount + recieveData.recieveAmount, 0, 999);
        itemList[recieveData.id] = item;

        GameManager.Instance.MissionManager.ItemCollected(recieveData.GetFullId(), recieveData.recieveAmount);
        UserInterfaceManager.instance.GameplayUI.OnItemRecieve(item, recieveData.recieveAmount);
    }

    public int ChangeItem(ItemType itemType, int id, int changeValue)
    {
        if (id < 0)
            return -1;

        var itemList = GetItemList(itemType);
        if (itemList == itemsFallback || id >= itemList.Count)
            return -1;

        var item = itemList[id];
        item.amount = Mathf.Clamp(item.amount + changeValue, 0, 999);
        itemList[id] = item;
        return item.amount;
    }

    public int FindAndChangeItem(ItemData itemData, int changeValue)
    {
        var id = ScriptableManager.instance.itemPreload.FindItemId(itemData);
        var res = ChangeItem(itemData.type, id, changeValue);

        // very bad, but works right now (..)
        var recieveData = new ItemRecieveData()
        {
            id = id,
            type = itemData.type
        };
        GameManager.Instance.MissionManager.ItemCollected(recieveData.GetFullId(), changeValue);

        return res;
    }

    public int GetItemAmount(ItemType itemType, int itemId)
    {
        if (itemId < 0)
            return 0;

        var itemList = GetItemList(itemType);
        if (itemList == itemsFallback || itemId >= itemList.Count)
            return 0;

        return itemList[itemId].amount;
    }

    public int FindAndGetItemAmount(ItemData itemData)
    {
        var id = ScriptableManager.instance.itemPreload.FindItemId(itemData);

        if (id < 0)
            return 0;

        var itemList = GetItemList(itemData.type);
        if (itemList == itemsFallback || id >= itemList.Count)
            return 0;

        return itemList[id].amount;
    }

    public int GetItemAmountFullId(int itemId)
    {
        return itemId switch
        {
            (>= 0) and (< 100) => GetItemFromList(ref itemsMaterial, itemId),
            (>= 100) and (< 200) => GetItemFromList(ref itemsConsumable, itemId),
            (>= 200) and (< 300) => GetItemFromList(ref itemsIngredient, itemId),
            (>= 300) and (< 400) => GetItemFromList(ref itemsGear, itemId),
            (>= 400) and (< 500) => GetItemFromList(ref itemsKeyitem, itemId),
            (< 0) or (>= 500) => -1,
        };
    }

    private int GetItemFromList(ref List<Item> itemList, int itemId)
    {
        if (itemId < 0 || itemId >= itemList.Count) return -1;
        return itemList[itemId].amount;
    }


    public ItemSaveData CreateSaveData()
    {
        var itemSaveData = new ItemSaveData
        {
            itemsMaterial = ArrayFromItemList(ref itemsMaterial),
            itemsConsumable = ArrayFromItemList(ref itemsConsumable),
            itemsIngredient = ArrayFromItemList(ref itemsIngredient),
            itemsGear = ArrayFromItemList(ref itemsGear),
            itemsKeyitem = ArrayFromItemList(ref itemsKeyitem)
        };
        return itemSaveData;
    }

    private int[] ArrayFromItemList(ref List<Item> itemList)
    {
        int[] itemAmountArray = new int[itemList.Count];
        for (int i = 0; i < itemList.Count; i++)
        {
            itemAmountArray[i] = itemList[i].amount;
        }

        return itemAmountArray;
    }

    private List<Item> CreateItemList(ref ItemData[] itemDataList, int[] itemAmountList)
    {
        var items = new List<Item>();
        for (int i = 0; i < itemDataList.Length; i++)
        {
            int amount = (i < itemAmountList.Length) ? itemAmountList[i] : 0;
            items.Add(new Item(i, amount, itemDataList[i]));
        }
        return items;
    }

    public ref List<Item> GetItemList(ItemType listType)
    {
        switch (listType)
        {
            case ItemType.Material:
                return ref itemsMaterial;
            case ItemType.Consumable:
                return ref itemsConsumable;
            case ItemType.Ingredient:
                return ref itemsIngredient;
            case ItemType.Gear:
                return ref itemsGear;
            case ItemType.Keyitem:
                return ref itemsKeyitem;
            default:
                break;
        }
        return ref itemsFallback;
    }


    public List<Item> GetFilteredGearList(int indexGearType) // index 0, 1, 2, else -> all
    {
        var filteredList = itemsGear.Where(item => item.data.gearData.CheckSameGearType(indexGearType) && item.amount > 0).ToList();
        return filteredList;
    }
}
