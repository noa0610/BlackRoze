using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models.Units;
using HighElixir.Timers;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace BlackRose
{
    public class VignetteAwater : MonoBehaviour, IPlayerFollower
    {
        [SerializeField] private TriggerSubject _subject;
        [SerializeField] private PostProcessVolume _volume;
        private Vignette _vignette;
        private UnitBase _target;

        // Intensity
        [SerializeField] private float _maxAmount = 0.5f;
        [Tooltip("最大値に達するまでにかかる時間")]
        [SerializeField] private float _maxTime = 1f;
        private float _defaultAmount;
        private float _currentTime = 0f;
        private TimerTicket _timerTicket;

        // Deta
        private bool _isActive = false;

        public void SetTarget(UnitBase target)
        {
            _target = target;
        }

        // === Unity LifeCycle ===
        private void Awake()
        {
            _subject.OnTrigger += (res, collider) =>
            {
                if (collider == null || collider.gameObject == null) return;
                if (_target == null) return;
                if (collider.gameObject.Equals(_target.gameObject))
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
                return;
            }
            _defaultAmount = _vignette.intensity.value;
            _timerTicket = GlobalTimer.Update.CountDownRegister(_maxTime, "Vignette Timer", initZero: true);
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