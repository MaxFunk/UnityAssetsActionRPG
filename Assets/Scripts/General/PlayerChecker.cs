using UnityEngine;

public class PlayerChecker : MonoBehaviour
{
    public Vector3 BoxCheckDimensions = Vector3.one;
    public Vector3 CenterOffset = Vector3.zero;
    public bool checkActive = true;
    public bool disableAfterSuccess = true;

    void Awake()
    {
        
    }

    void Update()
    {
        
    }

    public bool CheckForPlayerCharacer()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + CenterOffset, BoxCheckDimensions, transform.rotation);
        foreach (Collider collider in colliders)
        {
            var colliderPlayer = collider.GetComponent<HeroCharacterController>();
            if (colliderPlayer != null && colliderPlayer.IsPlayerControlled)
            {
                checkActive = !disableAfterSuccess;
                return true;
            }
        }

        return false;
    }
}
