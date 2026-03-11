using UnityEngine;

public abstract class ArtCastBase : MonoBehaviour
{
    [Header("Cast Base Data")]
    public Vector3 spawnPointOffset;
    public bool disableHits = false;
    public ArtData artData; // make readonly?    

    protected CombatArt art;    
    protected CombatData caster;
    protected CombatData selectedTarget;    

    protected float timeAlive = 0f;
    protected int hitsApplied = 0;
    protected bool cancelStarted = false;
    protected bool castDurationEnded = false;


    public abstract void OnCast(CombatData caster, CombatData selectedTarget, CombatArt art);

    protected abstract void SetPositionOnCast();

    public bool IsHitValid(Collider collider)
    {
        var colliderCombatData = collider.GetComponent<CombatData>();
        if (disableHits || colliderCombatData == null || colliderCombatData.IsDefeated())
            return false;

        if (IsAllowedToHitCombatant(colliderCombatData))
            return true;

        return false;
    }

    public void CheckCancelStart()
    {
        if (timeAlive > artData.cancelAfter && cancelStarted == false)
        {
            caster.OnCancelWindowStart(this, artData.cancelWindow);
            cancelStarted = true;
        }
    }

    public void ApplyArt(CombatData target)
    {
        if (disableHits || target.IsDefeated() ) return;

        if (artData.category == ArtData.Category.Physical || artData.category == ArtData.Category.Ether || artData.category == ArtData.Category.Soul)
        {
            if (target.IsOppositeFaction(caster))
            {
                var hitData = CombatCalcs.DamageCalculationArt(caster, target, art);
                if (!hitData.isMissed)
                {
                    target.RecieveDamage(hitData.value, caster);

                    foreach (var effect in artData.ArtEffects)
                        effect?.ApplyEffect(caster, target, hitData.value);

                    if (caster.isHero)
                        target.ChangeAggro(caster.characterId, art.GetBasePower() * 0.1f);
                    else
                        caster.ChangeAggro(target.characterId, art.GetBasePower() * -0.1f);
                }

                var damageType = ArtNumberLabel.NumberType.DamagePhysical;
                if (artData.category == ArtData.Category.Ether)
                    damageType = ArtNumberLabel.NumberType.DamageEther;
                else if (artData.category == ArtData.Category.Soul)
                    damageType = ArtNumberLabel.NumberType.DamageSoul;
                UserInterfaceManager.instance.GameplayUI.CreateArtNumber(damageType, target.transform.position, hitData);
            }            
            return;
        }

        if (artData.category == ArtData.Category.Buff)
        {
            if (target.IsOppositeFaction(caster) == false)
            {
                var effectBasePower = CombatCalcs.CalcEffectBasePower(caster, art);
                foreach (var effect in artData.ArtEffects)
                {
                    effect?.ApplyEffect(caster, target, effectBasePower);
                }
            }
            return;
        }

        if (artData.category == ArtData.Category.Debuff)
        {
            if (target.IsOppositeFaction(caster))
            {
                if (CombatCalcs.CheckIfEvaded(target))
                    return; // TODO: print miss?

                var effectBasePower = CombatCalcs.CalcEffectBasePower(caster, art);
                foreach (var effect in artData.ArtEffects)
                {
                    effect?.ApplyEffect(caster, target, effectBasePower);
                }
            }
            return;
        }

        if (artData.category == ArtData.Category.Heal)
        {
            if (target.IsOppositeFaction(caster) == false)
            {
                var hitData = CombatCalcs.HealingCalculation(caster, art);
                target.RecieveHealing(hitData.value);

                if (caster.isHero)
                {
                    UserInterfaceManager.instance.GameplayUI.CreateArtNumber(ArtNumberLabel.NumberType.Healing, target.transform.position, hitData);
                }
            }
            return;
        }
    }

    protected bool IsAllowedToHitCombatant(CombatData combatant)
    {
        if (artData.category == ArtData.Category.Heal || artData.category == ArtData.Category.Buff)
        {
            return !combatant.IsOppositeFaction(caster);
        }
        else
        {
            return combatant.IsOppositeFaction(caster);
        }
    }

    protected Vector3 GetSpawnPosition(Transform creator)
    {
        return creator.transform.position + Quaternion.LookRotation(creator.forward.normalized) * spawnPointOffset;
    }
}
