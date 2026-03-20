using UnityEngine;

public class MissionGiver : MonoBehaviour
{
    public int questID = 0;

    public void GiveQuest()
    {
        GameManager.Instance.MissionManager.RecieveNewMission(questID);
    }

    public void DialogCheckMissionGiven(int stepToLoad)
    {
        if (GameManager.Instance.MissionManager.HasRecievedMission(questID))
        {
            var dialogUI = UserInterfaceManager.instance.DialogUI;
            if (dialogUI != null)
                dialogUI.LoadStep(stepToLoad, true);
        }
    }

    public void DialogCheckMissionProgress(int checkData) //int stepToLoad * 100, int stepProgress
    {
        int stepProgress = checkData % 100;
        int stepToLoad = checkData / 100;

        if (GameManager.Instance.MissionManager.HasProgressedMission(questID, stepProgress))
        {
            var dialogUI = UserInterfaceManager.instance.DialogUI;
            if (dialogUI != null)
                dialogUI.LoadStep(stepToLoad, true);
        }
    }

    public void DialogCheckMissionCompleted(int stepToLoad)
    {
        if (GameManager.Instance.MissionManager.HasCompletedMission(questID))
        {
            var dialogUI = UserInterfaceManager.instance.DialogUI;
            if (dialogUI != null)
                dialogUI.LoadStep(stepToLoad, true);
        }
    }
}
