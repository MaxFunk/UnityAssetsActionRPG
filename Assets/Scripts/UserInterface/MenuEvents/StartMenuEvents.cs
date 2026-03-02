using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class StartMenuEvents : MonoBehaviour
{
    private SceneLoader sceneLoader;
    private UIDocument document;

    private VisualElement containerFrontPage;
    private VisualElement containerFileSelect;
    private Label labelLoadGame;
    private Label labelQuitGame;

    private List<VisualElement> saveFileViews = new List<VisualElement>();

    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        document = GetComponent<UIDocument>();

        containerFrontPage = document.rootVisualElement.Q("ContainerFrontPage");
        containerFileSelect = document.rootVisualElement.Q("ContainerFileSelect");
        labelLoadGame = document.rootVisualElement.Q("LabelLoadGame") as Label;
        labelQuitGame = document.rootVisualElement.Q("LabelQuitGame") as Label;

        labelLoadGame.RegisterCallback<ClickEvent>(OnLabelLoadGameClick);
        labelQuitGame.RegisterCallback<ClickEvent>(OnQuitGameClick);

        var previews = GameManager.Instance.SaveSystem.GenerateSaveDataPreviews();
        saveFileViews = document.rootVisualElement.Query<VisualElement>(className: "save-file-view").ToList();
        for (int i = 0; i < saveFileViews.Count; i++)
        {
            if (i >= previews.Count)
                continue;

            saveFileViews[i].dataSource = previews[i];
            saveFileViews[i].RegisterCallback<ClickEvent>(OnSaveFileClick);
        }

        containerFrontPage.style.display = DisplayStyle.Flex;
        containerFileSelect.style.display = DisplayStyle.None;

        InputHandler.instance.SetCursorConfined();
    }

    private void OnDisable()
    {
        labelLoadGame.UnregisterCallback<ClickEvent>(OnLabelLoadGameClick);
        labelQuitGame.UnregisterCallback<ClickEvent>(OnQuitGameClick);

        for (int i = 0; i < saveFileViews.Count; i++)
        {
            saveFileViews[i].UnregisterCallback<ClickEvent>(OnSaveFileClick);
        }
    }

    private void OnLabelLoadGameClick(ClickEvent evt)
    {
        containerFrontPage.style.display = DisplayStyle.None;
        containerFileSelect.style.display = DisplayStyle.Flex;
    }

    private void OnQuitGameClick(ClickEvent evt)
    {
        Debug.Log("Attempting to Quit Game");
        Application.Quit();
    }

    private void OnAllButtonsClick(ClickEvent evt)
    {
        Debug.Log("TODO: Play Sound File");
    }

    private void OnSaveFileClick(ClickEvent evt)
    {
        var dataSource = (evt.currentTarget as VisualElement).dataSource as SaveDataPreview;
        GameManager.Instance.SaveSystem.ReadFromFile(dataSource.fileIndex);
        sceneLoader.LoadScene();
    }
}
