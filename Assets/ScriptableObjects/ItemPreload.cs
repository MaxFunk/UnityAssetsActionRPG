using UnityEngine;

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
}
