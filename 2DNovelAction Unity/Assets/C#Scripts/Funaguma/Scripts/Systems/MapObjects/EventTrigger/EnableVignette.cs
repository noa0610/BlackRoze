using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static BlackRose.SpriteEffectHolders;

namespace BlackRose
{
    public class EnableVignette : MonoBehaviour, IObserver<bool>
    {
        [SerializeField] private TriggerSubject _subject;
        [SerializeField] private PostProcessVolume _volume;
        private Vignette _vignette;
        // Intensity
        private float _defaultAmount;
        [SerializeField] private float _maxAmount = 0.5f;
        [SerializeField] private float _maxTime = 2.5f;// 最大値に達するまでにかかる時間
        private float _currentTime = 0f;

        // Deta
        private bool _isActive = false;
        public void OnCompleted()
        {
            gameObject.SetActive(false);
        }

        public void OnError(Exception error)
        {
            gameObject.SetActive(false);
        }

        public void OnNext(bool value)
        {
            _isActive = value;
        }

        // === Unity LifeCycle ===
        private void Awake()
        {
            _subject.Subscribe(this);
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