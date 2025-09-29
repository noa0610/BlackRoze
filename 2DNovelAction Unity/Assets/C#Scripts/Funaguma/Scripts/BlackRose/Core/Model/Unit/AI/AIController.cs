using AIE2D;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using HighElixir;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(
        typeof(Rigidbody2D),
        typeof(UnityEngine.InputSystem.PlayerInput),
        typeof(DynamicAfterImageEffect2DPlayer))]
    public class AIController : GroundedUnit
    {
        public enum Mode { Normal, Light, Heavy }

        // 各モードに共通するステート
        public enum AIStates { Idle, Move, Jump, Fall, Dash, SpecialAttack, Dead, ShootInterval }
        public enum AITriggers
        {
            moveInput, cancelMove, dashInput, shootInput, halfCharge, fullCharge, jumpInput,
            shootCompleted, watingTimeHasElapsed, skillInput, skillFinished,
            landing, falling, stun, recoverFromStun,
            modeChanged
        }
        public enum Tags
        {
            Shoot, Stunned
        }
        public readonly static Dictionary<Tags, string> tags = EnumWrapper.GetDict<Tags>();
        [Header("Reference")]
        [SerializeField] private List<BulletData> _bullets = new();
        [SerializeField] private LayerMask _targetLayer;

        [Header("Option Settings")]
        [SerializeField] private bool _canChargeCount = false;

        // 時間管理
        [SerializeField] private float _shootBlockTime = 0.6f;
        [SerializeField] private float _coyoteTime = 0.2f;
        private float _shootPressTime = 0f;
        private Vector2 _shootDirection = Vector2.right;

        // ===== モード関連 =====
        [Header("Mode")]
        [SerializeField] private NormalMode _normalMode; 
        [SerializeField] private LightMode _lightMode;
        [SerializeField] private HeavyMode _heavyMode;
        private Mode _currentEnumMode = Mode.Normal;

        // ===== State Machine =====
        public AIModeBase CurrentMode => _currentEnumMode switch
        {
            Mode.Normal => _normalMode,
            Mode.Heavy => _heavyMode,
            Mode.Light => _lightMode,
            _ => _normalMode
        };

        public List<BulletData> Bullets => _bullets;
        public bool CanJump => !Timer.IsFinished(nameof(_coyoteTime));
        // 外部からのモード切替 API
        public void SwitchModeLight() => ChangeMode(Mode.Light);
        public void SwitchModeHeavy() => ChangeMode(Mode.Heavy);
        public void SwitchModeNormal() => ChangeMode(Mode.Normal);


        // === Input Action ===
        private void OnJump(InputValue value)
        {
            Debug.Log($"OnJump: CanJump={CanJump}, isPressed={value.isPressed}");
            if (CanJump)
            {
                CurrentMode.OnJump(value);
            }
            if (!value.isPressed)
            {
                CurrentMode.CanceldJump(value);
            }
        }

        private void OnDash(InputValue value)
        {
            if (!value.isPressed)
            {
                _stateMachine.LazyChange(AITriggers.cancelMove);
                return;
            }
            if (!IsGrounded) return;
            _stateMachine.ChangeState(AITriggers.dashInput);
        }

        private void OnMove(InputValue value)
        {
            var d = value.Get<Vector2>();
            var tmp = d;
            tmp.y = 0;
            MoveDirection = tmp;
            CurrentMode.OnInputMove(d);
            if (d.x == 0)
            {
                _stateMachine.ChangeState(AITriggers.cancelMove);
                return;
            }
            else if (d.x != 0 && !_stateMachine.CurrentState.HasTag(tags[Tags.Shoot], tags[Tags.Stunned]))
            {
                Direction = d.normalized;
                _shootDirection = d;
            }
            _stateMachine.ChangeState(AITriggers.moveInput);
        }

        private void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("AI Attack Pressed");
                CurrentMode.OnShoot(value);
                Timer.Start("chargeTime");
            }
            else
            {
                CurrentMode.OnReleaseShoot(value);
            }
        }

        private void OnSkill(InputValue value)
        {
            CurrentMode.OnSkill(value);
        }
        // === GroundedUnit の抽象 ===
        protected override void OnGrounded()
        {
            Timer.Reset(nameof(_coyoteTime));
            _stateMachine.ChangeState(AITriggers.landing);
            CurrentMode.OnGrounded();
        }
        protected override void OnFall()
        {
            _stateMachine.ChangeState(AITriggers.falling);
        }

        public override void AfterJump()
        {
            base.AfterJump();
            // ジャンプしたのでコヨーテタイムを終了させる
            Timer.Stop(nameof(_coyoteTime));
        }
        // === Private ===

        private void ChangeMode(Mode mode)
        {
            // ★ ステータス反映
            var status = CurrentMode.StatusData;
            statusManager.GetStatus(Status.MaxHP).SetDefault(status.maxHp);
            statusManager.GetStatus(Status.Speed).SetDefault(status.speed);
            statusManager.GetStatus(Status.SpeedInAir).SetDefault(status.speedInAir);
            statusManager.GetStatus(Status.JumpPower).SetDefault(status.jumpPower);
            statusManager.GetStatus(Status.DashSpeed).SetDefault(status.dashSpeed);
            statusManager.GetStatus(Status.Power).SetDefault(status.power);
            statusManager.GetStatus(Status.DamageRatio).SetDefault(status.damageTakeScale);

            _currentEnumMode = mode;

            // ★ ステートマシンへモードを通知（最重要！）
            _stateMachine.SetLayer(mode.ToString());

            // ★ 必要に応じてモード専用 Entry を走らせるなら Trigger で
            _stateMachine.LazyChange(AITriggers.modeChanged);
        }
        protected override void BeforeAwake()
        {
            Timer.CountDownRegister(nameof(_coyoteTime), _coyoteTime);
            Timer.CountDownRegister(nameof(_shootBlockTime), _shootBlockTime);
            Timer.CountUpRegister("chargeTime");

            _normalMode.Bind(this);
            _lightMode.Bind(this);
            _heavyMode.Bind(this);
        }

        protected override void AfterAwake()
        {
            ChangeMode(Mode.Normal); // ★ 初期モードへ（SetMode連動 & 遷移通知）
        }

        protected override void RegisterStats()
        {
            _normalMode.Register();
            _lightMode.Register();
            _heavyMode.Register();

            // モード非依存の共通フォールバック

            // Idle
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.Idle,
                (AITriggers.moveInput, AIStates.Move, ""),
                (AITriggers.dashInput, AIStates.Dash, ""),
                (AITriggers.falling, AIStates.Fall, "")
                );
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.ShootInterval,
                (AITriggers.watingTimeHasElapsed, AIStates.Idle, ""),
                (AITriggers.moveInput, AIStates.Move, ""),
                (AITriggers.dashInput, AIStates.Dash, ""),
                (AITriggers.falling, AIStates.Fall, "")
                );
            // Move
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.Move,
                (AITriggers.cancelMove, AIStates.Idle, ""),
                (AITriggers.dashInput, AIStates.Dash, ""),
                (AITriggers.jumpInput, AIStates.Jump, "")
                );
            // Fall
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.Fall,
                (AITriggers.landing, AIStates.Idle, "")
                );

            _stateMachine.AddState(AIStates.Idle, new Idle());
            _stateMachine.AddState(AIStates.Fall, new MoveOnAir());
            _stateMachine.AddState(AIStates.Move, new MoveOnGround());
            _stateMachine.AddState(AIStates.Dash, new DashOnGround(this));

            var idle = new Idle_LazyEvent();
            _stateMachine.AddState(AIStates.ShootInterval, idle);

            idle.SetTime(0.2f);
            idle.OnCompleted += () =>
            {
                _stateMachine.ChangeState(AITriggers.watingTimeHasElapsed);
            };
        }
        protected override void Start()
        {
            ReactiveDirection.Subscribe(d =>
            {
                if (d.x > 0) transform.localScale = Vector3.one;
                else if (d.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            }).AddTo(this);
            float dt = Time.deltaTime;
            this.UpdateAsObservable()
                .Where(_ => _canChargeCount && _isPlaying)
                .Subscribe(_ => _shootPressTime += dt)
                .AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => _isPlaying)
                .Subscribe(_ =>
                {
                    Timer.Update(dt);
                    CurrentMode.Update(dt);
                })
                .AddTo(this);
            this.FixedUpdateAsObservable()
                .Where(_ => _isPlaying)
                .Subscribe(_ => CurrentMode?.FixedUpdate(Time.fixedDeltaTime))
                .AddTo(this);
        }
    }
}
