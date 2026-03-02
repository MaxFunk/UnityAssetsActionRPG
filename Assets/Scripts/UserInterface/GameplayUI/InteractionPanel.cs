using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class InteractionPanel
{
    private VisualElement panelRoot;
    private Label labelInteraction;

    public void FetchVisualElements(VisualElement root)
    {
        panelRoot = root.Q<VisualElement>("InteractionContainer");
        labelInteraction = panelRoot.Q<Label>("LabelInteraction");
        panelRoot.style.visibility = Visibility.Hidden;
    }

    public void UpdateVisualElements(Interactable interactable) 
    {
        if (interactable != null && interactable.CanInteract())
        {
            labelInteraction.text = interactable.InteractionText;
            panelRoot.style.visibility = Visibility.Visible;
        }
        else
        {
            panelRoot.style.visibility = Visibility.Hidden;
        }
    }
}
