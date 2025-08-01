using BlackRose.UI;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
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
            // _stateMachine.CurrentState.key == States.shoot.ToString() &&
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
            statusManager.DeadCallBack += () =>
            {
                _stateMachine.ChangeState(Triggers.Died);
            };
            base.Awake();
            HPUI.instance.Get(this); // HPUIに登録  

            this.UpdateAsObservable().Where(_ => _isPlaying).Subscribe(_ =>
                {
                    _trishootIntervalCount = Mathf.Max(0f, _trishootIntervalCount - Time.deltaTime);
                    _shootIntervalCount = Mathf.Max(0f, _shootIntervalCount - Time.deltaTime);
                    Direction = _looking.Direction;
                    // ■ インターバルカウントダウン ■  
                    if (_shootIntervalCount <= 0f && _stateMachine.CurrentState.key == States.inVigilance.ToString())
                    {
                        _stateMachine.ChangeState(Triggers.ShootReady);
                    }
                    else if (_trishootIntervalCount <= 0f && _stateMachine.CurrentState.key == States.shootInterval.ToString())
                    {
                        // 3点バーストのインターバルが終わったら、次の弾を撃つ  
                        _trishootIntervalCount = _trishootInterval;
                        _stateMachine.ChangeState(Triggers.ShootReady);
                    }
                }).AddTo(this); // 発射完了時の処理を登録  
        }
        protected virtual void FixedUpdate()
        {
            if (!_isPlaying) return;
            SearchPlayer();           // プレイヤー検出＆セットアップ
        }
        private void OnDestroy()
        {
            HPUI.instance.Release(this); // HPUIから削除
            statusManager.DeadCallBack -= () =>
            {
                _stateMachine.ChangeState(Triggers.Died);
            };
        }
    }
}
