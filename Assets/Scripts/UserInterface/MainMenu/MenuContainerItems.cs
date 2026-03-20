using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MenuContainerItems : MenuContainer
{
    readonly string containerName = "ItemsView";
    int index = 0;
    ItemData.ItemType menuMode = ItemData.ItemType.Material;
    List<ItemManager.Item> itemList = new();

    private VisualElement detailContainer;
    private ItemManager itemManager;
    private VisualElement[] listElements;
    private Button buttonUseItem;
    private List<Label> headerLabels = new();

    public override void PrepareView(VisualElement rootElement)
    {
        containerObj = rootElement != null ? rootElement.Q<VisualElement>(containerName) : containerObj;
        if (containerObj == null) { return; }

        detailContainer = containerObj.Q<VisualElement>("ItemDetail");
        buttonUseItem = detailContainer.Q<Button>("ButtonUseItem");
        buttonUseItem.RegisterCallback<ClickEvent>(OnButtonUseClick);

        containerObj.Q<Label>("LabelBack").RegisterCallback<ClickEvent>(OnLabelBackClick);

        // Header Labels configuration
        var headerContainer = containerObj.Q<VisualElement>("Header");
        headerLabels = headerContainer.Query<Label>().ToList();
        for (int i = 0; i < headerLabels.Count; i++) // Count should be 5, since enum has 5 entries
        {
            headerLabels[i].RegisterCallback<ClickEvent, ItemData.ItemType>(OnHeaderLabelClick, (ItemData.ItemType) i);
        }

        ChangeMenuMode(menuMode);
    }

    public override void ConfirmEvent()
    {
        if (menuMode == ItemData.ItemType.Consumable && itemList.Count > 0)
        {
            OnButtonUseClick(null); // not really good, but fine for now
        }
    }

    public override void CancelEvent()
    {
        mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Main);
    }

    public override void SpecialEvent()
    {
        ChangeMenuMode(GetNextMenuMode());
    }

    public override void DirectionalEvent(Vector2 navInput)
    {
        if (Mathf.Abs(navInput.y) > 0.5)
        {
            int inputUpDown = Mathf.RoundToInt(navInput.y);
            index = Mathf.Clamp(index - inputUpDown, 0, itemList.Count - 1);
            UpdateSelection();
        }
    }


    private void ChangeMenuMode(ItemData.ItemType newTypeMode, bool keepIndex = false)
    {
        itemManager = GameManager.Instance.ItemManager;
        itemList = itemManager.GetItemList(newTypeMode).Where(item => item.amount > 0).ToList();
        var scrollView = containerObj.Q<ScrollView>();
        scrollView.Clear();

        listElements = new VisualElement[itemList.Count];
        for (int i = 0; i < itemList.Count; i++)
        {
            VisualElement itemListElementTree = mainMenuEvents.ItemListElement.CloneTree();
            var itemListElement = itemListElementTree.Q<VisualElement>("ItemListElement");
            itemListElement.dataSource = itemList[i];
            itemListElement.RegisterCallback<ClickEvent, int>(OnListElementClick, i);
            listElements[i] = itemListElement;
            scrollView.Add(itemListElementTree);
        }

        foreach (var label in headerLabels)
            label.RemoveFromClassList("header-selected");
        headerLabels[(int)newTypeMode].AddToClassList("header-selected");

        index = keepIndex? index : 0;
        menuMode = newTypeMode;
        buttonUseItem.style.display = menuMode == ItemData.ItemType.Consumable ? DisplayStyle.Flex : DisplayStyle.None;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        detailContainer.style.visibility = itemList.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        if (itemList.Count <= 0) return;
        if (detailContainer != null)
            detailContainer.dataSource = itemList[index];

        foreach (var element in listElements)
            element.RemoveFromClassList("item-element-selected");
        listElements[index].AddToClassList("item-element-selected");
    }


    private void OnHeaderLabelClick(ClickEvent evt, ItemData.ItemType itemType)
    {
        ChangeMenuMode(itemType);
    }

    private void OnListElementClick(ClickEvent evt, int index)
    {
        this.index = index;
        UpdateSelection();
    }

    private void OnLabelBackClick(ClickEvent evt)
    {
        CancelEvent();
    }

    private void OnButtonUseClick(ClickEvent evt)
    {
        var item = itemList[index];
        if (item.data.consumeEffect == null || item.amount <= 0) { return; }

        //Debug.Log("TODO: Selection for Character to use Item on. Using on everyone instead.");
        for (int i = 0; i < GameManager.Instance.characterDatas.Count; i++)
        {
            var chd = GameManager.Instance.characterDatas[i];
            item.data.consumeEffect.ApplyEffect(chd);
        }        
        item.amount -= 1;
        GameManager.Instance.ItemManager.ChangeItem(item.data.type, item.id, -1);
        ChangeMenuMode(menuMode, item.amount > 0);
    }

    private ItemData.ItemType GetNextMenuMode()
    {
        return menuMode switch
        {
            ItemData.ItemType.Material => ItemData.ItemType.Consumable,
            ItemData.ItemType.Consumable => ItemData.ItemType.Ingredient,
            ItemData.ItemType.Ingredient => ItemData.ItemType.Gear,
            ItemData.ItemType.Gear => ItemData.ItemType.Keyitem,
            ItemData.ItemType.Keyitem => ItemData.ItemType.Material,
            _ => ItemData.ItemType.Material,
        };
    }
}
