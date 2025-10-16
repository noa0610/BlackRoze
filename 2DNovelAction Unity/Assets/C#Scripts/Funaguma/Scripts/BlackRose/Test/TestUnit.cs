using BlackRose.Core.Models.States;
using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Test
{
    public class TestUnit : UnitBase
    {
#if UNITY_EDITOR
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
        [SerializeField] private bool _enabledGravity = true;
        private Rigidbody2D _rb;
        private Rigidbody2D Cache
        {
            get
            {
                if (_rb == null) _rb = GetComponent<Rigidbody2D>();
                return _rb;
            }
        }
        public void Invoke()
        {
            StateMachine.ChangeState(Triggers.invoked);
        }

        public void StateReset()
        {
            StateMachine.SetStateDirect(States.Idle.ToString());
            transform.position = _basePos;
            Cache.velocity = Vector2.zero;
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
            if (_enabledGravity)
                Cache.bodyType = RigidbodyType2D.Dynamic;
            else
                Cache.bodyType = RigidbodyType2D.Static;
            if (_state == null || _state.GetType() == typeof(StateComp))
            {
                _state = new Idle();
            }
            if (_state is IRigidbodyUser user) user.SetRB2(Cache);
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
#else
protected override void AfterAwake()
        {
            Destroy(gameObject);
        }
#endif
    }
}