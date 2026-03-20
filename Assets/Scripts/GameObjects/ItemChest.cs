using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class ItemChest : MonoBehaviour
{
    public Vector3 relativeItemDropPosition = Vector3.up;
    public ItemRecieveData[] itemDrops;
    public int chestId = -1;


    void Awake()
    {
        if (GameManager.Instance.GameDataGeneral.HasChestId(chestId)) 
            Destroy(gameObject);
    }

    public void DropLoot()
    {
        foreach (var itemDrop in itemDrops) 
        {
            GameManager.Instance.SpawnItemDrop(itemDrop, transform.position + relativeItemDropPosition, true);
        }
        GameManager.Instance.GameDataGeneral.AddChestId(chestId);
    }
}
