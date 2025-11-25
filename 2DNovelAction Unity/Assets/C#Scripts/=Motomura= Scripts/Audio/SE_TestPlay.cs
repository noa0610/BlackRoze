using UnityEngine;

//＝＝＝＝任意キーを押したらSEを流す機構＝＝＝＝

public class SE_TestPlay : MonoBehaviour
{
    [SerializeField] private AudioClip _SoundSource;//再生するSE
    [SerializeField] private AudioSource _AudioSource;//再生するSEのコンポーネント
    [SerializeField] private KeyCode _Input;//任意のキー


    private void Update()
    {
        if (Input.GetKeyDown(_Input)) //指定したキーが押されたら
        {
            Debug.Log("SE再生 : " + _SoundSource.name);//コンソールに表示
            _AudioSource.PlayOneShot(_SoundSource);//再生
        }
    }
}
