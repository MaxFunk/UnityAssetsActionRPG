using UnityEngine;
using UnityEngine.UIElements;

public class CombatAllyUltPanel
{
    public Visibility visibility = Visibility.Hidden;
    public Visibility visibilitySelection = Visibility.Hidden;
    public Texture2D iconHero = null;
    public string artName = string.Empty;

    private CombatData combatData;
    private CombatArt ultArt;


    public void ExternalUpdate()
    {
        if (ultArt != null)
        {
            if (ultArt.artData.isUlt && ultArt.ultPoints >= ultArt.artData.ultCost)
            {
                visibility = Visibility.Visible;
                visibilitySelection = combatData.TryCastingUlt ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                visibility = Visibility.Hidden;
                visibilitySelection = Visibility.Hidden;
            }
        }
    }

    public void BindToContainer(VisualElement rootElement)
    {
        rootElement.Q<VisualElement>("Container").dataSource = this;
    }

    public void LoadData(CombatData combatData)
    {
        visibility = Visibility.Hidden;

        if (combatData == null)
        {
            return;
        }

        var charData = GameManager.Instance.GetCharacterData(combatData.characterId);
        if (charData == null)
        {
            return;
        }

        this.combatData = combatData;
        ultArt = combatData.GetUltArt();

        iconHero = charData.staticData.iconMenuFull;
        artName = ultArt.artData.artName;
    }
}
