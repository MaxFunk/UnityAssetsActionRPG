using UnityEngine;

[System.Serializable]
public class MissionStep
{
    public enum ObjectiveType
    {
        EnemyDefeat,
        ItemHave,
        ItemCollect,
        InteractedWith,
        AreaReached
    }

    public string stepDescription = string.Empty;
    public ObjectiveType objectiveType = ObjectiveType.EnemyDefeat;
    public int objectiveId = -1;
    public int objectiveCount = 0;
}
