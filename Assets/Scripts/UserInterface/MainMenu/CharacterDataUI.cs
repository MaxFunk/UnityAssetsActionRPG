using UnityEngine;

public class CharacterDataUI
{
    public CharacterData characterData;
    public CharacterDataStatic staticData;
    public StatCollection stats;
    public ArtData[] artDatas;

    public void LoadFromCharacterData(CharacterData characterData)
    {
        this.characterData = characterData;
        staticData = characterData.staticData;

        // load stats and apply gear modifications
        stats = new StatCollection();
        stats.CopyFromOther(characterData.statsAccumulated);
        var itemPreload = ScriptableManager.instance.itemPreload;
        for (int i = 0; i < characterData.gearIds.Length; i++)
        {
            var gear = itemPreload.GetGear(characterData.gearIds[i]);
            if (gear != null)
                gear.gearData.ApplyGearEffectUI(stats);
        }

        artDatas = new ArtData[6];
        var artIds = characterData.GetActiveArtIds();
        for (int i = 0; i < artIds.Length; i++)
        {
            artDatas[i] = ScriptableManager.instance.GetArtData(artIds[i]);
        }
    }

    public void ReloadData()
    {
        // reload this
        LoadFromCharacterData(characterData);

        // update character
        var combatData = characterData.refCombatData;
        if (combatData != null)
            combatData.LoadFromCharacterData(characterData);
    }


    public ItemData GetGearItemData(int index)
    {
        if (index < 0 || index > 2 || characterData == null) 
            return null;
        
        return ScriptableManager.instance.itemPreload.GetGear(characterData.gearIds[index]);
    }
}
