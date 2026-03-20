using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Scriptable Objects/ShopData")]
public class ShopData : ScriptableObject
{
    public string ShopName = "Shop";
    public ItemData.ItemType shopType = ItemData.ItemType.Material;
    public int[] itemsBuyable = new int[0];
}
