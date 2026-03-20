using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    public enum MenuState
    {
        Main,
        Characters,
        Items,
        Missions,
        Todo
    }

    public VisualTreeAsset ItemListElement;
    public VisualTreeAsset ArtListElement;

    private SceneLoader sceneLoader;
    private UIDocument document;
    private InputHandler inputHandler;

    private MenuContainerMain containerMain = new();
    private MenuContainerCharacters containerCharacters = new();
    private MenuContainerItems containerItems = new();
    private MenuContainerMissions containerMissions = new();
    private MenuContainer currentContainer = null;

    private bool deleteGameObject = false;
    private float navigateInputCooldown = 0;
    private MenuState menuState = MenuState.Main;


    void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        document = GetComponent<UIDocument>();
        inputHandler = InputHandler.instance;

        document.rootVisualElement.visible = false;

        containerMain.mainMenuEvents = this;
        containerCharacters.mainMenuEvents = this;
        containerItems.mainMenuEvents = this;
        containerMissions.mainMenuEvents = this;
    }


    void Update()
    {
        if (deleteGameObject)
        {
            document.enabled = false;
            inputHandler = null;
            UserInterfaceManager.instance.DeleteMainMenu();
            return;
        }

        if (navigateInputCooldown > 0)
            navigateInputCooldown -= Time.deltaTime;

        var navigateInput = inputHandler.GetNavigateInput();
        if (navigateInput.magnitude > 0.2 && navigateInputCooldown <= 0)
        {
            currentContainer?.DirectionalEvent(navigateInput);
            navigateInputCooldown = 0.2f;
        }
        if (navigateInputCooldown > 0 && navigateInput.magnitude < 0.01)
            navigateInputCooldown = 0.0f;

        if (inputHandler.GetUICancelInputDown())
            currentContainer?.CancelEvent();

        if (inputHandler.GetUIConfirmInputDown())
            currentContainer?.ConfirmEvent();

        if (inputHandler.GetUISpecialInputDown())
            currentContainer?.SpecialEvent();
    }


    public void OpenMenu()
    {
        document.rootVisualElement.visible = true;
        ChangeMenuState(MenuState.Main);
        inputHandler.SetCursorConfined();
        inputHandler.inputMenuOnly = true;
    }

    public void CloseMenu() 
    {
        deleteGameObject = true;
        document.rootVisualElement.visible = false;
        inputHandler.SetCursorLocked();
        inputHandler.inputMenuOnly = false;
    }

    public void ChangeMenuState(MenuState newState)
    {
        menuState = newState;
        switch (menuState)
        {
            case MenuState.Main:
                ShowView("MenuMainVisualTree");
                containerMain.PrepareView(document.rootVisualElement);
                currentContainer = containerMain;
                break;
            case MenuState.Characters:
                ShowView("MenuCharactersVisualTree");
                containerCharacters.PrepareView(document.rootVisualElement);
                currentContainer = containerCharacters;
                break;
            case MenuState.Items:
                ShowView("MenuItemsVisualTree");
                containerItems.PrepareView(document.rootVisualElement);
                currentContainer = containerItems;
                break;
            case MenuState.Missions:
                ShowView("MenuMissions");
                containerMissions.PrepareView(document.rootVisualElement);
                currentContainer = containerMissions;
                break;
            default:
                return;
        }
    }

    private void ShowView(string viewName)
    {
        var root = document.rootVisualElement;
        foreach (var view in root.Q<VisualElement>("ViewContainer").Children())
        {
            view.RemoveFromClassList("active");
        }

        root.Q<VisualElement>(viewName).AddToClassList("active");
    }

    public void ReturnToStartscreen()
    {
        sceneLoader.sceneIndex = 0;
        sceneLoader.LoadScene();
        GameManager.Instance.SaveSystem.ClearCurrentSaveData();
    }

    


    /*// move out to somewhere else!
    static public Texture2D LoadCharacterIcon(int index)
    {
        Texture2D tex = Resources.Load<Texture2D>($"Icons/character_portrait_hero_{index}");
        return tex;
    }*/
}
