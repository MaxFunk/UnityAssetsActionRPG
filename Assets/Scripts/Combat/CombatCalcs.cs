using UnityEngine;
using static UnityEngine.GraphicsBuffer;

static class CombatCalcs
{
    const float baseCritChance = 0.05f;

    public static CombatHitData DamageCalculationArt(CombatData caster, CombatData target, CombatArt art)
    {
        var hitData = new CombatHitData
        {
            valueType = CombatHitData.ValueType.damage,
            value = 1,
            isCrit = false,
            isEvaded = false,
            fromPlayer = caster.isPlayerControlled,
            fromEnemy = !caster.isHero
        };

        if (CheckIfEvaded(target))
        {
            hitData.isEvaded = true;
            return hitData;
        }

        float basePower = art != null ? art.curBasePower : 1;

        float offense = GetOffenseValue(caster, art.artData);
        float defense = GetDefenseValue(target, art.artData);
        float damage = offense * basePower * (1f - defense);

        var isCrit = IsCrit(caster, target);
        if (isCrit)
            damage *= 1.5f;
        
        if (caster.isPlayerControlled)
            damage *= caster.GetCancelMultiplier();
        damage *= Random.Range(0.85f, 1.1f);

        hitData.value = Mathf.RoundToInt(Mathf.Clamp(damage, 1, 9999999));
        hitData.isCrit = isCrit;
        return hitData;
    }


    public static CombatHitData DamageCalculationAutoAttack(CombatData caster, CombatData target)
    {
        var hitData = new CombatHitData
        {
            valueType = CombatHitData.ValueType.damage,
            value = 1,
            isCrit = false,
            isEvaded = false,
            fromPlayer = caster.isPlayerControlled,
            fromEnemy = !caster.isHero
        };

        if (CheckIfEvaded(target))
        {
            hitData.isEvaded = true;
            return hitData;
        }

        float offense = GetOffenseValue(caster, null);
        float defense = GetDefenseValue(target, null);
        float damage = offense * (1f - defense);

        var isCrit = IsCrit(caster, target);
        if (isCrit)
            damage *= 1.5f;

        damage *= Random.Range(0.9f, 1.05f);

        hitData.value = Mathf.RoundToInt(Mathf.Clamp(damage, 1, 9999999));
        hitData.isCrit = isCrit;
        return hitData;
    }


    public static CombatHitData HealingCalculation(CombatData caster, CombatArt art)
    {
        float basePower = art != null ? art.curBasePower : 10;
        float healing = caster.ether * basePower / 10f;

        var hitData = new CombatHitData
        {
            valueType = CombatHitData.ValueType.healing,
            value = Mathf.RoundToInt(Mathf.Clamp(healing, 1, 9999999)),
            isCrit = false,
            fromPlayer = caster.isPlayerControlled
        };
        return hitData;
    }

    public static bool IsCrit(CombatData caster, CombatData target)
    {
        float statRelation = (caster.luck + 25f) / (target.luck + 25f);
        float critChance = statRelation * statRelation * baseCritChance;

        foreach (var modifier in caster.CombatModifiers)
        {
            // idea: collect boosts in combatdata_variable e.g. critboosts, change when ever mod is given or taken!
            if (modifier.modifierData.modifierType == CombatModifierData.ModifierType.CritBoost)
            {
                critChance += modifier.value;
            }
        }
        critChance = Mathf.Clamp(critChance, 0f, 1f);
        return Random.value < critChance;
    }

    public static bool CheckIfEvaded(CombatData target)
    {
        foreach (var modifier in target.CombatModifiers)
        {
            if (modifier.modifierData.IsEvasion())
                return true;
        }
        return false;
    }

    public static float GetOffenseValue(CombatData combatant, ArtData artData)
    {
        float attackValue = combatant.attack;
        float etherValue = combatant.ether;

        foreach (var modifier in combatant.CombatModifiers)
        {
            attackValue += attackValue * modifier.GetStatModifier(CombatModifierData.ModifierType.AttackChange);
            etherValue += etherValue * modifier.GetStatModifier(CombatModifierData.ModifierType.EtherChange);
        }

        if (artData == null)
            return attackValue;

        float offenseValue = 1f;
        switch (artData.category)
        {
            case ArtData.Category.Physical:
                offenseValue = attackValue;
                break;
            case ArtData.Category.Ether:
            case ArtData.Category.Buff:
            case ArtData.Category.Debuff:
            case ArtData.Category.Heal:
                offenseValue = etherValue;
                break;
            case ArtData.Category.Soul:
                offenseValue = attackValue + etherValue;
                break;
            default:
                break;
        }

        return offenseValue;
    }

    public static float GetDefenseValue(CombatData combatant, ArtData artData)
    {
        float physicalValue = combatant.physicalDefense;
        float etherValue = combatant.etherDefense;

        foreach (var modifier in combatant.CombatModifiers)
        {
            physicalValue += modifier.GetStatModifier(CombatModifierData.ModifierType.PhysDefChange);
            etherValue += modifier.GetStatModifier(CombatModifierData.ModifierType.EtherDefChange);
        }

        physicalValue = Mathf.Clamp(physicalValue, 0f, 1f);
        etherValue = Mathf.Clamp(etherValue, 0f, 1f);

        if (artData == null || artData.category == ArtData.Category.Physical)
        {
            return physicalValue;
        }
        
        if (artData.category == ArtData.Category.Ether)
        {
            return etherValue;
        }
        
        if (artData.category == ArtData.Category.Soul)
        {
           return (physicalValue + etherValue) * 0.5f;
        }

        return 1f;
    }

    public static float CalcEffectBasePower(CombatData caster, CombatArt art)
    {
        float basePower = art != null ? art.curBasePower : 1;
        float offense = GetOffenseValue(caster, art.artData);
        return Mathf.Clamp(offense * basePower, 10, 999999);
    }
}
