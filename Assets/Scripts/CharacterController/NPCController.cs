using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class NPCController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform headBone;
    public DialogData DialogData;
    public SplineContainer pathSpline;

    [Header("NPC Configuration")]
    public bool CanLookAtPlayer = false;
    public bool CanFollowPath = false;
    public bool CanTalk = false;

    [Header("Data")]
    public float MaxAngleHead = 20f;
    public float RotationSpeedHead = 8f;
    public float PathSpeed = 0.02f;
    public UnityEvent[] DialogEvents = new UnityEvent[0];

    private HeroCharacterController playerTarget = null;
    private Vector3 currentLookDir;
    private float pathProgress = 0f; // [0, 1]

    private bool isTalking = false;


    void Awake()
    {
        currentLookDir = transform.forward;
    }

    void Update()
    {
        if (CanFollowPath && !isTalking && pathSpline != null)
        {
            pathProgress += PathSpeed * Time.deltaTime;
            pathProgress %= 1f;

            transform.position = pathSpline.EvaluatePosition(pathProgress);
            Vector3 forward = pathSpline.EvaluateTangent(pathProgress);
            transform.rotation = Quaternion.LookRotation(forward);

            if (animator)
                animator.SetFloat("MoveSpeed", 1f);
        }
        else
        {
            if (animator)
                animator.SetFloat("MoveSpeed", 0f);
        }
    }

    void LateUpdate()
    {
        if (headBone != null && CanLookAtPlayer)
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
    
    public void StartTalk()
    {
        if (!CanTalk) return;

        var dialogUI = UserInterfaceManager.instance.CreateDialogUI();
        if (dialogUI != null)
        {
            isTalking = true;
            dialogUI.DialogBegin(this);
        }
    }

    public void OnEndTalking()
    {
        isTalking = false;
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
