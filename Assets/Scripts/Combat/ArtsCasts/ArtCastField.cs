using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ArtCastField : ArtCastBase
{
    public float fieldDuration = 5f;
    public float fieldRadius = 2f;
    public float TimeBetweenTicks = 1f;

    private float lastTick = 0f;
    private bool inactive = false;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (inactive) { return; }

        timeAlive += Time.deltaTime;
        if (timeAlive > fieldDuration)
        {
            if (castDurationEnded == false)
                caster.OnArtCastDurationEnd(this);

            animator.SetTrigger("Fade");
            Destroy(gameObject, 0.5f);
            inactive = true;
            return;
        }

        CheckCancelStart();

        lastTick += Time.deltaTime;
        if (lastTick > TimeBetweenTicks)
        {
            lastTick = 0f;
            CheckForTargets();
        }

        if (timeAlive > artData.castDuration && castDurationEnded == false) // do not destroy here if proper animations exist..
        {
            caster.OnArtCastDurationEnd(this);
            castDurationEnded = true;
        }
    }


    public override void OnCast(CombatData caster, CombatData selectedTarget, CombatArt art)
    {
        base.caster = caster;
        base.selectedTarget = selectedTarget;
        base.art = art;
        artData = art.artData;

        SetPositionOnCast();
    }


    protected override void SetPositionOnCast()
    {
        transform.rotation = caster.transform.rotation;
        if (artData.IsAffectingEnemies())
        {
            transform.position = GetSpawnPosition(selectedTarget.transform);
        }
        else
        {
            transform.position = GetSpawnPosition(caster.transform);
        }
    }


    private void CheckForTargets()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, fieldRadius, Vector3.up, fieldRadius/2f, -1, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            if (IsHitValidField(hit.collider))
            {
                ApplyArt(hit.collider.GetComponent<CombatData>());
            }
        }
    }

    // Fix in Base Class so that this function is not required?
    private bool IsHitValidField(Collider collider)
    {
        var colliderCombatData = collider.GetComponent<CombatData>();
        if (colliderCombatData == null || colliderCombatData.IsDefeated() || disableHits)
            return false;
        return true;
    }
}
