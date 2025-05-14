using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
        public SaveLoadManager saveLoadManager;

    [Header("Audio一覧（AudioSourceをアタッチ）")]
    public List<AudioSource> BGMAudioSources = new List<AudioSource>();
    public List<AudioSource> SEAudioSources = new List<AudioSource>();

    [Header("音量スライダー")]
    public Slider MasterSlider;
    public Slider BGMSlider;
    public Slider SESlider;

    private Data volumeData;


    void Start()
    {
        saveLoadManager = FindObjectOfType<SaveLoadManager>();

    volumeData = saveLoadManager.Load();

    MasterSlider.value = volumeData.Master_Volume;
    BGMSlider.value = volumeData.BGM_Volume;
    SESlider.value = volumeData.SE_Volume;

    ApplyVolume();

    MasterSlider.onValueChanged.AddListener(SetMasterVolume);
    BGMSlider.onValueChanged.AddListener(SetBGMVolume);
    SESlider.onValueChanged.AddListener(SetSEVolume);
    }

    public void SetMasterVolume(float value)
    {
        volumeData.Master_Volume = value;
        ApplyVolume();
        saveLoadManager.Save(volumeData);
    }

    public void SetBGMVolume(float value)
    {
        volumeData.BGM_Volume = value;
        ApplyVolume();
        saveLoadManager.Save(volumeData);
    }

    public void SetSEVolume(float value)
    {
        volumeData.SE_Volume = value;
        ApplyVolume();
        saveLoadManager.Save(volumeData);
    }

    private void ApplyVolume()//
    {
        float master = volumeData.Master_Volume;

        foreach (AudioSource bgm in BGMAudioSources)
        {
            bgm.volume = master * volumeData.BGM_Volume;
        }

        foreach (AudioSource se in SEAudioSources)
        {
            se.volume = master * volumeData.SE_Volume;
        }
    }
}
