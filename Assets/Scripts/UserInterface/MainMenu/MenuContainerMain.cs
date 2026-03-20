using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MenuContainerMain : MenuContainer
{
    int index = 0;
    List<Label> listLabels = new();

    public override void PrepareView(VisualElement rootElement)
    {
        containerObj = rootElement != null ? rootElement.Q<VisualElement>("MainView") : containerObj;
        if (containerObj == null) { return; }
        listLabels = containerObj.Query<Label>().ToList();

        for (int i = 0; i < listLabels.Count; i++)
        {
            listLabels[i].RegisterCallback<ClickEvent, int>(OnMainViewLabelClick, i);
            listLabels[i].RegisterCallback<MouseOverEvent, int>(OnMainViewLabelMouseOver, i);
        }

        foreach (var label in listLabels)
            label.RemoveFromClassList("label-button-hovered");
        index = 0;
        listLabels[index].AddToClassList("label-button-hovered");
    }

    public override void ConfirmEvent()
    {
        switch (index)
        {
            case 0:
                mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Characters);
                break;
            case 1:
                mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Items);
                break;
            case 2:
                mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Missions);
                break;
            case 3:
                GameManager.Instance.SaveSystem.WriteToFile();
                break;
            case 4:
                mainMenuEvents.ReturnToStartscreen();
                break;
            case 5:
                mainMenuEvents.CloseMenu();
                break;
            default:
                return;
        }
    }

    public override void CancelEvent()
    {
        mainMenuEvents.CloseMenu();
    }

    public override void SpecialEvent()
    {

    }

    public override void DirectionalEvent(Vector2 navInput)
    {
        index = Mathf.Clamp(index - Mathf.RoundToInt(navInput.y), 0, listLabels.Count - 1);
        foreach (var label in listLabels)
            label.RemoveFromClassList("label-button-hovered");
        listLabels[index].AddToClassList("label-button-hovered");
    }


    private void OnMainViewLabelClick(ClickEvent evt, int index)
    {
        this.index = index;
        ConfirmEvent();
    }

    private void OnMainViewLabelMouseOver(MouseOverEvent evt, int index)
    {
        this.index = index;
        foreach (var label in listLabels)
            label.RemoveFromClassList("label-button-hovered");
        listLabels[index].AddToClassList("label-button-hovered");
    }
}
