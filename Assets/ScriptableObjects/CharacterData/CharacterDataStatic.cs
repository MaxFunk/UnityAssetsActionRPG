using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterDataStatic", menuName = "Scriptable Objects/CharacterDataStatic", order = 1)]
public class CharacterDataStatic : ScriptableObject
{
    public string characterName = "Character";
    public int characterIndex = 0;
    public int startLevel = 1;
    public StatCollection statsLevel1;
    public StatCollection statsLevel99;
    public List<ArtSaveData> initArtSaveDatas = new();
    public List<Vector2Int> levelUpArts = new();
    public AutoAttackData autoAttackData = new();
    public Texture2D iconMenuFull = null;

    public int GetArtOnLevelUp(int level)
    {
        foreach (var entry in levelUpArts) 
        { 
            if (entry.x == level) return entry.y;
        }

        return -1;
    }
}
