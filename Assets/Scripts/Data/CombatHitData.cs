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
    public bool isEvaded = false;
    public bool fromPlayer = false;
    public bool fromEnemy = false;
}
