using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameDataGeneral
{
    [System.NonSerialized]
    public bool gameRunning = false;

    public float gameTime = 0f;
    public int money = 0;
    public int sceneIndex = 0;
    public int sceneSpawner = 0;

    public List<int> chestIdsOpened = new();


    public void RecieveMoney(int value)
    {
        money = Mathf.Clamp(money + value, 0, 999999);
    }

    public void AddChestId(int id)
    {
        if (id < 0 || chestIdsOpened.Contains(id)) return;

        chestIdsOpened.Add(id);
    }

    public bool HasChestId(int id)
    {
        return chestIdsOpened.Contains(id);
    }
}
