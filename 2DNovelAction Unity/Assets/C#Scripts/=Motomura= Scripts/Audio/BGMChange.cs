using System.Collections.Generic;
using UnityEngine;

public class BGMChange : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _BGM;
    private AudioSource _BGMSource;
    private int _i = 0;

    private void Start()
    {
        _BGMSource = this.GetComponent<AudioSource>();
    }
    public void Change()
    {
        _BGMSource.Stop();
        _BGMSource.clip = _BGM[_i];
        _BGMSource.Play();
        _i++;
    }

    public void Stop()
    {
        _BGMSource.Stop();
    }
}
