using System;

public class SaveDataPreview
{
    public int fileIndex = -1;
    public string fileText = "File X";
    public bool isNewGame = false;
    public float gameTime = 0;
    public string gameTimeText = "00:00:000";

    public void WriteData(int index, bool newGame, SaveSystem.SaveData saveData)
    {
        fileIndex = index;
        fileText = $"File {index + 1}";
        isNewGame = newGame;
        
        if (isNewGame)
        {
            gameTimeText = "New Game";
            gameTime = 0;
        }
        else
        {
            gameTimeText = FloatToTimeString(saveData.generalData.gameTime);
            gameTime = saveData.generalData.gameTime;
        }
    }

    public string FloatToTimeString(float time)
    {
        TimeSpan ts = TimeSpan.FromSeconds(time);
        string formattedTime = string.Format(
            "{0:000}:{1:00}:{2:00}",
            (int)ts.TotalHours,
            ts.Minutes,
            ts.Seconds
        );
        return formattedTime;
    }
}
