using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public partial class ActionRobot
    {
        private enum StateKey
        {
            none, // 状態なし
            idle,
            shoot,
            chargeShoot,
            fullChargeShoot,
            shootWait, // 連射待機()
            move,
            dash,
            stun,
            jump,
            fall,
            dead,
        }
        private enum Triggers
        {
            none,
            moveInput,
            dashInput,
            cancelMove,
            shootInput,
            chargeShoot,
            fullChargeShoot,
            shootComplete,
            death,
            falling,
            jumpInput,
            landing,
            stuned,
            finishedStun,
            shootInAir,
            halfChargeInAir,
            fullChargeInAir,
            watingTimeHasElapsed, // 攻撃待機中に攻撃しなかった場合に呼ばれる
        }
        [Header("States")]
        [SerializeField] private Jump _jump;
        private ShootForward _normal;
        private ShootForward _halfCharge;
        private ShootForward _fullCharge;
        private Dictionary<StateKey, string> _states = EnumWrapper.GetValueNameMap<StateKey>();

        protected override void RegisterStats()
        {
            // === 各ステートのトリガー一覧定義 ===
            var idleTriggers = new[]
            {
                (Triggers.shootInput, StateKey.shoot,""),
                (Triggers.chargeShoot, StateKey.chargeShoot,""),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot,""),
                (Triggers.moveInput, StateKey.move,"toWalk"),
                (Triggers.dashInput, StateKey.dash,"toDash"),
                (Triggers.jumpInput, StateKey.jump,"toJump"),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.falling, StateKey.fall,"toFall"),
                (Triggers.death, StateKey.dead,""),
            };
            var shootTriggers = new []
            {
                (Triggers.shootComplete, StateKey.shootWait,""),
                (Triggers.chargeShoot, StateKey.chargeShoot,""),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot,""),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.death, StateKey.dead,""),
            };

            // 遷移候補はidleとほぼ同じだが、チャージシュートができない
            var waitTriggers = new []
            {
                (Triggers.shootInput, StateKey.shootWait,""),
                (Triggers.watingTimeHasElapsed, StateKey.idle,"toIdle"),
                (Triggers.moveInput, StateKey.move,"toWalk"),
                (Triggers.dashInput, StateKey.dash,"toDash"),
                (Triggers.jumpInput, StateKey.jump,"toJump"),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.falling, StateKey.fall,"toFall"),
                (Triggers.death, StateKey.dead,""),
            };
            var chargeShootTriggers = new []
            {
                (Triggers.shootComplete, StateKey.idle,"toIdle"),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.death, StateKey.dead,""),
            };
            var fullChargeShootTriggers = new []
            {
                (Triggers.shootComplete, StateKey.idle,"toIdle"),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.death, StateKey.dead,""),
            };
            var moveTriggers = new []
            {
                (Triggers.moveInput, StateKey.move,"toWalk"),
                (Triggers.cancelMove, StateKey.idle,"toIdle"),
                (Triggers.shootInput, StateKey.shoot,""),
                (Triggers.chargeShoot, StateKey.chargeShoot,""),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot,""),
                (Triggers.dashInput, StateKey.dash,"toDash"),
                (Triggers.jumpInput, StateKey.jump,"toJump"),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.death, StateKey.dead,""),
            };
            var dashTriggers = new []
            {
                (Triggers.cancelMove, StateKey.idle,"toIdle"),
                (Triggers.shootInput, StateKey.shoot,""),
                (Triggers.chargeShoot, StateKey.chargeShoot,""),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot,""),
                (Triggers.jumpInput, StateKey.jump,"toJump"),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.death, StateKey.dead,""),
            };
            var jumpTriggers = new []
            {
                (Triggers.falling, StateKey.fall,""),
                (Triggers.landing, StateKey.idle,"toIdle"),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.death, StateKey.dead,""),
                (Triggers.shootInAir, StateKey.shoot,""),
                (Triggers.halfChargeInAir, StateKey.chargeShoot,""),
                (Triggers.fullChargeInAir, StateKey.fullChargeShoot,""),
            };
            var fallTriggers = new []
            {
                (Triggers.landing, StateKey.idle,"toIdle"),
                (Triggers.shootInAir, StateKey.shoot,""),
                (Triggers.halfChargeInAir, StateKey.chargeShoot,""),
                (Triggers.fullChargeInAir, StateKey.fullChargeShoot,""),
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.death, StateKey.dead,""),
            };
            var stunTriggers = new []
            {
                (Triggers.stuned, StateKey.stun,""),
                (Triggers.finishedStun, StateKey.idle,"toIdle"),
                (Triggers.death, StateKey.dead,"")
            };
            var deadTriggers = new (Triggers, StateKey)[]
            {
                // 死亡ステートからはトリガー無し or シーンリロードなど
            };

            // === TransitionGroup に登録 ===
            _stateMachine
                .AddTransitions(StateKey.idle, idleTriggers)
                .AddTransitions(StateKey.shootWait, waitTriggers)
                .AddTransitions(StateKey.shoot, shootTriggers)
                .AddTransitions(StateKey.chargeShoot, chargeShootTriggers)
                .AddTransitions(StateKey.fullChargeShoot, fullChargeShootTriggers)
                .AddTransitions(StateKey.move, moveTriggers)
                .AddTransitions(StateKey.dash, dashTriggers)
                .AddTransitions(StateKey.jump, jumpTriggers)
                .AddTransitions(StateKey.fall, fallTriggers)
                .AddTransitions(StateKey.stun, stunTriggers)
                .AddTransitions(StateKey.dead, deadTriggers);

            // === ステートコンポーネント登録 ===
            // idle
            _stateMachine.AddState(
                StateKey.idle,
                new Idle() // 何も動かさないデフォルトステート
            );

            var time = new Idle_LazyEvent(0.3f, false);
            time.OnCompleted += () =>
            {
                //Debug.Log("攻撃待機終了");
                _successionCount = 0;
                _stateMachine.LazyChange(Triggers.watingTimeHasElapsed);
            };
            _stateMachine.AddState(StateKey.shootWait, time);

            // shoot
            _normal = new ShootForward(_bullets[0], _targetLayer);
            _normal.SetGameObject(_muzzle);
            _normal.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            }).AddTo(this);
            _stateMachine.AddState(StateKey.shoot, _normal);

            // chargeShoot
            _halfCharge = new ShootForward(_bullets[1], _targetLayer);
            _halfCharge.SetGameObject(gameObject);
            _halfCharge.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            }).AddTo(this);
            _stateMachine.AddState(StateKey.chargeShoot, _halfCharge);

            // fullChargeShoot
            _fullCharge = new ShootForward(_bullets[2], _targetLayer);
            _fullCharge.SetGameObject(gameObject);
            _fullCharge.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            }).AddTo(this);
            _stateMachine.AddState(StateKey.fullChargeShoot, _fullCharge);
            // move
            _stateMachine.AddState(
                StateKey.move,
                new MoveOnGround()
            );

            // dash
            _stateMachine.AddState(
                StateKey.dash,
                new DashOnGround(this)
            );

            // jump
            _jump = new Jump();
            _stateMachine.AddState(
                StateKey.jump,
                _jump
                );

            // fall
            _stateMachine.AddState(
                StateKey.fall,
                new MoveOnAir()
                .SetAccel(20f)
                .SetFriction(-20f)
            );

            // stun
            _stunState = new Stun(_rigidbody, 0.7f, false)
                .SetKnockback(_stunKnockback);
            _stunState.OnCompleted += () =>
            {
                Debug.Log("スタン終了");
                _stateMachine.LazyChange(Triggers.finishedStun);
                IsInvincible = false;
            };
            _stateMachine.AddState(StateKey.stun, _stunState);

            // dead
            _stateMachine.AddState(
                StateKey.dead,
                new Idle()
            );
        }
    }
}