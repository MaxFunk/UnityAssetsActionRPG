using UnityEngine;

public class DebugGroundChecker : MonoBehaviour
{
    public float checkerRadius = 0.4f;
    public float slopeLimit = 45f;
    public float slopeAngle = 0f;
    public Vector3 groundNormal = Vector3.up;    
    
    void Update()
    {
        //var res = Physics.SphereCast(transform.position, checkerRadius * 0.9f, Vector3.down, out var hit, 1f);
        var res = Physics.Raycast(transform.position, Vector3.down, out var hit, 1f);
        Debug.DrawRay(transform.position, Vector3.down, res ? Color.white : Color.darkRed);
        if (res)
        {
            slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            groundNormal = hit.normal;

            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
            Debug.DrawRay(transform.position, slideDir, Color.darkRed);
        }
        else
        {
            slopeAngle = 0f;
            groundNormal = Vector3.up;
        }
    }
}
