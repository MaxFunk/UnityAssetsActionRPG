using UnityEngine;

[System.Serializable]
public class Mission
{
    public int id = -1;
    public bool isCompleted = false;
    public int curStep = -1;
    public int stepValue = 0;

    [System.NonSerialized]
    public MissionData missionData = null;
    
    public void LoadMissionData(MissionData missionData, int id, int step = 0)
    {
        this.missionData = missionData;
        this.id = id;
        isCompleted = false;

        LoadStep(step);
    }

    public void CheckEvent(MissionStep.ObjectiveType objectiveType, int eventId, int eventValue)
    {
        if (missionData == null || isCompleted) return;

        var curStepData = missionData.missionSteps[curStep];

        if (curStepData.objectiveType == objectiveType && curStepData.objectiveId == eventId)
        {
            stepValue += eventValue;
            if (stepValue >= curStepData.objectiveCount)
                LoadStep(curStep + 1);
        }
    }

    private void LoadStep(int step)
    {
        if (missionData == null) return;

        curStep = step;
        stepValue = 0;
        
        if (curStep >= missionData.missionSteps.Length)
        {
            isCompleted = true;
            Debug.Log($"MISSION FINISHED: {missionData.missionName}"); //notify that quest is finished
            return;
        }

        CheckIfItemsInInventory();
    }

    public void CheckIfItemsInInventory()
    {
        if (missionData == null || isCompleted) return;

        var curStepData = missionData.missionSteps[curStep];

        if (curStepData.objectiveType == MissionStep.ObjectiveType.ItemHave)
        {
            stepValue = GameManager.Instance.ItemManager.GetItemAmount(curStepData.objectiveId);
            if (stepValue >= curStepData.objectiveCount)
                LoadStep(curStep + 1);
        }
    }




    public string GetStepText()
    {
        if (isCompleted)
            return "Completed";

        return $"Step {curStep + 1} of {missionData.missionSteps.Length}";
    }

    public string GetProgressText()
    {
        if (isCompleted)
            return "";

        return $"Progress: {stepValue} / {missionData.missionSteps[curStep].objectiveCount}";
    }

    public string GetDescriptionText()
    {
        if (isCompleted)
            return missionData.descriptionCompleted;

        return missionData.missionSteps[curStep].stepDescription;
    }
}
