using UnityEngine;

public class ListenerRuralTown : MonoBehaviour
{
    public Transform npc = null;

    void Awake()
    {
        CheckCondition();
    }

    void Update()
    {
        CheckCondition();
    }

    private void CheckCondition()
    {
        if (npc == null)
        {
            Destroy(gameObject);
            return;
        }

        if (GameManager.Instance.EventFlags.GetFlag(EventFlags.EventFlag.InstructorDefeated))
        {
            npc.position = transform.position;
            Destroy(gameObject);
        }
    }
}
