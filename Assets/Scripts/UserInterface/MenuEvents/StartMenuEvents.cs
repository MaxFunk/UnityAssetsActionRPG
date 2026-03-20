using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StartMenuEvents : MonoBehaviour
{
    private enum MenuState
    {
        Front,
        SaveFile,
        Controls,
        BlockInput
    }

    private SceneLoader sceneLoader;
    private UIDocument document;
    private InputHandler inputHandler;

    private VisualElement containerFrontPage;
    private VisualElement containerFileSelect;
    private VisualElement containerControls;
    private Label labelBack;
    private Label labelLoadGame;
    private Label labelControls;
    private Label labelQuitGame;

    private List<VisualElement> saveFileViews = new();
    private readonly List<Label> labelsFront = new();
    private MenuState menuState = MenuState.Front;
    private float navigateInputCooldown = 0;
    private int indexFront = 0;
    private int indexSaveFile = 0;

    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        document = GetComponent<UIDocument>();
        inputHandler = InputHandler.instance;
        if (inputHandler != null)
            InputHandler.instance.SetCursorConfined();

        containerFrontPage = document.rootVisualElement.Q("ContainerFrontPage");
        containerFileSelect = document.rootVisualElement.Q("ContainerFileSelect");
        containerControls = document.rootVisualElement.Q("ContainerControls");

        labelBack = document.rootVisualElement.Q<Label>("LabelBack");
        labelLoadGame = document.rootVisualElement.Q<Label>("LabelLoadGame");
        labelControls = document.rootVisualElement.Q<Label>("LabelControls");
        labelQuitGame = document.rootVisualElement.Q<Label>("LabelQuitGame");

        labelsFront.Add(labelLoadGame);
        labelsFront.Add(labelControls);
        labelsFront.Add(labelQuitGame);

        for (int i = 0; i < labelsFront.Count; i++)
        {
            labelsFront[i].RegisterCallback<MouseOverEvent, int>(OnMouseOverLabelFront, i);
        }

        labelBack.RegisterCallback<ClickEvent>(OnLabelBackClick);
        labelLoadGame.RegisterCallback<ClickEvent>(OnLabelLoadGameClick);
        labelControls.RegisterCallback<ClickEvent>(OnLabelControlsClick);
        labelQuitGame.RegisterCallback<ClickEvent>(OnQuitGameClick);

        var previews = GameManager.Instance.SaveSystem.GenerateSaveDataPreviews();
        saveFileViews = document.rootVisualElement.Query<VisualElement>(className: "save-file-view").ToList();
        for (int i = 0; i < saveFileViews.Count; i++)
        {
            if (i >= previews.Count)
                continue;

            saveFileViews[i].dataSource = previews[i];
            saveFileViews[i].RegisterCallback<ClickEvent, int>(OnSaveFileClick, i);
            saveFileViews[i].RegisterCallback<MouseOverEvent, int>(OnMouseOverLabelSaveFile, i);
        }

        LoadFrontView();
    }

    private void Update()
    {
        if (navigateInputCooldown > 0)
            navigateInputCooldown -= Time.deltaTime;

        if (inputHandler == null)
        {
            inputHandler = InputHandler.instance;
            return;
        }

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


    private void DirectionalInput(Vector3 input)
    {
        if (menuState == MenuState.Front)
        {
            indexFront = Mathf.Clamp(indexFront - Mathf.RoundToInt(input.y), 0, labelsFront.Count - 1);
            UpdateSelectionFront();
        }

        if (menuState == MenuState.SaveFile)
        {
            indexSaveFile = Mathf.Clamp(indexSaveFile - Mathf.RoundToInt(input.y), 0, saveFileViews.Count - 1);
            UpdateSelectionSaveFiles();
        }
    }


    private void ConfirmInput()
    {
        if (menuState == MenuState.Front)
        {
            switch (indexFront)
            {
                case 0:
                    LoadSaveFilesView();
                    break;
                case 1:
                    LoadControlsView();
                    break;
                case 2:
                    OnQuitGameClick(null);
                    break;
                default:
                    break;
            }
            return;
        }

        if (menuState == MenuState.SaveFile)
        {
            LoadGame(indexSaveFile);
        }
    }

    private void CancelInput()
    {
        if (menuState == MenuState.SaveFile || menuState == MenuState.Controls)
        {
            OnLabelBackClick(null);
        }
    }


    private void LoadFrontView()
    {
        UpdateSelectionFront();
        containerFrontPage.style.display = DisplayStyle.Flex;
        containerControls.style.display = DisplayStyle.None;
        containerFileSelect.style.display = DisplayStyle.None;
        labelBack.style.display = DisplayStyle.None;
        menuState = MenuState.Front;
    }

    private void LoadSaveFilesView()
    {
        indexSaveFile = 0;
        UpdateSelectionSaveFiles();
        containerFrontPage.style.display = DisplayStyle.None;
        containerControls.style.display = DisplayStyle.None;
        containerFileSelect.style.display = DisplayStyle.Flex;
        labelBack.style.display = DisplayStyle.Flex;
        menuState = MenuState.SaveFile;
    }

    private void LoadControlsView()
    {
        containerFrontPage.style.display = DisplayStyle.None;
        containerControls.style.display = DisplayStyle.Flex;
        containerFileSelect.style.display = DisplayStyle.None;
        labelBack.style.display = DisplayStyle.Flex;
        menuState = MenuState.Controls;
    }

    private void LoadGame(int fileIndex)
    {
        menuState = MenuState.BlockInput;
        GameManager.Instance.SaveSystem.ReadFromFile(fileIndex);
        sceneLoader.sceneIndex = GameManager.Instance.GameDataGeneral.sceneIndex;
        sceneLoader.spawnerIndex = GameManager.Instance.GameDataGeneral.sceneSpawner;
        sceneLoader.LoadScene();
    }


    private void UpdateSelectionFront()
    {
        foreach (var label in labelsFront)
            label.RemoveFromClassList("label-selected");

        labelsFront[indexFront].AddToClassList("label-selected");
    }

    private void UpdateSelectionSaveFiles()
    {
        foreach (var view in saveFileViews)
            view.RemoveFromClassList("view-selected");

        saveFileViews[indexSaveFile].AddToClassList("view-selected");
    }


    private void OnDisable()
    {
        labelLoadGame.UnregisterCallback<ClickEvent>(OnLabelLoadGameClick);
        labelQuitGame.UnregisterCallback<ClickEvent>(OnQuitGameClick);

        for (int i = 0; i < saveFileViews.Count; i++)
        {
            saveFileViews[i].UnregisterCallback<ClickEvent, int>(OnSaveFileClick);
        }
    }

    private void OnLabelLoadGameClick(ClickEvent evt)
    {
        LoadSaveFilesView();
    }

    private void OnLabelControlsClick(ClickEvent evt)
    {
        LoadControlsView();
    }

    private void OnLabelBackClick(ClickEvent evt)
    {
        LoadFrontView();
    }

    private void OnQuitGameClick(ClickEvent evt)
    {
        Debug.Log("Attempting to Quit Game");
        Application.Quit();
    }

    private void OnSaveFileClick(ClickEvent evt, int index)
    {
        LoadGame(index);
    }

    private void OnMouseOverLabelFront(MouseOverEvent evt, int index)
    {
        indexFront = index;
        UpdateSelectionFront();
    }

    private void OnMouseOverLabelSaveFile(MouseOverEvent evt, int index)
    {
        indexSaveFile = index;
        UpdateSelectionSaveFiles();
    }
}
