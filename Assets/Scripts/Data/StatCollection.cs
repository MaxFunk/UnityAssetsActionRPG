using UnityEngine;

[System.Serializable]
public class StatCollection
{
    public int health = 0;
    public int attack = 0;
    public int ether = 0;
    public int agility = 0;
    public int luck = 0;

    public float physicalDefense = 0f;
    public float etherDefense = 0f;


    public void InterpolateStats(int level, StatCollection statsLevel1, StatCollection statsLevel99)
    {
        health = Interpolate(statsLevel1.health, statsLevel99.health, level);
        attack = Interpolate(statsLevel1.attack, statsLevel99.attack, level);
        ether = Interpolate(statsLevel1.ether, statsLevel99.ether, level);
        agility = Interpolate(statsLevel1.agility, statsLevel99.agility, level);
        luck = Interpolate(statsLevel1.luck, statsLevel99.luck, level);
        physicalDefense = statsLevel1.physicalDefense;
        etherDefense = statsLevel1.etherDefense;
        return;
    }


    public static StatCollection AccumulateStats(StatCollection baseStats, StatCollection bonusStats)
    {
        var accumulatedStats = new StatCollection();
        accumulatedStats.health = baseStats.health + bonusStats.health;
        accumulatedStats.attack = baseStats.attack + bonusStats.attack;
        accumulatedStats.ether = baseStats.ether + bonusStats.ether;
        accumulatedStats.agility = baseStats.agility + bonusStats.agility;
        accumulatedStats.luck = baseStats.luck + bonusStats.luck;
        accumulatedStats.physicalDefense = baseStats.physicalDefense;
        accumulatedStats.etherDefense = baseStats.etherDefense;
        return accumulatedStats;
    }

    public static int Interpolate(int statLevel1, int statLevel99, int level)
    {
        return Mathf.RoundToInt(statLevel1 + (level - 1f) * (statLevel99 - statLevel1) / 98f);
    }

    public void CopyFromOther(StatCollection otherStats)
    {
        health = otherStats.health;
        attack = otherStats.attack;
        ether = otherStats.ether;
        agility = otherStats.agility;
        luck = otherStats.luck;
        physicalDefense = otherStats.physicalDefense;
        etherDefense = otherStats.etherDefense;
    }
}
