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
    

    private void Awake()
    {
        desiredPos = transform.position;
        UpdateArmLength();
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPos, MoveSpeed * Time.deltaTime);

        UpdateArmLength(); // only do if called outside (when armlength is modified)
    }


    public void RotateVertical(float verticalInput)
    {
        float verticalRotation = -verticalInput * RotationSpeedVertical * Time.deltaTime;
        CameraArm.Rotate(new Vector3(verticalRotation, 0f, 0f), Space.Self);
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
        CameraArm.transform.localRotation = Quaternion.Euler(angleFromFloor, 0f, 0f);
    }

    public void SetDesiredPosition(Vector3 position)
    {
        desiredPos = position;
    }


    public void OnPlayerSpawn(HeroCharacterController playerChar)
    {
        transform.position = playerChar.transform.position;
        desiredPos = transform.position;
    }


    private void UpdateArmLength()
    {
        if (Camera == null) return;

        var localCamPos = Camera.transform.localPosition;
        localCamPos.z = -ArmLength;
        Camera.transform.localPosition = localCamPos;
    }
}
