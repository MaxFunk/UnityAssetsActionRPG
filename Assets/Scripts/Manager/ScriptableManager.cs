using UnityEngine;
using System.Collections.Generic;
using System;

public class ScriptableManager : MonoBehaviour
{
    public List<CharacterDataStatic> characterDataStatic;
    public ArtsPreload artsPreload;
    public ItemPreload itemPreload;

    public static ScriptableManager instance;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public ArtData GetArtData(int index)
    {
        if (index < 0 || index >= artsPreload.artDatas.Length)
            return null;
        
        return artsPreload.artDatas[index];
    }

    public int FindArtId(ArtData artData)
    {
        if (artData == null) return -1;

        return Array.FindIndex(artsPreload.artDatas, art => art == artData);
    }


    public CharacterDataStatic GetCharacterDataStatic(int index)
    {
        if (index < 0 || index >= characterDataStatic.Count)
            return null;

        return characterDataStatic[index];
    }
}
