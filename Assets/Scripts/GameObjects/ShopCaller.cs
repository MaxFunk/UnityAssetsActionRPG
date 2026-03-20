using UnityEngine;

public class ShopCaller : MonoBehaviour
{
    public ShopData shopData = null;

    public void LoadShopUI()
    {
        if (shopData == null) return;

        var shopUI = UserInterfaceManager.instance.CreateShopUI();
        shopUI.InitializeShop(shopData);
    }
}
