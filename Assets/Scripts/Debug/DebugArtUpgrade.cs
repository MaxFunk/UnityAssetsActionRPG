using UnityEngine;

public class DebugArtUpgrade : MonoBehaviour
{
    
    private PlayerChecker playerChecker;

    void Start()
    {
        playerChecker = GetComponent<PlayerChecker>();
    }


    void Update()
    {
        /*if (playerChecker.checkActive && playerChecker.CheckForPlayerCharacer())
        {
            var character = GameManager.instance.characterDatas[0];

            for (int i = 0; i < character.artIds.Count; i++)
            {
                int artId = character.artIds[i];
                var artData = ScriptableManager.instance.GetArtData(artId);
                if (artData != null && artData.ArtUpgradeData != null) 
                {
                    character.artIds[i] = artData.ArtUpgradeData.upgradeToId;
                    character.OnUpgradeArt(artId, artData.ArtUpgradeData.upgradeToId);
                }
            }
        }*/
    }
}
