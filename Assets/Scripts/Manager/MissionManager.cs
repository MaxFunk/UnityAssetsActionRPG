using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //remove later
public class MissionManager
{
    public List<Mission> missions = new();


    public void RecieveNewMission(int missionId)
    {
        foreach (var mission in missions)
        {
            if (mission.id == missionId)
                return; // already recieved this mission
        }

        var missionData = ScriptableManager.instance.missionsPreload.GetMissionData(missionId);
        if (missionData == null) return;

        var newMission = new Mission();
        newMission.LoadMissionData(missionData, missionId);
        missions.Add(newMission);
    }

    public void EnemyWasDefeated(int enemyId)
    {
        foreach (var mission in missions)
        {
            mission.CheckEvent(MissionStep.ObjectiveType.EnemyDefeat, enemyId, 1);
        }
    }

    public void AreaWasReached(int areaTriggerId)
    {
        foreach (var mission in missions)
        {
            mission.CheckEvent(MissionStep.ObjectiveType.AreaReached, areaTriggerId, 1);
        }
    }

    public void PlayerInteracted(int interactionId)
    {
        foreach (var mission in missions)
        {
            mission.CheckEvent(MissionStep.ObjectiveType.InteractedWith, interactionId, 1);
        }
    }

    public void ItemCollected(int interactionId, int amount)
    {
        foreach (var mission in missions)
        {
            mission.CheckIfItemsInInventory();
            mission.CheckEvent(MissionStep.ObjectiveType.ItemCollect, interactionId, amount);
        }
    }


    public void LoadMissionSaveData(List<Mission> savedMissions)
    {
        missions = new();

        foreach (var mission in savedMissions)
        {
            var missionData = ScriptableManager.instance.missionsPreload.GetMissionData(mission.id);
            if (missionData == null) continue;

            mission.missionData = missionData;
            missions.Add(mission);
        }
    }
}
