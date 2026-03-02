using UnityEngine;
using TMPro;

public class StartScreenUI : MonoBehaviour
{
    public CanvasGroup[] Pages;
    public TextMeshProUGUI[] PageFrontLabels;
    public float navigateCooldown = 0.2f;

    UserInterfaceInputHandler inputHandler;
    SceneLoader loader;

    private int currentPage = 0;
    private int currentPageIndex = 0;

    private float navigateCooldownTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputHandler = GetComponent<UserInterfaceInputHandler>();
        loader = GetComponent<SceneLoader>();

        DisplayPage();
        UpdatePageFrontLabels(currentPageIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (navigateCooldownTimer > 0f)
            navigateCooldownTimer -= Time.deltaTime;

        if (inputHandler.GetSubmitInputDown())
            SubmitEvent();

        if (inputHandler.GetCancelInputDown())
            CancelEvent();

        var navInput = inputHandler.GetNavigateInput();
        if (navigateCooldownTimer <= 0f && Mathf.Abs(navInput.y) > 0.5f) 
        {
            navigateCooldownTimer = navigateCooldown;
            currentPageIndex = navInput.y < 0 ? currentPageIndex + 1 : currentPageIndex - 1;
            
            // Front Page
            currentPageIndex = Mathf.Clamp(currentPageIndex, 0, 3);
            UpdatePageFrontLabels(currentPageIndex);
        }
    }

    void SubmitEvent()
    {
        if (currentPage == 0)
        {
            currentPage = currentPageIndex + 1;
            DisplayPage();
        }
        else if (currentPage == 1)
        {
            loader.LoadScene();
        }
    }

    void CancelEvent()
    {
        if (currentPage > 0)
        {
            currentPage = 0;
            DisplayPage();
        }
    }

    void DisplayPage() 
    {
        for (int i = 0; i < Pages.Length; i++) 
        {
            Pages[i].alpha = currentPage == i ? 1f : 0f;
        }
    }

    void UpdatePageFrontLabels(int indexSelection)
    {
        for (int i = 0; i < PageFrontLabels.Length; i++)
        {
            PageFrontLabels[i].color = indexSelection == i ? Color.white : Color.gray7;
        }
    }

    public void WriteDebugText(string text)
    {
        Debug.Log(text);
    }
}
