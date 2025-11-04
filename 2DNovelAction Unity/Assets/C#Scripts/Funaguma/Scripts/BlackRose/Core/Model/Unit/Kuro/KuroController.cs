using BlackRose.Core.Models.Units.State;
using HighElixir.StateMachine;

namespace BlackRose.Core.Models.Units
{
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
        private MoveOnGround<UnitBase> _onGround;
        private MoveOnAir<UnitBase> _onAir;
        private Jump<UnitBase> _jump;
        private Stun<UnitBase> _stun;

        #endregion
        protected override void RegisterStats()
        {
            _stateMachine.AddState("idle", new States.Idle());

            _fms.RegisterState(State.Idle, new Idle<UnitBase>());
            _fms.RegisterState(State.Move, _onGround);
            _fms.RegisterState(State.Fall, _onAir);
            _fms.RegisterState(State.Jump, _jump);
            _fms.RegisterState(State.Stun, _stun);
        }
    }
}