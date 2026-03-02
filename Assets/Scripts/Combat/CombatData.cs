using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

public class CombatData : MonoBehaviour
{
    public enum ActionState
    {
        Idle,
        AutoAttack,
        ArtCast,
        Defeated
    }

    [Header("General")]
    public int characterId = -1;
    public int partyIndex = -1;
    public string charName = string.Empty;
    public bool isInCombat = false;
    public bool isHero = false;
    public bool isPlayerControlled = false;

    [Header("Stats")]
    public int level = 1;
    public int exp = 0;
    public float maxHealth = 1;
    public float curHealth = 1;
    public float attack = 1;
    public float ether = 1;
    public float agility = 1;
    public float luck = 1;
    public float physicalDefense = 0f;
    public float etherDefense = 0f;

    [Header("Auto Attack Parameter")]
    public float autoAttackRange = 3f;
    public float autoAttackWaitTime = 2f;

    [Header("Enemy Data")]
    public List<int> artIds = new();
    public List<ItemRecieveData> itemDrops = new(); // id, amount, chance percentage 0-100 // TODO: replace with actual data object?
    public float MaxDistanceToHero = 10f;

    [Header("Other Data")]
    public float MaxTargetRange = 10f;
    public float timeModifierArtUse = 2.0f; // e.g. 2.0 times art cooldown
    public UnityEvent EventModifiersChanged;


    CharacterData refCharacterData = null;
    EnemyCharacterController enemyCharacterController = null;
    HeroCharacterController heroCharacterController = null;
    AutoAttackCast PrefabAutoAttackCast = null;

    CombatManager combatManager = null;
    public CombatData curTarget = null;

    ArtCastBase curArtCast = null;
    ActionState actionState = ActionState.Idle;

    private List<CombatArt> arts = new();
    private List<CombatModifier> combatModifiers = new();
    private int artCancelStacks = 0;
    public int[] aggro = new int[0];

    private bool isDefeated = false;
    private bool queryForDeletion = false;
    public bool canCancelArt = false;
    public bool tryCastingUlt = false;

    private float remainingCancelTime = 0f;
    private float remainingAutoAttackCD = 0f;
    private float aggroUpdateTime = 0f;
    private float timerActionsBlocked = 0f;

    public IReadOnlyList<CombatArt> Arts => arts;
    public IReadOnlyList<CombatModifier> CombatModifiers => combatModifiers;


    void Awake()
    {
        EventModifiersChanged = new UnityEvent();

        if (isHero == false)
        {
            enemyCharacterController = GetComponent<EnemyCharacterController>();
            foreach (var artId in artIds)
            {
                var newArt = new CombatArt();
                newArt.LoadData(this, artId, 0);
                arts.Add(newArt);
            }
        }
        else
        {
            heroCharacterController = GetComponent<HeroCharacterController>();

            arts.Clear();
            for (int i = 0; i < 6; i++)
                arts.Add(new CombatArt());
        }

        PrefabAutoAttackCast = ScriptableManager.instance.artsPreload.PrefabAutoAttackCast;
        // if still null, do failsafe creation of cast with component?

        combatManager = CombatManager.Instance;
    }


    void Update()
    {
        if (queryForDeletion)
        {
            if (enemyCharacterController != null)
                enemyCharacterController.StartDespawn();
            else
                Destroy(this.gameObject);
            return;
        }

        UpdateModfiers();

        if (timerActionsBlocked > 0f)
            timerActionsBlocked -= Time.deltaTime;

        if (remainingAutoAttackCD > 0f)
            remainingAutoAttackCD -= Time.deltaTime;

        if (remainingCancelTime > 0f)
        {
            remainingCancelTime -= Time.deltaTime;
            if (remainingCancelTime <= 0f)
                canCancelArt = false;
        }
            


        if (!isInCombat && !combatManager.CombatActive && curHealth < maxHealth) 
        {
            curHealth += maxHealth * 0.1f * Time.deltaTime; // 1 sec == 10%
            curHealth = Mathf.Min(curHealth, maxHealth);
        }

        if (isInCombat && !isHero) 
        {
            aggroUpdateTime += Time.deltaTime;
            if (aggroUpdateTime > 0.5f)
            {
                aggroUpdateTime = 0f;
                var newTarget = EvaluateAggroToHeros();
                SetNewTarget(newTarget);
                if (newTarget == null)
                    combatManager.CombatantLeave(this);
            }
        }


        if (actionState != ActionState.Defeated) // no updates of Arts, etc. when defeated!
        {
            foreach (var art in arts)
            {
                art?.Update();
            }
        }
    }


