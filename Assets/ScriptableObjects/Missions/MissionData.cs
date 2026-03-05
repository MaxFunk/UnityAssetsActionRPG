using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Scriptable Objects/MissionData")]
public class MissionData : ScriptableObject
{
    public string missionName = string.Empty;
    public string descriptionCompleted = string.Empty;
    public MissionStep[] missionSteps = new MissionStep[0];
    public int rewardMoney = 0;
    public int rewardItem = 0; //replace with item drop data?
}
