using System.IO;
using UnityEngine;

public class SaveSystem
{
    #region Singleton
    private static SaveSystem _instance = new SaveSystem();
    public static SaveSystem Instance => _instance;
    #endregion

    private SaveSystem() { LoadGame(); }

    public string Path => Application.dataPath + "/AudioData.json";
    
    public AudioData AudioData { get; private set; }

    public void SaveGame()
    {
        string jsonData = JsonUtility.ToJson(AudioData);
        StreamWriter writer = new StreamWriter(Path, false);
        writer.WriteLine(jsonData);
        writer.Flush();
        writer.Close();
    }

    public void LoadGame()
    {
        if (!File.Exists(Path))
        {
            Debug.Log("<color=red> セーブデータないよ </color>");
            AudioData = new AudioData();
            SaveGame();
            return;
        }
        Debug.Log("<color=green> セーブデータをロード </color>");

        StreamReader reader = new StreamReader(Path);
        string jsonData = reader.ReadToEnd();
        AudioData = JsonUtility.FromJson<AudioData>(jsonData);
        reader.Close();
    }
}