    public void LoadFromCharacterData(CharacterData chd)
    {
        characterId = chd.characterId;
        partyIndex = GameManager.Instance.GetPartyIndex(chd.characterId);
        refCharacterData = chd;
        chd.refCombatData = this;

        charName = chd.staticData.characterName;
        level = chd.level;
        exp = chd.totalExp;

        var accumStats = chd.statsAccumulated;
        maxHealth = accumStats.health;
        curHealth = accumStats.health;
        attack = accumStats.attack;
        ether = accumStats.ether;
        agility = accumStats.agility;
        luck = accumStats.luck;
        physicalDefense = accumStats.physicalDefense;
        etherDefense = accumStats.etherDefense;

        for (int i = 0; i < 6; i++)
        {
            var artSaveData = chd.GetArtSaveData(i);
            if (artSaveData != null)
                arts[i].LoadData(this, artSaveData.artId, artSaveData.artLevel);
            else
                arts[i].LoadData(this, -1, 0);
        }

        var itemPreload = ScriptableManager.instance.itemPreload;
        for (int i = 0; i < chd.gearIds.Length; i ++)
        {
            var gear = itemPreload.GetGear(chd.gearIds[i]);
            if (gear != null)
                gear.gearData.ApplyGearEffect(this, true, false);
        }
    }

    public void FetchNewTarget(int dir)
    {
        var targetables = FindObjectsByType<Targetable>(FindObjectsSortMode.None);
        List<CombatData> valid_targets = new();

        foreach (var targetable in targetables)
        {
            if (targetable == null)
                continue;

            var targetData = targetable.GetCombatData();
            if (targetData == null || !IsOppositeFaction(targetData) || GetDistanceTo(targetData) > MaxTargetRange)
                continue;

            valid_targets.Add(targetData);
        }

        if (valid_targets.Count <= 0)
        {
            SetNewTarget(null);
            return;
        }

        valid_targets.Sort((a, b) => a.GetDistanceTo(this).CompareTo(b.GetDistanceTo(this)));

        int index = valid_targets.IndexOf(curTarget);
        index += dir;
        if (index >= valid_targets.Count)
            index = 0;
        else if (index < 0)
            index = valid_targets.Count - 1;

        SetNewTarget(valid_targets[index]);
    }

    public CombatData GetCurrentTarget()
    { 
        return curTarget; 
    }

    public void SetNewTarget(CombatData combatData) 
    {
        curTarget = combatData;

        if (isPlayerControlled)
            UserInterfaceManager.instance.GameplayUI.OnNewTarget(combatData);
    }


    public void PerformAutoAttack()
    {
        if (isInCombat == false)
            combatManager.CombatantJoin(this);

        AutoAttackCast newAutoAttack = Instantiate(PrefabAutoAttackCast, transform.position, transform.rotation);
        newAutoAttack.OnCast(this, curTarget);

        artCancelStacks = 0;
        canCancelArt = false;
        remainingCancelTime = 0;
        actionState = ActionState.AutoAttack;
    }


    public bool CastArt(int index)
    {
        if (index >= arts.Count || index < 0) return false;

        CombatArt art = arts[index];
        
        if (art == null || !art.CanCastArt())
            return false;

        artCancelStacks = canCancelArt ? (artCancelStacks + 1) : 0;
        if (canCancelArt) 
        { 
            //Debug.Log($"Art Canceled!!! -> {artCancelStacks}");
            RecieveUltPoints(1);
            if (isPlayerControlled) 
                UserInterfaceManager.instance.GameplayUI.ShowCancelRing();            
        }
        canCancelArt = false;

        ArtCastBase newArt = Instantiate(art.artData.ArtCastPrefab);
        newArt.OnCast(this, curTarget, art);
        art.OnCastArt();        
        
        actionState = ActionState.ArtCast;
        curArtCast = newArt;
        RecieveUltPoints(3); // move to hits (take canceled artcastbase)

        return true;
    }

    public void RecieveDamage(int value, CombatData attacker, bool damageFromAutoAttack = false)
    {
        if (attacker == null || attacker.IsDefeated())
            return;

        if (isInCombat == false)
        {
            combatManager.CombatantJoin(this);
        }

        curHealth = Math.Max(curHealth - value, 0);
        if (curHealth <= 0)
        {
            OnDefeat();
            return;
        }

        int aggroIncrease = damageFromAutoAttack ? 1 : 3;
        ChangeAggro(attacker, aggroIncrease);
    }

    public void RecieveHealing(int value)
    {
        curHealth = Math.Min(curHealth + value, maxHealth);
    }

