using System.IO;
using UnityEngine;

//＝＝＝＝セーブデータを管理するクラス＝＝＝＝

public class SaveLoadManager : MonoBehaviour
{
    private string _filePath;// セーブデータのパス

    void Awake()
    {
        _filePath = Application.persistentDataPath + "/audio_settings.json";// セーブデータのパスを設定
    }

    public void Save(Data data)// セーブ処理
    {
        string json = JsonUtility.ToJson(data, true);// JSON形式に変換
        File.WriteAllText(_filePath, json);// ファイルに書き込み
        Debug.Log("セーブ完了");
    }


    public Data Load()// ロード処理
    {
        if (File.Exists(_filePath))// セーブデータが存在する場合
        {
            string json = File.ReadAllText(_filePath);// ファイルから読み込み
            Data data = JsonUtility.FromJson<Data>(json);// JSON形式からデシリアライズ
            Debug.Log("ロード完了");
            return data;// 読み込んだデータを返す
        }
        else
        {
            Debug.LogWarning("セーブデータが見つかりません");
            return new Data(); // デフォルトのデータを返す
        }
    }
}
