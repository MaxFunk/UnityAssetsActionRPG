using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuContainerMissions : MenuContainer
{
    private List<Mission> mainMissions = new();
    private List<Mission> sideMissions = new();
    private List<Mission> taskMissions = new();

    public Visibility visibilityDetail = Visibility.Visible;
    public Mission detailMission = null;
    public MissionData detailMissionData = null;

    private ScrollView scrollView = null;
    private int curType = 0;
    private int curIndex = 0;


    public override void PrepareView(VisualElement rootElement)
    {
        containerObj = rootElement != null ? rootElement.Q<VisualElement>("MissionsView") : containerObj;
        if (containerObj == null) { return; }

        scrollView = containerObj.Q<ScrollView>("ScrollView");
        var headerLabels = containerObj.Query<Label>("HeaderLabel").ToList();
        for (int i = 0; i < headerLabels.Count; i++)
        {
            headerLabels[i].RegisterCallback<ClickEvent, int>(OnHeaderLabelClick, i);
        }

        FetchAndSortMissions();
        LoadMissionList(0);
        containerObj.dataSource = this;
    }

    public override void CancelEvent()
    {
        mainMenuEvents.ChangeMenuState(MainMenuEvents.MenuState.Main);
    }

    public override void ConfirmEvent()
    {

    }

    public override void SpecialEvent()
    {

    }

    public override void DirectionalEvent(Vector2 navInput)
    {

    }   
    

    private void FetchAndSortMissions()
    {
        mainMissions.Clear();
        sideMissions.Clear();
        taskMissions.Clear();

        var missions = GameManager.Instance.MissionManager.missions;

        foreach(var mission in missions)
        {
            if (mission == null) continue;

            if (mission.id >= 0 && mission.id < 100)
            {
                mainMissions.Add(mission);
            }
            else if (mission.id >= 100 && mission.id < 200)
            {
                sideMissions.Add(mission);
            }
            else if (mission.id >= 200)
            {
                taskMissions.Add(mission);
            }
        }

        mainMissions.Sort((a, b) => a.id.CompareTo(b.id));
        sideMissions.Sort((a, b) => a.id.CompareTo(b.id));
        taskMissions.Sort((a, b) => a.id.CompareTo(b.id));
    }

    private void LoadMissionList(int type)
    {
        scrollView.Clear();
        
        curType = type;
        List<Mission> missions = curType switch
        {
            0 => mainMissions,
            1 => sideMissions,
            _ => taskMissions
        };

        for (int i = 0; i < missions.Count; i++)
        {
            Label newLabel = new()
            {
                text = missions[i].missionData.missionName
            };
            newLabel.RegisterCallback<ClickEvent, int>(OnListElementClick, i);
            newLabel.AddToClassList("list-label");
            scrollView.Add(newLabel);
        }

        LoadDetail(0);
    }

    private void LoadDetail(int index)
    {
        curIndex = index;

        List<Mission> missions = curType switch
        {
            0 => mainMissions,
            1 => sideMissions,
            _ => taskMissions
        };

        if (index < 0 || index >= missions.Count)
        {
            visibilityDetail = Visibility.Hidden;
            detailMission = null;
            detailMissionData = null;
            return;
        }

        visibilityDetail = Visibility.Visible;
        detailMission = missions[index];
        detailMissionData = detailMission.missionData;
    }

    private void OnListElementClick(ClickEvent evt, int index)
    {
        LoadDetail(index);
    }

    private void OnHeaderLabelClick(ClickEvent evt, int index)
    {
        LoadMissionList(index);
    }
}
