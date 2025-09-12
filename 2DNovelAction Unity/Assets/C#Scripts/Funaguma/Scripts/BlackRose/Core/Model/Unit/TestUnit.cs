using BlackRose.Core.Models.States;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class TestUnit : GroundedUnit
    {
        public enum Triggers
        {
            invoked,
            stateCompleted,
        }
        public enum States
        {
            Idle,
            TestState,
        }
        [SerializeReference, SubclassSelector]
        private IState _state = new Idle();

        [SerializeField] private Vector2 _basePos = Vector2.zero;
        public void Invoke()
        {
            StateMachine.ChangeState(Triggers.invoked);
        }

        public void StateReset()
        {
            StateMachine.SetStateDirect(States.Idle.ToString());
            transform.position = _basePos;
        }
        protected override void RegisterStats()
        {
            StateMachine.AddTransition(States.Idle, Triggers.invoked, States.TestState.ToString());
            StateMachine.AddTransition(States.TestState, Triggers.stateCompleted, States.Idle.ToString());

            StateMachine.AddState(States.Idle, new Idle());
            StateMachine.AddState(States.TestState, _state as StateComp);
        }

        private void OnValidate()
        {
            if (_state == null || _state.GetType() == typeof(StateComp))
            {
                _state = new Idle();
            }
            if (_state is IRigidbodyUser user) user.SetRB2(GetComponent<Rigidbody2D>()); 
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Debug.Log("上書き成功 : " + _state.GetType().ToString());
                StateMachine.AddState(States.TestState, _state as StateComp);
            }
        }

        protected override void AfterAwake()
        {
            if (_basePos == Vector2.zero)
            {
                _basePos = transform.position;
            }
        }
    }
}