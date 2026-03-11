using UnityEngine;

[System.Serializable]
public class AutoAttackData
{
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public float attackDuration = 1.5f;

    public float[] hitAfter = { 1f };
    public float cancelStart = 1.1f;
    public float cancelDuration = 0.3f;
    public float damageModifier = 1f;
}
