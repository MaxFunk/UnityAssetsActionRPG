using UnityEngine;

[System.Serializable]
public class ItemGearData
{
    public enum GearType
    {
        None,
        Outfit,
        Accessory,
        Augment
    }

    public enum GearEffect
    {
        None,
        StatChangeFlat,
        StatChangePercent,
        GiveModifier,
        Other
    }

    public GearType type = GearType.Outfit;
    public GearEffect effect = GearEffect.None;
    public int effectIndex = 0;
    public float effectValue = 0f;
    public CombatModifierData combatModifier;


    public void ApplyGearEffect(CombatData combatData, bool onLoad, bool onCombatBegin)
    {
        if (type == GearType.None)
            return;

        switch (effect)
        {
            case GearEffect.StatChangeFlat:
            case GearEffect.StatChangePercent:
                if (onLoad)
                    ChangeStats(ref combatData);
                break;
            case GearEffect.GiveModifier:
                if (onCombatBegin)
                    GiveModifier(ref combatData);
                break;
            default:
                break;
        }
    }

    public void ChangeStats(ref CombatData combatData)
    {
        switch (effectIndex)
        {
            case 0: // Health
                ApplyStatChange(ref combatData.maxHealth);
                ApplyStatChange(ref combatData.curHealth);
                break;
            case 1: // Attack
                ApplyStatChange(ref combatData.attack);
                break;
            case 2: // Ether
                ApplyStatChange(ref combatData.ether);
                break;
            case 3: // Agility
                ApplyStatChange(ref combatData.agility);
                break;
            case 4: // Luck
                ApplyStatChange(ref combatData.luck);
                break;
            default:
                break;
        }
    }

    private void ApplyStatChange(ref float stat)
    {
        if (effect == GearEffect.StatChangeFlat)
        {
            stat += effectValue;
        }
        else // percentage
        {
            stat += effectValue * stat;
        }
    }

    public void GiveModifier(ref CombatData target)
    {
        var artEffect = new ArtEffect
        {
            effectType = ArtEffect.EffectType.GiveModifier,
            value = effectValue,
            duration = effectIndex, // solve issue for finite gear modifier
            combatModifier = combatModifier,
        };
        artEffect.ApplyGiveModifier(target, 1f);
    }


    public void ApplyGearEffectUI(StatCollection characterStats)
    {
        if (type == GearType.None)
            return;

        if (effect == GearEffect.StatChangeFlat || effect == GearEffect.StatChangePercent)
        {
            switch (effectIndex)
            {
                case 0: // Health
                    ApplyStatChangeUI(ref characterStats.health);
                    break;
                case 1: // Attack
                    ApplyStatChangeUI(ref characterStats.attack);
                    break;
                case 2: // Ether
                    ApplyStatChangeUI(ref characterStats.ether);
                    break;
                case 3: // Agility
                    ApplyStatChangeUI(ref characterStats.agility);
                    break;
                case 4: // Luck
                    ApplyStatChangeUI(ref characterStats.luck);
                    break;
                default:
                    break;
            }
        }
    }

    private void ApplyStatChangeUI(ref int stat)
    {
        if (effect == GearEffect.StatChangeFlat)
            stat += Mathf.RoundToInt(effectValue);
        else // percentage
            stat += Mathf.RoundToInt(effectValue * stat);
    }


    public bool CheckSameGearType(int indexGearType)
    {
        return (GearType)indexGearType == type;
    }
}
