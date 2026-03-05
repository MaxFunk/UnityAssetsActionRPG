using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class Interactable : MonoBehaviour
{
    public int InteractionId = -1;
    public string InteractionText = "Interact with ???";
    public int MaxInteractions = -1;
    public UnityEvent OnInteraction;

    public int interactionsLeft = -1; // make private?

    void Awake()
    {
        interactionsLeft = MaxInteractions;
    }

    public void Interaction()
    {
        if (interactionsLeft != 0)
        {
            OnInteraction?.Invoke();
            GameManager.Instance.MissionManager.PlayerInteracted(InteractionId);

            if (interactionsLeft > 0)
                interactionsLeft -= 1;
        }
    }

    public bool CanInteract()
    {
        return interactionsLeft != 0;
    }
}
