using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SE_TestPlay : MonoBehaviour
{
    [SerializeField] private AudioClip _SoundSource;
    [SerializeField] private AudioSource _AudioSource;
    [SerializeField] KeyCode _Input;


    void Update () 
    {
        if (Input.GetKeyDown(_Input)) //指定したキーが押されたら
        {
            Debug.Log ("SE再生");
            _AudioSource.PlayOneShot(_SoundSource);//再生
        }
    }
}
