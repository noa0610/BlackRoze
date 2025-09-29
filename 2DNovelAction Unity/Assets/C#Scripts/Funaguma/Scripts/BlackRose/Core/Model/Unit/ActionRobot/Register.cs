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
        [SerializeField] private Jump _jump;
        private ShootForward _normal;
        private ShootForward _halfCharge;
        private ShootForward _fullCharge;
        private Dictionary<StateKey, string> _states = EnumWrapper.GetDict<StateKey>();
        protected override void RegisterStats()
        {
            // === 各ステートのトリガー一覧定義 ===
            var idleTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.shootInput, StateKey.shoot),
                (Triggers.chargeShoot, StateKey.chargeShoot),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot),
                (Triggers.moveInput, StateKey.move),
                (Triggers.dashInput, StateKey.dash),
                (Triggers.jumpInput, StateKey.jump),
                (Triggers.stuned, StateKey.stun),
                (Triggers.falling, StateKey.fall),
                (Triggers.death, StateKey.dead),
            };
            var shootTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.shootComplete, StateKey.shootWait),
                (Triggers.chargeShoot, StateKey.chargeShoot),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot),
                (Triggers.stuned, StateKey.stun),
                (Triggers.death, StateKey.dead),
            };

            // 遷移候補はidleとほぼ同じだが、チャージシュートができない
            var waitTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.shootInput, StateKey.shoot),
                (Triggers.watingTimeHasElapsed, StateKey.idle),
                (Triggers.moveInput, StateKey.move),
                (Triggers.dashInput, StateKey.dash),
                (Triggers.jumpInput, StateKey.jump),
                (Triggers.stuned, StateKey.stun),
                (Triggers.falling, StateKey.fall),
                (Triggers.death, StateKey.dead),
            };
            var chargeShootTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.shootComplete, StateKey.idle),
                (Triggers.stuned, StateKey.stun),
                (Triggers.death, StateKey.dead),
            };
            var fullChargeShootTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.shootComplete, StateKey.idle),
                (Triggers.stuned, StateKey.stun),
                (Triggers.death, StateKey.dead),
            };
            var moveTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.moveInput, StateKey.move),
                (Triggers.cancelMove, StateKey.idle),
                (Triggers.shootInput, StateKey.shoot),
                (Triggers.chargeShoot, StateKey.chargeShoot),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot),
                (Triggers.dashInput, StateKey.dash),
                (Triggers.jumpInput, StateKey.jump),
                (Triggers.stuned, StateKey.stun),
                (Triggers.death, StateKey.dead),
            };
            var dashTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.cancelMove, StateKey.idle),
                (Triggers.shootInput, StateKey.shoot),
                (Triggers.chargeShoot, StateKey.chargeShoot),
                (Triggers.fullChargeShoot, StateKey.fullChargeShoot),
                (Triggers.jumpInput, StateKey.jump),
                (Triggers.stuned, StateKey.stun),
                (Triggers.death, StateKey.dead),
            };
            var jumpTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.falling, StateKey.fall),
                (Triggers.landing, StateKey.idle),
                (Triggers.stuned, StateKey.stun),
                (Triggers.death, StateKey.dead),
                (Triggers.shootInAir, StateKey.shoot),
                (Triggers.halfChargeInAir, StateKey.chargeShoot),
                (Triggers.fullChargeInAir, StateKey.fullChargeShoot),
            };
            var fallTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.landing, StateKey.idle),
                (Triggers.shootInAir, StateKey.shoot),
                (Triggers.halfChargeInAir, StateKey.chargeShoot),
                (Triggers.fullChargeInAir, StateKey.fullChargeShoot),
                (Triggers.stuned, StateKey.stun),
                (Triggers.death, StateKey.dead),
            };
            var stunTriggers = new (Triggers, StateKey)[]
            {
                (Triggers.stuned, StateKey.stun),
                (Triggers.finishedStun, StateKey.idle),
                (Triggers.death, StateKey.dead),
            };
            var deadTriggers = new (Triggers, StateKey)[]
            {
                // 死亡ステートからはトリガー無し or シーンリロードなど
            };

            // === TransmissionGroup に登録 ===
            _stateMachine
                .AddTransmissions(StateKey.idle, idleTriggers)
                .AddTransmissions(StateKey.shootWait, waitTriggers)
                .AddTransmissions(StateKey.shoot, shootTriggers)
                .AddTransmissions(StateKey.chargeShoot, chargeShootTriggers)
                .AddTransmissions(StateKey.fullChargeShoot, fullChargeShootTriggers)
                .AddTransmissions(StateKey.move, moveTriggers)
                .AddTransmissions(StateKey.dash, dashTriggers)
                .AddTransmissions(StateKey.jump, jumpTriggers)
                .AddTransmissions(StateKey.fall, fallTriggers)
                .AddTransmissions(StateKey.stun, stunTriggers)
                .AddTransmissions(StateKey.dead, deadTriggers);

            // === ステートコンポーネント登録 ===
            // idle
            _stateMachine.AddState(
                StateKey.idle,
                new Idle() // 何も動かさないデフォルトステート
            );

            var time = new Idle_LazyEvent(0.3f, false);
            time.LazyEvent.AsObservable().Subscribe(_ =>
            {
                //Debug.Log("攻撃待機終了");
                _successionCount = 0;
                _stateMachine.LazyChange(Triggers.watingTimeHasElapsed);
            }).AddTo(this);
            _stateMachine.AddState(StateKey.shootWait, time);

            // shoot
            _normal = new ShootForward(_bullets[0], _targetLayer)
                .SetAnimeTrigger("Attack")
                .SetMuzzle(gameObject);
            _normal.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            }).AddTo(this);
            _stateMachine.AddState(StateKey.shoot, _normal);

            // chargeShoot
            _halfCharge = new ShootForward(_bullets[1], _targetLayer)
                .SetAnimeTrigger("Attack")
                .SetMuzzle(gameObject);
            _halfCharge.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            }).AddTo(this);
            _stateMachine.AddState(StateKey.chargeShoot, _halfCharge);

            // fullChargeShoot
            _fullCharge = new ShootForward(_bullets[2], _targetLayer)
                .SetAnimeTrigger("Attack")
                .SetMuzzle(gameObject);
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
                .SetAccel(40f)
                .SetAirFriction(-20f)
            );

            // stun
            _stunState = new Stun(_rigidbody, 0.7f, false)
                .SetKnockback(_stunKnockback)
                .SetThrower(_thrower);
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