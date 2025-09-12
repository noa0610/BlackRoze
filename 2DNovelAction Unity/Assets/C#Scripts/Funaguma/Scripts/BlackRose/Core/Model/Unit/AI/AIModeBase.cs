using BlackRose.Core.Models.States;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public abstract class AIModeBase : IAIState
    {
        [SerializeField] protected UnitStatusData _status;
        [SerializeField] protected Jump _jump;
        [SerializeField] protected float[] _chargeTime = new float[2] { 1.2f, 2.3f };
        [SerializeField] protected string _timerName = "chargeTimer";
        protected AIController _parent;
        public UnitStatusData StatusData => _status;
        public IStateMachine SM => _parent.StateMachine;
        public abstract void Register();

        // Grounded Event
        public virtual void OnGrounded()
        {
            _jump.ResetLeaptFlag();
        }
        // Input Action
        public void OnShoot(InputValue value)
        {
            if (value.isPressed)
            {
                InvokeShoot();
                _parent.TimeHolders.Start(_timerName);
            }
            else
            {
                OnReleaseShoot(value);
            }
        }
        public virtual void OnReleaseShoot(InputValue value)
        {
            if (!_parent.TimeHolders.TryGetRemaining(_timerName, out var t)) return;
            _parent.TimeHolders.Stop(_timerName);
            
            if (_chargeTime[1] > t)
            {
                InvokeHalfShoot();
            }
            else if (_chargeTime[0] > t)
            {
                InvokeFullShoot();
            }
            _parent.TimeHolders.Reset(_timerName);
        }

        public abstract void InvokeShoot();
        public abstract void InvokeHalfShoot();
        public abstract void InvokeFullShoot();
        public virtual void OnJump(InputValue value)
        {
            _parent.StateMachine.ChangeState(Triggers.jumpInput);
            _parent.AfterJump();
        }

        public virtual void CanceldJump(InputValue value)
        {
            _jump.Cut();
        }

        public abstract void OnSkill(InputValue value);


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
            _parent.TimeHolders.Register(_timerName, 0, isCountup: true);
        }
    }
}