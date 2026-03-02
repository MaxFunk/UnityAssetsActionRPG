using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "Scriptable Objects/DialogData")]
public class DialogData : ScriptableObject
{
    [Header("Data")]
    public DialogStepData[] dialogSteps = new DialogStepData[] { };
}
