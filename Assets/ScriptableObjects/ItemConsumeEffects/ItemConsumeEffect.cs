using UnityEngine;

[CreateAssetMenu(fileName = "ConsumeEffect", menuName = "Scriptable Objects/ItemConsumeEffect", order = 1)]
public class ItemConsumeEffect : ScriptableObject
{
    public enum EffectType
    {
        StatUpgrade,
        ExpBonus,
        Custom
    }

    public EffectType type;
    public int valueSelector;
    public int valueEffect;

    public void ApplyEffect(CharacterData chd)
    {
        switch (type)
        {
            case EffectType.StatUpgrade:
                chd.RecieveStatUpgrade(valueSelector, valueEffect);
                break;
            case EffectType.ExpBonus:
                chd.RecieveExp(valueEffect);
                break;
            default:
                break;
        }
    }
}
