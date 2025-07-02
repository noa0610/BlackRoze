using System;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class Enemy_Turret : UnitBase
    {
        // === Data ===
        [SerializeField] private BulletData _bulletData;
        [SerializeField] private float _rotateSpeed;
        [SerializeField] private float _shootInterval = 100f;
        [SerializeField] private int _shootFireCount;       // 1サイクルあたりの連射数
        [SerializeField] private LayerMask _targetLayer;

        // === Reference ===
        private SearchAssistanceMono _searchAssistance;

        // === Internal ===
        [SerializeField] private float _shootIntervalCount = 0f;  // 待機タイマー
        [SerializeField] private float _trishootInterval; // インターバルカウント
        private int _shootCount;            // 現在までに撃ったカウント

        // === StateMachine ===
        protected override StateComp DefaultState => new Idle_Rotate(transform, _rotateSpeed);
        private StateFlags _stateFlags = StateFlags.None;

        // ステート登録
        protected override void RegisterStats()
        {
            var shoot = new ShootForward(_bulletData, _targetLayer, "");
            shoot.onShootComplete += OnShootComplete;
            _stateMachine.AddState("shoot", shoot);
            _stateMachine.AddState("shootInterval", new Idle());
        }

        protected override void Update()
        {
            // ■ インターバルカウントダウン ■
            if (_shootIntervalCount > 0f)
                _shootIntervalCount = Mathf.Max(0f, _shootIntervalCount - Time.deltaTime);
            if (_trishootInterval > 0f)
                _trishootInterval = Mathf.Max(0f, _trishootInterval - Time.deltaTime);

            SearchPlayer();           // プレイヤー検出＆セットアップ
            base.Update();
        }

        private void SearchPlayer()
        {
            _stateFlags &= ~StateFlags.InShoot;
            var list = UnitManager.instance.GetUnitList();
            var result = _searchAssistance.Execute(list);
            if (result != null && result.Count > 0)
            {
                _stateFlags |= StateFlags.InShoot;
                // 最短距離のプレイヤーを狙う
                result.Sort((a, b) =>
                {
                    var diffA = a.Transform.position - transform.position;
                    var diffB = b.Transform.position - transform.position;
                    return diffA.sqrMagnitude
                        .CompareTo(diffB.sqrMagnitude);
                });
                _bulletData.originalstatus.direction = (result[0].Transform.position - transform.position).normalized;
            }
        }
        // ステート遷移判定
        protected override string StateDecision()
        {
            // 1) インターバル中は必ずshootInterval
            if (_shootIntervalCount > 0f || _trishootInterval > 0f)
                return "shootInterval";

            // 2) プレイヤー見つかってて、インターバル終了ならshoot
            if (_stateFlags.HasFlag(StateFlags.InShoot))
                return "shoot";

            // 3) それ以外は回転待機(idle)
            return "idle";
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
    }
}//unicode
