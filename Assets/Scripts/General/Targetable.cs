using UnityEngine;

public class Targetable : MonoBehaviour
{
    CombatData combatData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatData = GetComponent<CombatData>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public CombatData GetCombatData()
    { 
        return combatData;
    }
}
