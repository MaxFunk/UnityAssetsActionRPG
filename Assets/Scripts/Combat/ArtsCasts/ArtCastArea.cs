using Unity.VisualScripting;
using UnityEngine;

public class ArtCastArea : ArtCastBase
{
    public enum CastPositioning
    {
        FromCaster = 0,
        OnTarget = 1,
        CasterToTarget = 2
    }

    [Header("Area Cast Data")]
    public CastPositioning castPositioning = CastPositioning.FromCaster;
    public bool collisionIsBox = false;
    public Vector3 collisionHalfExtends = Vector3.zero; // is sphere, only x is read out as radius
    public Transform castTransform;

    [Header("Caster To Target Flight")]
    public float flightTime = 2f;
    public AnimationCurve upwardsVelocityCurve;

    private float timeHitAfter = 1f;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 endPosition = Vector3.zero;


    private void Awake()
    {
        if (castTransform == null)
            castTransform = transform;
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        if (castPositioning == CastPositioning.CasterToTarget)
            CasterToTargetCurve();

        if (timeAlive > timeHitAfter && hitsApplied < artData.hits.Length)
        {
            CheckForHits();
            hitsApplied++;

            if (hitsApplied < artData.hits.Length)
                timeHitAfter = artData.hits[hitsApplied];
        }

        CheckCancelStart();

        if (timeAlive > artData.castDuration && castDurationEnded == false) // do not destroy here if proper animations exist..
        {
            caster.OnArtCastDurationEnd(this);
            castDurationEnded = true;
            Destroy(this.gameObject);
        }
    }

    public override void OnCast(CombatData caster, CombatData selectedTarget, CombatArt art)
    {
        base.caster = caster;
        base.selectedTarget = selectedTarget;
        base.art = art;
        artData = art.artData;

        if (artData.hits.Length > 0)
            timeHitAfter = artData.hits[0];
        else
            timeHitAfter = 0f;

        SetPositionOnCast();
    }


    protected override void SetPositionOnCast()
    {
        if (castPositioning == CastPositioning.FromCaster)
        {
            var dir = selectedTarget.transform.position - caster.transform.position;
            dir.y = 0f;
            transform.SetPositionAndRotation(GetSpawnPosition(caster.transform), Quaternion.LookRotation(dir.normalized));
        }

        if (castPositioning == CastPositioning.OnTarget)
        {
            transform.position = GetSpawnPosition(selectedTarget.transform);
        }

        if (castPositioning == CastPositioning.CasterToTarget)
        {
            startPosition = GetSpawnPosition(caster.transform);
            endPosition = selectedTarget.transform.position;
            transform.position = startPosition;
        }
    }

    private void CheckForHits()
    {
        Collider[] colliders;
        if (collisionIsBox)
        {
            colliders = Physics.OverlapBox(castTransform.position, collisionHalfExtends, castTransform.rotation);
        }
        else
        {
            colliders = Physics.OverlapSphere(castTransform.position, collisionHalfExtends.x);
        }


        Debug.DrawLine(castTransform.position, castTransform.position + collisionHalfExtends, Color.indianRed, 2f);
        Debug.DrawLine(castTransform.position, castTransform.position - collisionHalfExtends, Color.indianRed, 2f);

        foreach (Collider collider in colliders)
        { 
            if (IsHitValid(collider))
            {
                ApplyArt(collider.GetComponent<CombatData>());
            }
        }
    }

    private void CasterToTargetCurve()
    {
        float remainingFlightTime = flightTime - timeAlive;
        if (remainingFlightTime < 0) return; // flight ended

        float flightVelocity = Vector3.Distance(transform.position, endPosition) / remainingFlightTime;
        var targetVelocity = flightVelocity * (endPosition - transform.position).normalized;
        targetVelocity += Vector3.up * upwardsVelocityCurve.Evaluate(timeAlive);
        transform.position += Time.deltaTime * targetVelocity;
    }
}
