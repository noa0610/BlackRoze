using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

//＝＝＝＝Audioを管理し保存する機構＝＝＝＝

public class AudioManager : MonoBehaviour
{
    [SerializeField, Header("BGM")] private AudioMixer BGMMixer;
    [SerializeField] private TextMeshProUGUI _BGMVolumeText;
    [SerializeField] private Slider _BGMSlider;
    [SerializeField, Range(0, 1.5f)] private float _bgmMultiplier = 1.0f;

    [SerializeField, Header("Voice")] private AudioMixer VoiceMixer;
    [SerializeField] private TextMeshProUGUI _VoiceVolumeText;
    [SerializeField] private Slider _VoiceSlider;
    [SerializeField, Range(0, 1.5f)] private float _seMultiplier = 1.0f;

    private void Start()
    {
        try
        {
            _BGMSlider.onValueChanged.AddListener(SetBGMVolume);
            _VoiceSlider.onValueChanged.AddListener(SetVoiceVolume);

            _BGMVolumeText.text = SaveSystem.Instance.AudioData.BGMVolume.ToString();
            _VoiceVolumeText.text = SaveSystem.Instance.AudioData.VoiceVolume.ToString();

            _BGMSlider.value = SaveSystem.Instance.AudioData.BGMVolume;
            _VoiceSlider.value = SaveSystem.Instance.AudioData.VoiceVolume;
        }
        catch
        {
            Debug.LogWarning("[オーディオ]書き込みは機能しません。");
        }

        BGMMixer.SetFloat("BGM", MapValue(SaveSystem.Instance.AudioData.BGMVolume));
        VoiceMixer.SetFloat("Voice", MapValue(SaveSystem.Instance.AudioData.VoiceVolume));


    }

    public void SetBGMVolume(float value)
    {
        SaveSystem.Instance.AudioData.BGMVolume = Mathf.FloorToInt(value);
        BGMMixer.SetFloat("BGM", MapValue(value));
        _BGMVolumeText.text = SaveSystem.Instance.AudioData.BGMVolume.ToString();
        Debug.Log($"値が変更されました<color=green>{SaveSystem.Instance.AudioData.BGMVolume}</color>");
        SaveSystem.Instance.SaveGame();
    }

    public void SetVoiceVolume(float value)
    {
        SaveSystem.Instance.AudioData.VoiceVolume = Mathf.FloorToInt(value);
        VoiceMixer.SetFloat("Voice", MapValue(value));
        _VoiceVolumeText.text = SaveSystem.Instance.AudioData.VoiceVolume.ToString();
        Debug.Log($"値が変更されました<color=green>{SaveSystem.Instance.AudioData.VoiceVolume}</color>");
        SaveSystem.Instance.SaveGame();
    }

    private float MapValue(float value)
    {
        // 0～100 → 0.0001～1.0 の対数スケールに変換してdB化
        float linear = Mathf.Pow(value / 100f, 0.1f); // ← 2.0 は調整可能（小さいほど中音量が大きくなる）
        float dB = Mathf.Lerp(-80f, 5f, linear);
        return dB;
    }

}
