using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    //PlayerCharacterController m_PlayerCharacterController;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private InputAction drawWeaponAction;
    private InputAction sheatheWeaponAction;
    private InputAction interactAction;
    private InputAction openMenuAction;
    private InputAction targetPrevAction;
    private InputAction targetNextAction;

    private InputAction artUpAction;
    private InputAction artLeftAction;
    private InputAction artDownAction;
    private InputAction artRightAction;
    private InputAction endCombatAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //m_PlayerCharacterController = GetComponent<PlayerCharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveAction = InputSystem.actions.FindAction("Player/Move");
        lookAction = InputSystem.actions.FindAction("Player/Look");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");

        drawWeaponAction = InputSystem.actions.FindAction("Player/DrawWeapon");
        sheatheWeaponAction = InputSystem.actions.FindAction("Player/SheatheWeapon");
        interactAction = InputSystem.actions.FindAction("Player/Interact");
        targetPrevAction = InputSystem.actions.FindAction("Player/TargetPrev");
        targetNextAction = InputSystem.actions.FindAction("Player/TargetNext");
        openMenuAction = InputSystem.actions.FindAction("Player/OpenMenu");

        artUpAction = InputSystem.actions.FindAction("Combat/ArtUp");
        artLeftAction = InputSystem.actions.FindAction("Combat/ArtLeft");
        artDownAction = InputSystem.actions.FindAction("Combat/ArtDown");
        artRightAction = InputSystem.actions.FindAction("Combat/ArtRight");
        endCombatAction = InputSystem.actions.FindAction("Combat/EndCombat");

        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();

        drawWeaponAction.Enable();
        sheatheWeaponAction.Enable();
        interactAction.Enable();
        openMenuAction.Enable();
        targetPrevAction.Enable();
        targetNextAction.Enable();

        artUpAction.Enable();
        artLeftAction.Enable();
        artDownAction.Enable();
        artRightAction.Enable();
        endCombatAction.Enable();
    }

    private void Update()
    {
        Debug.Log(openMenuAction.enabled);
    }

    public void ContinueAfterMenu() // Temporary solution
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput())
        {
            var input = moveAction.ReadValue<Vector2>();
            Vector3 move = new Vector3(input.x, 0f, input.y);
            move = Vector3.ClampMagnitude(move, 1);
            return move;
        }

        return Vector3.zero;
    }

    public float GetLookInputsHorizontal()
    {
        if (!CanProcessInput())
            return 0.0f;

        float input = lookAction.ReadValue<Vector2>().x;

        //if (InvertXAxis)
        //    input *= -1;
        //input *= LookSensitivity;

        return input;
    }

    public float GetLookInputsVertical()
    {
        if (!CanProcessInput())
            return 0.0f;

        float input = lookAction.ReadValue<Vector2>().y;

        //if (InvertYAxis)
        //    input *= -1;
        //input *= LookSensitivity;

        return input;
    }


    public bool GetJumpInputDown()
    {
        if (CanProcessInput())
            return jumpAction.WasPressedThisFrame();
        return false;
    }

    public bool GetTargetPrevInputDown()
    {
        if (CanProcessInput())
            return targetPrevAction.WasPressedThisFrame();
        return false;
    }

    public bool GetTargetNextInputDown()
    {
        if (CanProcessInput())
            return targetNextAction.WasPressedThisFrame();
        return false;
    }

    public bool GetDrawWeaponInputDown()
    {
        if (CanProcessInput())
            return drawWeaponAction.WasPressedThisFrame();
        return false;
    }

    public bool GetSheatheWeaponInputDown()
    {
        if (CanProcessInput())
            return sheatheWeaponAction.WasPressedThisFrame();
        return false;
    }

    public bool GetInteractInputDown()
    {
        if (CanProcessInput())
            return interactAction.WasPressedThisFrame();
        return false;
    }

    public bool GetOpenMenuInputDown()
    {
        if (CanProcessInput())
            return openMenuAction.WasPressedThisFrame();
        return false;
    }

    public bool GetArtUpInputDown()
    {
        if (CanProcessInput())
            return artUpAction.WasPressedThisFrame();
        return false;
    }

    public bool GetArtLeftInputDown()
    {
        if (CanProcessInput())
            return artLeftAction.WasPressedThisFrame();
        return false;
    }

    public bool GetArtDownInputDown()
    {
        if (CanProcessInput())
            return artDownAction.WasPressedThisFrame();
        return false;
    }

    public bool GetArtRightInputDown()
    {
        if (CanProcessInput())
            return artRightAction.WasPressedThisFrame();
        return false;
    }

    public bool GetEndCombatInputDown()
    {
        if (CanProcessInput())
            return endCombatAction.WasPressedThisFrame();
        return false;
    }
}
