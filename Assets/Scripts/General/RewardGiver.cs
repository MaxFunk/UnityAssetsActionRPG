using UnityEngine;

public class RewardGiver : MonoBehaviour
{
    public ItemRecieveData[] items;
    public int money = 0;

    public void GiveItems()
    {
        var itemManager = GameManager.Instance.ItemManager;
        foreach (var item in items)
        {
            itemManager.RecieveItems(item);
        }
    }

    public void GiveMoney()
    {
        GameManager.Instance.GameDataGeneral.RecieveMoney(money);
    }

    public void GiveBoth()
    {
        GiveItems();
        GiveMoney();
    }
}
