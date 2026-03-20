using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    public static UserInterfaceManager instance;

    [Header("Prefabs")]
    public GameplayUserInterface PrefabGameplayUi;
    public MainMenuEvents PrefabMainMenu;
    public LoadingScreen PrefabLoadingScreen;
    public DialogUserInterface PrefabDialogUI;
    public ShopUserInterface PrefabShopUI;

    [Header("References")]
    public MainMenuEvents mainMenu = null;
    public LoadingScreen loadingScreen = null;

    public GameplayUserInterface GameplayUI { get; private set; } = null;
    public DialogUserInterface DialogUI { get; private set; } = null;
    public ShopUserInterface ShopUI { get; private set; } = null;


    void Awake()
    {
        if (instance != this && instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this);
    }


    public void CreateMainMenu()
    {
        if (mainMenu != null) return;
        mainMenu = Instantiate(PrefabMainMenu);
        mainMenu.OpenMenu();
    }

    public void DeleteMainMenu()
    {
        if (mainMenu == null) return;
        Destroy(mainMenu.gameObject);
    }

    public void CreateGameplayUI()
    {
        if (GameplayUI != null)
        {
            Destroy(GameplayUI.gameObject);
        }
        GameplayUI = Instantiate(PrefabGameplayUi);
    }

    public float CreateLoadingScreen()
    {
        if (loadingScreen == null)
        {
            loadingScreen = Instantiate(PrefabLoadingScreen);
            DontDestroyOnLoad(loadingScreen);
        }
        return loadingScreen.fadeDuration;
    }

    public DialogUserInterface CreateDialogUI()
    {
        if (DialogUI == null)
        {
            DialogUI = Instantiate(PrefabDialogUI);
        }
        return DialogUI;
    }

    public void DestroyDialogUI()
    {
        if (DialogUI != null)
            Destroy(DialogUI.gameObject);
        DialogUI = null;
    }


    public ShopUserInterface CreateShopUI()
    {
        if (ShopUI == null)
        {
            ShopUI = Instantiate(PrefabShopUI);
        }
        return ShopUI;
    }

    public void DestroyShopUI()
    {
        if (ShopUI != null)
            Destroy(ShopUI.gameObject);
        ShopUI = null;
    }
}
