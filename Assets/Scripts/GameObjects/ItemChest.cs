using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class ItemChest : MonoBehaviour
{
    public Vector3 relativeItemDropPosition = Vector3.up;
    public ItemRecieveData[] itemDrops;
    private Interactable interactable;


    void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    public void DropLoot()
    {
        foreach (var itemDrop in itemDrops) 
        {
            GameManager.Instance.SpawnItemDrop(itemDrop, transform.position + relativeItemDropPosition);
        }
    }
}
