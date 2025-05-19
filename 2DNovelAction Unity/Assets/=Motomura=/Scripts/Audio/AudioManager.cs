using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//＝＝＝＝Master・BGM・SEを管理し保存する機構＝＝＝＝

public class AudioManager : MonoBehaviour
{
    public SaveLoadManager saveLoadManager;//セーブデータを管理するクラス

    [Header("Audio一覧（AudioSourceをアタッチ）")]
    public List<AudioSource> BGMAudioSources = new List<AudioSource>();//BGMのAudioSourceを格納するリスト
    public List<AudioSource> SEAudioSources = new List<AudioSource>();//SEのAudioSourceを格納するリスト

    [Header("各音量スライダー")]
    [SerializeField] private Slider _MasterSlider;//マスターボリューム
    [SerializeField] private Slider _BGMSlider;//BGMボリューム
    [SerializeField] private Slider _SESlider;//SEボリューム

    private Data volumeData;//TempData


    void Start()
    {
        volumeData = saveLoadManager.Load();//以前のデータを読み込む

        _MasterSlider.value = volumeData.Master_Volume;
        _BGMSlider.value = volumeData.BGM_Volume;//各データをスライダーに反映（Master・BGM・SE）
        _SESlider.value = volumeData.SE_Volume;

        ApplyVolume();//一括適用

        _MasterSlider.onValueChanged.AddListener(SetMasterVolume);
        _BGMSlider.onValueChanged.AddListener(SetBGMVolume);//変更時に呼び出すための登録（Master・BGM・SE）
        _SESlider.onValueChanged.AddListener(SetSEVolume);
    }

    public void SetMasterVolume(float value)//Masterのスライダーが変更されたとき呼ばれる
    {
        volumeData.Master_Volume = value;//今の値をJsonに代入
        ApplyVolume();
        saveLoadManager.Save(volumeData);//Jsonに保存
    }

    public void SetBGMVolume(float value)//BGMのスライダーが変更されたとき呼ばれる
    {
        volumeData.BGM_Volume = value;//今の値をJsonに代入
        ApplyVolume();
        saveLoadManager.Save(volumeData);//Jsonに保存
    }

    public void SetSEVolume(float value)//SEのスライダーが変更されたとき呼ばれる
    {
        volumeData.SE_Volume = value;//今の値をJsonに代入
        ApplyVolume();
        saveLoadManager.Save(volumeData);//Jsonに保存
    }

    private void ApplyVolume()//各音量を一括適用
    {
        float master = volumeData.Master_Volume;

        foreach (AudioSource bgm in BGMAudioSources)//BGMをMasterの値を掛け算して音量を調整
        {
            bgm.volume = master * volumeData.BGM_Volume;
        }

        foreach (AudioSource se in SEAudioSources)//SEをMasterの値を掛け算して音量を調整
        {
            se.volume = master * volumeData.SE_Volume;
        }
    }
}