    public void RecieveEffectDamage(int value)
    {
        curHealth = Math.Max(curHealth - value, 1f);
    }

    // Returns Hero with highest aggro or null if no one is left
    public CombatData EvaluateAggroToHeros()
    {
        CombatData highestAggroHero = null;

        foreach (var hero in combatManager.heroCombatants)
        {
            if (hero != null && hero.isDefeated == false && hero.isActiveAndEnabled) 
            {
                int charId = hero.characterId;

                if (charId < 0 || charId >= aggro.Length) continue;

                if (Vector3.Distance(hero.transform.position, transform.position) > MaxDistanceToHero)
                {
                    aggro[charId] = -1;
                }

                if (aggro[charId] >= 0 && (highestAggroHero == null || aggro[charId] >= aggro[highestAggroHero.characterId]))
                    highestAggroHero = hero;
            }
        }

        return highestAggroHero;
    }

    public void ChangeAggro(CombatData aggressor, int value, bool forceOverwrite = false)
    {
        if (aggressor.characterId < 0 || aggressor.characterId >= aggro.Length || isHero)
            return;

        if (forceOverwrite)
        {
            aggro[aggressor.characterId] = value;
            return;
        }           

        aggro[aggressor.characterId] = Mathf.Clamp(aggro[aggressor.characterId] + value, 0, 9999);
    }

    public void RecieveUltPoints(int value)
    {
        if (!isHero || arts.Count < 6 || arts[5] == null)
            return;

        arts[5].IncreaseUltPoints(value);
    }


    public void Revive()
    {
        isDefeated = false;
        actionState = ActionState.Idle;
        curHealth = Mathf.Round(maxHealth * 0.33f);
        timerActionsBlocked = 1f;
        heroCharacterController.OnRevive();
    }

    private void UpdateModfiers()
    {
        var updatedCombatModifiers = new List<CombatModifier>();
        bool listChanged = false;

        for (int i = 0; i < combatModifiers.Count; i++)
        {
            if (combatModifiers[i] != null)
            {
                var stillActive = combatModifiers[i].ExternalUpdate(this);
                if (stillActive)
                    updatedCombatModifiers.Add(combatModifiers[i]);
                else
                    listChanged = true;
            }
        }

        combatModifiers = updatedCombatModifiers;
        if (listChanged)
            EventModifiersChanged.Invoke();
    }

    public void RecieveCombatModifier(CombatModifier newModifier)
    {
        foreach (var modifier in combatModifiers)
        {
            if (modifier.IsSameModifier(newModifier))
            {
                modifier.CopyFromModifier(newModifier);
                EventModifiersChanged.Invoke();
                return;
            }
        }

        combatModifiers.Add(newModifier);
        EventModifiersChanged.Invoke();
    }


    public void InitiateEarlyCombatEnd()
    {
        SetNewTarget(null);
        combatManager.EndCombat();
    }

    public void OnDefeat()
    {
        if (isDefeated)
            return; // for safety

        isDefeated = true;
        actionState = ActionState.Defeated;
                
        if (isHero)
        {
            foreach (var art in arts)
            {
                art?.ResetCooldown();
            }

            var reviveCircle = Instantiate(combatManager.PrefabReviveCirle, transform.position, transform.rotation);
            reviveCircle.LinkDefeatedHero(this);
            heroCharacterController.OnDefeat();
        }
        else
        {
            foreach (var dropData in itemDrops)
                GameManager.Instance.SpawnItemDrop(dropData, gameObject.transform.position);

            queryForDeletion = !isHero;
        }

        combatManager.OnCombatantDefeated(this);
    }

    public void OnCombatantWasDefeated(CombatData combatant)
    {
        if (combatant == null || curTarget != combatant) return;

        if (isHero)
            RecieveExp(combatant.exp);
        else
            ChangeAggro(combatant, 0, true);

        var newTarget = combatManager.GiveNewTarget(this);
        SetNewTarget(newTarget);
        /*if (newTarget == null)
            combatManager.CombatantLeave(this);
        else
            SetNewTarget(newTarget);*/
    }


    public void OnCombatJoin()
    {
        isInCombat = true;

        if (isHero) 
        {
            heroCharacterController.OnCombatJoin();
            aggro = new int[0];

            // load gear effects, e.g. Modifier
            var itemPreload = ScriptableManager.instance.itemPreload;
            for (int i = 0; i < refCharacterData.gearIds.Length; i++)
            {
                var gear = itemPreload.GetGear(refCharacterData.gearIds[i]);
                if (gear != null)
                    gear.gearData.ApplyGearEffect(this, false, true);
            }
        }
        else
        {
            enemyCharacterController.OnCombatJoin();
            aggro = combatManager.GiveInitialAggroForEnemies();
        }
    }


