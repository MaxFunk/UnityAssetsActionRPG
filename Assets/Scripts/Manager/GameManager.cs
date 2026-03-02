using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public SaveSystem SaveSystem { get; private set; } = new();    
    public GameDataGeneral GameDataGeneral { get; private set; } = new();    
    public ItemManager ItemManager { get; private set; } = new();

    public EventFlags EventFlags { get; set; } = new();
    public HeroCharacterController PlayerCharacter { get; set; } = null;

    public List<CharacterData> characterDatas = new(); // > character manager / GameDataParty    
    public int[] partyData = new int[] { -1, -1, -1 }; // > character manager

    [Header("Prefabs")]
    public ItemDrop ItemDropPrefab;
    [Header("Debug")]
    public bool DebugMode = false;

    private bool hasSwitchedThisFrame = false; // > character manager


    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this);

        //BindingConverters.RegisterConverters();

        if (DebugMode)
        {
            Debug.Log("GAME MANAGER IS STARTED DEBUG MODE");
            SaveSystem.ReadFromFile(-1);
            StartGame(null);
        }
    }


    void Update()
    {
        if (GameDataGeneral.gameRunning)
            GameDataGeneral.gameTime += Time.deltaTime;

        if (hasSwitchedThisFrame)
            hasSwitchedThisFrame = false;
    }


    public void StartGame(GameDataGeneral generalData)
    {
        if (generalData == null)
            GameDataGeneral.gameTime = 0;
        else
            GameDataGeneral.gameTime = generalData.gameTime;

        GameDataGeneral.gameRunning = true;
    }

    public void ClearData()
    {
        GameDataGeneral = new();
        EventFlags = new();
        characterDatas = new();
        ItemManager = new();
        partyData = new int[] { -1, -1, -1 };
    }


    public void SpawnItemDrop(ItemRecieveData dropData, Vector3 position)
    {
        if (ItemDropPrefab == null) return;

        if (Random.value <= dropData.recieveChance)
        {
            ItemDrop newItemDrop = Instantiate(ItemDropPrefab, position + (Vector3.up * 0.1f), Quaternion.identity);
            newItemDrop.dropData = dropData;
            newItemDrop.SetInitialVelocity(new Vector3((Random.value - 0.5f), 1.5f, (Random.value - 0.5f)));
        }
    }

    public int GetPartyIndex(int characterId)
    {
        if (partyData.Contains(characterId))
        {
            for (int i = 0; i < partyData.Length; i++) 
            {
                if (partyData[i] == characterId) return i;
            }
        }
        return -1;
    }


    public void FindSpawnAndRespawn()
    {
        var spawnPoints = FindObjectsByType<PlayerSpawner>(FindObjectsSortMode.None);
        foreach (var spawn in spawnPoints)
        {
            if (spawn != null)
            {
                spawn.Respawn();
                return;
            }
        }
    }

    public void SwitchToNextHero()
    {
        if (hasSwitchedThisFrame || (partyData[1] < 0 && partyData[2] < 0))
            return;

        if (partyData[1] < 0)
        {
            (partyData[0], partyData[2]) = (partyData[2], partyData[0]);
        }
        else if (partyData[2] < 0)
        {
            (partyData[0], partyData[1]) = (partyData[1], partyData[0]);
        }
        else
        {
            (partyData[0], partyData[1], partyData[2]) = (partyData[1], partyData[2], partyData[0]);
        }

        var heroCharacterControllers = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);

        /*CameraController playerCam = null;
        foreach (var heroChar in heroCharacterControllers)
        {
            if (heroChar != null && heroChar.IsPlayerControlled)
                playerCam = heroChar.GetPlayerCamera();
        }*/

        HeroCharacterController playerChar = null;
        for (var i = 0; i < 3; i++)
        {
            foreach (var heroChar in heroCharacterControllers)
            {
                if (heroChar.GetCombatData().characterId == partyData[i])
                {
                    if (i == 0)
                        playerChar = heroChar;

                    heroChar.UpdatePartyIndex(i, playerChar);
                    //heroChar.SetPlayerCamera(playerCam);
                    UserInterfaceManager.instance.GameplayUI.OnHeroLoad(heroChar.GetCombatData(), i);
                }
            }
        }

        hasSwitchedThisFrame = true;
    }


    // move to character manager
    public CharacterData GetCharacterData(int characterId)
    {
        if (characterId >= 0 && characterId < characterDatas.Count)
            return characterDatas[characterId];
        return null;
    }
}
