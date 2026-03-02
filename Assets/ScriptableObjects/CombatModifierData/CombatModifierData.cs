using UnityEngine;

[CreateAssetMenu(fileName = "CombatModifierData", menuName = "Scriptable Objects/CombatModifierData", order = 1)]
public class CombatModifierData : ScriptableObject
{
    public enum ModifierType
    {
        AttackChange,
        EtherChange,
        AccuracyChange,
        PhysDefChange,
        EtherDefChange,
        HealthRegenFlat,
        HealthRegenPercent,
        ArtCooldownChange,
        Evasion,
        CritBoost,
        AttackSpeedChange,
        MoveSpeedChange,
        DamageOverTime,
        Weather,
        ExpGainModifier
    }

    public ModifierType modifierType = ModifierType.AttackChange;
    public float tickInterval = 0f;
    public bool showInPanels = true;
    public bool infinite = false;
    public bool unremovable = false;
    public CombatModifierData exlusiveWithModifier = null;
    public Texture2D icon;

    public bool IsEvasion()
    {
        return modifierType == ModifierType.Evasion;
    }

    public bool IsDamageOverTime()
    {
        return modifierType == ModifierType.DamageOverTime;
    }

    public bool IsHealthRegen()
    {
        return modifierType == ModifierType.HealthRegenFlat || modifierType == ModifierType.HealthRegenPercent;
    }
}
