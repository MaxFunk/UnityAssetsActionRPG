using UnityEngine;

public class ReviveCirle : MonoBehaviour
{
    public float ReviveRadius = 1.5f;
    public float RevivalAfter = 5f;
    public float timeUntilRevival = 0f;

    private CombatData heroToBeRevived;
    private bool circleActive = false;
    private bool isReviving = false;


    void Update()
    {
        if (heroToBeRevived.IsDefeated() == false) // failsafe deletion (currently also to clear on combat end)
        {
            Destroy(gameObject);
        }

        if (isReviving == false)
        {
            timeUntilRevival = RevivalAfter;
        }

        if (circleActive)
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, ReviveRadius, Vector3.up, 0.5f, -1, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if (IsHitValid(hit.collider))
                {
                    isReviving = true;
                    timeUntilRevival -= Time.deltaTime;
                    break;
                }
                else
                {
                    isReviving = false;
                }
            }

            if (timeUntilRevival <= 0f)
            {
                ReviveHero();
            }
        }
    }


    public void LinkDefeatedHero(CombatData defeatedHero)
    {
        heroToBeRevived = defeatedHero;
        timeUntilRevival = RevivalAfter;
        circleActive = true;
    }

    public void ReviveHero()
    {
        heroToBeRevived.Revive();
        circleActive = false;
        Destroy(gameObject);
    }


    private bool IsHitValid(Collider collider)
    {
        var colliderCombatData = collider.GetComponent<CombatData>();
        if (colliderCombatData == null || colliderCombatData.isHero == false || colliderCombatData == heroToBeRevived || colliderCombatData.IsDefeated())
            return false;
        return true;
    }
}
