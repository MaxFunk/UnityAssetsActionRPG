using UnityEngine;
using UnityEngine.UIElements;

public class ArtNumberLabel
{
    public enum NumberType
    {
        DamagePhysical,
        DamageEther,
        DamageSoul,
        DamageEffects,
        Healing        
    }

    public Label label = new();
    public VisualElement parentContainer;

    private NumberType numType = NumberType.DamagePhysical;
    private Vector3 worldPos = Vector3.zero;

    private float timeAlive = 0f;
    private float timeFadeBegin = 0.75f;
    private float timeFade = 0.25f;
    private float maxAlive = 1f;

    private static readonly string classPlayer = "number-player";
    private static readonly string classAlly = "number-ally";
    private static readonly string classDamagePhysical = "number-damage-physical";
    private static readonly string classDamageEther = "number-damage-ether";
    private static readonly string classDamageSoul = "number-damage-soul";
    private static readonly string classDamageEffect = "number-damage-effect";
    private static readonly string classHealing = "number-healing";
    private static readonly string classCrit = "number-crit";
    private static readonly string classMissed = "number-evade";
    private static readonly string classDamageEnemy = "number-damage-enemy";

    // Returns true, if object should stay existent
    public bool ExternalUpdate(Camera camera)
    {
        timeAlive += Time.deltaTime;
        if (timeAlive > timeFadeBegin)
        {
            label.style.opacity = label.style.opacity.value - Time.deltaTime / timeFade;
        }

        if (timeAlive > maxAlive)
            return false;

        var screenPos = camera.WorldToScreenPoint(worldPos);
        label.style.left = screenPos.x;
        label.style.top = Screen.height - screenPos.y;
        label.style.display = screenPos.z >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        return true;
    }

    public void AddToUI(VisualElement container, Camera camera, Vector3 worldPosition, NumberType type, CombatHitData hitData)
    {
        // wtf is a Manipulator?????
        if (container == null) return;

        parentContainer = container;
        worldPos = worldPosition;
        numType = type;

        container.Add(label);
        label.text = hitData.value.ToString();
        label.AddToClassList(hitData.fromPlayer ? classPlayer : classAlly);
        label.style.opacity = 1f;

        var screenPos = camera.WorldToScreenPoint(worldPos);
        label.style.left = screenPos.x;
        label.style.top = Screen.height - screenPos.y;
        label.style.display = screenPos.z >= 0 ? DisplayStyle.Flex : DisplayStyle.None;

        if (hitData.fromEnemy)
        {
            label.AddToClassList(classDamageEnemy);
            if (hitData.isMissed)
            {
                label.text = "Missed";
                label.AddToClassList(classMissed);
            }
            return;
        }
        
        switch (numType) 
        {
            case NumberType.DamagePhysical:
                label.AddToClassList(classDamagePhysical); break;
            case NumberType.DamageEther:
                label.AddToClassList(classDamageEther); break;
            case NumberType.DamageSoul:
                label.AddToClassList(classDamageSoul); break;
            case NumberType.DamageEffects:
                label.AddToClassList(classDamageEffect); break;
            case NumberType.Healing:
                label.AddToClassList(classHealing); break;
            default:
                break;
        }

        if (hitData.isMissed)
        {
            label.text = "Missed";
            label.AddToClassList(classMissed);
            return;
        }

        if (hitData.isCrit)
            label.AddToClassList(classCrit);
    }
}
