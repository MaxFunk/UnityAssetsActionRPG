using UnityEngine;

public class PlayerChecker : MonoBehaviour
{
    public Vector3 BoxCheckDimensions = Vector3.one;
    public Vector3 CenterOffset = Vector3.zero;
    public bool checkActive = true;
    public bool disableAfterSuccess = true;
    public bool allowInCombat = false;

    public bool CheckForPlayerCharacer()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + CenterOffset, BoxCheckDimensions, transform.rotation);
        foreach (Collider collider in colliders)
        {
            var colliderPlayer = collider.GetComponent<HeroCharacterController>();
            if (colliderPlayer != null && colliderPlayer.IsPlayerControlled)
            {
                var comData = colliderPlayer.GetCombatData();
                if ((comData.isInCombat && allowInCombat) || !comData.isInCombat)
                {
                    checkActive = !disableAfterSuccess;
                    return true;
                }
            }
        }

        return false;
    }
}
