using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController), typeof(CombatData), typeof(NavMeshAgent))]
public class HeroCharacterControllerOld : MonoBehaviour
{
    public enum CharacterState
    {
        Stopped,
        Explore,
        CombatInactive,
        CombatActive,
        InDialog
    }
    public enum NavState
    {
        PlayerControlled = 0,
        Idle = 1,
        Following = 2,
        JumpLink = 3,
        Combat = 9
    }

    //[Header("References and Prefabs")]
    [Header("Movement and Control")]
    public float MaxSpeedOnGround = 8f;
    public float MovementFriction = 8f;
    public float RotationSpeed = 10f; // make rot speed for camera as different param
    public float JumpStrength = 5f;
    public float Gravity = -9.81f;
    public float MaxTargetRange = 10f;
    [Header("Navigation Parameter")]
    public float MaxDistanceToPlayerStart = 3f;
    public float MaxDistanceToPlayerEnd = 1f;
    public float MaxDistanceToTarget = 3f;
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
    HeroCharacterController playerChar;
    CameraController playerCamera;

    public CharacterState characterState = CharacterState.Explore;
    public NavState navState = NavState.PlayerControlled;
    private Vector3 characterVelocity = Vector3.zero;
    private Vector3 lastPosition;

    private bool isJumping = true;
    private bool wasLastActionArt = true;
    private float timerLookAtTarget = 0.0f;
    private float timerPathUpdate = 0.5f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        combatData = GetComponent<CombatData>();
        navAgent = GetComponent<NavMeshAgent>();
        inputHandler = InputHandler.instance;

        navAgent.isStopped = true;
        //navAgent.stoppingDistance = MaxDistanceToPlayer - navAgent.radius;
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;

