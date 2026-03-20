using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController), typeof(CombatData), typeof(NavMeshAgent))]
public class HeroCharacterController : MonoBehaviour
{
    public enum CharacterState
    {
        Stopped,
        Explore,
        CombatInactive,
        CombatActive
    }

    //[Header("References and Prefabs")]
    [Header("Movement and Control")]
    public float MaxSpeedOnGround = 8f;
    public float MovementFriction = 8f;
    public float RotationSpeed = 10f; // make rot speed for camera as different param
    public float JumpStrength = 5f;
    public float Gravity = -9.81f;
    public float SlideSpeed = 3f;
    public float MaxTargetRange = 10f;

    [Header("Navigation Parameter")]
    public float MaxDistanceToPlayer = 3f;
    public float PathUpdateAfter = 0.5f;
    public float DistanceTeleport = 50f;

    [Header("Other")]
    public bool IsPlayerControlled = false;
    public int partyIndex = -1;

    CharacterController controller;
    CombatData combatData;
    NavMeshAgent navAgent;

    InputHandler inputHandler;
    Animator animator;
    Animator animatorWeapon;
    SkinnedMeshRenderer meshWeapon;
    HeroCharacterController playerChar;
    CameraController playerCamera;

    public CharacterState characterState = CharacterState.Explore;
    public Vector3 characterVelocity = Vector3.zero;
    public Vector3 groundNormal = Vector3.up;
    private Vector3 lastPosition;

    private bool isJumping = false;
    private bool isSprinting = false;
    private bool isGrounded = false;
    private bool coyoteTimeAvailable = true;
    private bool wasLastActionArt = true;

    private float timerLookAtTarget = 0.0f;
    private float timerPathUpdate = 0.5f;
    private float timerCoyoteTime = 0.0f;

    public bool DebugCanMove = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        combatData = GetComponent<CombatData>();
        navAgent = GetComponent<NavMeshAgent>();
        inputHandler = InputHandler.instance;

