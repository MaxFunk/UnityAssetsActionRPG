using UnityEngine;

public class CombatAutoAttack
{
    public bool Disabled { get; set; } = false;

    private AutoAttackData autoAttackData = null;
    private CombatData owner;
    private CombatData target;

    private bool active = false;
    private float timeActive = 0f;
    private int hitsApplied = 0;
    private bool cancelStarted = false;
    private float remainingCooldown = 0f;


    public void ExternalUpdate()
    {
        if (remainingCooldown > 0f)
            remainingCooldown -= Time.deltaTime;

        if (!active || remainingCooldown > 0f) return;

        timeActive += Time.deltaTime;
        if (hitsApplied < autoAttackData.hitAfter.Length && timeActive > autoAttackData.hitAfter[hitsApplied])
        {
            ApplyHit();
        }

        if (timeActive > autoAttackData.cancelStart && cancelStarted == false) // maybe call via delayed call when begin AA?
        {
            owner.OnCancelWindowStart(null, autoAttackData.cancelDuration);
            cancelStarted = true;
        }

        if (timeActive > autoAttackData.attackDuration)
            EndAutoAttack();
    }

    public void InitializeData(CombatData owner, AutoAttackData autoAttackData)
    {
        this.owner = owner;
        this.autoAttackData = autoAttackData;
    }


    public void BeginAutoAttack(CombatData target)
    {
        this.target = target;
        active = true;
        cancelStarted = false;
        hitsApplied = 0;
        timeActive = 0f;
    }

    public void EndAutoAttack()
    {
        target = null;
        active = false;

        float cooldownModifier = 1f;
        foreach (var modifier in owner.CombatModifiers)
        {
            if (modifier.modifierData.modifierType == CombatModifierData.ModifierType.AttackSpeedChange)
                cooldownModifier += modifier.value;
        }
        remainingCooldown = autoAttackData.attackCooldown * cooldownModifier;

        if (!Disabled)
            owner.OnAutoAttackEnd();
    }

    public bool CanAutoAttack(float distanceToTarget)
    {
        return distanceToTarget <= autoAttackData.attackRange && remainingCooldown <= 0f && !active;
    }

    public void AddCooldown(float cdTime)
    {
        remainingCooldown += cdTime;
    }

    public float GetAutoAttackRange()
    {
        return autoAttackData?.attackRange ?? 0f;
    }


    private void ApplyHit()
    {
        if (Disabled) return;

        var hitData = CombatCalcs.DamageCalculationAutoAttack(owner, target, autoAttackData.damageModifier);
        if (!hitData.isMissed)
        {
            target.RecieveDamage(hitData.value, owner, true);
            owner.RecieveUltPoints(1);

            if (owner.isHero)
                target.ChangeAggro(owner.characterId, 1f * autoAttackData.damageModifier);
            else
                owner.ChangeAggro(target.characterId, -1f * autoAttackData.damageModifier);
        }

        var damageType = ArtNumberLabel.NumberType.DamagePhysical; // can be ether under certain circumstances (not yet implemented though)
        UserInterfaceManager.instance.GameplayUI.CreateArtNumber(damageType, target.transform.position, hitData);
        hitsApplied++;
    }
}
