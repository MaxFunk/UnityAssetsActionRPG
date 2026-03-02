using TMPro;
using UnityEngine;

public class EnemyTargetUI : MonoBehaviour
{
    public CanvasGroup CanvasGroup;
    public HealthBar EnemyHealthBar;
    public TextMeshProUGUI NameText;

    public void OnNewEnemyTarget(CombatData targetData)
    {
        if (targetData != null)
        {
            CanvasGroup.alpha = 1;
            EnemyHealthBar.LinkCombatData(targetData);
            NameText.text = targetData.charName;
        }
        else
        {
            CanvasGroup.alpha = 0;
            EnemyHealthBar.LinkCombatData(null);
            NameText.text = "No Target";
        }
    }

    public void HideUI()
    {
        CanvasGroup.alpha = 0;
    }
}
