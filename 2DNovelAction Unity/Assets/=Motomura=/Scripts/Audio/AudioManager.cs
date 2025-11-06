using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//＝＝＝＝Audioを管理し保存する機構＝＝＝＝

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> BGMAudioSources;//AudioSourceを格納するリスト
    [SerializeField] private TextMeshProUGUI _AudioVolumeText;
    [SerializeField] private Slider _AudioSlider;//BGMボリューム

    void Start()
    {
        _AudioSlider.onValueChanged.AddListener(SetBAudioVolume);//変更時に呼び出すための登録
        //SaveSystem.Instance.LoadGame();
        _AudioSlider.value = SaveSystem.Instance.AudioData.AudioVolume;
        _AudioVolumeText.text = SaveSystem.Instance.AudioData.AudioVolume.ToString();
    }

    public void SetBAudioVolume(float value)//BGMのスライダーが変更されたとき呼ばれる
    {
        SaveSystem.Instance.AudioData.AudioVolume = Mathf.FloorToInt(value);
        _AudioVolumeText.text = SaveSystem.Instance.AudioData.AudioVolume.ToString();
        Debug.Log($"値が変更されました<color=green>{SaveSystem.Instance.AudioData.AudioVolume}</color>");
        SaveSystem.Instance.SaveGame();
    }

}
