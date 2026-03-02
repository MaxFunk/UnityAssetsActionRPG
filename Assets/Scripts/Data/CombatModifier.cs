using UnityEngine;
using static CombatModifierData;

[System.Serializable]
public class CombatModifier
{
    public CombatModifierData modifierData;
    public float value = 0f;
    public float maxDuration = 0f;
    public float tickValue = 0f;

    public float remainingDuration = 0f;
    public float lastTick = 0f;
    


    public void LoadData(CombatModifierData data, ArtEffect artEffect, float basePower)
    {
        if (data == null)
            return;

        modifierData = data;
        value = artEffect.value;
        tickValue = value * basePower;
        maxDuration = artEffect.duration;

        remainingDuration = artEffect.duration;
        lastTick = data.tickInterval;

        if (modifierData.infinite)
        {
            remainingDuration = 999999f;
        }
    }

    public bool ExternalUpdate(CombatData combatant)
    {
        if (remainingDuration <= 0f || modifierData == null)
            return false;

        if (modifierData.tickInterval > 0f && combatant != null)
        {
            lastTick -= Time.deltaTime;
            if (lastTick < 0f)
            {
                lastTick = modifierData.tickInterval;
                if (modifierData.IsDamageOverTime())
                    ApplyTickDamage(combatant);
                else if (modifierData.IsHealthRegen())
                    ApplyHealthRegenTick(combatant);
            }
        }

        if (!modifierData.infinite)
            remainingDuration -= Time.deltaTime;
        return true;
    }

    private void ApplyTickDamage(CombatData target)
    {
        int tickDamage = Mathf.RoundToInt(tickValue);
        target.RecieveEffectDamage(tickDamage);

        var hitData = new CombatHitData
        {
            valueType = CombatHitData.ValueType.effectDamage,
            value = tickDamage
        };

        if (modifierData.showInPanels)
            UserInterfaceManager.instance.GameplayUI.CreateArtNumber(ArtNumberLabel.NumberType.DamageEffects, target.transform.position, hitData);
    }

    private void ApplyHealthRegenTick(CombatData target)
    {
        int healing;
        if (modifierData.modifierType == ModifierType.HealthRegenFlat)
            healing = Mathf.RoundToInt(value);
        else
            healing = Mathf.RoundToInt(value * target.maxHealth);

        var hitData = new CombatHitData
        {
            valueType = CombatHitData.ValueType.healing,
            value = healing
        };

        target.RecieveHealing(healing);
        if (target.isHero && modifierData.showInPanels)
        {
            UserInterfaceManager.instance.GameplayUI.CreateArtNumber(ArtNumberLabel.NumberType.Healing, target.transform.position, hitData);
        }
    }

    public float GetStatModifier(ModifierType queriedModifierType)
    {
        if (modifierData.modifierType == queriedModifierType)
            return value;

        return 0f;
    }


    public bool IsSameModifier(CombatModifier otherModifier)
    {
        return modifierData == otherModifier.modifierData || modifierData.exlusiveWithModifier == otherModifier.modifierData;
    }

    public void CopyFromModifier(CombatModifier otherModifier)
    {
        value = otherModifier.value;
        tickValue = otherModifier.tickValue;
        maxDuration = otherModifier.maxDuration;
        remainingDuration = otherModifier.maxDuration;
    }
}
