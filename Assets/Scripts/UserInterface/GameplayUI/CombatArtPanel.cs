using UnityEngine;
using UnityEngine.UIElements;

public class CombatArtPanel
{
    private VisualElement panelRoot;
    private VisualElement artView1;
    private VisualElement artView2;
    private VisualElement artView3;
    private VisualElement artView4;
    private VisualElement artView5;
    private VisualElement artViewUlt;

    private CombatData heroData;


    public void FetchVisualElements(VisualElement root)
    {
        panelRoot = root.Q<VisualElement>("ArtPanelRoot");
        artView1 = panelRoot.Q<VisualElement>("ArtView1");
        artView2 = panelRoot.Q<VisualElement>("ArtView2");
        artView3 = panelRoot.Q<VisualElement>("ArtView3");
        artView4 = panelRoot.Q<VisualElement>("ArtView4");
        artView5 = panelRoot.Q<VisualElement>("ArtView5");
        artViewUlt = panelRoot.Q<VisualElement>("UltView");
    }

    public void AddDataSource(CombatData newHero)
    {
        heroData = newHero;
        UpdateArtView(artView1, newHero.Arts[0]);
        UpdateArtView(artView2, newHero.Arts[1]);
        UpdateArtView(artView3, newHero.Arts[2]);
        UpdateArtView(artView4, newHero.Arts[3]);
        UpdateArtView(artView5, newHero.Arts[4]);
        UpdateArtView(artViewUlt, newHero.Arts[5]);
    }

    public void SetVisibility(bool value)
    {
        panelRoot.style.opacity = value ? 1f : 0f;
    }

    private void UpdateArtView(VisualElement artViewRoot, CombatArt combatArt)
    {
        var artView = artViewRoot.Q<VisualElement>("ArtView");
        artView.dataSource = combatArt;
        artView.style.visibility = combatArt.artData != null ? Visibility.Visible : Visibility.Hidden;
    }
}
