using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using HighElixir;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_huyuu : UnitBase
    {
        [Header("自爆状態")]
        [SerializeField] private SuicideBombing _suicideBombing;
        [SerializeField] private GameObject _ExplosionPartecl; // 爆発のエフェクト
        [SerializeField] private float _ExplosionParteclTime = 4f;  // エフェクト発生時間

        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();

        [Header("死亡状態")]
        [SerializeField] private float _DeadEndwaitTime = 0.2f;
        [SerializeField] private GameObject _DeadPartecl; // 死亡時のエフェクト
        [SerializeField] private float _DeadParteclTime = 4f;  // エフェクト発生時間


        [Header("SE")]
        [SerializeField] private string _ExplosionSEName = "大砲1";
        [SerializeField] private float _ExplosionSEVolume = 0.1f;
        [SerializeField] private string _DamageSEName = "敵ダメージ1";
        [SerializeField] private float _DamageSEVolume = 0.2f;
        [SerializeField] private string _DeadSEName = "敵ダメージ2";
        [SerializeField] private float _DeadSEVolume = 0.4f;

        
        // 実装
        private SearchAssistanceMono _searchAssistance;


        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.move) && _searchAssistance.Execute("red", list, out _))
            {
                PlaySE(_ExplosionSEName, _ExplosionSEVolume);
                if (_ExplosionPartecl != null)
                {
                    Destroy(
                        Instantiate(_ExplosionPartecl, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y), Quaternion.identity, null),
                        _ExplosionParteclTime);
                }
                _stateMachine.ChangeState(Triggers.AttackRange);
            }
            if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
            else if (!_searchAssistance.Execute("green", list, out _))
            {
                _stateMachine.ChangeState(Triggers.MissingPlayer);
            }

        }
        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        /// <summary>
        /// 外部から呼び出されるダメージ処理
        /// </summary>
        protected override void OnTakeDamage(IUnit from, float damage)
        {
            PlaySE(_DamageSEName, _DamageSEVolume);
            if (statusManager.ReadValue(Status.HP) <= 0)
            {
                _stateMachine.ChangeState(Triggers.Died);
            }
        }
        private async void Dead()
        {
            if (_DeadPartecl != null)
            {
                Destroy(
                    Instantiate(_DeadPartecl, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 1), Quaternion.identity, null),
                    _DeadParteclTime);
            }

            PlaySE(_DeadSEName, _DeadSEVolume);

            await UniTask.Delay(TimeSpan.FromSeconds(_DeadEndwaitTime));

            UnitManager.instance.RemoveUnit(this);
            Destroy(gameObject);

            // UniTaskエラー対策
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_DeadEndwaitTime));
            }
            catch (OperationCanceledException)
            {
                // キャンセルされたら何もしない
                return;
            }
            // オブジェクトが既に破棄されていたら続行しない
            if (this == null) return;

            UnitManager.instance.RemoveUnit(this);
            if (this != null) Destroy(gameObject);
        }

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
            if (_player != null)
            {
                Direction = (_player.Transform.position - transform.position).normalized;

                // 見た目の向きを変更（左右反転）
                if (Direction.x != 0)
                {
                    var scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * (Direction.x > 0 ? 1 : -1);
                    transform.localScale = scale;
                }
            }
        }

        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
    }
}