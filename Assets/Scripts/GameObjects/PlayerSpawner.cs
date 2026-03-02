using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public HeroCharacterController HeroPrefab;
    public CameraController CameraPrefab;
    public RuntimeAnimatorController[] Controller;
    public Animator[] HeroModels;

    private CameraController currentPlayerCamera;
    public bool DebugRespawn = false;

    
    void Awake()
    {
        SpawnCharacters();
    }

    void Update()
    {
        if (DebugRespawn)
        {
            DebugRespawn = false;
            Respawn();
        }
    }

    private HeroCharacterController SpawnHero(int heroId, int partyIndex)
    {
        var spawnOffset = Vector3.zero;
        if (partyIndex == 1)
            spawnOffset = Vector3.left;
        if (partyIndex == 2)
            spawnOffset = Vector3.right;

        var newHeroCharacter = Instantiate(HeroPrefab, gameObject.transform.position + spawnOffset, gameObject.transform.rotation);
        var modelCopy = Instantiate(HeroModels[heroId], newHeroCharacter.gameObject.transform);
        modelCopy.runtimeAnimatorController = Controller[heroId];
        newHeroCharacter.OnModelLoad(heroId, partyIndex);
        return newHeroCharacter;
    }

    private void SpawnCharacters()
    {
        var partyData = GameManager.Instance.partyData;
        currentPlayerCamera = Instantiate(CameraPrefab);
        
        UserInterfaceManager.instance.CreateGameplayUI();

        var playerChar = SpawnHero(partyData[0], 0); // must be a valid id, otherwise logic fails (right now)
        playerChar.MakePlayerCharacter();
        playerChar.SetPlayerCamera(currentPlayerCamera);

        if (partyData[1] >= 0 && partyData[1] < HeroModels.Length) 
        { 
            var allyChar = SpawnHero(partyData[1], 1);            
            allyChar.MakeAllyCharacter(playerChar);
            allyChar.SetPlayerCamera(currentPlayerCamera);
        }
        if (partyData[2] >= 0 && partyData[1] < HeroModels.Length) 
        {
            var allyChar = SpawnHero(partyData[2], 2);            
            allyChar.MakeAllyCharacter(playerChar);
            allyChar.SetPlayerCamera(currentPlayerCamera);
        }
    }

    private void DeleteCharacters()
    {
        Destroy(currentPlayerCamera.gameObject);

        var heroChars = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
        for (var i = 0; i < heroChars.Length; ++i)
        {
            Destroy(heroChars[i].gameObject);
        }
    }

    public void Respawn()
    {
        float fadeInTime = UserInterfaceManager.instance.CreateLoadingScreen();
        fadeInTime += 0.1f;
        StartCoroutine(CallRespawnAfterDelay(fadeInTime));
    }

    private IEnumerator CallRespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeleteCharacters();
        SpawnCharacters();
    }
}
