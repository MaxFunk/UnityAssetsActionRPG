using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float BarWidth = 98f;

    private RectTransform rectTransform;
    private CombatData linkedCombatData = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (linkedCombatData != null)
        {
            float rectWidth = BarWidth * linkedCombatData.curHealth / linkedCombatData.maxHealth;
            rectTransform.sizeDelta = new Vector2(rectWidth, rectTransform.sizeDelta.y);
        }
    }

    public void LinkCombatData(CombatData data)
    {
        linkedCombatData = data;
    }
}
