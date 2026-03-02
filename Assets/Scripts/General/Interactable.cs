using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string InteractionText = "Interact with ???";
    public int MaxInteractions = -1;
    public int interactionsLeft = -1;
    public UnityEvent OnInteraction;

    void Awake()
    {
        interactionsLeft = MaxInteractions;
    }

    public void Interaction()
    {
        if (interactionsLeft != 0)
        {
            OnInteraction?.Invoke();
            if (interactionsLeft > 0)
                interactionsLeft -= 1;
        }
    }

    public bool CanInteract()
    {
        return interactionsLeft != 0;
    }
}
