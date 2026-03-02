using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;

public class TalkableNPC : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform headBone;
    public DialogData DialogData;

    [Header("Data")]
    public float MaxAngleHead = 20f;
    public float RotationSpeedHead = 8f;

    private HeroCharacterController playerTarget = null;
    private Vector3 currentLookDir;


    void Awake()
    {
        currentLookDir = transform.forward;
    }

    void LateUpdate()
    {
        if (headBone != null)
        {
            Vector3 desiredLookDir;

            if (playerTarget != null)
            {
                desiredLookDir = (playerTarget.transform.position - headBone.position);
                desiredLookDir.y = 0;
                desiredLookDir.Normalize();
            }
            else
            {
                desiredLookDir = transform.forward;
            }

            float angleHead = Vector3.Angle(transform.forward, desiredLookDir);
            if (angleHead > MaxAngleHead)
            {
                float t = MaxAngleHead / angleHead;
                desiredLookDir = Vector3.Slerp(transform.forward, desiredLookDir, t);
            }

            currentLookDir = Vector3.Slerp(currentLookDir, desiredLookDir, RotationSpeedHead * Time.deltaTime);
            headBone.rotation = Quaternion.LookRotation(currentLookDir);
        }
    }
    
    public void StartTalkNPC()
    {
        var dialogUI = UserInterfaceManager.instance.CreateDialogUI();
        if (dialogUI != null)
        {
            dialogUI.DialogBegin(this);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        var heroChar = other.GetComponent<HeroCharacterController>();
        if (heroChar != null && heroChar.IsPlayerControlled == true)
        {
            playerTarget = heroChar;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var heroChar = other.GetComponent<HeroCharacterController>();
        if (heroChar != null && heroChar.IsPlayerControlled == true)
        {
            playerTarget = null;
        }
    }
}
