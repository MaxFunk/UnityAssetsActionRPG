using UnityEngine;
using UnityEngine.UIElements;

public class MenuContainerItems : MenuContainer
{
    readonly string containerName = "ItemsView";
    int index = 0;
    ItemData.ItemType menuMode = ItemData.ItemType.Material;

    private VisualElement detailContainer;
    private ItemManager itemManager;
    private VisualElement[] listElements;
    private Button buttonUseItem;

    public override void PrepareView(VisualElement rootElement)
    {
        containerObj = rootElement != null ? rootElement.Q<VisualElement>(containerName) : containerObj;
        if (containerObj == null) { return; }

        detailContainer = containerObj.Q<VisualElement>("ItemDetail");
        buttonUseItem = detailContainer.Q<Button>("ButtonUseItem");
        buttonUseItem.RegisterCallback<ClickEvent>(OnButtonUseClick);

        ChangeMenuMode(menuMode, true);

        // Header Labels configuration
        var headerContainer = containerObj.Q<VisualElement>("Header");
        var headerLabels = headerContainer.Query<Label>().ToList();
        for (int i = 0; i < headerLabels.Count; i++) // Count should be 5, since enum has 5 entries
        {
            headerLabels[i].RegisterCallback<ClickEvent, ItemData.ItemType>(OnHeaderLabelClick, (ItemData.ItemType) i);
        }

        UpdateSelection();
    }

    public override void ConfirmEvent()
    {

    }

    public override void CancelEvent()
    {
        mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Main);
    }

    public override void SpecialEvent()
    {

    }

    public override void DirectionalEvent(Vector2 navInput)
    {
        if (Mathf.Abs(navInput.y) > 0.5)
        {
            int inputUpDown = Mathf.RoundToInt(navInput.y);
            var itemList = itemManager.GetItemList(menuMode);
            index = Mathf.Clamp(index - inputUpDown, 0, itemList.Count - 1);
            UpdateSelection();
        }
    }


    private void ChangeMenuMode(ItemData.ItemType newTypeMode, bool initialLoad = false)
    {
        if (menuMode == newTypeMode && !initialLoad)
            return;

        itemManager = GameManager.Instance.ItemManager;
        var itemList = itemManager.GetItemList(newTypeMode);
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

        index = 0;
        menuMode = newTypeMode;
        buttonUseItem.style.display = menuMode == ItemData.ItemType.Consumable ? DisplayStyle.Flex : DisplayStyle.None;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        var itemList = itemManager.GetItemList(menuMode);
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

    private void OnButtonUseClick(ClickEvent evt)
    {
        var itemList = itemManager.GetItemList(menuMode);
        var item = itemList[index];
        if (item.data.consumeEffect == null || item.amount <= 0) { return; }

        Debug.Log("TODO: Selection for Character to use Item on. Using index 0 instead.");
        var chd = GameManager.Instance.characterDatas[0];

        item.data.consumeEffect.ApplyEffect(chd);
        item.amount -= 1;
        itemList[index] = item;
        UpdateSelection();
    }
}
