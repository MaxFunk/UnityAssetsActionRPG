using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CombatHeroPanel
{
    private VisualElement panelRoot;
    private CombatData heroData;

    private readonly string containerNameModifiers = "ContainerModifiers";
    private readonly string classNameIconModifier = "hero-panel-icon-mod";

    public void FetchVisualElements(VisualElement root, int panelIndex)
    {
        var panelTree = root.Q<VisualElement>($"HeroPanel{panelIndex + 1}");
        panelRoot = panelTree.Q<VisualElement>($"HeroPanelRoot");
    }

    public void AddDataSource(CombatData newHero)
    {
        heroData = newHero;
        panelRoot.dataSource = heroData;
        LoadModifierList();

        if (heroData != null)
            heroData.EventModifiersChanged.AddListener(LoadModifierList);
    }

    public void SetVisibility(bool value)
    {
        panelRoot.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
    }

    public void LoadModifierList()
    {
        var containerModifier = panelRoot.Q<VisualElement>(containerNameModifiers);
        containerModifier.Clear();

        if (heroData == null) return;

        foreach (var modifier in heroData.CombatModifiers)
        {
            if (modifier == null || modifier.modifierData.showInPanels == false) continue;

            var newVisualElement = new VisualElement();
            newVisualElement.AddToClassList(classNameIconModifier);
            newVisualElement.style.backgroundImage = modifier.modifierData.icon;
            containerModifier.Add(newVisualElement);
        }
    }
}
