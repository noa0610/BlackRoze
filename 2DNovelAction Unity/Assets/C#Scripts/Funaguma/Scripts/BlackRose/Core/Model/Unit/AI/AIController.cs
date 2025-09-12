using AIE2D;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using Fungus;
using HighElixir;
using HighElixir.UI;
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
        public enum AIStates { Idle, Move, Jump, Fall, Shoot, HalfCharge, FullCharge, Skill, Dash, SpecialAttac, Deadk }
        public enum AITriggers
        {
            moveInput, cancelMove, dashInput, shootInput, jumpInput,
            shootComplete, watingTimeHasElapsed, skillInput, skillFinished,
            landing, falling, stun, recoverFromStun,
            modeChanged
        }

        [Header("Reference")]
        [SerializeField] private List<BulletData> _bullets = new();
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private TextThrower _thrower;
        private Stun _stunState;
        private Rigidbody2D _rigidbody;
        private TimeHolders _timeHolders = new();

        [Header("Option Settings")]
        [SerializeField] private bool _canChargeCount = false;
        [SerializeField] private Vector2 _stunKnockback = Vector2.zero;
        [SerializeField] private int _maxSuccession = 3;

        // 時間管理
        [SerializeField] private float[] _chargeShoot = new float[2] { 1.8f, 3.4f };
        [SerializeField] private float _shootBlockTime = 0.6f;
        [SerializeField] private float _coyoteTime = 0.2f;
        private int _successionCount = 0;
        private float _shootPressTime = 0f;
        private Vector2 _shootDirection = Vector2.right;

        // ===== モード関連 =====
        [Header("Mode")]
        [SerializeField] private NormalMode _normalMode;   // ScriptableObject なら Serialize でOK
        [SerializeField] private LightMode _lightMode;
        [SerializeField] private HeavyMode _heavyMode;
        private Mode _currentEnumMode = Mode.Normal;       // 実体保持（任意）


        public TimeHolders TimeHolders => _timeHolders;
        public AIModeBase CurrentMode => _currentEnumMode switch
        {
            Mode.Normal => _normalMode,
            Mode.Heavy => _heavyMode,
            Mode.Light => _lightMode,
            _ => _normalMode
        };
        public bool CanJump => !TimeHolders.IsFinished(nameof(_coyoteTime));
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
            Direction = d.normalized;
            CurrentMode.OnInputMove(Direction);
            if (d == Vector2.zero)
            {
                _stateMachine.ChangeState(AITriggers.cancelMove);
                return;
            }
            else if (d.x != 0) _shootDirection = d;
            _stateMachine.ChangeState(AITriggers.moveInput);
        }

        private void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("AI Attack Pressed");
                CurrentMode.OnShoot(value);
            }
            else
            {
                CurrentMode.OnReleaseShoot(value);
            }
        }

        private void OnSkill(InputValue value)
        {
            Debug.Log("Skill");
            CurrentMode.OnSkill(value);
        }
        // === GroundedUnit の抽象 ===
        protected override void OnGrounded()
        {
            _timeHolders.Reset(nameof(_coyoteTime));
            _stateMachine.ChangeState(AITriggers.landing);
        }
        protected override void OnFall()
        {
            _stateMachine.ChangeState(AITriggers.falling);
        }

        public override void AfterJump()
        {
            base.AfterJump();
            // ジャンプしたのでコヨーテタイムを終了させる
            _timeHolders.Stop(nameof(_coyoteTime));
        }
        // === Private ===
        private void InitAIState()
        {
            // ★ ここで各モードの IAIState 実装を作る
            _normalMode.Bind(this);

            //_rightMode = new RightMode(this, _rightStatus);
            //_heavyMode = new HeavyMode(this, _heavyStatus);
        }

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
            _rigidbody = GetComponent<Rigidbody2D>();
            _timeHolders.Register(nameof(_coyoteTime), _coyoteTime);
            _timeHolders.Register(nameof(_shootBlockTime), _shootBlockTime);

            _normalMode.Bind(this);
        }

        protected override void AfterAwake()
        {
            InitAIState();     // ★ モードクラス生成＆登録
            RegisterStats();   // ★ 既存：各モードが自分の遷移を登録
            ChangeMode(Mode.Normal); // ★ 初期モードへ（SetMode連動 & 遷移通知）
        }

        protected override void RegisterStats()
        {
            _normalMode.Register();
            //_rightMode.Register();
            //_heavyMode.Register();

            // モード非依存の共通フォールバック
            _stateMachine.AddTransmissions(AIStates.Idle,
                (AITriggers.moveInput, AIStates.Move, ""),
                (AITriggers.dashInput, AIStates.Dash, ""),
                (AITriggers.falling, AIStates.Fall, ""));
            _stateMachine.AddTransmissions(AIStates.Move,
                (AITriggers.cancelMove, AIStates.Idle, ""),
                (AITriggers.dashInput, AIStates.Dash, ""));

            _stateMachine.AddState(AIStates.Idle, new Idle());
            _stateMachine.AddState(AIStates.Fall, new Idle());
            _stateMachine.AddState(AIStates.Move, new MoveOnGround());
            _stateMachine.AddState(AIStates.Dash, new DashOnGround(this));
        }
        protected override void Start()
        {
            ReactiveDirection.Subscribe(d =>
            {
                if (d.x > 0) transform.localScale = Vector3.one;
                else if (d.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            }).AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => _canChargeCount)
                .Subscribe(_ => _shootPressTime += Time.deltaTime)
                .AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => _isPlaying)
                .Subscribe(_ => _timeHolders.Update(Time.deltaTime))
                .AddTo(this);
            this.FixedUpdateAsObservable()
                .Where(_ => _isPlaying)
                .Subscribe(_ => CurrentMode?.FixedUpdate(Time.fixedDeltaTime))
                .AddTo(this);
        }
    }
}
