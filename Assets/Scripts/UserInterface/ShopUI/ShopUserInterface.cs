using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class ShopUserInterface : MonoBehaviour
{
    private enum ShopMode
    {
        None,
        Buy,
        Sell
    }

    public ShopData shopData = null;
    public DisplayStyle visibilityFront = DisplayStyle.Flex;
    public DisplayStyle visibilityMain = DisplayStyle.None;
    public string headerTextMode = string.Empty;
    public string textMoney = "0 M";

    private UIDocument document;
    private VisualElement rootElement;
    private InputHandler inputHandler;

    private List<Label> labelsFront = new();
    private List<VisualElement> containersList = new();

    private ShopMode shopMode = ShopMode.None;
    private float navigateInputCooldown = 0;
    private int indexFront = 0;
    private int indexList = 0;

    private void Update()
    {
        if (navigateInputCooldown > 0)
            navigateInputCooldown -= Time.deltaTime;

        var navigateInput = inputHandler.GetNavigateInput();
        if (navigateInput.magnitude > 0.2 && navigateInputCooldown <= 0)
        {
            DirectionalInput(navigateInput);
            navigateInputCooldown = 0.2f;
        }
        if (navigateInputCooldown > 0 && navigateInput.magnitude < 0.01)
            navigateInputCooldown = 0.0f;

        if (inputHandler.GetUICancelInputDown())
            CancelInput();

        if (inputHandler.GetUIConfirmInputDown())
            ConfirmInput();
    }

    public void InitializeShop(ShopData data)
    {
        var heroChars = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
        foreach (var character in heroChars)
        {
            if (character != null)
                character.OnDialogStart();
        }

        shopData = data;
        document = GetComponent<UIDocument>();
        rootElement = document.rootVisualElement.Q<VisualElement>("ContainerRoot");
        inputHandler = InputHandler.instance;

        if (rootElement == null)
        {
            EndShop();
            return;
        }

        indexFront = 0;
        rootElement.dataSource = this;
        AddGeneralBindings();
        OpenFrontPage();

        inputHandler.SetCursorConfined();
        inputHandler.inputMenuOnly = true;
    }

    public void EndShop()
    {
        inputHandler.SetCursorLocked();
        inputHandler.inputMenuOnly = false;

        var heroChars = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
        foreach (var character in heroChars)
        {
            if (character != null)
                character.OnDialogEnd();
        }

        UserInterfaceManager.instance.DestroyShopUI();
    }

    private void DirectionalInput(Vector3 input)
    {
        if (shopMode == ShopMode.None)
        {
            indexFront = Mathf.Clamp(indexFront - Mathf.RoundToInt(input.y), 0, labelsFront.Count - 1);
            UpdateSelectionFront();
        }
        else
        {
            indexList = Mathf.Clamp(indexList - Mathf.RoundToInt(input.y), 0, containersList.Count - 1);
            UpdateSelectionList();
        }
    }

    private void ConfirmInput()
    {
        if (shopMode == ShopMode.None)
        {
            OnLabelFrontClick(null, indexFront);
        }
        else
        {
            if (containersList.Count > 0)
            {
                ClickEvent evt = ClickEvent.GetPooled();
                evt.target = containersList[indexList];
                containersList[indexList].SendEvent(evt);
            }
        }
    }

    private void CancelInput()
    {
        if (shopMode == ShopMode.None)
        {
            EndShop();
        }
        else
        {
            OpenFrontPage();
        }
    }

    private void AddGeneralBindings()
    {
        labelsFront = rootElement.Query<Label>(className: "label-front").ToList();
        for (int i = 0; i < labelsFront.Count; i++)
        {
            labelsFront[i].RegisterCallback<ClickEvent, int>(OnLabelFrontClick, i);
            labelsFront[i].RegisterCallback<MouseOverEvent, int>(OnLabelFrontMouse, i);
        }

        var labelBack = rootElement.Q<Label>(className: "label-back");
        labelBack.RegisterCallback<ClickEvent>(OnLabelBackClick);
    }

    private void OpenMainPage(bool reloadIndex = true)
    {
        visibilityFront = DisplayStyle.None;
        visibilityMain = DisplayStyle.Flex;

        headerTextMode = shopMode switch
        {
            ShopMode.Buy => "Buy Items",
            ShopMode.Sell => "Sell Items",
            _ => string.Empty,
        };

        var scrollView = rootElement.Q<ScrollView>(className: "list-scroll-view");
        scrollView.Clear();
        containersList.Clear();

        if (shopMode == ShopMode.Buy)
            FillListBuy(scrollView);

        if (shopMode == ShopMode.Sell)
            FillListSell(scrollView);

        WriteOwnedMoney();

        if (reloadIndex)
            indexList = Mathf.Clamp(indexList, 0, containersList.Count - 1);
        UpdateSelectionList();
    }

    private void OpenFrontPage()
    {
        shopMode = ShopMode.None;
        visibilityFront = DisplayStyle.Flex;
        visibilityMain = DisplayStyle.None;
        UpdateSelectionFront();
    }

    private void CreateListElement(ScrollView scrollView, ItemData itemData, bool buyMode, int amount)
    {
        VisualElement newContainer = new();
        newContainer.AddToClassList("list-elem-container");

        Image newIcon = new()
        {
            image = itemData.icon
        };
        newIcon.AddToClassList("list-elem-icon");

        Label newLabelName = new()
        {
            text = itemData.itemName
        };
        newLabelName.AddToClassList("list-elem-itemname");

        var itemValue = buyMode ? itemData.buyValue : itemData.sellValue;
        Label newLabelMoney = new()
        {
            text = $"{itemValue} M"
        };
        newLabelMoney.AddToClassList("list-elem-label");

        Label newLabelAmount = new()
        {
            text = $"x {amount}"
        };
        newLabelAmount.AddToClassList("list-elem-label");

        newContainer.RegisterCallback<ClickEvent, ItemData>(OnListElementClick, itemData);
        newContainer.RegisterCallback<MouseOverEvent, int>(OnListElemMouse, containersList.Count);

        newContainer.Add(newIcon);
        newContainer.Add(newLabelName);
        newContainer.Add(newLabelMoney);
        newContainer.Add(newLabelAmount);
        scrollView.Add(newContainer);
        containersList.Add(newContainer);
    }

    private void FillListBuy(ScrollView scrollView)
    {
        var itemPreload = ScriptableManager.instance.itemPreload; // GetItem

        foreach (var item in shopData.itemsBuyable)
        {
            var itemData = itemPreload.GetItem(shopData.shopType, item);
            if (itemData != null)
            {
                var amount = GameManager.Instance.ItemManager.FindAndGetItemAmount(itemData);
                CreateListElement(scrollView, itemData, true, amount);
            }
        }
    }

    private void FillListSell(ScrollView scrollView)
    {
        var items = GameManager.Instance.ItemManager.GetItemList(shopData.shopType);
        var filteredItems = items.Where(item => item.amount > 0).ToList();

        foreach (var item in filteredItems)
        {
            CreateListElement(scrollView, item.data, false, item.amount);
        }
    }

    private void WriteOwnedMoney()
    {
        textMoney = $"{GameManager.Instance.GameDataGeneral.money} M";
    }

    private void UpdateSelectionFront()
    {
        foreach (var label in labelsFront)
            label.RemoveFromClassList("label-front-selected");

        labelsFront[indexFront].AddToClassList("label-front-selected");
    }

    private void UpdateSelectionList()
    {
        foreach (var container in containersList)
            container.RemoveFromClassList("list-elem-container-selected");

        if (containersList.Count > 0)
            containersList[indexList].AddToClassList("list-elem-container-selected");
    }


    private void OnLabelFrontClick(ClickEvent evt, int index)
    {
        switch (index)
        {
            case 0:
                shopMode = ShopMode.Buy;
                OpenMainPage();
                break;
            case 1:
                shopMode = ShopMode.Sell;
                OpenMainPage();
                break;
            case 2:
                EndShop();
                break;
            default:
                break;
        }
    }

    private void OnLabelFrontMouse(MouseOverEvent evt, int index)
    {
        indexFront = index;
        UpdateSelectionFront();
    }

    private void OnListElemMouse(MouseOverEvent evt, int index)
    {
        indexList = index;
        UpdateSelectionList();
    }

    private void OnLabelBackClick(ClickEvent evt)
    {
        OpenFrontPage();
    }

    private void OnListElementClick(ClickEvent evt, ItemData itemData)
    {
        var gameManager = GameManager.Instance;

        if (shopMode == ShopMode.Buy && gameManager.GameDataGeneral.money >= itemData.buyValue)
        {
            gameManager.ItemManager.FindAndChangeItem(itemData, 1);
            gameManager.GameDataGeneral.RecieveMoney(-itemData.buyValue);

            OpenMainPage(false); // reload
        }

        if (shopMode == ShopMode.Sell)
        {
            var res = gameManager.ItemManager.FindAndChangeItem(itemData, -1);
            gameManager.GameDataGeneral.RecieveMoney(itemData.sellValue);

            OpenMainPage(res <= 0); // reload
        }
    }
}
