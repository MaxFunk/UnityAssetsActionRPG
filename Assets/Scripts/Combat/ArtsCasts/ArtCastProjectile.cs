using System.Collections.Generic;
using UnityEngine;

public class ArtCastProjectile : ArtCastBase
{
    [Header("Projectile Data")]
    public CombatProjectile PrefabProjectile;

    private readonly List<CombatProjectile> projectilesAlive = new();

    private int projectilesSpawned = 0;
    private float timeSpawnNext = 0f;
    private Vector3 endPosition = Vector3.zero;
    


    public override void OnCast(CombatData caster, CombatData selectedTarget, CombatArt art)
    {
        base.caster = caster;
        base.selectedTarget = selectedTarget;
        base.art = art;
        artData = art.artData;

        if (artData.hits.Length > 0)
            timeSpawnNext = artData.hits[0];
        else
            timeSpawnNext = 0f;

        transform.SetPositionAndRotation(GetSpawnPosition(caster.transform), caster.transform.rotation);
        endPosition = selectedTarget.transform.position + Vector3.up * 0.7f;
    }

    protected override void SetPositionOnCast()
    {

    }


    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive > timeSpawnNext && projectilesSpawned < artData.hits.Length)
        {
            SpawnChildProjectile();

            if (projectilesSpawned < artData.hits.Length)
                timeSpawnNext = artData.hits[projectilesSpawned];
        }

        if (timeAlive > artData.castDuration && castDurationEnded == false)
        {
            caster.OnArtCastDurationEnd(this);
            castDurationEnded = true;
        }

        CheckCancelStart();
    }


    public void ChildProjectileWasDestroyed(CombatProjectile projectile)
    {
        projectilesAlive.Remove(projectile);

        if (projectilesAlive.Count <= 0 && projectilesSpawned >= artData.hits.Length)
        {
            Destroy(gameObject);
        }
    }

    private void SpawnChildProjectile()
    {
        if (PrefabProjectile == null) return;

        CombatProjectile newProjectile = Instantiate(PrefabProjectile, transform.position, transform.rotation);
        var forwardDir = (endPosition - transform.position).normalized;
        newProjectile.InitProjectileParams(this, selectedTarget, forwardDir);
        projectilesAlive.Add(newProjectile);
        projectilesSpawned++;
    }

    public void CombatantRemovedFromCombat(CombatData combatant)
    {
        if (combatant == selectedTarget)
        {
            selectedTarget = null;
            foreach (var projectile in projectilesAlive) 
            {
                projectile.OnTargetGotInvalid();
            }
        }
    }

    /*
     {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // Sphere cast
            Vector3 displacementSinceLastFrame = transform.position - lastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition, 1.0f,
                displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, -1, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if (IsHitValid(hit.collider) && hit.distance < closestHit.distance)
                {
                    foundHit = true;
                    closestHit = hit;
                }
            }

            if (foundHit)
            {
                // Handle case of casting while already inside a collider
                if (closestHit.distance <= 0f)
                {
                    closestHit.point = transform.position;
                    closestHit.normal = -transform.forward;
                }

                //OnHit(closestHit.point, closestHit.normal, closestHit.collider);
                ApplyArt(closestHit.collider.GetComponent<CombatData>());

                if (castDurationEnded == false)
                    caster.OnArtCastDurationEnd(this);
                Destroy(this.gameObject);
            }
        }
     */
}
