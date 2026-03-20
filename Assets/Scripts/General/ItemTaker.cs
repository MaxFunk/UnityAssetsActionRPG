using UnityEngine;

public class ItemTaker : MonoBehaviour
{
    public ItemRecieveData[] itemDatas;

    public void TakeItemsAway()
    {
        foreach (var item in itemDatas)
        {
            GameManager.Instance.ItemManager.ChangeItem(item.type, item.id, item.recieveAmount);
        }
    }
}
