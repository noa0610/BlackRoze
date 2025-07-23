using BlackRose.UI;
using System;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public partial class Enemy_Turret : UnitBase
    {
        // === Data ===
        [SerializeField] private BulletData _bulletData;
        // バーストが終わった後の待機時間
        [SerializeField] private float _shootInterval = 100f;
        // 1サイクルあたりの連射数
        [SerializeField] private int _shootFireCount;
        [SerializeField] private float _trishootInterval = 0.2f; // インターバルカウント
        // 砲塔
        [SerializeField] private GameObject _turret;

        // 弾丸が検知できるレイヤー
        [SerializeField] private LayerMask _targetLayer;

        // === Reference ===
        private SearchAssistanceMono _searchAssistance;

        // === Internal ===
        private float _shootIntervalCount = 0f;  // 待機タイマー
        private int _shootCount;            // 現在までに撃ったカウント
        private float _trishootIntervalCount = 0f; // 3点バースト用のインターバルタイマー

        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (_searchAssistance.Execute("", list, out var units))
            {
                // 最短距離のプレイヤーを狙う
                units.Sort((a, b) =>
                {
                    var diffA = a.Transform.position - transform.position;
                    var diffB = b.Transform.position - transform.position;
                    return diffA.sqrMagnitude
                        .CompareTo(diffB.sqrMagnitude);
                });
                _looking.SetTarget(units[0].gameObject);
                _stateMachine.ChangeState(Triggers.FindPlayer);
            }
            else
                _stateMachine.ChangeState(Triggers.MissingPlayer);
        }

        // ShootForward が１発撃ち終わるたびに呼ばれる
        private void OnShootComplete()
        {
            Debug.Log("OnShootComplete called.");
            _stateMachine.ChangeState(Triggers.ShootComplete);
            _shootCount++;
            _trishootIntervalCount = _trishootInterval;
            if (_shootCount >= _shootFireCount)
            {
                // n発撃ったらインターバルスタート
                _shootIntervalCount = _shootInterval;
                _shootCount = 0;
                _stateMachine.LazyChange(Triggers.ShootReserve);
            }
        }

        protected override void Awake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
            base.Awake();
            HPUI.instance.Get(this); // HPUIに登録
        }

        protected override void Update()
        {
            if (!_isPlaying) return;
            // ■ インターバルカウントダウン ■
            if (_shootIntervalCount <= 0f && _stateMachine.CurrentState.key == States.inVigilance.ToString())
            {
                Direction = _looking.Direction;
                _stateMachine.ChangeState(Triggers.ShootReady);
            }
            else if (_trishootIntervalCount <= 0f && _stateMachine.CurrentState.key == States.shootInterval.ToString())
            {
                // 3点バーストのインターバルが終わったら、次の弾を撃つ
                _trishootIntervalCount = _trishootInterval;
                _stateMachine.ChangeState(Triggers.ShootReady);
            }
            else
            {
                _trishootIntervalCount = Mathf.Max(0f, _trishootIntervalCount - Time.deltaTime);
                _shootIntervalCount = Mathf.Max(0f, _shootIntervalCount - Time.deltaTime);
            }
            base.Update();
        }
        protected virtual void FixedUpdate()
        {
            if (!_isPlaying) return;
            SearchPlayer();           // プレイヤー検出＆セットアップ
        }
    }
}
