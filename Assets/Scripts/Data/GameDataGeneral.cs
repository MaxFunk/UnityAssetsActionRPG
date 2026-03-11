using UnityEngine;

[System.Serializable]
public class GameDataGeneral
{
    [System.NonSerialized]
    public bool gameRunning = false;

    public float gameTime = 0f;
    public int money = 0;


    public void RecieveMoney(int value)
    {
        money = Mathf.Clamp(money + value, 0, 999999);
        Debug.Log($"DEBUG: Current Money: {money}");
    }
}