        lastPosition = gameObject.transform.position;
    }

    void Update()
    {
        CheckFallenOutOfWorld();

        switch (characterState)
        {
            case CharacterState.Stopped:
                return;
            case CharacterState.Explore:
                if (IsPlayerControlled)
                    UpdateExplorationPlayer();
                else
                    UpdateExplorationAlly();
                break;
            case CharacterState.CombatInactive:
            case CharacterState.CombatActive:
                if (IsPlayerControlled)
                    UpdateCombatPlayer();
                else
                    UpdateCombatAlly();
                break;
            default:
                break;
        }

        var moveSpeed = Vector3.Distance(lastPosition, transform.position) / Time.deltaTime;
        lastPosition = transform.position;
        //wasCharacterGrounded = controller.isGrounded;

        animator.SetBool("isGrounded", controller.isGrounded);
        animator.SetFloat("MoveSpeed", moveSpeed);
    }

    private void UpdateExplorationPlayer()
    {
        if (controller.isGrounded)
        {
            if (inputHandler.GetSwitchCharacterInputDown())
            {
                GameManager.Instance.SwitchToNextHero();
                return;
            }

            if (inputHandler.GetOpenMenuInputDown())
            {
                UserInterfaceManager.instance.CreateMainMenu();
                //characterState = CharacterState.Stopped; // stop every other object update calls -> look what unity has
                return;
            }

            var res = CheckForInteractables();
            if (res) return; // maybe remove here and move to end of function?

            if (characterVelocity.y < 0)
                characterVelocity.y = 0f;

            if (inputHandler.GetDrawWeaponInputDown() && combatData.GetCurrentTarget() != null)
            {
                DrawWeapon();
                return;
            }

            if (inputHandler.GetSheatheWeaponInputDown())
                combatData.SetNewTarget(null);

            if (inputHandler.GetJumpInputDown())
                Jump();
        }

        if (inputHandler.GetTargetNextInputDown())
            GetNewTarget(1);
        else if (inputHandler.GetTargetPrevInputDown())
            GetNewTarget(-1);

        if (timerLookAtTarget > 0 && combatData.GetCurrentTarget() != null)
        {
            timerLookAtTarget -= Time.deltaTime;
            var direction = combatData.GetCurrentTarget().transform.position - transform.position;
            playerCamera.RotateHorizontalWithSlerp(direction);
        }
        else
            playerCamera.RotateHorizontal(inputHandler.GetLookInputsHorizontal());
        playerCamera.RotateVertical(inputHandler.GetLookInputsVertical());
                       
        // Movement Input
        var moveDirection = playerCamera.transform.TransformVector(inputHandler.GetMoveInput());
        if (combatData.CanMove() == false)
            moveDirection = Vector3.zero;
        else
            moveDirection.y = 0f;
        MoveCharacter(moveDirection.normalized, 1f);
        playerCamera.SetDesiredPosition(transform.position);
    }

    private void UpdateCombatPlayer()
    {
        if (characterState == CharacterState.CombatActive)
        {
            if (inputHandler.GetSheatheWeaponInputDown())
            {
                SheatheWeapon();
                return;
            }

            if (inputHandler.GetSwitchCharacterInputDown() && combatData.isInCombat == true && !combatData.IsDefeated())
            {
                GameManager.Instance.SwitchToNextHero();
                return;
            }

            if (inputHandler.GetTargetNextInputDown())
                GetNewTarget(1);
            else if (inputHandler.GetTargetPrevInputDown())
                GetNewTarget(-1);

            if (inputHandler.GetAlly1UltInputDown())
                CallAllyUlt(1);
            if (inputHandler.GetAlly2UltInputDown())
                CallAllyUlt(2);

            if (combatData.CanPerformAutoAttack())
            {
                combatData.PerformAutoAttack();
                animator.SetTrigger("TrBasicAttack");
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
                    animator.SetTrigger("TrArt");
            }
        }

        if (characterState == CharacterState.CombatInactive)
        {
            if (inputHandler.GetDrawWeaponInputDown() && combatData.GetCurrentTarget() != null)
            {
                DrawWeapon();
            }
        }

        playerCamera.RotateHorizontal(inputHandler.GetLookInputsHorizontal());
        playerCamera.RotateVertical(inputHandler.GetLookInputsVertical());

        var combatTarget = combatData.GetCurrentTarget();
        var moveDirection = playerCamera.transform.TransformVector(inputHandler.GetMoveInput());
        if (combatData.CanMove() == false)
            moveDirection = Vector3.zero;

        if (combatTarget != null && characterState == CharacterState.CombatActive)
        {
            var direction = (combatTarget.transform.position - transform.position).normalized;
            direction = new Vector3(direction.x, 0, direction.z).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }

        moveDirection.y = 0f;
        MoveCharacter(moveDirection.normalized, 0.5f, characterState == CharacterState.CombatActive);
        playerCamera.SetDesiredPosition(transform.position);
    }

    private void UpdateExplorationAlly()
    {
        Vector3 moveDirection = Vector3.zero;
        var distanceToPlayer = Vector3.Distance(playerChar.transform.position, transform.position);

        if (distanceToPlayer > DistanceTeleport)
        {
            transform.position = playerChar.transform.position;
            navAgent.nextPosition = transform.position;
            navState = NavState.Idle;
            return;
        }

        if (navState == NavState.Idle)
        {
            if (playerChar == null || playerChar == this)
                return;

            if (distanceToPlayer > MaxDistanceToPlayerStart)
                SetPathToPlayer();
        }

        if (navState == NavState.Following)
        {
            timerPathUpdate -= Time.deltaTime;
            if (timerPathUpdate < 0f)
            {
                timerPathUpdate = PathUpdateAfter;
                SetPathToPlayer();
            }

            if (distanceToPlayer < MaxDistanceToPlayerEnd)
            {
                navState = NavState.Idle;
                return;
            }

            if (navAgent.isOnOffMeshLink)
            {
                navState = NavState.JumpLink;
            }

            // movement agent
            if (navAgent.path.corners.Length > 1)
            {
                Vector3 nextCorner = navAgent.path.corners[1];
                moveDirection = nextCorner - transform.position;
                moveDirection.y = 0f;
            }
        }

        if (navState == NavState.JumpLink)
        {
            var startPos = navAgent.currentOffMeshLinkData.startPos;
            var endPos = navAgent.currentOffMeshLinkData.endPos;

            if (Vector3.Distance(endPos, transform.position) < 0.1)
            {
                navAgent.CompleteOffMeshLink();
                navState = NavState.Idle;
            }
            else
            {
                if (!isJumping && startPos.y < endPos.y)
                {
                    Jump();
                    isJumping = true;
                }

                moveDirection = endPos - transform.position;
                moveDirection.y = 0f;
            }
        }

        if (combatData.CanMove() == false)
            moveDirection = Vector3.zero;
        MoveCharacter(moveDirection, 1f);
        navAgent.nextPosition = transform.position;

        if (controller.isGrounded)
        {
            characterVelocity.y = 0;
            isJumping = false;
        }
    }

    private void UpdateCombatAlly()
    {
        Vector3 moveDirection = Vector3.zero;
        var combatTarget = combatData.GetCurrentTarget();
        bool skipRotation = false;

        if (combatTarget != null)
        {
            var distToTarget = Vector3.Distance(transform.position, combatTarget.transform.position);
            navState = distToTarget < MaxDistanceToTarget ? NavState.Combat : NavState.Following;
        }
        else
        {
            navState = NavState.Following;
        }

        if (navState == NavState.Following)
        {
            timerPathUpdate -= Time.deltaTime;
            if (timerPathUpdate < 0f)
            {
                timerPathUpdate = PathUpdateAfter;
                if (combatTarget != null)
                    SetPathToTarget(combatTarget);
                else
                    SetPathToPlayer();
            }

            if (navAgent.path.corners.Length > 1)
            {
                Vector3 nextCorner = navAgent.path.corners[1];
                moveDirection = nextCorner - transform.position;
                moveDirection.y = 0f;
            }
        }

        if (navState == NavState.Combat)
        {
            if (combatTarget != null)
            {
                var lookDirection = (combatTarget.transform.position - transform.position);
                lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
                skipRotation = true;
            }

            if (combatData.CanPerformAutoAttack())
            {
                combatData.PerformAutoAttack();
                wasLastActionArt = false;
                animator.SetTrigger("TrBasicAttack");
            }

            if (combatData.CanPerformArtCastNPC() && wasLastActionArt == false)
            {
                int artIndex = combatData.tryCastingUlt ? 5 : Random.Range(0, 4);
                bool artSuccess = combatData.CastArt(artIndex);
                    
                if (artSuccess)
                {
                    animator.SetTrigger("TrArt");
                    wasLastActionArt = true;

                    if (combatData.tryCastingUlt)
                        combatData.tryCastingUlt = false;
                }
            }
        }

        MoveCharacter(moveDirection, 0.5f, skipRotation);
        navAgent.nextPosition = transform.position;

        if (controller.isGrounded)
        {
            characterVelocity.y = 0;
        }
    }


    private void CheckFallenOutOfWorld()
    {
        if (transform.position.y < -25f)
        {
            if (IsPlayerControlled)
            {
                GameManager.Instance.FindSpawnAndRespawn();
            }
            else
            {
                transform.position = playerChar.transform.position; // Not failsafe!
            }
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


    private void MoveCharacter(Vector3 moveDirection, float speedModifier, bool skipRotation = false)
    {
        if (moveDirection.sqrMagnitude < 0.01f)
            moveDirection = Vector3.zero;
        else
            moveDirection.Normalize();

        if (moveDirection != Vector3.zero && !skipRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        var targetVelocity = MaxSpeedOnGround * speedModifier * moveDirection;
        var characterVelocityXZ = new Vector3(characterVelocity.x, 0, characterVelocity.z);
        characterVelocityXZ = Vector3.Lerp(characterVelocityXZ, targetVelocity, MovementFriction * Time.deltaTime);
        characterVelocity.y += Gravity * Time.deltaTime;

        var finalVelocity = characterVelocityXZ + (characterVelocity.y * Vector3.up);
        characterVelocity = finalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }


    private void Jump()
    {
        animator.SetTrigger("TrJump");
        characterVelocity.y = Mathf.Sqrt(-JumpStrength * Gravity);
    }

    private void GetNewTarget(int dir)
    {
        if (combatData.CanMove() == false)
            return;

        combatData.FetchNewTarget(dir);
        if (combatData.GetCurrentTarget() == null)
            return;

        timerLookAtTarget = 0.5f;
    }

    private void SetPathToPlayer()
    {
        navState = NavState.Following;
        navAgent.SetDestination(playerChar.transform.position);
    }

    private void SetPathToTarget(CombatData target)
    {
        if (target == null || target.IsDefeated())
        {
            navAgent.SetDestination(transform.position);
            navState = NavState.Idle;
            return;
        }

        navState = NavState.Following;
        navAgent.SetDestination(target.transform.position);
    }


    public void UpdatePartyIndex(int newIndex, HeroCharacterController playerChar)
    {
        partyIndex = newIndex;
        combatData.partyIndex = partyIndex;
        if (partyIndex == 0) 
        {
            MakePlayerCharacter();
        }
        else
        {
            MakeAllyCharacter(playerChar);
        }
            
    }

    public void MakePlayerCharacter()
    {
        //GameManager.Instance.PlayerCharacter = this;
        //playerChar = this;
        IsPlayerControlled = true;
        combatData.isPlayerControlled = true;

        navState = NavState.PlayerControlled;
        navAgent.enabled = false;

        inputHandler.SetCursorLocked();
    }

    public void MakeAllyCharacter(HeroCharacterController playerChar)
    {
        this.playerChar = playerChar;
        IsPlayerControlled = false;
        combatData.isPlayerControlled = false;

        navState = NavState.Idle;
        navAgent.enabled = true;
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

    public void DrawWeapon()
    {
        characterState = CharacterState.CombatActive;
        animator.SetBool("isInCombat", true);

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
            //combatData.SetNewTarget(playerChar.combatData.GetCurrentTarget());
        }
    }

    public void SheatheWeapon()
    {
        characterState = CharacterState.CombatInactive;
        animator.SetBool("isInCombat", false);

        if (IsPlayerControlled)
        {
            var heros = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
            foreach (var hero in heros)
            {
                if (hero.IsPlayerControlled == false)
                    hero.SheatheWeapon();
            }

            /*if (combatData.isInCombat == true && !combatData.IsDefeated())
            {
                combatData.InitiateEarlyCombatEnd();
                return true;
            }*/
        }
        else
        {
            combatData.SetNewTarget(null);
        }
    }


    public void OnModelLoad(int heroId, int partyIndex)
    {
        this.partyIndex = partyIndex;
        combatData.LoadFromCharacterData(GameManager.Instance.characterDatas[heroId]);
        UserInterfaceManager.instance.GameplayUI.OnHeroLoad(combatData, partyIndex);

        animator = GetComponentInChildren<Animator>();
        animator.SetBool("isInCombat", false);
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
        if (!IsPlayerControlled)
        {
            navState = NavState.Idle;
        }

        characterState = CharacterState.Explore;
        animator.SetBool("isInCombat", false);
    }

    public void OnDefeat()
    {
        animator.SetTrigger("TrDefeat");
    }

    public void OnRevive()
    {
        animator.SetTrigger("TrRevive");
    }

    public void OnPlayerCallsUltUse()
    {
        if (combatData.CanCastUlt() && !combatData.tryCastingUlt)
        {
            combatData.tryCastingUlt = true;
        }
    }

    public void OnDialogStart()
    {
        characterState = CharacterState.InDialog;
    }

    public void OnDialogEnd()
    {
        characterState = CharacterState.Explore;
    }



    public CombatData GetCombatData()
    {
        return combatData;
    }


    public void SetPlayerCamera(CameraController cam)
    {
        playerCamera = cam;
        //if (IsPlayerControlled) cam.OnPlayerSpawn(this);
    }
}
