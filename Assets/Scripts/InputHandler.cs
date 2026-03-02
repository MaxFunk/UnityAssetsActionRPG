using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;
    
    public bool inputBlocked = false;
    public bool inputMenuOnly = false;

    [SerializeField]
    private InputActionAsset inputActionsAsset;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction drawWeaponAction;
    private InputAction sheatheWeaponAction;
    private InputAction interactAction;
    private InputAction targetPrevAction;
    private InputAction targetNextAction;
    private InputAction openMenuAction;
    private InputAction switchCharacterAction;
    private InputAction focusAlliesAction;

    private InputAction art1Action;
    private InputAction art2Action;
    private InputAction art3Action;
    private InputAction art4Action;
    private InputAction art5Action;
    private InputAction artUltAction;
    private InputAction ally1UltAction;
    private InputAction ally2UltAction;

    private InputAction navigateAction;
    private InputAction confirmAction;
    private InputAction cancelAction;
    private InputAction specialAction;


    private void Awake()
    {
        if (instance != this && instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this);

        FindAndEnableAction(ref moveAction, "Gameplay/Move");
        FindAndEnableAction(ref lookAction, "Gameplay/Look");
        FindAndEnableAction(ref jumpAction, "Gameplay/Jump");
        FindAndEnableAction(ref drawWeaponAction, "Gameplay/DrawWeapon");
        FindAndEnableAction(ref sheatheWeaponAction, "Gameplay/SheatheWeapon");
        FindAndEnableAction(ref interactAction, "Gameplay/Interact");
        FindAndEnableAction(ref targetPrevAction, "Gameplay/TargetPrev");
        FindAndEnableAction(ref targetNextAction, "Gameplay/TargetNext");
        FindAndEnableAction(ref openMenuAction, "Gameplay/OpenMenu");
        FindAndEnableAction(ref switchCharacterAction, "Gameplay/SwitchCharacter");
        FindAndEnableAction(ref focusAlliesAction, "Gameplay/FocusAllies");

        FindAndEnableAction(ref art1Action, "Gameplay/Art1");
        FindAndEnableAction(ref art2Action, "Gameplay/Art2");
        FindAndEnableAction(ref art3Action, "Gameplay/Art3");
        FindAndEnableAction(ref art4Action, "Gameplay/Art4");
        FindAndEnableAction(ref art5Action, "Gameplay/Art5");
        FindAndEnableAction(ref artUltAction, "Gameplay/ArtUlt");
        FindAndEnableAction(ref ally1UltAction, "Gameplay/Ally1Ult");
        FindAndEnableAction(ref ally2UltAction, "Gameplay/Ally2Ult");

        FindAndEnableAction(ref navigateAction, "UI/Navigate");
        FindAndEnableAction(ref confirmAction, "UI/Confirm");
        FindAndEnableAction(ref cancelAction, "UI/Cancel");
        FindAndEnableAction(ref specialAction, "UI/Special");
    }

    private void FindAndEnableAction(ref InputAction inputAction, string actionName)
    {
        inputAction = inputActionsAsset.FindAction(actionName);
        inputAction.Enable();
    }

    public Vector3 GetMoveInput()
    {
        if (inputBlocked || inputMenuOnly) return Vector3.zero;

        var input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y);
        move = Vector3.ClampMagnitude(move, 1);
        return move;
    }

    public float GetLookInputsHorizontal()
    {
        if (inputBlocked || inputMenuOnly) return 0f;

        float input = lookAction.ReadValue<Vector2>().x;
        //if (InvertXAxis)
        //    input *= -1;
        //input *= LookSensitivity;
        return input;
    }

    public float GetLookInputsVertical()
    {
        if (inputBlocked || inputMenuOnly) return 0f;

        float input = lookAction.ReadValue<Vector2>().y;
        //if (InvertYAxis)
        //    input *= -1;
        //input *= LookSensitivity;
        return input;
    }

    public bool GetJumpInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return jumpAction.WasPressedThisFrame();
    }

    public bool GetDrawWeaponInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return drawWeaponAction.WasPressedThisFrame();
    }

    public bool GetSheatheWeaponInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return sheatheWeaponAction.WasPressedThisFrame();
    }

    public bool GetInteractInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return interactAction.WasPressedThisFrame();
    }

    public bool GetTargetPrevInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return targetPrevAction.WasPressedThisFrame();
    }

    public bool GetTargetNextInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return targetNextAction.WasPressedThisFrame();
    }

    public bool GetOpenMenuInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return openMenuAction.WasPressedThisFrame();
    }

    public bool GetSwitchCharacterInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return switchCharacterAction.WasPressedThisFrame();
    }

    public bool GetFocusAlliesInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return focusAlliesAction.WasPressedThisFrame();
    }



    public bool GetArt1InputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return art1Action.WasPressedThisFrame();
    }

    public bool GetArt2InputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return art2Action.WasPressedThisFrame();
    }

    public bool GetArt3InputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return art3Action.WasPressedThisFrame();
    }

    public bool GetArt4InputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return art4Action.WasPressedThisFrame();
    }

    public bool GetArt5InputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return art5Action.WasPressedThisFrame();
    }

    public bool GetArtUltInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return artUltAction.WasPressedThisFrame();
    }

    public bool GetAlly1UltInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return ally1UltAction.WasPressedThisFrame();
    }

    public bool GetAlly2UltInputDown()
    {
        if (inputBlocked || inputMenuOnly) return false;
        return ally2UltAction.WasPressedThisFrame();
    }



    public Vector2 GetNavigateInput()
    {
        if (inputBlocked) return Vector2.zero;
        return navigateAction.ReadValue<Vector2>();
    }

    public bool GetUISpecialInputDown()
    {
        if (inputBlocked) return false;
        return specialAction.WasPressedThisFrame();
    }

    public bool GetUIConfirmInputDown()
    {
        if (inputBlocked) return false;
        return confirmAction.WasPressedThisFrame();
    }

    public bool GetUICancelInputDown()
    {
        if (inputBlocked) return false;
        return cancelAction.WasPressedThisFrame();
    }



    public void SetCursorLocked()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetCursorConfined()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
}
