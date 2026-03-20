using UnityEngine;

[CreateAssetMenu(fileName = "ArtData", menuName = "Scriptable Objects/ArtData", order = 1)]
public class ArtData : ScriptableObject
{
    public enum CastType
    {
        Direct,
        Area,
        Projectile,
        Field,
        Custom
    }

    public enum Category
    {
        Physical,
        Ether,
        Buff,
        Debuff,
        Heal,
        Soul
    }

    [Header("General")]
    public string artName = "Art";
    public CastType castType = CastType.Direct;
    public Category category = Category.Physical;
    public string descriptionLong = string.Empty;
    public string descriptionShort = string.Empty;

    [Header("Values")]
    public bool isUlt = false;
    public bool isSelfCast = false;
    public int ultCost = 0;
    public float maxCastDistance = 5f;
    public float castDuration = 1.5f;
    public float cancelAfter = 1.0f;
    public float cancelWindow = 0.5f;
    public float baseAccuracy = 0.98f;
    public float[] basePower = new float[5] { 20f, 25f, 30f, 35f, 40f };
    public float[] artCooldown = new float[5] { 10f, 9f, 8f, 7f, 6f };
    public float[] hits = new float[1] { 0.5f };

    [Header("Effects")]
    public ArtEffect[] ArtEffects;

    [Header("References")]
    public ArtCastBase ArtCastPrefab = null;
    public Texture2D icon = null;


    public bool IsAffectingEnemies()
    {
        if (category == Category.Buff || category == Category.Heal)
            return false;
        return true;
    }

    public bool IsInCastRange(CombatData caster, CombatData target)
    {
        if (caster == null || target == null) return false;

        if (isSelfCast) return true;

        return Vector3.Distance(caster.transform.position, target.transform.position) <= maxCastDistance;
    }

    public bool IsPhysicalArt()
    {
        return category == Category.Physical;
    }
}
