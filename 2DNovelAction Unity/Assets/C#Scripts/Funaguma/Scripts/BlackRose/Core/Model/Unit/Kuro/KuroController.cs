using BlackRose.Core.Models.Units.State;
using HighElixir.StateMachine;
using HighElixir.StateMachine.Extention;
using HighElixir.Unity.Loggings;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
    public class KuroController : GroundedUnit
    {
        private enum State
        {
            None,
            Idle,
            Move,
            Stun,
            Jump,
            Fall,
            Dead,
        }
        private enum Trigger
        {
            none,

            // 移動
            moveInput,
            cancelMove,

            // 落下系
            falling,
            jumpInput,
            landing,

            // 行動不能
            stuned,
            finishedStun,
            death,
        }

        private StateMachine<UnitBase, Trigger, State> _fms;
        #region State
        [Header("States")]
        [SerializeField] private MoveOnGround<UnitBase> _onGround;
        [SerializeField] private MoveOnAir<UnitBase> _onAir;
        [SerializeField] private Jump<UnitBase> _jump;
        [SerializeField] private Stun<UnitBase> _stun;

        #endregion

        [Header("Options")]
        [SerializeField] private float _horizontalDecel = 20f;

#if UNITY_EDITOR
        public string Current_State = "";
#endif

        #region StateMachine 
        protected override void RegisterStats()
        {
            // 元のステートマシンのバグ抑制用
            _stateMachine.AddState("idle", new States.Idle());

            #region ステートマシン初期化
            StateMachineOption<UnitBase, Trigger, State> op = new(this);
            op.LogLevel = RequiredLoggerLevel.ALL;
            op.QueueMode = HighElixir.StateMachine.QueueMode.UntilSuccesses;
            op.Logger = new UnityLogger();

            _fms = new(op);
            #endregion

#if UNITY_EDITOR
            _fms.OnTransition.Subscribe(x => Current_State = x.ToState.ToString());
#endif
            _fms.RegisterState(State.Idle, new Idle<UnitBase>());
            _fms.RegisterState(State.Move, _onGround);
            _fms.RegisterState(State.Fall, _onAir);
            _fms.RegisterState(State.Jump, _jump);
            _fms.RegisterState(State.Stun, _stun);

            #region 遷移

            // 待機
            _fms.RegisterTransitions(
                State.Idle,
                (Trigger.moveInput, State.Move, "Walking"),
                (Trigger.jumpInput, State.Jump, "Jump"),
                (Trigger.falling, State.Fall, "Fall"),
                (Trigger.stuned, State.Stun, "")
                );

            // 移動
            _fms.RegisterTransitions(
                State.Move,
                (Trigger.cancelMove, State.Idle, "CancelWalking"),
                (Trigger.jumpInput, State.Jump, "Jumping"),
                (Trigger.falling, State.Fall, "Falling"),
                (Trigger.stuned, State.Stun, "")
                );

            _fms.RegisterTransitions(
                State.Jump,
                (Trigger.falling, State.Fall, "Falling"),
                (Trigger.stuned, State.Stun, "")
                );

            _fms.RegisterTransitions(
                State.Fall,
                (Trigger.landing, State.Idle, "Landing"),
                (Trigger.stuned, State.Stun, "")
                );

            _fms.RegisterTransitions(
                State.Stun,
                (Trigger.finishedStun, State.Idle, "")
                );

            _fms.RegisterAnyTransition(Trigger.death, State.Dead, "");

            #endregion
            // 起動
            _fms.Awake(State.Idle);
        }

        protected override void AfterUpdate()
        {
            _fms.Update(Time.deltaTime);


            if (Mathf.Abs(MoveDirection.x) < 0.01f && Rigidbody2D != null)
            {
                var v = Rigidbody2D.velocity;
                v.x = Mathf.MoveTowards(v.x, 0f, _horizontalDecel * Time.deltaTime);
                Rigidbody2D.velocity = v;
                if (Mathf.Abs(Rigidbody2D.velocity.x) < 0.01f)
                    _fms.Send(Trigger.cancelMove);
            }
        }
        #endregion

        private void OnMove(InputValue input)
        {
            var vec = input.Get<Vector2>();
            MoveDirection = new(vec.x, 0);
            //Debug.Log("AAAAA");
            if (vec != Vector2.zero)
            {
                Direction = vec;
                _fms.Send(Trigger.moveInput);
            }
        }
    }
}