    public void OnCombatEnd(bool playerLost = false)
    {
        Debug.Log($"Combat End for {charName}");
        
        isInCombat = false;
        SetNewTarget(null);
        actionState = ActionState.Idle;
        artCancelStacks = 0;
        curArtCast = null;
        combatModifiers.Clear();
        EventModifiersChanged.Invoke();

        if (isHero)
        {
            if (actionState == ActionState.Defeated)
            {
                Revive();
                curHealth = 1f;
            }

            RecieveUltPoints(-99999);
            heroCharacterController.OnCombatEnd();
        }
        else 
        {
            enemyCharacterController.OnCombatEnd();
        }
    }

    public bool IsOppositeFaction(CombatData cData)
    { 
        return (isHero == true && cData.isHero == false) || (isHero == false && cData.isHero == true);
    }

    public bool IsDefeated()
    { 
        return isDefeated || actionState == ActionState.Defeated; 
    }


    public bool CanMove()
    {
        if (actionState == ActionState.ArtCast || actionState == ActionState.Defeated)
            return false;
        return true;
    }

    public bool CanPerformAutoAttack()
    {
        if (actionState != ActionState.Idle || curTarget == null || timerActionsBlocked > 0f) return false;

        bool isInDistance = Vector3.Distance(gameObject.transform.position, curTarget.gameObject.transform.position) <= autoAttackRange;
        return isInDistance && remainingAutoAttackCD <= 0f;
    }

    public bool CanPerformArtCast()
    {
        if (timerActionsBlocked > 0f || actionState == ActionState.Defeated || curTarget == null)
            return false;
        return (actionState != ActionState.ArtCast || canCancelArt) && isInCombat;
    }

    public bool CanPerformArtCastNPC()
    {
        if (timerActionsBlocked > 0f || actionState == ActionState.Defeated || curTarget == null)
            return false;
        return actionState == ActionState.Idle && canCancelArt == false && isInCombat;
    }

    public bool CanCastUlt()
    {
        if (isHero && arts.Count >= 6 && arts[5] != null)
        {
            if (arts[5].artData.isUlt)
                return arts[5].ultPoints >= arts[5].artData.ultCost;
            else
                return false; // massive error in logic if this is ever reached?
        }

        return false;
    }

    public float GetCancelMultiplier()
    {
        return 1.0f + artCancelStacks * 0.25f;
    }

    public float GetDistanceTo(CombatData cData)
    {
        return Vector3.Distance(cData.transform.position, transform.position);
    }

    public CombatArt GetUltArt()
    {
        if (arts.Count >= 6)
            return arts[5];
        return null;
    }


    public void RecieveExp(int expAmount)
    {
        if (refCharacterData == null || actionState == ActionState.Defeated) return;

        refCharacterData.RecieveExp(expAmount);
        exp = refCharacterData.totalExp;

        if (level != refCharacterData.level)
        {
            level = refCharacterData.level;
            var accumStats = refCharacterData.statsAccumulated;
            maxHealth = accumStats.health;
            curHealth = accumStats.health; // Does a full heal on level up
            attack = accumStats.attack;
            ether = accumStats.ether;
            agility = accumStats.agility;
            luck = accumStats.luck;       
            physicalDefense = accumStats.physicalDefense;
            etherDefense = accumStats.etherDefense;
        }
    }

    public int GetRandomArtIndex()
    {
        if (arts.Count == 0) return -1;
        return UnityEngine.Random.Range(0, arts.Count);
    }

    public ArtData GetCurrentArt()
    {
        return curArtCast.artData;
    }    

    public void OnAutoAttackCastEnd()
    {
        remainingAutoAttackCD = autoAttackWaitTime;

        if (curArtCast == null)
            actionState = ActionState.Idle;
    }

    public void OnArtCastDurationEnd(ArtCastBase artCast)
    {
        remainingAutoAttackCD = Mathf.Max(remainingAutoAttackCD, 0.25f);

        if (curArtCast == artCast)
        {
            curArtCast = null;
            actionState = ActionState.Idle;
        }
    }

    public void OnCancelWindowStart(ArtCastBase artCast, float duration)
    {
        if (curArtCast != artCast)
            return;
        if (artCast != null && artCast.artData.isUlt) // maybe not?
            return;
        if (artCast == null && actionState != ActionState.AutoAttack)
            return;

        canCancelArt = true;
        remainingCancelTime = duration;
        //if (isPlayerControlled) Debug.Log($"CANCEL START {duration}");
    }
}
