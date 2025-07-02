using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace BlackRose
{
    public class EnableVignette : MonoBehaviour
    {
        [SerializeField] private TriggerSubject _subject;
        [SerializeField] private PostProcessVolume _volume;
        [SerializeField] private string _tag;
        private Vignette _vignette;
        // Intensity
        private float _defaultAmount;
        [SerializeField] private float _maxAmount = 0.5f;
        [SerializeField] private float _maxTime = 1f;// 最大値に達するまでにかかる時間
        private float _currentTime = 0f;

        // Deta
        private bool _isActive = false;

        // === Unity LifeCycle ===
        private void Awake()
        {
            _subject.OnTrigger += (res, collider) =>
            {
                if (collider.CompareTag(_tag))
                {
                    if (res)
                        _isActive = true;
                    else
                        _isActive = false;
                }
            };
            if (!_volume.profile.TryGetSettings(out _vignette))
            {
                Debug.LogError("This Volume Didn't set Vignette!");
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_isActive) _currentTime = Mathf.Min(_maxTime, _currentTime + Time.deltaTime);
            else _currentTime = Mathf.Max(0, _currentTime - Time.deltaTime);
            _vignette.enabled.Override(_currentTime > 0f);
            _vignette.intensity.Override(Mathf.Lerp(_defaultAmount, _maxAmount, Mathf.Clamp01(_currentTime / _maxTime)));
        }
    }
}