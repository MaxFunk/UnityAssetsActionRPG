using NUnit.Framework.Interfaces;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuContainerCharacters : MenuContainer
{
    private enum ViewMode
    {
        CharSelect = 0,
        Overview = 1,
        GearSelect = 2,
        ArtSelect = 3,
        LevelUp = 4,
    }

    private enum GearEquipMode
    {
        None = 0,
        Outfit = 1,
        Accessory = 2,
        Augment = 3
    }

    private ViewMode viewMode = ViewMode.CharSelect;
    private GearEquipMode equipMode = GearEquipMode.None;
    private int characterIndex = -1;
    private int artequippedIndex = 0;

    private VisualElement[] containers = null;
    private Button buttonBack = null;
    private CharacterDataUI charDataUI = null;
    private ArtData selectedNewArt = null;

    private readonly string containerNameCharSelect = "ContainerCharSelect";
    private readonly string containerNameOverview = "ContainerOverview";
    private readonly string containerNameGearSelect = "ContainerGearSelect";
    private readonly string containerNameArtSelect = "ContainerArtSelect";
    private readonly string containerNameLevelUp = "ContainerLevelUp";

    private readonly string panelNameCharSelect = "CharSelectPanel";
    private readonly string panelNameOverviewArtPanel = "OverviewArtPanel";
    private readonly string panelNameOverviewGearPanel = "OverviewGearPanel";
    private readonly string panelNameGearEquipPanel = "GearEquipedPanel";

    private readonly string subcontainerNameGearList = "GearList";
    private readonly string subcontainerNameGearDetail = "GearDetail";

    private readonly string classNameContainerActive = "container-active";
    private readonly string classNameClickableContainer = "clickable-container";


    public override void PrepareView(VisualElement rootElement)
    {
        containerObj = rootElement;
        containers = new VisualElement[5];
        containers[0] = containerObj.Q<VisualElement>(containerNameCharSelect);
        containers[1] = containerObj.Q<VisualElement>(containerNameOverview);
        containers[2] = containerObj.Q<VisualElement>(containerNameGearSelect);
        containers[3] = containerObj.Q<VisualElement>(containerNameArtSelect);
        containers[4] = containerObj.Q<VisualElement>(containerNameLevelUp);
        buttonBack = containerObj.Q<Button>("ButtonBack");

        RegisterEventsToClickables();
        LoadCharSelect();
    }

    public override void ConfirmEvent()
    {
        if (viewMode == ViewMode.ArtSelect && selectedNewArt != null)
        {
            ChangeWithEquipedArt(selectedNewArt);
        }
    }

    public override void CancelEvent()
    {
        if (viewMode == ViewMode.CharSelect)
        {
            mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Main);
            return;
        }

        if (viewMode == ViewMode.Overview)
        {
            LoadCharSelect();
            return;
        }

        if (viewMode == ViewMode.GearSelect)
        {
            if (equipMode == GearEquipMode.None)
            {
                LoadCharacterOverview(characterIndex);
            }
            else
            {
                LoadGearItemList(GearEquipMode.None);
            }
            return;
        }

        if (viewMode == ViewMode.ArtSelect)
        {
            LoadCharacterOverview(characterIndex);
            return;
        }

        if (viewMode == ViewMode.LevelUp)
        {
            LoadCharacterOverview(characterIndex);
            return;
        }
    }

    public override void SpecialEvent()
    {
        if (viewMode == ViewMode.GearSelect)
        {
            if (equipMode != GearEquipMode.None)
            {
                OnGearListItemClickEvent(null, -1);
            }
        }

        if (viewMode == ViewMode.ArtSelect)
        {
            ChangeWithEquipedArt(null);
        }
    }

    public override void DirectionalEvent(Vector2 navInput)
    {

    }


    private void SetContainerDisplayStyle(ViewMode newViewMode)
    {
        viewMode = newViewMode;

        foreach (var container in containers)
            container.RemoveFromClassList(classNameContainerActive);
        containers[(int)viewMode].AddToClassList(classNameContainerActive); // unsafe!, breaks if containers.Count != ViewMode enum entries
    }

    private void LoadCharSelect()
    {
        characterIndex = -1;
        charDataUI = null;
        SetContainerDisplayStyle(ViewMode.CharSelect);

        ref var container = ref containers[0];
        var charPanels = container.Query<VisualElement>(panelNameCharSelect).ToList();
        for (int i = 0; i < charPanels.Count; i++) 
        {
            var charData = GameManager.Instance.GetCharacterData(i);
            charPanels[i].dataSource = charData?.staticData;
            charPanels[i].style.visibility = charData != null ? Visibility.Visible : Visibility.Hidden; // maybe use display instead
            charPanels[i].UnregisterCallback<ClickEvent, int>(OnCharSelectPanelClick);
            if (charData != null)
                charPanels[i].RegisterCallback<ClickEvent, int>(OnCharSelectPanelClick, i);
        }
    }

    private void LoadCharacterOverview(int characterId)
    {
        characterIndex = characterId;
        SetContainerDisplayStyle(ViewMode.Overview);

        charDataUI = new CharacterDataUI();
        charDataUI.LoadFromCharacterData(GameManager.Instance.GetCharacterData(characterId)); // not fully safe, but should only be called with valid IDs
        containers[1].dataSource = charDataUI;

        var artPanels = containers[1].Query<VisualElement>(panelNameOverviewArtPanel).ToList();
        var artIds = charDataUI.characterData.GetActiveArtIds();
        for (int i = 0; i < artPanels.Count ; i++) 
        {
            if (i < 0 || i >= artIds.Length) // safety check
            {
                artPanels[i].style.visibility = Visibility.Hidden;
                continue;
            }

            var artData = ScriptableManager.instance.GetArtData(artIds[i]);
            artPanels[i].Q<VisualElement>("ArtView").dataSource = artData;
            artPanels[i].style.visibility = artData != null ? Visibility.Visible : Visibility.Hidden; // instead load other class if no art..
        }

        var gearPanels = containers[1].Query<VisualElement>(panelNameOverviewGearPanel).ToList();
        for (int i = 0; i < gearPanels.Count; i++)
        {
            var gearData = charDataUI.GetGearItemData(i);
            gearPanels[i].dataSource = gearData;
            gearPanels[i].style.visibility = gearData != null ? Visibility.Visible : Visibility.Hidden; // same as art panel above
        }
    }


    private void LoadGearSelection()
    {
        SetContainerDisplayStyle(ViewMode.GearSelect);

        var gearPanels = containers[2].Query<VisualElement>(panelNameGearEquipPanel).ToList();
        for (int i = 0; i < gearPanels.Count; i++)
        {
            var gearData = charDataUI.GetGearItemData(i);
            gearPanels[i].dataSource = gearData;
            gearPanels[i].style.visibility = gearData != null ? Visibility.Visible : Visibility.Hidden;
        }

        LoadGearItemList(GearEquipMode.None);
        LoadGearItemDetail(null); // Update Gear
    }

    private void LoadGearItemList(GearEquipMode newEquipMode)
    {
        equipMode = newEquipMode;
        if (equipMode == GearEquipMode.None)
        {
            containers[2].Q<VisualElement>(subcontainerNameGearList).style.visibility = Visibility.Hidden;
            return;
        }

        containers[2].Q<VisualElement>(subcontainerNameGearList).style.visibility = Visibility.Visible;

        var itemList = GameManager.Instance.ItemManager.GetFilteredGearList((int)equipMode);
        var scrollView = containers[2].Q<ScrollView>();
        scrollView.Clear();
        //listElements = new VisualElement[itemList.Count];
        for (int i = 0; i < itemList.Count; i++)
        {
            VisualElement itemListElementTree = mainMenuEvents.ItemListElement.CloneTree();
            var itemListElement = itemListElementTree.Q<VisualElement>("ItemListElement");
            itemListElement.dataSource = itemList[i];
            itemListElement.RegisterCallback<ClickEvent, int>(OnGearListItemClickEvent, itemList[i].id);
            //listElements[i] = itemListElement;
            scrollView.Add(itemListElementTree);
        }
    }

    private void LoadGearItemDetail(ItemData itemData)
    {
        var gearDetail = containers[2].Q<VisualElement>(subcontainerNameGearDetail);
        gearDetail.style.visibility = itemData != null ? Visibility.Visible : Visibility.Hidden;
        gearDetail.dataSource = itemData;
    }


    private void LoadArtSelection()
    {
        SetContainerDisplayStyle(ViewMode.ArtSelect);

        var artPanels = containers[3].Query<VisualElement>("ArtPanel").ToList();
        for (int i = 0; i < artPanels.Count; i++) 
        {
            var artData = charDataUI.artDatas[i];
            artPanels[i].Q<VisualElement>("ArtView").dataSource = artData;
            artPanels[i].style.visibility = artData != null ? Visibility.Visible : Visibility.Hidden;
            artPanels[i].RegisterCallback<ClickEvent, int>(OnEquipedArtClickEvent, i);
        }

        var ultPanel = containers[3].Q<VisualElement>("UltPanel");
        ultPanel.Q<VisualElement>("ArtView").dataSource = charDataUI.artDatas[5];
        ultPanel.RegisterCallback<ClickEvent, int>(OnEquipedArtClickEvent, 5);

        //containers[3].Q<ScrollView>().style.visibility = Visibility.Hidden;
        LoadArtList();

        artPanels[0].Focus();
        artequippedIndex = 0;
        LoadArtDetail(charDataUI.artDatas[0]);
    }

    private void LoadArtList()
    {
        selectedNewArt = null;
        containers[3].Q<ScrollView>().style.visibility = Visibility.Visible;
        var scrollView = containers[3].Q<ScrollView>();
        scrollView.Clear();

        var artList = charDataUI.characterData.GetUnequipedArts(artequippedIndex == 5);
        for (int i = 0; i < artList.Count; i++)
        {
            VisualElement artListElement = mainMenuEvents.ArtListElement.CloneTree();
            artListElement.Q<VisualElement>("ArtView").dataSource = artList[i];
            artListElement.RegisterCallback<ClickEvent, ArtData>(OnArtListElementClickEvent, artList[i]);
            scrollView.Add(artListElement);
        }
    }

    private void LoadArtDetail(ArtData artData)
    {
        var containerDetail = containers[3].Q<VisualElement>("ContainerDetail");
        containerDetail.style.visibility = artData != null ? Visibility.Visible : Visibility.Hidden;
        containerDetail.dataSource = artData;
    }

    private void LoadLevelUpView()
    {
        SetContainerDisplayStyle(ViewMode.LevelUp);
    }


    private void RegisterEventsToClickables()
    {
        buttonBack.RegisterCallback<ClickEvent>(OnButtonBackClickEvent);

        var overviewClickables = containers[1].Query<VisualElement>(className: classNameClickableContainer).ToList();
        for (int i = 0; i < overviewClickables.Count; i++) 
        {
            overviewClickables[i].RegisterCallback<ClickEvent, int>(OnOverviewClickEvent, i);
        }

        var gearClickables = containers[2].Query<VisualElement>(className: classNameClickableContainer).ToList();
        for (int i = 0; i < gearClickables.Count; i++)
        {
            gearClickables[i].RegisterCallback<ClickEvent, int>(OnGearEquipPanelClickEvent, i);
            gearClickables[i].RegisterCallback<MouseOverEvent, int>(OnGearEquipPanelMouseOverEvent, i);
        }
    }

    private void ChangeWithEquipedArt(ArtData artData)
    {
        charDataUI.characterData.SwapArtIds(artequippedIndex, artData);
        charDataUI.ReloadData();
        LoadArtSelection();
    }



    private void OnCharSelectPanelClick(ClickEvent evt, int index)
    {
        LoadCharacterOverview(index);
    }

    private void OnOverviewClickEvent(ClickEvent evt, int index)
    {
        switch (index)
        {
            case 0:
                LoadLevelUpView();
                break;
            case 1:
                LoadGearSelection();
                break;
            case 2:
                LoadArtSelection();
                break;
            default:
                break;
        }
    }

    private void OnGearEquipPanelClickEvent(ClickEvent evt, int index)
    {
        switch (index)
        {
            case 0:
                LoadGearItemList(GearEquipMode.Outfit);
                break;
            case 1:
                LoadGearItemList(GearEquipMode.Accessory);
                break;
            case 2:
                LoadGearItemList(GearEquipMode.Augment);
                break;
            default:
                break;
        }
    }

    private void OnGearEquipPanelMouseOverEvent(MouseOverEvent evt, int index)
    {
        LoadGearItemDetail(charDataUI.GetGearItemData(index));
    }

    private void OnGearListItemClickEvent(ClickEvent evt, int itemId)
    {
        int slot = (int)equipMode - 1;
        charDataUI.characterData.SwapGearIds(slot, itemId);
        charDataUI.ReloadData();
        LoadGearSelection();
    }


    private void OnEquipedArtClickEvent(ClickEvent evt, int index)
    {
        var visElem = (VisualElement) evt.target;
        visElem.Focus();
        artequippedIndex = index;
        LoadArtDetail(charDataUI.artDatas[index]);
        //LoadArtList();
    }


    private void OnArtListElementClickEvent(ClickEvent evt, ArtData artData)
    {
        if (artData == selectedNewArt)
        {
            ChangeWithEquipedArt(artData);
            return;
        }

        selectedNewArt = artData;
        LoadArtDetail(artData);
    }

    private void OnButtonBackClickEvent(ClickEvent evt)
    {
        CancelEvent();
    }
}
