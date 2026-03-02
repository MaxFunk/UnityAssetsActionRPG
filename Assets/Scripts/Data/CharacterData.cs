using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    public int characterId = -1;
    public int level = 1;
    public int totalExp = 0;

    public int artUpgradePoints = 0;
    public int statUpgradesRecieved = 0;
    public StatCollection statsBonus = new();
    public List<ArtSaveData> artSaveDatas = new();

    public int[] gearIds = new int[3] { -1, -1, -1 };

    [System.NonSerialized]
    public StatCollection statsBase = new();
    [System.NonSerialized]
    public StatCollection statsAccumulated = new();
    [System.NonSerialized]
    public CombatData refCombatData = null;
    [System.NonSerialized]
    public CharacterDataStatic staticData = null;
    //private float movementSpeed = 100f;


    public void Initialize(int index, bool firstCreation = false)
    {
        characterId = index;
        staticData = ScriptableManager.instance.GetCharacterDataStatic(characterId);

        if (firstCreation)
        {
            level = staticData.startLevel;
            totalExp = (level - 1) * 10;
            artSaveDatas = staticData.initArtSaveDatas;
        }

        statsBase.InterpolateStats(level, staticData.statsLevel1, staticData.statsLevel99);
        statsAccumulated = StatCollection.AccumulateStats(statsBase, statsBonus);
    }


    public int[] GetActiveArtIds()
    {
        int[] activeArtIds = new int[6] { -1, -1, -1, -1, -1, -1 };

        foreach (var artSaveData in artSaveDatas) 
        {
            if (artSaveData != null && artSaveData.artSlot < 6 && artSaveData.artSlot >= 0)
                activeArtIds[artSaveData.artSlot] = artSaveData.artId;
        }

        return activeArtIds;
    }

    public List<ArtData> GetUnequipedArts(bool getUlts)
    {
        List<ArtData> otherArts = new();

        foreach (var artSaveData in artSaveDatas)
        {
            if (artSaveData != null && artSaveData.artSlot < 0)
            {
                var artData = ScriptableManager.instance.GetArtData(artSaveData.artId);
                if (artData == null) continue;

                if ((artData.isUlt && getUlts) || (!artData.isUlt && !getUlts))
                    otherArts.Add(artData);
            }
        }

        return otherArts;
    }


    public void RecieveExp(int expAmount)
    {
        totalExp += expAmount;
        int newLevel = Mathf.Min(1 + Mathf.FloorToInt(totalExp / 10f), 99);

        if (newLevel == level) return;

        for (int lvl = level + 1; lvl <= newLevel; lvl++)
        {
            Debug.Log($"Level UP: {lvl}");
            // Check if Art is learned on Level up
            var id = staticData.GetArtOnLevelUp(lvl);
            if (id > 0) LearnNewArt(id);
        }

        level = newLevel;
        statsBase.InterpolateStats(level, staticData.statsLevel1, staticData.statsLevel99);
        statsAccumulated = StatCollection.AccumulateStats(statsBase, statsBonus);

        refCombatData.LoadFromCharacterData(this);
    }

    public void RecieveStatUpgrade(int selector, int value)
    {
        switch (selector)
        {
            case 0:
                statsBonus.health += value;
                break;
            default:
                break;
        }

        statUpgradesRecieved += 1;
        statsAccumulated = StatCollection.AccumulateStats(statsBase, statsBonus);
        refCombatData.LoadFromCharacterData(this);
    }

    public void SwapArtIds(int slot, ArtData artData)
    {
        if (slot < 0 || slot >= 6)
            return;

        int artId = ScriptableManager.instance.FindArtId(artData);

        foreach (var artSaveData in artSaveDatas)
        {
            if (artSaveData.artId == artId)
            {
                artSaveData.artSlot = slot;
            }

            if (artSaveData.artSlot == slot && artSaveData.artId != artId)
            {
                artSaveData.artSlot = -1;
            }
        }

        //refCombatData.LoadFromCharacterData(this);
    }

    public void LearnNewArt(int artId)
    {
        Debug.Log($"Art Learned: {artId}");
        ArtSaveData newArt = new()
        {
            artId = artId,
            artLevel = 0,
            artSlot = GetNextFreeArtSlot()
        };
        artSaveDatas.Add(newArt);

        if (refCombatData != null)
        {
            refCombatData.LoadFromCharacterData(this);

            if (refCombatData.isPlayerControlled)
                UserInterfaceManager.instance.GameplayUI.OnHeroLoad(refCombatData, 0);
        }
    }

    public void SwapGearIds(int slot, int newId)
    {
        if (slot < 0 || slot >= gearIds.Length)
            return;

        int previousId = gearIds[slot];
        gearIds[slot] = newId;
        var itemManager = GameManager.Instance.ItemManager;
        itemManager.ChangeItem(ItemData.ItemType.Gear, previousId, 1);
        itemManager.ChangeItem(ItemData.ItemType.Gear, newId, -1);
    }

    public ArtSaveData GetArtSaveData(int slot)
    {
        foreach (var artSaveData in artSaveDatas)
        {
            if (artSaveData.artSlot == slot)
                return artSaveData;
        }
        return null;
    }

    public int GetNextFreeArtSlot()
    {
        List<int> freeSlots = new () { 0, 1, 2, 3, 4, 5 };

        foreach (var artSaveData in artSaveDatas)
        {
            if (freeSlots.Contains(artSaveData.artSlot))
            {
                freeSlots.Remove(artSaveData.artSlot);
            }
        }

        if (freeSlots.Count > 0)
            return freeSlots[0];
        return -1;
    }
}
