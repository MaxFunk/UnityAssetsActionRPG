using UnityEngine;

[System.Serializable]
public class EventFlags
{
    public enum EventFlag
    {
        InstructorDefeated = 0,
        RedSentinelDefeated = 1,
        SRankDefeated = 2
    }

    public bool[] flags = new bool[0];


    public EventFlags()
    {
        flags = new bool[3];
    }


    public bool GetFlag(EventFlag flag)
    {
        return flags[(int)flag];
    }

    // Bad design, but i dont care right now
    public void SetFlag(EventFlag flag, bool value)
    {
        flags[(int)flag] = value;
        Debug.Log($"FLAGS: {flag} set to {value}");
    }
}
