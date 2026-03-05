using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    public int currentFileIndex = -1;
    private SaveData saveData = new();

    [System.Serializable]
    public struct SaveData
    {
        public int index;
        public GameDataGeneral generalData;
        public EventFlags eventFlags;
        public List<CharacterData> characterDatas;
        public int[] partyData;
        public ItemManager.ItemSaveData itemData;
        public List<Mission> missions;
    }

    public string SaveFilePath(int index)
    {
        string saveFilePath = Application.persistentDataPath + "/save" + $"{index}" + ".save";
        return saveFilePath;
    }

    public void WriteToFile()
    {
        var gameManager = GameManager.Instance;
        if (gameManager.DebugMode)
        {
            Debug.Log("CANNOT SAVE IN DEBUG MODE");
            return;
        }

        // C:/Users/max_f/AppData/LocalLow/OtterDevs/ProjectSouls
        saveData.index = currentFileIndex;
        saveData.generalData = gameManager.GameDataGeneral;
        saveData.eventFlags = gameManager.EventFlags;
        saveData.characterDatas = gameManager.characterDatas;
        saveData.itemData = gameManager.ItemManager.CreateSaveData();
        saveData.partyData = gameManager.partyData;
        saveData.missions = gameManager.MissionManager.missions;

        File.WriteAllText(SaveFilePath(currentFileIndex), JsonUtility.ToJson(saveData, true));
        Debug.Log($"Saved to File Index {saveData.index}");
    }

    public void ReadFromFile(int index)
    {
        var gameManager = GameManager.Instance;
        var savePath = SaveFilePath(index);
        if (File.Exists(savePath))
        {
            string saveContent = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<SaveData>(saveContent);

            currentFileIndex = index;
            gameManager.EventFlags = saveData.eventFlags;
            gameManager.ItemManager.LoadItems(saveData.itemData);
            gameManager.characterDatas = saveData.characterDatas;            
            gameManager.partyData = saveData.partyData;
            gameManager.MissionManager.LoadMissionSaveData(saveData.missions);

            for (int i = 0; i < gameManager.characterDatas.Count; i++)
            {
                gameManager.characterDatas[i].Initialize(i);
            }

            gameManager.StartGame(saveData.generalData);
        }
        else
        {
            ClearCurrentSaveData();
            Debug.Log("Started New Game");

            var characterDatas = new List<CharacterData>();
            for (int i = 0; i < 3; i++)
            {
                var newChd = new CharacterData();
                newChd.Initialize(i, true);
                characterDatas.Add(newChd);
            }

            currentFileIndex = index;
            gameManager.characterDatas = characterDatas;
            gameManager.ItemManager.LoadItems(new ItemManager.ItemSaveData());
            gameManager.partyData = new int[] { 0, 1, 2 };
            gameManager.StartGame(null);
        }
    }


    public void ClearCurrentSaveData()
    {
        currentFileIndex = -1;
        saveData = new SaveData();
        GameManager.Instance.ClearData();
    }


    public List<SaveDataPreview> GenerateSaveDataPreviews()
    {
        List<SaveDataPreview> previews = new List<SaveDataPreview>();

        for (int i = 0; i < 3; i++) 
        {
            SaveDataPreview preview = new();
            SaveData newSaveData = new();     
            bool newGame = false;

            var savePath = SaveFilePath(i);
            if (File.Exists(savePath))
            {
                string saveContent = File.ReadAllText(savePath);
                newSaveData = JsonUtility.FromJson<SaveData>(saveContent);
            }
            else
            {
                newSaveData.index = i;
                newGame = true;
            }

            preview.WriteData(i, newGame, newSaveData);
            previews.Add(preview);
        }

        return previews;
    }
}
