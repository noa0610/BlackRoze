using System;
using UniRx;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class Enemy_Turret : UnitBase
    {
        private enum States
        {
            none = 0,
            idle,
            shoot,
            shootInterval,
            dead,
        }
        private enum Triggers
        {
            None,
            MissingPlayer,
            ShootReserve,
            ShootReady,
            Died,
        }
        // === Data ===
        [SerializeField] private BulletData _bulletData;
        // バーストが終わった後の待機時間
        [SerializeField] private float _shootInterval = 100f;
        // 1サイクルあたりの連射数
        [SerializeField] private int _shootFireCount;

        // 弾丸が検知できるレイヤー
        [SerializeField] private LayerMask _targetLayer;

        // === Reference ===
        private SearchAssistanceMono _searchAssistance;

        // === Internal ===
        [SerializeField] private float _shootIntervalCount = 0f;  // 待機タイマー
        [SerializeField] private float _trishootInterval; // インターバルカウント
        private int _shootCount;            // 現在までに撃ったカウント


        // ステート登録
        protected override void RegisterStats()
        {
            var shoot = new ShootForward(_bulletData, _targetLayer);
            shoot.onShootComplete.AsObservable().Subscribe(_ => OnShootComplete());
            _stateMachine.AddState(States.idle, new Idle());
            _stateMachine.AddState(States.shoot, shoot);
            _stateMachine.AddState(States.shootInterval, new Idle());
            _stateMachine.AddState(States.dead, new Idle());

            var idleTrigger = new[]
            {
                (Triggers.ShootReady, States.shoot),
                (Triggers.ShootReserve, States.shootInterval),
                (Triggers.Died, States.dead)
            };
            var shootTrigger = new[]
            {
                (Triggers.ShootReady, States.shoot),
                (Triggers.ShootReserve, States.shootInterval),
                (Triggers.Died, States.dead),
                (Triggers.MissingPlayer, States.idle)
            };
            var reserveTrigger = new[]
            {
                (Triggers.ShootReady, States.shoot),
                (Triggers.MissingPlayer, States.idle),
                (Triggers.Died, States.dead)
            };
        }

        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (_searchAssistance.Execute("", list, out var units))
            {
                if (_shootInterval >= 0f || _trishootInterval >= 0f)
                {
                    _stateMachine.ChangeState(Triggers.ShootReserve);
                    return;
                }
                // 最短距離のプレイヤーを狙う
                units.Sort((a, b) =>
                {
                    var diffA = a.Transform.position - transform.position;
                    var diffB = b.Transform.position - transform.position;
                    return diffA.sqrMagnitude
                        .CompareTo(diffB.sqrMagnitude);
                });
                _bulletData.originalstatus.direction = (units[0].Transform.position - transform.position).normalized;
                _stateMachine.ChangeState(Triggers.ShootReady);
            }
            else
                _stateMachine.ChangeState(Triggers.MissingPlayer);
        }

        // ShootForward が１発撃ち終わるたびに呼ばれる
        private void OnShootComplete()
        {
            _trishootInterval = 0.3f;
            _shootCount++;
            if (_shootCount >= _shootFireCount)
            {
                // n発撃ったらインターバルスタート
                _shootIntervalCount = _shootInterval;
                _shootCount = 0;
            }
        }

        protected override void Awake()
        {
            Direction = Vector2.right;
            base.Awake();
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        protected override void Update()
        {
            if (!_isPlaying) return;
            // ■ インターバルカウントダウン ■
            if (_shootIntervalCount > 0f)
                _shootIntervalCount = Mathf.Max(0f, _shootIntervalCount - Time.deltaTime);
            if (_trishootInterval > 0f)
                _trishootInterval = Mathf.Max(0f, _trishootInterval - Time.deltaTime);

            SearchPlayer();           // プレイヤー検出＆セットアップ
            base.Update();
        }
    }
}//unicode
