using UnityEngine;

[System.Serializable]
public class ArtEffect
{
    public enum EffectType
    {
        None,
        ChangeAggro,
        BoostCrit,
        BoostAccuracy,
        GiveModifier,
        Other
    }

    public EffectType effectType;
    public float value = 0f;
    public float duration = 0f;
    public CombatModifierData combatModifier;


    public void ApplyEffect(CombatData caster, CombatData target, float basePower)
    {
        var combatManager = CombatManager.Instance;

        switch (effectType)
        {
            case EffectType.ChangeAggro:
                ApplyChangeAggro(combatManager, caster);
                break;
            case EffectType.GiveModifier:
                ApplyGiveModifier(target, basePower);
                break;
            default:
                break;
        }
    }

    public void ApplyChangeAggro(CombatManager combatManager, CombatData caster)
    {
        foreach (var enemy in combatManager.enemyCombatants)
        {
            enemy.ChangeAggro(caster.characterId, value);
        }
    }

    public void ApplyGiveModifier(CombatData target, float basePower)
    {
        var newModifier = new CombatModifier();
        newModifier.LoadData(combatModifier, this, basePower);
        target.RecieveCombatModifier(newModifier);
    }

    public float CritBoost()
    {
        return effectType == EffectType.BoostCrit ? value : 0f;
    }

    public float AccuracyBoost()
    {
        return effectType == EffectType.BoostAccuracy ? value : 0f;
    }
}
