using UnityEngine;

[System.Serializable]
public class DialogStepData
{
    public enum PromptAction
    {
        None = 0,
        GoToStep = 1,
        SkipSteps = 2,
        EndTalk = 3,
        Event = 4,
    }

    [System.Serializable]
    public struct PromptData
    {
        public string Text;
        public PromptAction Action;
        public int valueInt;
    }


    public string speakerName = string.Empty;
    public string dialogText = string.Empty;    
    public Texture2D speakerIcon = null;
    public bool hasPrompt = false;
    public bool isEnd = false;
    public int unconditionalEventIndex = -1;
    public PromptData[] promptData = new PromptData[0];
}
