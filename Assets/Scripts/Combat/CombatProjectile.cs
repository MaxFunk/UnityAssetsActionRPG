using UnityEngine;

public class CombatProjectile : MonoBehaviour
{
    [Header("Projectile Param")]
    public float MaxLifeTime = 5f;
    public float DestroyDelayTime = 0.2f;
    public float ProjectileSpeed = 5.0f;
    public float ProjectileRadius = 0.25f;
    public float VelocityOffsetCone = 0.0f; // in degrees
    public float VelocityLerpTime = 1f;
    public string TriggerDestroy = "Destroy";

    private ArtCastProjectile artCastProjectile;
    private Animator animator;
    private CombatData target;

    private Vector3 velocity;
    private float timeAlive = 0.0f;
    private float timeUntilDestroy = 0.0f;
    private bool isDestroyingSoon = false;
    private bool isTargetValid = true;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDestroyingSoon)
        {
            timeUntilDestroy -= Time.deltaTime;
            if (timeUntilDestroy < 0.0f) 
            {
                artCastProjectile.ChildProjectileWasDestroyed(this);
                Destroy(gameObject);
                return;
            }
        }

        Vector3 velocityTowardsTarget;
        if (target != null && isTargetValid)
        {
            var endPos = target.transform.position + Vector3.up * 0.7f; // get target center of target
            velocityTowardsTarget = (endPos - transform.position).normalized * ProjectileSpeed;
        }
        else
        {
            velocityTowardsTarget = velocity;
        }

        timeAlive += Time.deltaTime;
        float lerpTime = timeAlive / VelocityLerpTime;
        velocity = Vector3.Lerp(velocity, velocityTowardsTarget, lerpTime * lerpTime);
        transform.position += velocity * Time.deltaTime;

        if (isDestroyingSoon)
            return;

        if (timeAlive > MaxLifeTime)
        {
            // notify parent that self has finished (also do when hit occurs)

            timeUntilDestroy = DestroyDelayTime;
            isDestroyingSoon = true;
            animator.SetTrigger(TriggerDestroy);
        }

        CheckForHits();
    }


    public void InitProjectileParams(ArtCastProjectile artCastProjectile, CombatData target, Vector3 forwardDir)
    {
        this.artCastProjectile = artCastProjectile;
        this.target = target;
        var sampledDir = Quaternion.AngleAxis(Random.Range(0f, VelocityOffsetCone), Random.insideUnitSphere) * forwardDir;
        velocity = sampledDir * ProjectileSpeed;

        if (target == null)
            isTargetValid = false;
    }


    private void CheckForHits()
    {
        if (artCastProjectile == null) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, ProjectileRadius);

        foreach (Collider collider in colliders)
        {
            if (artCastProjectile.IsHitValid(collider))
            {
                artCastProjectile.ApplyArt(collider.GetComponent<CombatData>());

                timeUntilDestroy = DestroyDelayTime;
                isDestroyingSoon = true;
                animator.SetTrigger(TriggerDestroy);
                return;
            }
        }
    }

    public void OnTargetGotInvalid()
    {
        isTargetValid = false;
        target = null;
    }
}
