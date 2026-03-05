using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AreaTrigger : MonoBehaviour
{
    public int areaId = -1;
    public bool deactivateAfterEnter = false;

    private Collider areaCollider;

    void Awake()
    {
        areaCollider = GetComponent<Collider>();
        areaCollider.isTrigger = true;

        areaCollider.enabled = true; // deactivate via check through gamemanager/savedata? -> EventFlags, etc.
    }

    private void OnTriggerEnter(Collider other)
    {
        var hero = other.GetComponent<HeroCharacterController>();
        if (hero != null && hero.IsPlayerControlled)
        {
            GameManager.Instance.MissionManager.AreaWasReached(areaId);
            if (deactivateAfterEnter)
                areaCollider.enabled = false;
        }
    }
}
