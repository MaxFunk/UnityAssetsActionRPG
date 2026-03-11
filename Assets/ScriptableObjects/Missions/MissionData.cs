using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Scriptable Objects/MissionData")]
public class MissionData : ScriptableObject
{
    public enum MissionType
    {
        Main,
        Side,
        Task,
        Other
    }

    public MissionType missionType = MissionType.Main;
    public string missionName = string.Empty;
    public string descriptionCompleted = string.Empty;
    public MissionStep[] missionSteps = new MissionStep[0];
    public int rewardMoney = 0;
    public ItemRecieveData[] rewardItems;


    public string MissionTypeToString()
    {
        return missionType switch
        {
            MissionType.Main => "Main Mission",
            MissionType.Side => "Side Mission",
            MissionType.Task => "Task",
            MissionType.Other => "Special Mission",
            _ => "Mission",
        };
    }
}
