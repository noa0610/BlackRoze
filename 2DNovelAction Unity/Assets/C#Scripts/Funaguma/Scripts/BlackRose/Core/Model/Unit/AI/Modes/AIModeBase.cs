using HighElixir.StateMachine;
using HighElixir.StateMachine.Extention;
using HighElixir.Timers;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using static BlackRose.Core.Models.Units.AIController;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public abstract class AIModeBase : IAIState
    {
        public enum SubState
        {
            // Cancelable
            Idle,
            ShootInterval,
            Move,
            Jump,
            // Shoot
            Shoot,
            Half,
            Full,
            //
            Dash,
            Falling,
            Skill,
            SpecialAttack,

            // Ather
            Other1,
            Other2,
            Other3,
        }
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private string _currentState;
#endif
        [Header("AI Mode Settings")]
        [SerializeField] protected UnitStatusData _status;
        [SerializeField] protected State.Jump<AIController> _jump;
        [SerializeField] protected float[] _chargeTime = new float[2] { 1.2f, 2.3f };

        [Header("Animator")]
        [SerializeField] private string _skillAnimeTrigger;

        protected AIController _parent;
        protected StateMachine<AIController, Triggers, SubState> _stateMachine;
        public UnitStatusData StatusData => _status;
        protected Timer Timer => _parent.Timer;
        protected TimerTicket ChargeTime => _parent.ChargeTime;

        public abstract AIStates Attach { get; }

        public virtual void Register()
        {
            Debug.Log(GetType().Name + ":登録処理");
            var op = new StateMachineOption<AIController, AITriggers, SubState>(_parent);
            op.Logger = _parent.logger;
            op.QueueMode = HighElixir.StateMachine.QueueMode.DoEverything;
            op.LogLevel = RequiredLoggerLevel.ERRORS;
            op.EnableOverriding = true;

            _stateMachine = new(op);
            _stateMachine.OnTransitionLogging();
            _stateMachine.RegisterProcessor = new StateProcessor<AIController, AITriggers, SubState>((x, y) =>
            {
                if (y.State.HasTag("Shoot"))
                {
                    //Debug.Log("Interval");
                    y.RegisterTransition(Triggers.shootCompleted, SubState.ShootInterval);
                }
                if (y.State.HasTag("Cancelable"))
                {
                    y.RegisterTransition(Triggers.skillInput, SubState.Skill, _skillAnimeTrigger);
                    y.RegisterTransition(Triggers.shootInput, SubState.Shoot);
                    y.RegisterTransition(Triggers.halfCharge, SubState.Half);
                    y.RegisterTransition(Triggers.fullCharge, SubState.Full);
                }
            });
#if UNITY_EDITOR
            _stateMachine.OnTransition.Subscribe(x =>
            {
                _currentState = x.ToState.ToString();
            });
#endif
            _stateMachine.RegisterState(SubState.Idle, new Idle<AIController>(), "OnGround", "Cancelable");
            _stateMachine.RegisterState(SubState.Jump, _jump, "OnAir", "Cancelable");
            _stateMachine.RegisterState(SubState.Move, _parent.MoveOnGround, "Cancelable");
            var hook = _stateMachine.RegisterState(SubState.Dash, _parent.Dash, "Cancelable");
            hook.OnEnter.Subscribe(x =>
            {
                _parent.DynamicAfterImageEffect2D.SetActive(true);
            });
            hook.OnExit.Subscribe(x =>
            {
                if (x is StateMachine<AIController, AITriggers, SubState>.StateInfo info && info.ID == SubState.Jump)
                {
                    _parent.OnAirToGround.Subscribe(_ => _parent.DynamicAfterImageEffect2D.SetActive(false));
                }
                else
                {
                    _parent.DynamicAfterImageEffect2D.SetActive(false);
                }
            });
            _stateMachine.RegisterState(SubState.ShootInterval, new Idle<AIController>(), "Cancelable");
            _stateMachine.RegisterState(SubState.Falling, _parent.MoveAir, "OnAir", "Falling", "Cancelable");

            RegisterStates();

            // Idle
            _stateMachine.RegisterTransitions(
                SubState.Idle,
                    (Triggers.moveInput, SubState.Move, ""),
                    (Triggers.dashInput, SubState.Dash, ""),
                    (Triggers.jumpInput, SubState.Jump, "")
                );

            // ShootInterval
            _stateMachine.RegisterTransitions(
                SubState.ShootInterval,
                    (Triggers.watingTimeHasElapsed, SubState.Idle, ""),
                    (Triggers.jumpInput, SubState.Jump, ""),
                    (Triggers.moveInput, SubState.Move, "")
                );

            // Move
            _stateMachine.RegisterTransitions(
                SubState.Move,
                    (Triggers.cancelMove, SubState.Idle, ""),
                    (Triggers.jumpInput, SubState.Jump, ""),
                    (Triggers.dashInput, SubState.Dash, "")
                );

            _stateMachine.RegisterTransitions(SubState.Dash,
                (Triggers.cancelMove, SubState.Idle, ""),
                (Triggers.jumpInput, SubState.Jump, "")
                );


            // 任意遷移
            _stateMachine.RegisterAnyTransition(AITriggers.falling, SubState.Falling);
            _stateMachine.RegisterAnyTransition(AITriggers.landing, SubState.Idle);

            // 共通アニメータ登録

            RegisterTransitions();

            _stateMachine.OnEnterEvent(SubState.Jump).Subscribe(_ => _parent.AfterJump());

            _stateMachine.OnCompletion.SkipWhile(_ => !_stateMachine.Awaked).Where(x => x.State.HasTag("Shoot")).Subscribe(_ =>
            {
                _stateMachine.LazySend(Triggers.shootCompleted);
            });

            _parent.Machine.AttachSubMachine<SubState>(Attach, _stateMachine, SubState.Idle, new StateMachine<AIController, Triggers, AIStates>.SubMachineOptions<SubState>()
            {
                OnExitResetState = true,
                ForwardEventsFirst = true,
            });
        }
        protected abstract void RegisterStates();
        protected virtual void RegisterTransitions() { }
        // Grounded Event
        public virtual void OnGrounded()
        {
        }

        public virtual void OnAirToGround()
        {
            _jump.ResetLeaptFlag();
        }
        // Input Action
        public void OnShoot(InputValue value)
        {
            InvokeShoot();
        }
        public virtual void OnReleaseShoot(float chargeTime)
        {
            Debug.Log($"[AI] Charge: {chargeTime}");
            if (chargeTime > _chargeTime[1])
            {
                InvokeFullShoot();
            }
            else if (chargeTime > _chargeTime[0])
            {
                InvokeHalfShoot();
            }
        }

        public abstract void InvokeShoot();
        public abstract void InvokeHalfShoot();
        public abstract void InvokeFullShoot();
        public virtual void OnJump(InputValue value)
        {
            _stateMachine.Send(Triggers.jumpInput);
        }

        public virtual void CanceldJump(InputValue value)
        {
            _jump.Cut();
        }

        public abstract void OnSkill(InputValue value);

        // モードチェンジはボタンを離した瞬間に呼ばれる
        public abstract void ModeChange_C();
        public abstract void ModeChange_V();
        /// <summary>
        /// asdwを入力されたときにだけ呼ばれる
        /// </summary>
        public virtual void OnInputMove(Vector2 dir)
        {
        }

        public virtual void Update(float deltaTime)
        {
        }
        public virtual void FixedUpdate(float deltaTime)
        {
        }
        public void Bind(AIController parent)
        {
            _parent = parent;
        }
    }
}