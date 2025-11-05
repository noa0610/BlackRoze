using UnityEngine;

public class SaveTest : MonoBehaviour
{
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveSystem.Instance.SaveGame();
            SaveLog();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SaveSystem.Instance.LoadGame();
            Debug.Log("Load");

        }
    }

    void SaveLog()
    {
        Debug.Log($"セーブしました。\n=================\n音量: {SaveSystem.Instance.AudioData.AudioVolume}\n=================\n");
    }

}
