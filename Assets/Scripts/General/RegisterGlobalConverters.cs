using UnityEngine;
using UnityEngine.UIElements;

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
        var group = new ConverterGroup("Float To Percent");

        group.AddConverter((ref float value) =>
        {
            return $"{value * 100f:0}%";
        });

        ConverterGroups.RegisterConverterGroup(group);
    }
}

