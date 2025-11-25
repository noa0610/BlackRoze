using HighElixir.Timers;
using HighElixir.Unity.Pools;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    // TimeManagement
    public partial class ActionRobot
    {
        [Header("時間設定")]
        [SerializeField] private float _coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        [SerializeField] private float _shootBlockTime = 0.6f; // 連射をブロックする時間
        [SerializeField] private float _invincibleTime = 0.6f; // 無敵時間
        [SerializeField] private float _intervalTime = 0.4f; // shootIntervel => idleへの時間

        [Header("回数設定")]
        [SerializeField] private int _maxSuccession = 3; // 最大連射回数
        [SerializeField] private float[] _chargeShoot = new float[2] { 0.7f, 1.8f }; // チャージ攻撃用の時間配列
        private int _successionCount = 0; // 連射回数
        private bool _chargeSEPlayed = false; // チャージSE再生済みフラグ

        [Header("Effect")]
        [SerializeField] private ParticleSystem _chargeEffect; // チャージエフェクト
        [SerializeField] private Transform _container;
        [SerializeField] private ObjectPool<ParticleSystem> _chargeEffectPool;
        private bool _halfEffectPlayed = false;
        private bool _fullEffectPlayed = false;

        #region Ticket
        private TimerTicket _coyoteTicket;
        private TimerTicket _shootBlockTicket;
        private TimerTicket _invincibleTicket;
        private TimerTicket _chargeTicket;

        #endregion

        protected override void BeforeAwake()
        {
            _chargeEffectPool = new ObjectPool<ParticleSystem>(_chargeEffect, 3, _container);
            _chargeEffectPool.Pool.OnGetEvt += Pool_OnGetEvt;
            _coyoteTicket = Timer.CountDownRegister(_coyoteTime, nameof(_coyoteTime));
            _shootBlockTicket = Timer.CountDownRegister(_shootBlockTime, nameof(_shootBlockTime), () =>
            {
                Debug.Log("シュート可能");
                _successionCount = 0;
            });
            _invincibleTicket = Timer.CountDownRegister(_invincibleTime, nameof(_invincibleTime), () => IsInvincible = false);
            _chargeTicket = Timer.CountUpRegister("チャージ");
            Timer.GetReactiveProperty(_chargeTicket).Subscribe(time =>
            {
                if (!_chargeSEPlayed && time.Current >= 0.7f)
                {
                    PlaySE("ビーム砲チャージ", 0.3f);
                    _chargeSEPlayed = true;
                }
                if (!_halfEffectPlayed && time.Current >= _chargeShoot[1])
                {
                    // 最大チャージエフェクト
                    var effect = _chargeEffectPool.Pool.Get();
                    effect.transform.localScale = Vector3.one * 1.5f;
                    _halfEffectPlayed = true;
                }
                else if (!_fullEffectPlayed && time.Current >= _chargeShoot[0])
                {
                    // 小チャージエフェクト
                    var effect = _chargeEffectPool.Pool.Get();
                    effect.transform.localScale = Vector3.one;
                    _fullEffectPlayed = true;
                }
            }).AddTo(this);
        }

        private void Pool_OnGetEvt(ParticleSystem obj)
        {
            obj.Play();
            obj.transform.position = transform.position;
        }
    }
}