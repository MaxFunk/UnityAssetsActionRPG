using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController), typeof(CombatData), typeof(NavMeshAgent))]
public class EnemyCharacterController : MonoBehaviour
{
    public enum NavigationBehavior
    {
        Static,
        Stationary,
        Patrolling,
        Guarding
    }


    [Header("References")]
    public EnemySpawner Spawner;
    public LineRenderer targetLine;

    [Header("Movement and Control")]
    public float MaxSpeedOnGround = 8f;
    public float MovementFriction = 8f;
    public float RotationSpeed = 10f;
    public float Gravity = -9.81f;
    public float MaxTargetRange = 10f;
    public float TimerNavRepathWalking = 3f;
    public float TimerNavRepathCombat = 0.5f;
    public float MaxDistanceToSpawn = 10f;

    [Header("Player Detection")]
    public bool Aggressive = false;
    public float MaxViewDistance = 8f;
    public float MaxViewAngle = 60f;

    [Header("Specific Data")]
    public NavigationBehavior navBehavior = NavigationBehavior.Static;
    public float timeNextArt = 0;
    public float despawnScaleFactor = 0.5f;
    public List<int> artIds = new();
    public List<ItemRecieveData> itemDrops = new();
    public AutoAttackData autoAttackData = new();

    CombatData combatData;
    CharacterController controller;
    Animator animator = null;
    NavMeshAgent agent = null;
    EventFlagChecker flagChecker = null;

    private Vector3 spawnPosition = Vector3.zero;
    private Quaternion spawnRotation = Quaternion.identity;
    private Vector3 characterVelocity = Vector3.zero;
    private float timeUntilNavRepath = 1f;
    private bool isDespawning = false;
    private bool inCombat = false;

    void Awake()
    {
        // drop data and other enemy specific data in this class as Editor Params
        // then give to combatData? or just let combatData have a reference to this, so that it can be looked up
        combatData = GetComponent<CombatData>();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        flagChecker = GetComponentInChildren<EventFlagChecker>();

        timeNextArt = combatData.timeModifierArtUse;
        if (targetLine != null) targetLine.enabled = false;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.autoTraverseOffMeshLink = false;

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    void Update()
    {
        if (isDespawning)
        {
            gameObject.transform.localScale -= despawnScaleFactor * Time.deltaTime * Vector3.one;
            if (gameObject.transform.localScale.x < 0.01f)
                Despawn();
            return;
        }

        bool lookIntoWalkDir = true;
        bool dontMove = false;
        timeUntilNavRepath -= Time.deltaTime;

        if (inCombat)
        {
            var currentTarget = combatData.GetCurrentTarget();
            DrawTargetLine(currentTarget);

            if (currentTarget != null)
            {
                // dont walk if in AA range
                if (Vector3.Distance(transform.position, currentTarget.transform.position) <= combatData.GetAutoAttackRange())
                {
                    dontMove = true;
                    lookIntoWalkDir = false;

                    var lookDir = currentTarget.transform.position - transform.position;
                    lookDir.y = 0;
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

                    TryArtCast();
                }

                if (combatData.CanPerformAutoAttack())
                {
                    combatData.StartAutoAttack();
                    animator.SetTrigger("TrBasicAttack");
                }
                
                if (timeUntilNavRepath < 0)
                {
                    agent.SetDestination(currentTarget.transform.position);
                    timeUntilNavRepath = TimerNavRepathCombat;
                }
            }            
        }
        else
        {
            var isCloseToSpawn = Vector3.Distance(transform.position, spawnPosition) < 0.1f;

            DetectPlayer();

            // Update navigation paths
            if (timeUntilNavRepath < 0)
            {
                if (navBehavior == NavigationBehavior.Patrolling)
                {
                    if (Vector3.Distance(transform.position, spawnPosition) > MaxDistanceToSpawn)
                    {
                        agent.SetDestination(spawnPosition);
                    }
                    else
                    {
                        Vector3 sampleDir = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                        agent.SetDestination(transform.position + sampleDir.normalized * 5f);
                    }
                }

                if (navBehavior == NavigationBehavior.Guarding && !isCloseToSpawn)
                {
                    agent.SetDestination(spawnPosition);
                }

                timeUntilNavRepath = TimerNavRepathWalking;
            }

            // stationary looks at original forward, guarding also looks that way, if at spawnpos
            if (navBehavior == NavigationBehavior.Stationary || (navBehavior == NavigationBehavior.Guarding && isCloseToSpawn))
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, spawnRotation, RotationSpeed * Time.deltaTime);
                lookIntoWalkDir = false;
            }
        }

