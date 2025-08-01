using UniRx;
namespace BlackRose
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
        }

        private Jump _jump;
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
        (Triggers.shootInput, StateKey.shoot),
        (Triggers.shootComplete, StateKey.idle),
        (Triggers.chargeShoot, StateKey.chargeShoot),
        (Triggers.fullChargeShoot, StateKey.fullChargeShoot),
        (Triggers.stuned, StateKey.stun),
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
            };
            var fallTriggers = new (Triggers, StateKey)[]
            {
        (Triggers.landing, StateKey.idle),
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
                // ★追加★ 死亡ステートからはトリガー無し or シーンリロードなど
            };

            // === TransmissionGroup に登録 ===
            _stateMachine
                .AddTransmissions(StateKey.idle, idleTriggers)
                .AddTransmissions(StateKey.shoot, shootTriggers)
                .AddTransmissions(StateKey.chargeShoot, chargeShootTriggers)
                .AddTransmissions(StateKey.fullChargeShoot, fullChargeShootTriggers)
                .AddTransmissions(StateKey.move, moveTriggers)
                .AddTransmissions(StateKey.dash, dashTriggers)
                .AddTransmissions(StateKey.jump, jumpTriggers)
                .AddTransmissions(StateKey.fall, fallTriggers)
                .AddTransmissions(StateKey.dead, deadTriggers);

            // === ステートコンポーネント登録 ===
            // idle
            _stateMachine.AddState(
                StateKey.idle.ToString(),
                new Idle() // 何も動かさないデフォルトステート
            );

            // shoot
            var normal = new ShootForward(_bullets[0], _targetLayer)
                .SetAnimeTrigger("Attack");
            _stateMachine.AddState(StateKey.shoot.ToString(), normal);
            normal.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            });
            // chargeShoot
            var charge = new ShootForward(_bullets[1], _targetLayer)
                .SetAnimeTrigger("Attack");
            _stateMachine.AddState(StateKey.chargeShoot.ToString(), charge);
            charge.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            });
            // fullChargeShoot
            var full = new ShootForward(_bullets[2], _targetLayer)
                .SetAnimeTrigger("Attack");
            _stateMachine.AddState(StateKey.fullChargeShoot.ToString(), full);
            full.onShootComplete.AsObservable().Subscribe(_ =>
            {
                _stateMachine.ChangeState(Triggers.shootComplete);
            });
            // move
            _stateMachine.AddState(
                StateKey.move.ToString(),
                new MoveOnGround(
                    _rigidbody,
                    statusManager.GetStatusAmount(Status.Speed)
                )
            );

            // dash
            _stateMachine.AddState(
                StateKey.dash.ToString(),
                new DashOnGround(
                    _rigidbody,
                    this,
                    statusManager.GetStatusAmount(Status.DashSpeed)
                )
            );

            // jump
            _jump = new Jump(_rigidbody, statusManager.GetStatusAmount(Status.SpeedInAir));
            _stateMachine.AddState(
                StateKey.jump.ToString(),
                _jump
                );

            // fall
            _stateMachine.AddState(
                StateKey.fall.ToString(),
                new MoveOnGround(
                    _rigidbody,
                    statusManager.GetStatusAmount(Status.SpeedInAir)
                )
            );

            // stun
            _stunState = new Stun(_rigidbody)
                .SetDuration(0.7f)
                .SetKnockback(_stunKnockback)
                .SetThrower(_thrower);
            _stateMachine.AddState(StateKey.stun.ToString(), _stunState);

            // dead
            _stateMachine.AddState(
                StateKey.dead.ToString(),
                new Idle()
            );
        }
    }
}