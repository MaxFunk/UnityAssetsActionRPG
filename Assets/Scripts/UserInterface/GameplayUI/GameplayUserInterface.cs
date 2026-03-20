using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayUserInterface : MonoBehaviour
{
    public struct MissionDisplayData
    {
        public string Name;
        public string UpdateText;
    }

    public VisualTreeAsset VTAItemCollectElement;

    private UIDocument document;
    private VisualElement root;

    private VisualElement containerExplore;
    private VisualElement containerCombat;

    private CombatHeroPanel[] heroPanels;
    private CombatTargetPanel targetPanel;
    private CombatArtPanel artPanel;
    private VisualElement artNumberContainerPlayer;
    private VisualElement artNumberContainerAllies;
    private VisualElement cancelRing;
    private InteractionPanel interactionPanel;
    private CombatAllyUltPanel allyUltPanel1;
    private CombatAllyUltPanel allyUltPanel2;
    private VisualElement missionUpdatePanel;
    private PanelItemCollectList panelItemCollect = new();

    private Camera currentCamera;
    private List<ArtNumberLabel> artNumberLabels = new();
    private readonly List<MissionDisplayData> queuedMissionUpdates = new();

    private bool checkMissionUpdate = true;
    private bool cancelRingActive = false;
    private float timerRemainingMissionDisplay = 0f;
    private float timerBetweenMissionDisplay = 0f;


    void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement.Q<VisualElement>("RootContainer");
        root.dataSource = this;

        containerExplore = document.rootVisualElement.Q<VisualElement>("ContainerExplore");
        containerCombat = document.rootVisualElement.Q<VisualElement>("ContainerCombat");
        containerCombat.RemoveFromClassList("active");
        containerExplore.AddToClassList("active");

        heroPanels = new CombatHeroPanel[3];
        for (int i = 0; i < heroPanels.Length; i++)
        {
            heroPanels[i] = new CombatHeroPanel();
            heroPanels[i].FetchVisualElements(root, i);
        }

        targetPanel = new CombatTargetPanel();
        targetPanel.FetchVisualElements(root);
        targetPanel.SetVisibility(false);

        artPanel = new CombatArtPanel();
        artPanel.FetchVisualElements(root);
        artPanel.SetVisibility(false);

        artNumberContainerPlayer = root.Q<VisualElement>("NumberContainerPlayer");
        artNumberContainerAllies = root.Q<VisualElement>("NumberContainerAllies");

        var camControllers = FindObjectsByType<CameraController>(FindObjectsSortMode.None);
        if (camControllers.Count() > 0) 
        {
            currentCamera = camControllers[0].Camera;
        }

        cancelRing = root.Q<VisualElement>("CancelRingContainer");
        cancelRing.style.opacity = 0f;

        interactionPanel = new();
        interactionPanel.FetchVisualElements(root);

        var allyUltPanelVEs = root.Query<VisualElement>("AllyUltPanel").ToList();
        allyUltPanel1 = new CombatAllyUltPanel();
        allyUltPanel1.BindToContainer(allyUltPanelVEs[0]); // unsafe access
        allyUltPanel2 = new CombatAllyUltPanel();
        allyUltPanel2.BindToContainer(allyUltPanelVEs[1]); // unsafe access

        missionUpdatePanel = root.Q<VisualElement>("MissionUpdatePanel");
        missionUpdatePanel.style.visibility = Visibility.Hidden;

        panelItemCollect.LinkToUI(root.Q<VisualElement>("PanelItemCollect"), VTAItemCollectElement);
    }

    private void Update()
    {
        List<ArtNumberLabel> copiedNumLabels = new();
        foreach (var numLabel in artNumberLabels)
        {
            var keep = numLabel.ExternalUpdate(currentCamera);
            if (keep)
                copiedNumLabels.Add(numLabel);
            else
                numLabel.parentContainer.Remove(numLabel.label);
        }

        artNumberLabels = copiedNumLabels;

        if (cancelRingActive)
        {
            float newOpacity = cancelRing.style.opacity.value - Time.deltaTime * 2f;
            if (newOpacity < 0f)
            {
                cancelRing.style.opacity = 0f;
                cancelRingActive = false;
            }
            else
            {
                cancelRing.style.opacity = newOpacity;
            }
        }

        if (root.style.display == DisplayStyle.None) return;

        allyUltPanel1.ExternalUpdate();
        allyUltPanel2.ExternalUpdate();
        panelItemCollect.ExternalUpdate();
        UpdateMissionPanel();
    }

    private void UpdateMissionPanel()
    {
        if (!checkMissionUpdate) return;

        if (timerRemainingMissionDisplay > 0f)
        {
            timerRemainingMissionDisplay -= Time.deltaTime;

            if (timerRemainingMissionDisplay <= 0f)
                HideMissionPanel();

            return;
        }

        if (timerBetweenMissionDisplay > 0f)
        {
            timerBetweenMissionDisplay -= Time.deltaTime;
            return;
        }

        if (queuedMissionUpdates.Count > 0)
            ShowNextMission();
    }

    private void ShowNextMission()
    {
        missionUpdatePanel.dataSource = queuedMissionUpdates[0];
        queuedMissionUpdates.RemoveAt(0);
        missionUpdatePanel.style.visibility = Visibility.Visible;
        timerRemainingMissionDisplay = 3f;
    }

    private void HideMissionPanel()
    {
        missionUpdatePanel.dataSource = null;
        missionUpdatePanel.style.visibility = Visibility.Hidden;
        timerBetweenMissionDisplay = 0.33f;
    }


    public void OnHeroLoad(CombatData heroData, int partyIndex)
    {
        if (partyIndex >= 0 && partyIndex < heroPanels.Length)
            heroPanels[partyIndex].AddDataSource(heroData);

        if (partyIndex == 0)
            artPanel.AddDataSource(heroData);

        if (partyIndex == 1)
            allyUltPanel1.LoadData(heroData);

        if (partyIndex == 2)
            allyUltPanel2.LoadData(heroData);
    }

    public void OnNewTarget(CombatData newTarget)
    {
        targetPanel.AddDataSource(newTarget);
        targetPanel.SetVisibility(newTarget != null);
        artPanel.SetVisibility(newTarget != null);
    }

    public void SetVisibility(bool isVisible) 
    {
        //root.style.visibility = visibility ? Visibility.Visible : Visibility.Hidden;
        root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }


    public void CreateArtNumber(ArtNumberLabel.NumberType type, Vector3 worldPos, CombatHitData hitData)
    {
        if (currentCamera == null) return;

        //var screenPos = currentCamera.WorldToScreenPoint(worldPos);
        //var viewportPos = currentCamera.WorldToViewportPoint(worldPos);
        //Debug.Log($"{screenPos} {viewportPos}");

        worldPos.x += Random.Range(-0.5f, 0.5f);
        worldPos.y += 1f;
        worldPos.z += Random.Range(-0.5f, 0.5f);

        ArtNumberLabel newNumLabel = new();
        newNumLabel.AddToUI(hitData.fromPlayer ? artNumberContainerPlayer : artNumberContainerAllies, currentCamera, worldPos, type, hitData);
        artNumberLabels.Add(newNumLabel);
    }

    public void ShowCancelRing()
    {
        cancelRingActive = true;
        cancelRing.style.opacity = 1f;
    }

    public void UpdateInteractionPanel(Interactable interactable)
    {
        interactionPanel.UpdateVisualElements(interactable);
    }


    public void OnCombatStart()
    {
        containerExplore.RemoveFromClassList("active");
        containerCombat.AddToClassList("active");
        //checkMissionUpdate = false;
    }

    public void OnCombatEnd()
    {
        containerCombat.RemoveFromClassList("active");
        containerExplore.AddToClassList("active");
        //checkMissionUpdate = true;
    }


    public void QueueMissionUpdate(Mission mission)
    {
        if (mission == null) return;

        var newUpdate = new MissionDisplayData
        {
            Name = mission.GetMissionName(),
            UpdateText = mission.GetMissionUpdateInfo()
        };
        queuedMissionUpdates.Add(newUpdate);
    }

    public void OnItemRecieve(ItemManager.Item item, int amountRecieved)
    {
        panelItemCollect.QueueNewItem(item, amountRecieved);
    }
}
