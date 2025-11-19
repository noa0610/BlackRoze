using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.UI;
using BlackRose.Core.Models.Helper;
using BlackRose.Datas.Definitions;
using System;
using UnityEngine;
using BlackRose.Core.Models.Units.Helpers;
using Cysharp.Threading.Tasks;
using BlackRose.Core.Models.Units.State;

namespace BlackRose.Core.Models.Units
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
        [SerializeField] private float _shootIntervalCount = 0f;  // 待機タイマー
        [SerializeField] private int _shootCount;            // 現在までに撃ったカウント
        [SerializeField] private float _trishootIntervalCount = 0f; // 3点バースト用のインターバルタイマー

        private void SearchPlayer()
        {
            if (_searchAssistance.Execute("", UnitManager.instance.GetUnitList(), out var units))
            {
                var unit = units.GetUnitNearest(transform.position);
                if (unit != null)
                    _looking.SetTarget(unit.gameObject);
                _stateMachine.ChangeState(Triggers.FindPlayer);
            }
            else
                _stateMachine.ChangeState(Triggers.MissingPlayer);
        }

        // ShootForward が１発撃ち終わるたびに呼ばれる
        private void OnShootComplete()
        {
            // _stateMachine.CurrentState.key == States.shoot.ToString() &&
            //Debug.Log("OnShootComplete called.");
            _shootCount++;
            _trishootIntervalCount = _trishootInterval;
            if (_shootCount >= _shootFireCount)
            {
                // n発撃ったらインターバルスタート
                _shootIntervalCount = _shootInterval;
                _shootCount = 0;
                _stateMachine.LazyChange(Triggers.ShootReserve);
            }
            else
            {
                _stateMachine.LazyChange(Triggers.ShootComplete);
            }
        }

        protected override void AfterAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
            // HPUI.instance.Get(this); // HPUIに登録  
        }
        protected override void AfterFixedUpdate()
        {
            // Debug.Log($"{IsInvincible}");
            // クールタイムのカウントダウン
            var dt = Time.fixedDeltaTime;
            _trishootIntervalCount = Mathf.Max(0f, _trishootIntervalCount - dt);

            // 砲塔の向き更新
            Direction = _looking.Direction;
            ShootDir = _looking.Direction;
            // ステートの判断
            if (_shootIntervalCount <= 0f && IsMatchState(States.inVigilance) && _looking.IsLookingTarget(35f))
            {
                _shoot.SetDirection(Direction);
                _stateMachine.ChangeState(Triggers.ShootReady);
            }
            else if (_trishootIntervalCount <= 0f && IsMatchState(States.shootInterval))
            {
                // 3点バーストのインターバルが終わったら、次の弾を撃つ  
                _trishootIntervalCount = _trishootInterval;
                _stateMachine.ChangeState(Triggers.IntervalIsFinished);
            }
            if (IsMatchState(States.idle, States.inVigilance))
            {
                _shootIntervalCount = Mathf.Max(0f, _shootIntervalCount - dt);
                SearchPlayer();           // プレイヤー検出＆セットアップ
            }
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            UnitManager.instance.RemoveUnit(this);
            Destroy(gameObject);
        }
        private bool IsMatchState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
        private bool IsMatchState(States arg1, States arg2)
        {
            return IsMatchState(arg1) || IsMatchState(arg2);
        }
        private void OnDestroy()
        {
            // if (HPUI.instance != null)
            //     HPUI.instance.Release(this); // HPUIから削除
        }
    }
}
