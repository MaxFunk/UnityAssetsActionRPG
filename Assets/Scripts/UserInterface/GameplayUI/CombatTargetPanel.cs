using UnityEngine;
using UnityEngine.UIElements;

public class CombatTargetPanel
{
    private VisualElement panelRoot;
    private CombatData targetData;

    private readonly string containerNameModifiers = "ContainerModifiers";
    private readonly string classNameIconModifier = "target-panel-icon-mod";

    public void FetchVisualElements(VisualElement root)
    {
        panelRoot = root.Q<VisualElement>("TargetPanelRoot");
    }

    public void AddDataSource(CombatData newHero)
    {
        targetData = newHero;
        panelRoot.dataSource = targetData;
        LoadModifierList();

        if (targetData != null)
            targetData.EventModifiersChanged.AddListener(LoadModifierList);
    }

    public void SetVisibility(bool value)
    {
        panelRoot.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
    }

    public void LoadModifierList()
    {
        var containerModifier = panelRoot.Q<VisualElement>(containerNameModifiers);
        containerModifier.Clear();

        if (targetData == null) return;

        foreach (var modifier in targetData.CombatModifiers)
        {
            if (modifier == null || modifier.modifierData.showInPanels == false) continue;

            var newVisualElement = new VisualElement();
            newVisualElement.AddToClassList(classNameIconModifier);
            newVisualElement.style.backgroundImage = modifier.modifierData.icon;
            containerModifier.Add(newVisualElement);
        }
    }
}
