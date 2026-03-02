using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

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
    public float TimerNavRepath = 5f;
    public float MaxDistanceToSpawn = 10f;

    [Header("Player Detection")]
    public float MaxViewDistance = 8f;
    public float MaxViewAngle = 60f;
    public float MaxInitAttackRange = 3f;

    [Header("Specific Data")]
    public NavigationBehavior navBehavior = NavigationBehavior.Static;
    public float timeNextArt = 0;
    public float despawnScaleFactor = 0.5f;

    CombatData combatData;
    CharacterController controller;
    Animator animator = null;
    NavMeshAgent agent = null;

    private HeroCharacterController targetedHeroChar = null;
    private Vector3 spawnPosition = Vector3.zero;
    private Quaternion spawnRotation = Quaternion.identity;
    private Vector3 characterVelocity = Vector3.zero;
    private float timeUntilNavRepath = 1f;
    private bool isDespawning = false;

    void Awake()
    {
        // drop data and other enemy specific data in this class as Editor Params
        // then give to combatData? or just let combatData have a reference to this, so that it can be looked up
        combatData = GetComponent<CombatData>();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

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

        if (combatData.isInCombat && timeNextArt > 0)
            timeNextArt -= Time.deltaTime;

        switch (navBehavior)
        {
            case NavigationBehavior.Static:
                break;
            case NavigationBehavior.Stationary:
                UpdateStationary();
                break;
            case NavigationBehavior.Patrolling:
                UpdatePatrolling();
                break;
            case NavigationBehavior.Guarding:
                UpdateGuarding();
                break;
            default:
                break;
        }

        agent.nextPosition = transform.position;
        if (controller.isGrounded)
            characterVelocity.y = 0;
    }

    private void UpdateStationary()
    {
        if (combatData.isInCombat)
        {
            var currentTarget = combatData.GetCurrentTarget();
            targetLine.enabled = currentTarget != null;
            if (currentTarget != null)
            {
                TryArtCast();
                TryAutoAttack();

                LookAtPosition(currentTarget.transform.position);

                if (targetLine != null) 
                {
                    var distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                    targetLine.SetPosition(1, new Vector3(0, 0, distToTarget));
                }

                /*if (Vector3.Distance(currentTarget.transform.position, transform.position) > MaxDistanceInCombat)
                {
                    combatData.ChangeAggro(currentTarget, 0, true);

                    var newTarget = CombatManager.Instance.GiveNewTarget(combatData);
                    combatData.SetNewTarget(newTarget);
                    if (newTarget == null)
                    {
                        CombatManager.Instance.CombatantLeave(combatData);
                    }
                }*/
            }
        }
    }

    private void UpdatePatrolling()
    {
        if (combatData.isInCombat)
        {
            var currentTarget = combatData.GetCurrentTarget();
            targetLine.enabled = currentTarget != null;
            if (currentTarget != null)
            {
                TryArtCast();
                TryAutoAttack();

                LookAtPosition(currentTarget.transform.position);

                if (targetLine != null)
                {
                    var distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                    targetLine.SetPosition(1, new Vector3(0, 0, distToTarget));
                }
            }
        }
        else
        {
            Vector3 moveDir = Vector3.zero;

            timeUntilNavRepath -= Time.deltaTime;
            if (timeUntilNavRepath < 0 || agent.remainingDistance < 0.5f)
            {
                FetchNewPatrolPath();
                timeUntilNavRepath = TimerNavRepath;
            }

            if (agent.path != null && agent.path.corners.Length > 1)
            {
                moveDir = agent.path.corners[1] - transform.position;
                moveDir.y = 0f;
            }

            if (combatData.CanMove() == false)
                moveDir = Vector3.zero;

            MoveCharacter(moveDir);
        }
    }

    private void UpdateGuarding()
    {
        if (combatData.isInCombat)
        {
            var currentTarget = combatData.GetCurrentTarget();
            targetLine.enabled = currentTarget != null;
            if (currentTarget != null)
            {
                TryArtCast();
                TryAutoAttack();

                LookAtPosition(currentTarget.transform.position);

                if (targetLine != null)
                {
                    var distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                    targetLine.SetPosition(1, new Vector3(0, 0, distToTarget));
                }
            }
        }
        else
        {
            Vector3 moveDir = Vector3.zero;
            var skipRotation = false;
            targetedHeroChar = CheckPlayer();

            timeUntilNavRepath -= Time.deltaTime;
            if (timeUntilNavRepath < 0)
            {
                var destCondition = targetedHeroChar != null && Vector3.Distance(transform.position, spawnPosition) < MaxDistanceToSpawn;
                var destination = destCondition ? targetedHeroChar.transform.position : spawnPosition;
                agent.SetDestination(destination);
                timeUntilNavRepath = TimerNavRepath;
            }

            // return to original rotation of spawn
            if (targetedHeroChar == null && Vector3.Distance(transform.position, spawnPosition) < 0.1)
            {
                skipRotation = true;
                transform.rotation = Quaternion.Slerp(transform.rotation, spawnRotation, RotationSpeed * Time.deltaTime);
            }

            // attack/start battle when close to player
            if (targetedHeroChar != null && Vector3.Distance(targetedHeroChar.transform.position, transform.position) < MaxInitAttackRange)
            {
                // join combat...;
            }

            // copy pasted from patrol, maybe make own func?
            if (agent.path != null && agent.path.corners.Length > 1)
            {
                moveDir = agent.path.corners[1] - transform.position;
                moveDir.y = 0f;
            }
            if (combatData.CanMove() == false)
                moveDir = Vector3.zero;

            MoveCharacter(moveDir, skipRotation:skipRotation);
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


    private void LookAtPosition(Vector3 pos)
    {
        var direction = pos - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);
    }

    private void FetchNewPatrolPath()
    {
        if (agent.isOnNavMesh)
        {
            if (Spawner != null && Vector3.Distance(transform.position, Spawner.transform.position) > MaxDistanceToSpawn)
            {
                agent.SetDestination(Spawner.GetPointNearSpawner()); // instead of spawner just use spawnPosition?
            }
            else
            {
                Vector3 sampleDir = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                agent.SetDestination(transform.position + sampleDir.normalized * 5f);
            }
        }
        else
        {
            agent.ResetPath();
        }
    }

    private HeroCharacterController CheckPlayer()
    {
        var playerChar = GameManager.Instance.PlayerCharacter;

        if (playerChar == null || Vector3.Distance(playerChar.transform.position, transform.position) > MaxViewDistance) return null;

        var dirToPlayer = (playerChar.transform.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) > MaxViewAngle) return null;

        return playerChar;
    }


    private void TryArtCast()
    {
        if (timeNextArt <= 0 && combatData.CanPerformArtCast())
        {
            var randomIndex = combatData.GetRandomArtIndex();
            if (randomIndex >= 0)
            {
                var success = combatData.CastArt(randomIndex);
                if (success)
                {
                    timeNextArt = combatData.GetCurrentArt().artCooldown[0] * combatData.timeModifierArtUse;
                    animator.SetTrigger("TrArt");
                }
            }
        }
    }

    private void TryAutoAttack()
    {
        if (combatData.CanPerformAutoAttack())
        {
            combatData.PerformAutoAttack();
            animator.SetTrigger("TrBasicAttack");
        }
    }

    private void MoveCharacter(Vector3 moveDirection, float speedModifier = 1f, bool skipRotation = false)
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

        var targetVelocity = MaxSpeedOnGround * speedModifier * moveDirection;
        var characterVelocityXZ = new Vector3(characterVelocity.x, 0, characterVelocity.z);
        characterVelocityXZ = Vector3.Lerp(characterVelocityXZ, targetVelocity, MovementFriction * Time.deltaTime);
        characterVelocity.y += Gravity * Time.deltaTime;

        var finalVelocity = characterVelocityXZ + (characterVelocity.y * Vector3.up);
        characterVelocity = finalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }


    public void OnCombatJoin()
    {
        animator.SetBool("isInCombat", true);
    }

    public void OnCombatEnd()
    {
        animator.SetBool("isInCombat", false);
        targetLine.enabled = false;
    }

    public CombatData GetCombatData()
    {
        return combatData;
    }
}
