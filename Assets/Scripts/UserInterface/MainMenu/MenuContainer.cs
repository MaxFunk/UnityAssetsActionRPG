using UnityEngine;
using UnityEngine.UIElements;

public abstract class MenuContainer
{
    public VisualElement containerObj = null;
    public MainMenuEvents mainMenuEvents = null;

    public abstract void PrepareView(VisualElement rootElement);

    public abstract void ConfirmEvent();

    public abstract void CancelEvent();

    public abstract void SpecialEvent();

    public abstract void DirectionalEvent(Vector2 navInput);
}
