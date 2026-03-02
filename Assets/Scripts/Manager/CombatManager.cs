using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Prefabs")]
    public ReviveCirle PrefabReviveCirle;

    public static CombatManager Instance { get; private set; }
    public float TimeInCombat { get; private set; } = 0f;
    public bool CombatActive { get; private set; } = false;

    public List<CombatData> combatants = new();
    public List<CombatData> heroCombatants = new();
    public List<CombatData> enemyCombatants = new();

    private const int maxHeroCharacters = 8; // max playable characters -> move somewhere else!

    public float debugTimeInCombat = 0f;
    public bool debugCombatActive = false;


    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        if (CombatActive)
        {
            TimeInCombat += Time.deltaTime;
        }

        debugTimeInCombat = TimeInCombat;
        debugCombatActive = CombatActive;
    }

    public void EndCombat(bool playerLost = false)
    {
        Debug.Log($"PLAYER LOST: {playerLost}");
        foreach (var hero in heroCombatants)
        {
            if (hero != null)
                hero.OnCombatEnd(playerLost);
        }
        foreach (var enemy in enemyCombatants)
            enemy.OnCombatEnd();

        var reviveCircles = FindObjectsByType<ReviveCirle>(FindObjectsSortMode.None);
        foreach (var circle in reviveCircles)
            Destroy(circle.gameObject); // fade away, instead of destroy..

        var artCasts = FindObjectsByType<ArtCastBase>(FindObjectsSortMode.None);
        foreach (var cast in artCasts)
            cast.disableHits = true;

        var autoattackCasts = FindObjectsByType<AutoAttackCast>(FindObjectsSortMode.None);
        foreach (var cast in autoattackCasts)
            cast.disableHits = true;

        ClearData();
        CombatActive = false;
        TimeInCombat = 0f;
    }


    public void CombatantJoin(CombatData newCombatant)
    {
        if (newCombatant == null || combatants.Contains(newCombatant))
            return;

        CombatActive = true;

        combatants.Add(newCombatant);
        if (newCombatant.isHero)
        {
            heroCombatants.Add(newCombatant);
        }
        else
        {
            enemyCombatants.Add(newCombatant);
            ApplyAggressionWave(4f, newCombatant.transform.position);
        }

        newCombatant.OnCombatJoin();
        Debug.Log($"Joined Combat: {newCombatant.charName}");
    }


    public void CombatantLeave(CombatData leavingCombatant)
    {
        if (leavingCombatant == null || combatants.Contains(leavingCombatant) == false)
            return;

        combatants.Remove(leavingCombatant);
        if (leavingCombatant.isHero)
            heroCombatants.Remove(leavingCombatant);
        else
            enemyCombatants.Remove(leavingCombatant);

        leavingCombatant.OnCombatEnd();

        var castProjectiles = FindObjectsByType<ArtCastProjectile>(FindObjectsSortMode.None);
        foreach (var cast in castProjectiles)
            cast.CombatantRemovedFromCombat(leavingCombatant);

        Debug.Log($"Leaved Combat: {leavingCombatant.charName}");

        if (enemyCombatants.Count <= 0)
            EndCombat(false);
    }



    // Collects nearby Enemy CombatData and adds them to combat
    public void ApplyAggressionWave(float strength, Vector3 origin)
    {
        var combatDataList = FindObjectsByType<CombatData>(FindObjectsSortMode.None);
        for (var i = 0; i < combatDataList.Length; ++i)
        {
            var cData = combatDataList[i];
            if (cData != null && !cData.isInCombat && !cData.isHero && Vector3.Distance(origin, cData.transform.position) < strength)
            {
                CombatantJoin(cData);
            }
        }
    }

    public CombatData GiveNewTarget(CombatData caller)
    {
        List<CombatData> targets = caller.isHero ? enemyCombatants : heroCombatants;

        if (targets.Count <= 0)
            return null;

        CombatData closestTarget = null;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null && !targets[i].IsDefeated())
            {
                closestTarget = targets[i];
                break;
            }
        }
        if (closestTarget == null)
            return null;
        var closestDistance = Vector3.Distance(closestTarget.transform.position, caller.transform.position);

        foreach (var target in targets)
        {
            if (target == null || target.IsDefeated())
                continue;

            var newDist = Vector3.Distance(target.transform.position, caller.transform.position);
            if (newDist < closestDistance)
            {
                closestDistance = newDist;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    public int[] GiveInitialAggroForEnemies()
    {
        int[] aggro = new int[maxHeroCharacters];

        for (int i = 0; i < aggro.Length; i++)
        {
            aggro[i] = -1;
        }

        foreach (var hero in heroCombatants)
        {
            var id = hero.characterId;
            if (id >= 0 && id < aggro.Length)
                aggro[id] = 0;
        }

        return aggro;
    }

    public void OnCombatantDefeated(CombatData defeatedCombatant)
    {
        if (defeatedCombatant == null)
            return;

        bool endCombat;
        bool playerLost = false;

        var castProjectiles = FindObjectsByType<ArtCastProjectile>(FindObjectsSortMode.None);
        foreach (var cast in castProjectiles)
        {
            cast.CombatantRemovedFromCombat(defeatedCombatant);
        }

        if (defeatedCombatant.isHero)
        {
            foreach (var enemy in enemyCombatants)
            {
                enemy.OnCombatantWasDefeated(defeatedCombatant);
            }

            endCombat = true;
            foreach (var hero in heroCombatants)
            {
                if (hero != null && hero.IsDefeated() == false)
                    endCombat = false;
            }
            if (endCombat)
                playerLost = true;
        }
        else
        {
            foreach (var hero in heroCombatants) 
            {
                hero.OnCombatantWasDefeated(defeatedCombatant);
            }

            enemyCombatants.Remove(defeatedCombatant);
            combatants.Remove(defeatedCombatant);
            endCombat = enemyCombatants.Count <= 0;
        }

        if (endCombat)
        {
            EndCombat(playerLost);
            if (playerLost) 
            {
                var respawnPoints = FindObjectsByType<PlayerSpawner>(FindObjectsSortMode.None);
                if (respawnPoints.Length <= 0)
                {
                    Debug.LogError("ERROR: NO SPAWNPOINT AVAILABLE FOR RESPAWN. TODO: FAILSAFE ACTION?");
                }
                else
                {
                    respawnPoints[0].Respawn();
                }
            }
        }
            
    }

    public HeroCharacterController FindHeroChar(int partyIndex)
    {
        foreach (var hero in heroCombatants)
        {
            if (hero != null && hero.partyIndex == partyIndex)
                return hero.gameObject.GetComponent<HeroCharacterController>();
        }

        return null;
    }

    
    private void ClearData()
    {
        combatants.Clear();
        heroCombatants.Clear();
        enemyCombatants.Clear();
    }
}
