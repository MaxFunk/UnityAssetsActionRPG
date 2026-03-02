using UnityEngine;

public class ArtCastDirect : ArtCastBase
{
    public enum CastPositioning
    {
        OnCaster = 0,
        OnTarget = 1,
        TravelFromCaster = 2,
        TravelCasterToTarget = 3,
        TravelTargetToCaster = 4
    }

    [Header("Direct Cast Data")]
    public CastPositioning castPositioning = CastPositioning.OnCaster;
    public float travelSpeed = 5f;
    public float travelStartDelay = 0f;

    private Vector3 initTravelDir = Vector3.zero;
    private float timeHitAfter = 1f;
    private bool canTravel = true;


    void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive > timeHitAfter && hitsApplied < artData.hits.Length)
        {
            ApplyArt(selectedTarget);
            hitsApplied++;

            if (hitsApplied < artData.hits.Length)
                timeHitAfter = artData.hits[hitsApplied];
        }

        CheckCancelStart();
        Travel();

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

        hitsApplied = 0;
        cancelStarted = false;

        if (artData.isSelfCast)
            base.selectedTarget = caster;

        if (artData.hits.Length > 0)
            timeHitAfter = artData.hits[0];
        else
            timeHitAfter = 0f;

        InitTravel();
    }

    protected override void SetPositionOnCast()
    {

    }


    private void InitTravel()
    {
        switch (castPositioning)
        {
            case CastPositioning.OnCaster:
                transform.SetPositionAndRotation(GetSpawnPosition(caster.transform), caster.transform.rotation);
                canTravel = false;
                break;
            case CastPositioning.OnTarget:
                transform.SetPositionAndRotation(GetSpawnPosition(selectedTarget.transform), selectedTarget.transform.rotation);
                canTravel = false;
                break;
            case CastPositioning.TravelFromCaster:
                transform.SetPositionAndRotation(GetSpawnPosition(caster.transform), caster.transform.rotation);
                initTravelDir = caster.transform.forward;
                break;
            case CastPositioning.TravelCasterToTarget:
                transform.SetPositionAndRotation(GetSpawnPosition(caster.transform), caster.transform.rotation);
                break;
            case CastPositioning.TravelTargetToCaster:
                transform.SetPositionAndRotation(GetSpawnPosition(selectedTarget.transform), selectedTarget.transform.rotation);
                break;
            default:
                break;
        }
    }


    private void Travel()
    {
        if (!canTravel || timeAlive < travelStartDelay) return;

        Vector3 travelDir;
        if (castPositioning == CastPositioning.TravelCasterToTarget)
        {
            travelDir = (selectedTarget.transform.position - transform.position).normalized;
        }
        else if (castPositioning == CastPositioning.TravelTargetToCaster)
        {
            travelDir = (caster.transform.position - transform.position).normalized;
        }
        else 
        {
            travelDir = initTravelDir;
        }

        transform.position += Time.deltaTime * travelSpeed * travelDir;
    }
}
