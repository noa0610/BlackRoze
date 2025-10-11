using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using HighElixir;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_huyuu : UnitBase
    {
        [SerializeField] private SuicideBombing _suicideBombing;
        [SerializeField] private FreeMove _freeMove;
        [SerializeField] private Rigidbody2D _RB2;
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetDict<States>();
        public enum States
        {
            none,
            move, // 移動
            idle,// 待機
            dead,// 死亡
            explosion,// 爆発
        }
        private enum Triggers
        {
            None,
            MissingPlayer, // プレイヤーを見失った
            FoundPlayer,   // プレイヤーを発見した
            AttackRange,   // 攻撃範囲に入った
            Explosion,     // 爆発した
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.move),
                (Triggers.Died, States.dead)
            };
            var moveTrigger = new[]
            {
                (Triggers.MissingPlayer, States.idle),
                (Triggers.AttackRange, States.explosion),
                (Triggers.Died, States.dead)
            };
            var explosionTrigger = new[]
            {
                (Triggers.Explosion, States.dead),
                (Triggers.Died, States.dead)
            };
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.move, moveTrigger)
                .AddTransitions(States.explosion, explosionTrigger);
            // 死んだときに何もしないならDeadの設定はいらない

            // 待機
            var idle = new Idle().SetAnimeTrigger("Drone_Idle").SetCancelableProgress(0);
            _stateMachine.AddState(States.idle, idle);

            // 移動
            var move = _freeMove.SetAnimeTrigger("move").SetCancelableProgress(0);
            _stateMachine.AddState(States.move, move);

            //爆発
            _suicideBombing.SetAnimeTrigger("Explosion").SetCancelableProgress(0);
            _suicideBombing.OnExplode.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Died);
            });
            _stateMachine.AddState(States.explosion, _suicideBombing);
            // 死亡
            var died = new Idle().SetAnimeTrigger("died").SetCancelableProgress(0);
            died.OnAnimationCompleted.AddListener(() =>
            {
                Debug.Log("Enemy_huyuu: 死亡アニメーションが完了しました。");
                UnitManager.instance.RemoveUnit(this);
                Destroy(gameObject);
            });
            _stateMachine.AddState(States.dead, died);
        }
        // 実装
        private SearchAssistanceMono _searchAssistance;


        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.move) && _searchAssistance.Execute("red", list, out _))
            {
                _stateMachine.ChangeState(Triggers.AttackRange);
            }
            if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
            else if (!_searchAssistance.Execute("green", list, out _))
                _stateMachine.ChangeState(Triggers.MissingPlayer);

        }
        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
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