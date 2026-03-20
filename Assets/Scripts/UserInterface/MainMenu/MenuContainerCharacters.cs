using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI.MessageBox;

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
    private bool isEquippingArt = false;
    private int artequippedIndex = 0;

    private int indexCharSelect = 0;
    private int indexOverview = 0;
    private int indexGearEquip = 0;
    private int indexGearList = 0;
    private int indexArtEquip = 0;
    private int indexArtList = 0;

    private VisualElement[] containers = null;
    private Button buttonBack = null;
    private CharacterDataUI charDataUI = null;

    private List<VisualElement> charPanels = new();
    private List<VisualElement> overviewClickables = new();
    private List<VisualElement> gearClickables = new();
    private List<VisualElement> gearList = new();
    private List<VisualElement> artPanels = new();
    private List<VisualElement> artListPanels = new();

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
        if (viewMode == ViewMode.CharSelect)
        {
            LoadCharacterOverview(indexCharSelect);
            return;
        }

        if (viewMode == ViewMode.Overview)
        {
            ClickEvent evt = ClickEvent.GetPooled();
            evt.target = overviewClickables[indexOverview];
            overviewClickables[indexOverview].SendEvent(evt);
            return;                
        }

        if (viewMode == ViewMode.GearSelect)
        {
            if (equipMode == GearEquipMode.None)
            {
                OnGearEquipPanelClickEvent(null, indexGearEquip);
            }
            else
            {
                if (gearList.Count > 0)
                {
                    ClickEvent evt = ClickEvent.GetPooled();
                    evt.target = gearList[indexGearList];
                    gearList[indexGearList].SendEvent(evt);
                }
            }
            return;
        }

        if (viewMode == ViewMode.ArtSelect)
        {
            if (isEquippingArt)
            {
                if (artListPanels.Count > 0)
                {
                    ClickEvent evt = ClickEvent.GetPooled();
                    evt.target = artListPanels[indexArtList];
                    artListPanels[indexArtList].SendEvent(evt);
                }
            }
            else
            {
                LoadArtList();
                isEquippingArt = true;
            }
            return;
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
            if (isEquippingArt)
            {
                LoadArtSelection();
                isEquippingArt = false;
            }
            else
            {
                LoadCharacterOverview(characterIndex);
            }
            
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

        if (viewMode == ViewMode.ArtSelect && isEquippingArt)
        {
            ChangeWithEquipedArt(null);
        }
    }

    public override void DirectionalEvent(Vector2 navInput)
    {
        if (viewMode == ViewMode.CharSelect)
        {
            indexCharSelect = Mathf.Clamp(indexCharSelect + Mathf.RoundToInt(navInput.x), 0, 2); // hardcoded to not break demo
            UpdateSelectionCharSelect();
        }

        if (viewMode == ViewMode.Overview)
        {
            indexOverview = Mathf.Clamp(indexOverview + Mathf.RoundToInt(navInput.x), 0, 2);
            indexOverview = Mathf.Clamp(indexOverview - 2 * Mathf.RoundToInt(navInput.y), 0, 2);
            UpdateSelectionOverview();
        }

        if (viewMode == ViewMode.GearSelect)
        {
            if (equipMode == GearEquipMode.None)
            {
                indexGearEquip = Mathf.Clamp(indexGearEquip - Mathf.RoundToInt(navInput.y), 0, gearClickables.Count - 1);
                LoadGearItemDetail(charDataUI.GetGearItemData(indexGearEquip));
                UpdateSelectionGearEquip();
            }
            else
            {
                indexGearList = Mathf.Clamp(indexGearList - Mathf.RoundToInt(navInput.y), 0, gearList.Count - 1);
                UpdateSelectionGearList();
            }
        }

        if (viewMode == ViewMode.ArtSelect)
        {
            if (isEquippingArt)
            {
                indexArtList = Mathf.Clamp(indexArtList - Mathf.RoundToInt(navInput.y), 0, artListPanels.Count - 1);
                UpdateSelectionArtList();
            }
            else
            {
                indexArtEquip = Mathf.Clamp(indexArtEquip - Mathf.RoundToInt(navInput.y), 0, artPanels.Count - 1);
                LoadArtDetail(charDataUI.artDatas[indexArtEquip]);
                UpdateSelectionArtEquip();
            }
        }
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
        indexCharSelect = Mathf.Clamp(characterIndex, 0, 2);
        characterIndex = -1;
        charDataUI = null;
        SetContainerDisplayStyle(ViewMode.CharSelect);

        ref var container = ref containers[0];
        charPanels = container.Query<VisualElement>(panelNameCharSelect).ToList();
        for (int i = 0; i < charPanels.Count; i++) 
        {
            var charData = GameManager.Instance.GetCharacterData(i);
            charPanels[i].dataSource = charData?.staticData;
            charPanels[i].style.visibility = charData != null ? Visibility.Visible : Visibility.Hidden; // maybe use display instead
            charPanels[i].UnregisterCallback<ClickEvent, int>(OnCharSelectPanelClick);
            if (charData != null)
            {
                charPanels[i].RegisterCallback<ClickEvent, int>(OnCharSelectPanelClick, i);
                charPanels[i].RegisterCallback<MouseOverEvent, int>(OnMouseOverCharSelect, i);
            }
        }

        UpdateSelectionCharSelect();
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

        indexOverview = 0;
        UpdateSelectionOverview();
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

        indexGearEquip = 0;
        UpdateSelectionGearEquip();

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
        gearList.Clear();
        for (int i = 0; i < itemList.Count; i++)
        {
            VisualElement itemListElementTree = mainMenuEvents.ItemListElement.CloneTree();
            var itemListElement = itemListElementTree.Q<VisualElement>("ItemListElement");
            itemListElement.dataSource = itemList[i];
            itemListElement.RegisterCallback<ClickEvent, int>(OnGearListItemClickEvent, itemList[i].id);
            itemListElement.RegisterCallback<MouseOverEvent, int>(OnMouseOverGearList, i);
            scrollView.Add(itemListElementTree);
            gearList.Add(itemListElement);
        }

        indexGearList = 0;
        UpdateSelectionGearList();
    }

    private void LoadGearItemDetail(ItemData itemData)
    {
        var gearDetail = containers[2].Q<VisualElement>(subcontainerNameGearDetail);
        gearDetail.style.visibility = itemData != null ? Visibility.Visible : Visibility.Hidden;
        gearDetail.dataSource = itemData;
    }


    private void LoadArtSelection(bool keepIndex = false)
    {
        SetContainerDisplayStyle(ViewMode.ArtSelect);

        artPanels = containers[3].Query<VisualElement>("ArtPanel").ToList();
        for (int i = 0; i < artPanels.Count; i++) 
        {
            var artData = charDataUI.artDatas[i];
            artPanels[i].Q<VisualElement>("ArtView").dataSource = artData;
            artPanels[i].Q<VisualElement>("ArtView").style.visibility = artData != null ? Visibility.Visible : Visibility.Hidden;
            artPanels[i].RegisterCallback<ClickEvent, int>(OnEquipedArtClickEvent, i); 
            artPanels[i].RegisterCallback<MouseOverEvent, int>(OnMouseOverArtEquip, i);
        }

        var ultPanel = containers[3].Q<VisualElement>("UltPanel");
        ultPanel.Q<VisualElement>("ArtView").dataSource = charDataUI.artDatas[5];
        ultPanel.Q<VisualElement>("ArtView").style.visibility = charDataUI.artDatas[5] != null ? Visibility.Visible : Visibility.Hidden;
        ultPanel.RegisterCallback<ClickEvent, int>(OnEquipedArtClickEvent, 5);
        ultPanel.RegisterCallback<MouseOverEvent, int>(OnMouseOverArtEquip, 5);
        artPanels.Add(ultPanel);

        containers[3].Q<ScrollView>().style.visibility = Visibility.Hidden;
        //LoadArtList();

        indexArtEquip = keepIndex ? indexArtEquip : 0;
        UpdateSelectionArtEquip();
        LoadArtDetail(charDataUI.artDatas[0]);
    }

    private void LoadArtList()
    {
        containers[3].Q<ScrollView>().style.visibility = Visibility.Visible;
        var scrollView = containers[3].Q<ScrollView>();
        scrollView.Clear();
        artListPanels.Clear();

        var artList = charDataUI.characterData.GetUnequipedArts(indexArtEquip == 5);
        for (int i = 0; i < artList.Count; i++)
        {
            VisualElement artListElement = mainMenuEvents.ArtListElement.CloneTree();
            artListElement.Q<VisualElement>("ArtView").dataSource = artList[i];
            artListElement.RegisterCallback<ClickEvent, ArtData>(OnArtListElementClickEvent, artList[i]);
            artListElement.RegisterCallback<MouseOverEvent, int>(OnMouseOverArtList, i);
            scrollView.Add(artListElement);
            artListPanels.Add(artListElement);
        }

        indexArtList = 0;
        UpdateSelectionArtList();
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

        overviewClickables = containers[1].Query<VisualElement>(className: classNameClickableContainer).ToList();
        for (int i = 0; i < overviewClickables.Count; i++) 
        {
            overviewClickables[i].RegisterCallback<ClickEvent, int>(OnOverviewClickEvent, i);
            overviewClickables[i].RegisterCallback<MouseOverEvent, int>(OnMouseOverOverview, i);
        }

        gearClickables = containers[2].Query<VisualElement>(className: classNameClickableContainer).ToList();
        for (int i = 0; i < gearClickables.Count; i++)
        {
            gearClickables[i].RegisterCallback<ClickEvent, int>(OnGearEquipPanelClickEvent, i);
            gearClickables[i].RegisterCallback<MouseOverEvent, int>(OnGearEquipPanelMouseOverEvent, i);
        }
    }

    private void ChangeWithEquipedArt(ArtData artData)
    {
        charDataUI.characterData.SwapArtIds(indexArtEquip, artData);
        charDataUI.ReloadData();
        isEquippingArt = false;
        LoadArtSelection(true);
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
        if (equipMode != GearEquipMode.None) return;

        indexGearEquip = index;
        UpdateSelectionGearEquip();
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
        ChangeWithEquipedArt(artData);
    }

    private void OnButtonBackClickEvent(ClickEvent evt)
    {
        CancelEvent();
    }


    private void UpdateSelectionCharSelect()
    {
        foreach (var panel in charPanels)
            panel.RemoveFromClassList("clickable-container-selected");

        charPanels[indexCharSelect].AddToClassList("clickable-container-selected");
    }

    private void OnMouseOverCharSelect(MouseOverEvent evt, int index)
    {
        indexCharSelect = index;
        UpdateSelectionCharSelect();
    }


    private void UpdateSelectionOverview()
    {
        foreach (var panel in overviewClickables)
            panel.RemoveFromClassList("clickable-container-selected");

        overviewClickables[indexOverview].AddToClassList("clickable-container-selected");
    }

    private void OnMouseOverOverview(MouseOverEvent evt, int index)
    {
        indexOverview = index;
        UpdateSelectionOverview();
    }

    private void UpdateSelectionGearEquip()
    {
        foreach (var panel in gearClickables)
            panel.RemoveFromClassList("clickable-container-selected");

        gearClickables[indexGearEquip].AddToClassList("clickable-container-selected");
    }

    private void UpdateSelectionGearList()
    {
        foreach (var panel in gearList)
            panel.RemoveFromClassList("selected");

        if (gearList.Count > 0)
            gearList[indexGearList].AddToClassList("selected");
    }

    private void OnMouseOverGearList(MouseOverEvent evt, int index)
    {
        indexGearList = index;
        UpdateSelectionGearList();
    }


    private void UpdateSelectionArtEquip()
    {
        foreach (var panel in artPanels)
            panel.RemoveFromClassList("arts-equip-panel-selected");

        artPanels[indexArtEquip].AddToClassList("arts-equip-panel-selected");
    }

    private void OnMouseOverArtEquip(MouseOverEvent evt, int index)
    {
        indexArtEquip = index;
        LoadArtDetail(charDataUI.artDatas[index]);
        UpdateSelectionArtEquip();
    }


    private void UpdateSelectionArtList()
    {
        foreach (var panel in artListPanels)
            panel.RemoveFromClassList("arts-equip-panel-selected");

        if (artListPanels.Count > 0)
            artListPanels[indexArtList].AddToClassList("arts-equip-panel-selected");
    }

    private void OnMouseOverArtList(MouseOverEvent evt, int index)
    {
        indexArtList = index;
        //LoadArtDetail(charDataUI.artDatas[index]);
        UpdateSelectionArtList();
    }
}
