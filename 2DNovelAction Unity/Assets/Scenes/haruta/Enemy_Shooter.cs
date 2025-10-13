using UnityEngine;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_Shooter : GroundedUnit
    {
        [SerializeField] private BulletData _bulletData;
        [SerializeField] private Rigidbody2D _rb2;

        public enum States
        {
            none,
            idle,
            move,
            shootReady,
            shoot,
            knockBack,
            dead
        }

        private enum Triggers
        {
            None,
            MissingPlayer,
            FoundPlayer,
            AttackRange,
            shootCoolDown,
            shoot,
            Died,
            time,
            Damage,
        }

        private SearchAssistanceMono _searchAssistance;

        // shootReady 用タイマー
        private float shootReadyTimer = 0f;
        private float shootReadyDuration = 1.0f; // クールダウン秒数

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        protected override void OnGrounded() { }
        protected override void OnUnGrounded() { }

        protected override void RegisterStats()
        {
            // トランジション設定
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.shootReady),
                (Triggers.MissingPlayer, States.move),
                (Triggers.Damage, States.knockBack)
            };
            var moveTrigger = new[]
            {
                (Triggers.FoundPlayer, States.shootReady),
                (Triggers.Damage, States.knockBack)
            };
            var shootTrigger = new[]
            {
                (Triggers.shoot, States.idle),
                (Triggers.Damage, States.knockBack)
            };
            var shootReadyTrigger = new[]
            {
                (Triggers.shootCoolDown, States.shoot),
                (Triggers.Damage, States.knockBack)
            };
            var knockBackTrigger = new[]
            {
                (Triggers.time, States.idle),
                (Triggers.Died, States.dead)
            };

            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.move, moveTrigger)
                .AddTransmissions(States.shoot, shootTrigger)
                .AddTransmissions(States.shootReady, shootReadyTrigger)
                .AddTransmissions(States.knockBack, knockBackTrigger);

            // ステート設定
            _stateMachine.AddState(States.idle, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));
            var s = statusManager.GetStatus(Status.Speed);
            var move = new MoveOnGround(_rb2, true).SetAnimeTrigger("move").SetCancelableProgress(0);
            _stateMachine.AddState(States.move, move);

            var shootReady = new Idle().SetAnimeTrigger("shootReady").SetCancelableProgress(0);
            _stateMachine.AddState(States.shootReady, shootReady);

            var attack = new ShootForward();
            attack.SetBullet(_bulletData);
            attack.SetAnimeTrigger("shoot");
            attack.SetCancelableProgress(0);
            _stateMachine.AddState(States.shoot, attack);

            _stateMachine.AddState(States.knockBack, new Stun(_rb2, 0.6f, false).SetAnimeTrigger("knockBack").SetCancelableProgress(0));
            _stateMachine.AddState(States.dead, new Idle());
        }

        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (_stateMachine.CurrentState.key == States.move.ToString() && _searchAssistance.Execute("red", list, out var ui))
            {
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
            if (_stateMachine.CurrentState.key == States.idle.ToString() && _searchAssistance.Execute("yellow", list, out var units))
            {
                units.Sort((a, b) =>
                {
                    var diffA = a.Transform.position - transform.position;
                    var diffB = b.Transform.position - transform.position;
                    return diffA.sqrMagnitude
                        .CompareTo(diffB.sqrMagnitude);
                });
                //         // _player = units[0];エラー発生中のためコメントアウト
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
            else
                _stateMachine.ChangeState(Triggers.MissingPlayer);

        }

        protected override  void AfterFixedUpdate()
        {
            SearchPlayer();

            Debug.Log(_stateMachine.CurrentState.key);
        }
    }
}