        navAgent.isStopped = true;
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;
        navAgent.nextPosition = transform.position;
        lastPosition = transform.position;
    }

    void Update()
    {
        CheckFallenOutOfWorld();
        CheckCoyoteTime();

        if (characterState != CharacterState.Stopped)
        {
            if (IsPlayerControlled)
                UpdateAsPlayer();
            else
                UpdateAsAlly();
        }

        var moveSpeed = Vector3.Distance(lastPosition, transform.position) / Time.deltaTime;
        lastPosition = transform.position;

        isGrounded = controller.isGrounded;
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("MoveSpeed", moveSpeed);

        if (isGrounded && characterVelocity.y < 0f)
            characterVelocity.y = -1f;

        DebugCanMove = combatData.CanMove();
    }

    void LateUpdate()
    {
        if (!IsPlayerControlled)
        {
            if (playerChar)
                isSprinting = playerChar.isSprinting;

            if (characterState == CharacterState.Explore || characterState == CharacterState.CombatInactive)
            {
                float distanceToPlayer = Vector3.Distance(playerChar.transform.position, transform.position);
                if (distanceToPlayer > DistanceTeleport)
                {
                    transform.position = playerChar.transform.position;
                    navAgent.nextPosition = transform.position;
                }
            }
        }
    }


    private void UpdateAsPlayer()
    {
        Vector3 moveDirection;
        float moveSpeedModifier = 1f;
        bool moveSkipRotation = false;

        if (characterState == CharacterState.Explore)
        {
            if (inputHandler.GetJumpInputDown())
                TryJump();

            if (inputHandler.GetSprintToggleInputDown())
                isSprinting = !isSprinting;

            if (isGrounded) // maybe also check these with coyote time
            {
                if (inputHandler.GetOpenMenuInputDown())
                {
                    UserInterfaceManager.instance.CreateMainMenu();
                    return;
                }

                if (CheckForInteractables()) return;

                if (inputHandler.GetSheatheWeaponInputDown())
                    combatData.SetNewTarget(null);

                if (inputHandler.GetDrawWeaponInputDown() && combatData.GetCurrentTarget() != null)
                    DrawWeapon();
            }
        }

        else if (characterState == CharacterState.CombatInactive)
        {
            if (inputHandler.GetJumpInputDown())
                TryJump();

            if (inputHandler.GetSprintToggleInputDown())
                isSprinting = !isSprinting;

            if (isGrounded)
            {
                if (inputHandler.GetSheatheWeaponInputDown())
                    SheatheWeapon();

                if (inputHandler.GetDrawWeaponInputDown() && combatData.GetCurrentTarget() != null)
                    DrawWeapon();
            }
                
        }

        else if (characterState == CharacterState.CombatActive)
        {
            moveSpeedModifier = 0.5f;
            moveSkipRotation = true;

            LookAtTarget();

            if (inputHandler.GetAlly1UltInputDown())
                CallAllyUlt(1);
            if (inputHandler.GetAlly2UltInputDown())
                CallAllyUlt(2);
            if (inputHandler.GetFocusAlliesInputDown())
                CombatManager.Instance.FocusAlliesOnCurrentTarget(combatData.GetCurrentTarget());

            if (combatData.CanPerformAutoAttack())
            {
                combatData.StartAutoAttack();
                animator.SetTrigger("TrBasicAttack");
                animatorWeapon.SetTrigger("TrBasicAttack");
            }

            if (combatData.CanPerformArtCast())
            {
                bool artSuccess = false;
                if (inputHandler.GetArt1InputDown())
                    artSuccess = combatData.CastArt(0);
                if (inputHandler.GetArt2InputDown() && !artSuccess)
                    artSuccess = combatData.CastArt(1);
                if (inputHandler.GetArt3InputDown() && !artSuccess)
                    artSuccess = combatData.CastArt(2);
                if (inputHandler.GetArt4InputDown() && !artSuccess)
                    artSuccess = combatData.CastArt(3);
                if (inputHandler.GetArt5InputDown() && !artSuccess)
                    artSuccess = combatData.CastArt(4);
                if (inputHandler.GetArtUltInputDown() && !artSuccess)
                    artSuccess = combatData.CastArt(5);

                if (artSuccess)
                {
                    var triggerString = combatData.GetCurrentArt().IsPhysicalArt() ? "TrArtPhysical" : "TrArtEther";
                    animator.SetTrigger(triggerString);
                    animatorWeapon.SetTrigger(triggerString);
                }
            }

            if (inputHandler.GetSheatheWeaponInputDown() && combatData.CanMove())
                SheatheWeapon();
        }


        if (inputHandler.GetTargetNextInputDown())
            GetNewTarget(1);
        else if (inputHandler.GetTargetPrevInputDown())
            GetNewTarget(-1);

        if (inputHandler.GetSwitchCharacterInputDown() && !combatData.IsDefeated() && characterState != CharacterState.CombatInactive)
        {
            GameManager.Instance.SwitchToNextHero();
        }

        if (timerLookAtTarget > 0 && combatData.GetCurrentTarget() != null)
        {
            timerLookAtTarget -= Time.deltaTime;
            var direction = combatData.GetCurrentTarget().transform.position - transform.position;
            playerCamera.RotateHorizontalWithSlerp(direction);
            // vertical focus?
        }
        else
        {
            playerCamera.RotateHorizontal(inputHandler.GetLookInputsHorizontal());
            playerCamera.RotateVertical(inputHandler.GetLookInputsVertical());
        }

        if (combatData.CanMove())
        {
            moveDirection = playerCamera.transform.TransformVector(inputHandler.GetMoveInput());
            moveDirection.y = 0;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        MoveCharacter(moveDirection, moveSpeedModifier, moveSkipRotation);
        playerCamera.SetDesiredPosition(transform.position);
    }

    private void UpdateAsAlly()
    {
        Vector3 moveDirection = Vector3.zero;
        bool moveSkipRotation = false;
        float moveSpeedModifier = 1f;
        var combatTarget = combatData.GetCurrentTarget();

        // Check queued characterStateChanges
        /*if (combatData.CanMove())
        {

        }*/

        // Nav Path Update
        timerPathUpdate -= Time.deltaTime;
        if (timerPathUpdate < 0f && !navAgent.isOnOffMeshLink)
        {
            timerPathUpdate = PathUpdateAfter;
            if (characterState == CharacterState.Explore || characterState == CharacterState.CombatInactive || combatTarget == null)
                SetPathToPlayer();
            else if (characterState == CharacterState.CombatActive)
                SetPathToTarget(combatTarget);
        }

        if (characterState == CharacterState.Explore || characterState == CharacterState.CombatInactive)
        {
            float distanceToPlayer = Vector3.Distance(playerChar.transform.position, transform.position);
            if (navAgent.path.corners.Length > 1 && distanceToPlayer > MaxDistanceToPlayer)
                moveDirection = navAgent.path.corners[1] - transform.position;
        }

        else if (characterState == CharacterState.CombatActive && combatTarget != null)
        {
            float distanceToTarget = Vector3.Distance(combatTarget.transform.position, transform.position);

            moveSpeedModifier = 0.5f;
            moveSkipRotation = true;
            LookAtTarget();

            // Check if in range of target, else run towards
            if (distanceToTarget < combatData.GetAutoAttackRange())
            {
                if (combatData.CanPerformAutoAttack())
                {
                    combatData.StartAutoAttack();
                    wasLastActionArt = false;
                    animator.SetTrigger("TrBasicAttack");
                    animatorWeapon.SetTrigger("TrBasicAttack");
                }

                if (combatData.CanPerformArtCastNPC() && wasLastActionArt == false)
                {
                    int artIndex = combatData.TryCastingUlt ? 5 : Random.Range(0, 4);
                    bool artSuccess = combatData.CastArt(artIndex);

                    if (artSuccess)
                    {
                        var triggerString = combatData.GetCurrentArt().IsPhysicalArt() ? "TrArtPhysical" : "TrArtEther";
                        animator.SetTrigger(triggerString);
                        animatorWeapon.SetTrigger(triggerString);
                        wasLastActionArt = true;

                        if (combatData.TryCastingUlt)
                            combatData.TryCastingUlt = false;
                    }
                }
            }
            else
            {
                if (navAgent.path.corners.Length > 1)
                    moveDirection = navAgent.path.corners[1] - transform.position;
            }
        }


        if (navAgent.isOnOffMeshLink)
        {
            var startPos = navAgent.currentOffMeshLinkData.startPos;
            var endPos = navAgent.currentOffMeshLinkData.endPos;

            if (startPos.y < endPos.y)
                TryJump();

            navAgent.CompleteOffMeshLink();
            timerPathUpdate = -1f;
        }

        if (combatData.CanMove())
            moveDirection.y = 0;
        else
            moveDirection = Vector3.zero;

        MoveCharacter(moveDirection, moveSpeedModifier, moveSkipRotation);
        navAgent.nextPosition = transform.position;
    }


    /* Checks performed every frame */

    private void CheckFallenOutOfWorld()
    {
        if (transform.position.y < -25f)
        {
            if (IsPlayerControlled)
                GameManager.Instance.FindSpawnAndRespawn();
            else
                transform.position = playerChar.transform.position + Vector3.one; // Not failsafe!
        }
    }

    private void CheckCoyoteTime()
    {
        if (timerCoyoteTime > 0f)
            timerCoyoteTime -= Time.deltaTime;

        if (isGrounded)
        {
            timerCoyoteTime = 0f;
            isJumping = false;
            coyoteTimeAvailable = true;
        }
        else if (!isGrounded && coyoteTimeAvailable)
        {
            timerCoyoteTime = 0.25f;
            coyoteTimeAvailable = false;
        }
    }

    private bool CheckForInteractables()
    {
        bool hasInteracted = false;
        var center = transform.position + Vector3.up * 0.5f + transform.forward * 0.3f;
        int mask = LayerMask.GetMask("Interactables", "NPCs");

        var hits = Physics.RaycastAll(center, transform.forward, 3f, mask, QueryTriggerInteraction.Ignore);
        Debug.DrawRay(center, transform.forward * 3f, hits.Length > 0 ? Color.green : Color.red);

        Interactable interactable = null;
        if (hits.Length > 0)
        {
            interactable = hits[0].collider.GetComponent<Interactable>();// only first hit
            if (inputHandler.GetInteractInputDown() && interactable != null)
            {
                interactable.Interaction();
                hasInteracted = true;
            }
                
        }
        UserInterfaceManager.instance.GameplayUI.UpdateInteractionPanel(interactable);

        return hasInteracted;
    }


    /* Movement Handling functions used by both player and ally characters */

    private void MoveCharacter(Vector3 moveDirection, float speedModifier, bool skipRotation = false)
    {
        if (moveDirection.sqrMagnitude < 0.01f)
            moveDirection = Vector3.zero;
        else
            moveDirection.Normalize();

        if (moveDirection != Vector3.zero && !skipRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }

        if (isSprinting)
            speedModifier *= 1.4f;
        var targetVelocity = MaxSpeedOnGround * speedModifier * combatData.GetMoveSpeedFactor() * moveDirection;
        var characterVelocityXZ = new Vector3(characterVelocity.x, 0, characterVelocity.z);
        characterVelocityXZ = Vector3.Lerp(characterVelocityXZ, targetVelocity, MovementFriction * Time.deltaTime);
        characterVelocity.y += Gravity * Time.deltaTime;
        var slideVelocity = Time.deltaTime * SlideSpeed * GetSlideDir();

        characterVelocity = characterVelocityXZ + (characterVelocity.y * Vector3.up) + slideVelocity;
        controller.Move(characterVelocity * Time.deltaTime);
    }

    private Vector3 GetSlideDir()
    {
        if (!isGrounded) return Vector3.zero;

        Vector3 slideDir = Vector3.zero;
        var res = Physics.SphereCast(transform.position + new Vector3(0f, 0.4f, 0f), controller.radius * 0.9f, Vector3.down, out var hit, 0.5f);
        //Debug.DrawRay(transform.position + new Vector3(0f, 0.4f, 0f), Vector3.down * 0.5f, res ? Color.white : Color.darkRed);
        if (res)
        {
            var slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            groundNormal = hit.normal;

            if (slopeAngle > controller.slopeLimit)
            {
                slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                //Debug.DrawRay(transform.position + new Vector3(0f, 0.4f, 0f), slideDir, Color.darkRed);
            }
        }
        else
        {
            groundNormal = Vector3.up;
        }
        return slideDir;
    }


    private void TryJump()
    {
        if (isJumping || (controller.isGrounded == false && timerCoyoteTime <= 0f)) return;

        characterVelocity.y = Mathf.Sqrt(-JumpStrength * Gravity);
        isJumping = true;
        animator.SetTrigger("TrJump");
    }

    private void LookAtTarget()
    {
        var combatTarget = combatData.GetCurrentTarget();
        if (combatTarget != null)
        {
            var direction = combatTarget.transform.position - transform.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }


    /* Navigation Agent Management, used primarily by allies */

    private void SetPathToPlayer()
    {
        navAgent.SetDestination(playerChar.transform.position);
    }

    private void SetPathToTarget(CombatData target)
    {
        navAgent.SetDestination(target.transform.position);
    }


    /* Character Data Management */

    private void GetNewTarget(int dir)
    {
        if (combatData.CanMove() == false)
            return;

        combatData.FetchNewTarget(dir);
        if (combatData.GetCurrentTarget() == null)
            return;

        timerLookAtTarget = 0.5f;
    }

    public void UpdatePartyIndex(int newIndex, HeroCharacterController playerChar)
    {
        partyIndex = newIndex;
        combatData.partyIndex = partyIndex;
        if (partyIndex == 0)
            MakePlayerCharacter();
        else
            MakeAllyCharacter(playerChar);
    }

    public void MakePlayerCharacter()
    {
        GameManager.Instance.PlayerCharacter = this;
        playerChar = this;
        IsPlayerControlled = true;
        combatData.isPlayerControlled = true;
        navAgent.enabled = false;
        inputHandler.SetCursorLocked();
    }

    public void MakeAllyCharacter(HeroCharacterController playerChar)
    {
        this.playerChar = playerChar;
        IsPlayerControlled = false;
        combatData.isPlayerControlled = false;
        navAgent.enabled = true;
    }


    /* Combat functions */

    public void DrawWeapon()
    {
        characterState = CharacterState.CombatActive;
        isSprinting = false;
        meshWeapon.enabled = true;
        animator.SetBool("isInCombat", true);
        animatorWeapon.SetBool("isInCombat", true);

        if (IsPlayerControlled)
        {
            var heros = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
            foreach (var hero in heros)
            {
                if (hero.IsPlayerControlled == false)
                    hero.DrawWeapon();
            }
        }
        else
        {
            combatData.SetNewTarget(playerChar.combatData.GetCurrentTarget());
        }
    }

    public void SheatheWeapon()
    {
        characterState = CombatManager.Instance.CombatActive ? CharacterState.CombatInactive : CharacterState.Explore;
        animator.SetBool("isInCombat", false);
        animatorWeapon.SetBool("isInCombat", false);
        meshWeapon.enabled = false;

        if (IsPlayerControlled)
        {
            var heros = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
            foreach (var hero in heros)
            {
                if (hero.IsPlayerControlled == false)
                    hero.SheatheWeapon();
            }

            if (characterState == CharacterState.Explore)
                combatData.SetNewTarget(null);
        }
        else
        {
            combatData.SetNewTarget(null);
        }
    }

    public void ForceIntoCombat(CombatData forcer, bool hardForce) // hardFroce = false
    {
        characterState = hardForce ? CharacterState.CombatActive : CharacterState.CombatInactive;
        animator.SetBool("isInCombat", hardForce);
        animatorWeapon.SetBool("isInCombat", hardForce);

        if (hardForce || IsPlayerControlled)
        {
            combatData.SetNewTarget(forcer);
        }
    }

    private void CallAllyUlt(int partyIndex)
    {
        if (partyIndex < 1 || partyIndex > 2) return;

        var heroChar = CombatManager.Instance.FindHeroChar(partyIndex);
        if (heroChar != null) 
        {
            heroChar.OnPlayerCallsUltUse();
        }
    }


    /* On something Functions, usually called from outside */

    public void OnModelLoad(int heroId, int partyIndex)
    {
        this.partyIndex = partyIndex;
        combatData.LoadFromCharacterData(GameManager.Instance.characterDatas[heroId]);
        UserInterfaceManager.instance.GameplayUI.OnHeroLoad(combatData, partyIndex);

        var animators = GetComponentsInChildren<Animator>();
        animator = animators[0];
        animatorWeapon = animators[1];
        animator.SetBool("isInCombat", false);
        animatorWeapon.SetBool("isInCombat", false);

        meshWeapon = animatorWeapon.GetComponentInChildren<SkinnedMeshRenderer>();
        meshWeapon.enabled = false;
    }

    public void OnCombatJoin()
    {
        if (characterState != CharacterState.CombatActive)
            characterState = CharacterState.CombatInactive;

        animator.SetFloat("MoveSpeed", 0f); // maybe irrelevant now
        //animator.SetBool("isInCombat", true);

        if (IsPlayerControlled)
            UserInterfaceManager.instance.GameplayUI.UpdateInteractionPanel(null);
    }

    public void OnCombatEnd()
    {
        characterState = CharacterState.Explore;
        animator.SetBool("isInCombat", false);
        animatorWeapon.SetBool("isInCombat", false);
        meshWeapon.enabled = false;
    }

    public void OnDefeat()
    {
        animator.SetTrigger("TrDefeat");
        animatorWeapon.SetTrigger("TrDefeat");
    }

    public void OnRevive()
    {
        animator.SetTrigger("TrRevive");
        animatorWeapon.SetTrigger("TrRevive");
    }

    public void OnPlayerCallsUltUse()
    {
        if (combatData.CanCastUlt() && !combatData.TryCastingUlt)
        {
            combatData.TryCastingUlt = true;
        }
    }

    public void OnDialogStart()
    {
        characterState = CharacterState.Stopped;
    }

    public void OnDialogEnd()
    {
        characterState = CharacterState.Explore;
    }


    /* Get and Set certain private variables */

    public CombatData GetCombatData()
    {
        return combatData;
    }

    public void SetPlayerCamera(CameraController cam)
    {
        playerCamera = cam;
        if (IsPlayerControlled) cam.OnPlayerSpawn(this);
    }
}
