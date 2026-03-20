using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class DialogUserInterface : MonoBehaviour
{
    UIDocument document;
    CameraController cameraController;
    InputHandler inputHandler;
    DialogData dialogData;
    VisualElement promptContainer;
    NPCController npc;

    [Header("Params")]
    public float BlockInputTime = 0.25f;
    public float LettersPerSecond = 30f;

    [Header("Make Read Only")]
    public DialogStepData stepData = null;
    public int step = -1;
    public string dialogText = string.Empty;
    public float heightPromptContainer = 60f;
    public DisplayStyle displayPromptContainer = DisplayStyle.None;
    public DisplayStyle displayIconContinue = DisplayStyle.None;    

    private readonly float promptElementHeight = 60f;
    private List<Label> promptLabels = new();
    private int promptIndex = 0;
    private float timerBlockInput = 0f;
    private bool isWritingText = false;
    private float lettersVisible = 0f;

    public UnityEvent[] DialogEvents = new UnityEvent[0];


    private void Update()
    {
        if (isWritingText)
        {
            lettersVisible += LettersPerSecond * Time.deltaTime;
            int roundedLettersVisible = Mathf.FloorToInt(lettersVisible);

            if (roundedLettersVisible >= stepData.dialogText.Length)
                OnFinishedWriting();
            else
                dialogText = stepData.dialogText[..roundedLettersVisible];
        }

        if (inputHandler != null)
        {
            if (timerBlockInput > 0f)
                timerBlockInput -= Time.deltaTime;

            if (inputHandler.GetUIConfirmInputDown() && timerBlockInput <= 0f)
            {
                if (isWritingText)
                    OnFinishedWriting();
                else
                {
                    CheckUnconditionalEvent();
                    if (stepData.hasPrompt)
                        EvalPromptAction();
                    else
                        LoadStep(step + 1);
                }
                    

                timerBlockInput = BlockInputTime;
            }

            if (timerBlockInput <= 0f && !isWritingText)
            {
                var dirInput = inputHandler.GetNavigateInput();
                EvalUpDownInput(dirInput.y);
            }
        }
    }


    public void DialogBegin(NPCController npc)
    {
        if (npc.DialogData == null || npc.DialogData.dialogSteps.Length < 1)
        {
            UserInterfaceManager.instance.DestroyDialogUI();
            return;
        }

        document = GetComponent<UIDocument>();
        document.rootVisualElement.Q<VisualElement>("ContainerMain").dataSource = this;
        PreparePromptElements();

        this.npc = npc;
        dialogData = npc.DialogData;
        DialogEvents = npc.DialogEvents;
        inputHandler = InputHandler.instance;
        UserInterfaceManager.instance.GameplayUI.SetVisibility(false);

        var heroChars = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
        foreach (var character in heroChars)
        {
            if (character != null)
                character.OnDialogStart();
        }

        cameraController = FindAnyObjectByType<CameraController>(); // better if more than one camera: search for the one linked to player/active?
        if (cameraController != null) 
        {
            cameraController.SetDesiredPosition(npc.transform.position);
            cameraController.SetRotation(-npc.transform.forward, 30f);
        }

        LoadStep(0);
        CheckInitialEvents(); // not optimal
    }

    public void DialogEnd() 
    {
        if (npc != null)
            npc.OnEndTalking();

        var heroChars = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
        foreach (var character in heroChars)
        {
            if (character != null)
                character.OnDialogEnd();
        }

        UserInterfaceManager.instance.GameplayUI.SetVisibility(true);
        UserInterfaceManager.instance.DestroyDialogUI();
    }

    private void PreparePromptElements()
    {
        promptContainer = document.rootVisualElement.Q<VisualElement>("PromptContainer");
        promptLabels = promptContainer.Query<Label>("LabelPrompt").ToList();
    }

    public void LoadStep(int nextStep, bool ignoreEnd = false)
    {
        step = nextStep;        
        if (step >= dialogData.dialogSteps.Length || (stepData != null && stepData.isEnd && !ignoreEnd))
        {
            DialogEnd();
            return;
        }

        stepData = dialogData.dialogSteps[step];
        displayPromptContainer = DisplayStyle.None;
        displayIconContinue = DisplayStyle.None;
        dialogText = string.Empty;
        isWritingText = true;
        lettersVisible = 0f;

        if (stepData.hasPrompt == false) return; // no need to change prompt?

        heightPromptContainer = stepData.promptData.Length * promptElementHeight;
        for (int i = 0; i < promptLabels.Count(); i++) 
        {
            if (i < stepData.promptData.Length)
            {
                promptLabels[i].text = stepData.promptData[i].Text;
                promptLabels[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                promptLabels[i].style.display = DisplayStyle.None;
            }
        }

        UpdatePromptIndex(-99); // clamps to zero;
    }

    private void UpdatePromptIndex(int change)
    {
        promptIndex = Mathf.Clamp(promptIndex + change, 0, stepData.promptData.Length - 1);
        for (int i = 0; i < promptLabels.Count(); i++)
        {
            if (i == promptIndex)
            {
                promptLabels[i].AddToClassList("prompt-selected");
            }
            else
            {
                promptLabels[i].RemoveFromClassList("prompt-selected");
            }
        }
    }

    private void EvalUpDownInput(float value)
    {
        if (stepData.hasPrompt == false) return;

        if (value > 0.1f)
        {
            UpdatePromptIndex(-1);
            timerBlockInput = BlockInputTime;
        }

        if (value < -0.1f)
        {
            UpdatePromptIndex(1);
            timerBlockInput = BlockInputTime;
        }
    }

    private void EvalPromptAction()
    {
        var promptData = stepData.promptData[promptIndex]; // maybe be unsafe
        switch (promptData.Action)
        {
            case DialogStepData.PromptAction.None:
                LoadStep(step + 1);
                break;
            case DialogStepData.PromptAction.GoToStep:
                LoadStep(promptData.valueInt);
                break;
            case DialogStepData.PromptAction.SkipSteps:
                LoadStep(step + 1 + promptData.valueInt);
                break;
            case DialogStepData.PromptAction.EndTalk:
                DialogEnd();
                break;
            case DialogStepData.PromptAction.Event:
                CheckPromptEvent(promptData.valueInt);
                LoadStep(step + 1);
                break;
            default:
                DialogEnd();
                break;
        }
    }

    private void OnFinishedWriting()
    {
        isWritingText = false;
        dialogText = stepData.dialogText;
        displayPromptContainer = stepData.hasPrompt ? DisplayStyle.Flex : DisplayStyle.None;

        var continueCondition = step < dialogData.dialogSteps.Length - 1 && !stepData.isEnd;
        displayIconContinue = continueCondition ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void CheckInitialEvents()
    {
        foreach (var eventIndex in dialogData.initialEventIndices)
        {
            if (eventIndex < 0 || eventIndex >= DialogEvents.Length) continue;

            DialogEvents[eventIndex]?.Invoke();
        }
    }

    private void CheckUnconditionalEvent()
    {
        var eventIndex = stepData.unconditionalEventIndex;
        if (eventIndex < 0 || eventIndex >= DialogEvents.Length) return;

        DialogEvents[eventIndex]?.Invoke();
    }

    private void CheckPromptEvent(int eventIndex)
    {
        if (eventIndex < 0 || eventIndex >= DialogEvents.Length) return;

        DialogEvents[eventIndex]?.Invoke();
    }
}
