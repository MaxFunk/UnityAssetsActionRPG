using UnityEngine;
using UnityEngine.InputSystem;

public class UserInterfaceInputHandler : MonoBehaviour
{
    private InputAction navigateAction;
    private InputAction clickAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        clickAction = InputSystem.actions.FindAction("UI/Click");
        submitAction = InputSystem.actions.FindAction("UI/Submit");
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");

        navigateAction.Enable();
        clickAction.Enable();
        submitAction.Enable();
        cancelAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Confined;
    }

    public Vector2 GetNavigateInput()
    {
        if (CanProcessInput())
            return navigateAction.ReadValue<Vector2>();

        return Vector2.zero;
    }

    public bool GetClickInputDown()
    {
        if (CanProcessInput())
            return clickAction.WasPressedThisFrame();
        return false;
    }

    public bool GetSubmitInputDown()
    {
        if (CanProcessInput())
            return submitAction.WasPressedThisFrame();
        return false;
    }

    public bool GetCancelInputDown()
    {
        if (CanProcessInput())
            return cancelAction.WasPressedThisFrame();
        return false;
    }
}
