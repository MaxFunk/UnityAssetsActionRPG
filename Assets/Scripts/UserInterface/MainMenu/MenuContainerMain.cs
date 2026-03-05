using UnityEngine;
using UnityEngine.UIElements;

public class MenuContainerMain : MenuContainer
{
    string containerName = "MainView";
    int index = 0;

    public override void PrepareView(VisualElement rootElement)
    {
        containerObj = rootElement != null ? rootElement.Q<VisualElement>(containerName) : containerObj;
        if (containerObj == null) { return; }
        var listLabels = containerObj.Query<Label>().ToList();

        for (int i = 0; i < listLabels.Count; i++)
        {
            VisualElementCallData callData = new();
            callData.value = i;
            listLabels[i].dataSource = callData;
            listLabels[i].RegisterCallback<ClickEvent>(OnMainViewLabelClick);
            listLabels[i].RegisterCallback<MouseOverEvent>(OnMainViewLabelMouseOver);
        }

        listLabels[index].Focus();
    }

    public override void ConfirmEvent()
    {
        switch (index)
        {
            case 0:
                mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Characters);
                break;
            case 1:
                Debug.Log("TODO PARTY");
                break;
            case 2:
                mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Items);
                break;
            case 3:
                mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Missions);
                break;
            case 4:
                Debug.Log("TODO MAP");
                break;
            case 5:
                GameManager.Instance.SaveSystem.WriteToFile();
                break;
            case 6:
                mainMenuEvents.ReturnToStartscreen();
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

    }


    private void OnMainViewLabelClick(ClickEvent evt)
    {
        var dataSource = (evt.currentTarget as VisualElement).dataSource as VisualElementCallData;
        index = dataSource.value;
        ConfirmEvent();
    }

    private void OnMainViewLabelMouseOver(MouseOverEvent evt)
    {
        var dataSource = (evt.currentTarget as VisualElement).dataSource as VisualElementCallData;
        var listLabels = containerObj.Query<Label>().ToList();
        index = dataSource.value;
        listLabels[index].Focus();
    }
}
