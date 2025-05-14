using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private string filePath;

    void Awake()
    {
        filePath = Application.persistentDataPath + "/audio_settings.json";
    }

    // セーブ処理
    public void Save(Data data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("セーブ完了");
    }

    // ロード処理
    public Data Load()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Data data = JsonUtility.FromJson<Data>(json);
            Debug.Log("ロード完了");
            return data;
        }
        else
        {
            Debug.LogWarning("セーブデータが見つかりません");
            return new Data(); // 初期値
        }
    }
}
