using UnityEngine;

public class CombatHitData
{
    public enum ValueType
    {
        damage,
        healing,
        effect,
        effectDamage
    }

    public ValueType valueType = ValueType.damage;
    public int value = 0;
    public bool isCrit = false;
    public bool isMissed = false;
    public bool fromPlayer = false;
    public bool fromEnemy = false;
}
