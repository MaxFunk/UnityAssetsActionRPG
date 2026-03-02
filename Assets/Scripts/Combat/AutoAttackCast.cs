using UnityEngine;

public class AutoAttackCast : MonoBehaviour
{
    public float timeHitAfter = 0.5f;
    public float timeDuration = 1f;
    public float timeCancelStart = 0.6f;
    public float cancelDuration = 0.35f;
    public bool disableHits = false;

    private CombatData caster;
    private CombatData target;

    private float timeAlive = 0f;
    private bool hitApplied = false;
    private bool cancelStarted = false;


    void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive > timeHitAfter && hitApplied == false)
        {
            ApplyHit();
            hitApplied = true;
        }

        if (timeAlive > timeCancelStart && cancelStarted == false)
        {
            caster.OnCancelWindowStart(null, cancelDuration);
            cancelStarted = true;
        }

        if (timeAlive > timeDuration)
        {
            if (!disableHits) caster.OnAutoAttackCastEnd();
            Destroy(this.gameObject);
        }
    }

    public void OnCast(CombatData caster, CombatData selectedTarget)
    {
        this.caster = caster;
        this.target = selectedTarget;
        timeAlive = 0f;
    }

    private void ApplyHit()
    {
        if (disableHits) return;

        var hitData = CombatCalcs.DamageCalculationAutoAttack(caster, target);
        if (hitData.isEvaded == false) 
        {
            target.RecieveDamage(hitData.value, caster, true);
            caster.RecieveUltPoints(1);
        }
        else
        {
            if (!caster.isHero) 
            {
                caster.ChangeAggro(target, 1);
            }
        }

        var damageType = ArtNumberLabel.NumberType.DamagePhysical; // can be ether under certain circumstances (not yet implemented though)
        UserInterfaceManager.instance.GameplayUI.CreateArtNumber(damageType, target.transform.position, hitData);
    }
}
