using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int level;
}

public class SaveLoadManager : MonoBehaviour
{
    public GameData gameData;

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(gameData);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }
    public bool LoadGame()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(json);
            return true;
        }
        else return false;
    }
}
