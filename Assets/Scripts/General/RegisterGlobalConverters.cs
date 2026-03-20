using UnityEngine;
using UnityEngine.UIElements;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

static class RegisterGlobalConverters
{
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
    static void Register()
    {
        var groupFloat = new ConverterGroup("Float To Percent");
        groupFloat.AddConverter((ref float value) =>
        {
            return $"{value * 100f:0}%";
        });
        ConverterGroups.RegisterConverterGroup(groupFloat);

        var groupFloat2 = new ConverterGroup("Float Ceiled");
        groupFloat2.AddConverter((ref float value) =>
        {
            return $"{Mathf.Ceil(value)}";
        });
        ConverterGroups.RegisterConverterGroup(groupFloat2);

        var groupInt = new ConverterGroup("Int to Amount");
        groupInt.AddConverter((ref int value) =>
        {
            return $"x{value}";
        });
        ConverterGroups.RegisterConverterGroup(groupInt);

        RegisterMission("Mission to Name", m => m?.GetMissionName() ?? "");
        RegisterMission("Mission to UpdateInfo", m => m?.GetMissionUpdateInfo() ?? "");
        RegisterMission("Mission to Step", m => m?.GetStepText() ?? "");
        RegisterMission("Mission to Progress", m => m?.GetProgressText() ?? "");
        RegisterMission("Mission to Description", m => m?.GetDescriptionText() ?? "");
    }


    static void RegisterMission(string name, Func<Mission, string> func)
    {
        var group = new ConverterGroup(name);
        group.AddConverter((ref Mission mission) =>
        {
            return mission == null ? "" : func(mission);
        });
        ConverterGroups.RegisterConverterGroup(group);
    }
}

