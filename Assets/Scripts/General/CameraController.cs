using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Camera Camera;
    public Transform CameraArm;

    [Header("Params")]
    public float MoveSpeed = 0.5f;
    public float RotationSpeedVertical = 5f;
    public float RotationSpeedHorizontal = 10f;
    public float RotationSpeedSlerp = 10f;
    public float ArmLength = 5f;

    private Vector3 desiredPos = Vector3.zero;
    float currentPitch = 0f;
    float currentArmLength = 5f;


    private void Awake()
    {
        desiredPos = transform.position;
        currentArmLength = ArmLength;
        UpdateCameraArm();
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPos, MoveSpeed * Time.deltaTime);
        UpdateArmLength();
    }


    public void RotateVertical(float verticalInput)
    {
        float delta = -verticalInput * RotationSpeedVertical * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch + delta, -30f, 60f);
        CameraArm.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    public void RotateHorizontal(float horizontalInput)
    {
        float horizontalRotation = horizontalInput * RotationSpeedHorizontal * Time.deltaTime;
        transform.Rotate(new Vector3(0f, horizontalRotation, 0f), Space.Self);
    }

    public void RotateHorizontalWithSlerp(Vector3 lookDirection)
    {
        lookDirection.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeedSlerp * Time.deltaTime);
    }

    public void SetRotation(Vector3 lookDirection, float angleFromFloor)
    {
        lookDirection.y = 0f;
        transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
        CameraArm.localRotation = Quaternion.Euler(angleFromFloor, 0f, 0f);
        currentPitch = angleFromFloor;
    }

    public void SetDesiredPosition(Vector3 position)
    {
        desiredPos = position;
    }


    public void OnPlayerSpawn(HeroCharacterController playerChar)
    {
        transform.position = playerChar.transform.position;
        desiredPos = transform.position;
        transform.rotation = playerChar.transform.rotation;
    }


    private void UpdateCameraArm()
    {
        if (Camera == null) return;

        var localCamPos = Camera.transform.localPosition;
        localCamPos.z = -currentArmLength;
        Camera.transform.localPosition = localCamPos;
    }

    private void UpdateArmLength()
    {
        int mask = LayerMask.GetMask("Default", "Navigateable");
        var rayDir = (Camera.transform.position - CameraArm.position).normalized;

        if (Physics.Raycast(CameraArm.position, rayDir, out RaycastHit hit, ArmLength, mask))
            currentArmLength = Mathf.Clamp(hit.distance - 0.15f, 0, ArmLength);
        else
            currentArmLength = ArmLength;

        UpdateCameraArm();
    }
}
