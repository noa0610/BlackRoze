using BlackRose.Core.Models.Units.State;
using HighElixir.StateMachine;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    // ステート、モード管理
    public partial class AIController
    {
        public enum Mode
        {
            Normal,
            Light,
            Heavy
        }

        public enum AIStates : int
        {
            Dead,
            Normal,
            Light,
            Heavy
        }
        public enum AITriggers : int
        {
            moveInput, cancelMove, dashInput, shootInput, halfCharge, fullCharge, jumpInput,
            shootCompleted, watingTimeHasElapsed, skillInput, skillFinished,
            landing, falling, stun, recoverFromStun,

            // モードチェンジ
            mC_l, mC_h, mC_n, dead
        }

        // 外部
        private AISpriteResolver _spriteResolver;

#if UNITY_EDITOR
        [SerializeField] private string _State;
#endif
        // Common State
        [SerializeField] private MoveOnGround<AIController> _move = new();
        [SerializeField] private DashOnGround<AIController> _dash = new();
        [SerializeField] private MoveOnAir<AIController> _moveAir = new();
        public MoveOnGround<AIController> MoveOnGround => _move;
        public DashOnGround<AIController> Dash => _dash;
        public MoveOnAir<AIController> MoveAir => _moveAir;
        // ===== State Machine =====
#if UNITY_EDITOR
        public override bool ShoudBeLogging => true;
        public HighElixir.Loggings.ILogger logger = null;//new UnityLogger();
#else
        public HighElixir.Loggings.ILogger logger = null;
#endif
        private StateMachine<AIController, AITriggers, AIStates> _fms;
        private AIStates _currentEnumMode = AIStates.Normal;
        public StateMachine<AIController, AITriggers, AIStates> Machine => _fms;
        public AIModeBase CurrentMode => _currentEnumMode switch
        {
            AIStates.Normal => _normalMode,
            AIStates.Heavy => _heavyMode,
            AIStates.Light => _lightMode,
            _ => _normalMode
        };
        public AITriggers ModeChange => _currentEnumMode switch
        {
            AIStates.Normal => AITriggers.mC_n,
            AIStates.Heavy => AITriggers.mC_h,
            AIStates.Light => AITriggers.mC_l,
            _ => AITriggers.mC_n,
        };
        // 外部からのモード切替 API
        public void SwitchModeLight()
        {
            _spriteResolver.Change_L();
            ChangeMode(AIStates.Light);
        }
        public void SwitchModeHeavy()
        {
            _spriteResolver.Change_H();
            ChangeMode(AIStates.Heavy);
        }
        public void SwitchModeNormal()
        {
            _spriteResolver.Change_N();
            ChangeMode(AIStates.Normal);
        }

        // === Private ===

        private void ChangeMode(AIStates mode)
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

            _fms.Send(ModeChange);

            Debug.Log("ModeChanged");
        }
        protected void ModeRegist()
        {
            _spriteResolver = GetComponent<AISpriteResolver>();
            _normalMode.Bind(this);
            _lightMode.Bind(this);
            _heavyMode.Bind(this);
        }

        protected override void AfterAwake()
        {
            base.AfterAwake();
            ChangeMode(AIStates.Normal);
        }

        protected override void RegisterStats()
        {
            _stateMachine.AddState("Idle", new Models.States.Idle());
            var op = new StateMachineOption<AIController, AITriggers, AIStates>(this);
            op.LogLevel = RequiredLoggerLevel.ALL;
            op.Logger = logger;
            op.QueueMode = HighElixir.StateMachine.QueueMode.UntilSuccesses;
            _fms = new(op);


#if UNITY_EDITOR
            _fms.OnTransition.Subscribe(x => _State = x.ToState.ToString());
#endif
            _fms.RegisterState(AIStates.Normal, new Idle<AIController>());
            _fms.RegisterState(AIStates.Light, new Idle<AIController>());
            _fms.RegisterState(AIStates.Heavy, new Idle<AIController>());
            _fms.RegisterState(AIStates.Dead, new Idle<AIController>());
            // 任意遷移
            _fms.RegisterAnyTransition(AITriggers.mC_h, AIStates.Heavy);
            _fms.RegisterAnyTransition(AITriggers.mC_l, AIStates.Light);
            _fms.RegisterAnyTransition(AITriggers.mC_n, AIStates.Normal);
            _fms.RegisterAnyTransition(AITriggers.dead, AIStates.Dead);

            _normalMode.Register();
            _lightMode.Register();
            _heavyMode.Register();

            _fms.Awake(AIStates.Normal);
        }
    }
}