        Vector3 moveDir = dontMove ? Vector3.zero : EvaluateMoveDirection();
        MoveCharacter(moveDir, lookIntoWalkDir);

        if (controller.isGrounded)
            characterVelocity.y = 0;
    }

    private Vector3 EvaluateMoveDirection()
    {
        if (combatData.CanMove() == false || navBehavior == NavigationBehavior.Static || navBehavior == NavigationBehavior.Stationary)
            return Vector3.zero;

        Vector3 moveDir = Vector3.zero;
        if (agent.path != null && agent.path.corners.Length > 1)
        {
            moveDir = agent.path.corners[1] - transform.position;
            moveDir.y = 0f;
        }
        return moveDir.normalized;
    }

    private void MoveCharacter(Vector3 moveDirection, bool lookIntoWalkDir, float speedModifier = 1f)
    {
        if (moveDirection.sqrMagnitude < 0.01f)
            moveDirection = Vector3.zero;

        if (moveDirection != Vector3.zero && lookIntoWalkDir)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }

        var targetVelocity = MaxSpeedOnGround * speedModifier * moveDirection;
        var characterVelocityXZ = new Vector3(characterVelocity.x, 0, characterVelocity.z);
        characterVelocityXZ = Vector3.Lerp(characterVelocityXZ, targetVelocity, MovementFriction * Time.deltaTime);
        characterVelocity.y += Gravity * Time.deltaTime;

        var finalVelocity = characterVelocityXZ + (characterVelocity.y * Vector3.up);
        characterVelocity = finalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
        agent.nextPosition = transform.position;
    }

    private void DrawTargetLine(CombatData target)
    {
        if (targetLine != null)
        {
            targetLine.enabled = target != null;
            if (targetLine.enabled)
            {
                var distToTarget = Vector3.Distance(transform.position, target.transform.position);
                targetLine.SetPosition(1, new Vector3(0, 0, distToTarget));

                var dirToTarget = (target.transform.position - transform.position).normalized;
                targetLine.transform.rotation = Quaternion.LookRotation(dirToTarget);
            }
        }
    }

    private void DetectPlayer()
    {
        if (!Aggressive || inCombat) return;

        var playerChar = GameManager.Instance.PlayerCharacter;
        if (playerChar == null || Vector3.Distance(playerChar.transform.position, transform.position) > MaxViewDistance) return;

        var dirToPlayer = (playerChar.transform.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) > MaxViewAngle) return;

        CombatManager.Instance.EnemyInitiateCombat(combatData);
        combatData.SetNewTarget(playerChar.GetCombatData());
    }


    private void TryArtCast()
    {
        timeNextArt -= Time.deltaTime;
        if (timeNextArt <= 0 && combatData.CanPerformArtCast())
        {
            var randomIndex = combatData.GetRandomArtIndex();
            if (randomIndex >= 0)
            {
                var success = combatData.CastArt(randomIndex);
                if (success)
                {
                    timeNextArt = combatData.GetCurrentArt().artCooldown[0] * combatData.timeModifierArtUse;
                    animator.SetTrigger("TrArtEther");
                }
            }
        }
    }


    public void StartDespawn()
    {
        isDespawning = true;
        animator.SetTrigger("TrDefeat");
    }

    public void Despawn()
    {
        if (Spawner != null)
            Spawner.OnDespawnCharacter(this);

        Destroy(gameObject);
    }


    public void OnCombatJoin()
    {
        inCombat = true;
        animator.SetBool("isInCombat", true);
    }

    public void OnCombatEnd()
    {
        inCombat = false;
        animator.SetBool("isInCombat", false);
        targetLine.enabled = false;
    }

    public void OnDefeat()
    {
        if (flagChecker != null)
        {
            flagChecker.WriteFlags();
        }
    }

    public CombatData GetCombatData()
    {
        return combatData;
    }
}
