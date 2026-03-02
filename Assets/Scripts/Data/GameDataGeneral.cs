using UnityEngine;

[System.Serializable]
public class GameDataGeneral
{
    [System.NonSerialized]
    public bool gameRunning = false;

    public float gameTime = 0f;
    public int money = 0;
}
