using UnityEngine;

public class DebugMissionGiver : MonoBehaviour
{
    public int questID = 0;

    public void GiveQuest()
    {
        GameManager.Instance.MissionManager.RecieveNewMission(questID);
    }
}
