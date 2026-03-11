using UnityEngine;

[CreateAssetMenu(fileName = "MissionsPreload", menuName = "Scriptable Objects/MissionsPreload")]
public class MissionsPreload : ScriptableObject
{
    public MissionData[] missionsMain = new MissionData[0];

    public MissionData[] missionsSide = new MissionData[0];

    public MissionData[] missionsTasks = new MissionData[0];

    public MissionData GetMissionData(int id)
    {
        if (id >= 0 && id < 100)
        {
            if (id >= 0 && id < missionsMain.Length)
                return missionsMain[id];
        }
        else if (id >= 100 && id < 200) 
        {
            id -= 100;
            if (id >= 0 && id < missionsSide.Length)
                return missionsSide[id];
        }
        else if (id >= 200)
        {
            id -= 200;
            if (id >= 0 && id < missionsTasks.Length)
                return missionsTasks[id];
        }

        return null;
    }

    /*public string GetMissionTypeString(int id)
    {
        return id switch
        {
            (>= 0) and (< 100) => "Main Mission",
            (>= 100) and (< 200) => "Side Mission",
            (>= 200) => "Task",
            _ => "Mission",
        };
    }*/
}
