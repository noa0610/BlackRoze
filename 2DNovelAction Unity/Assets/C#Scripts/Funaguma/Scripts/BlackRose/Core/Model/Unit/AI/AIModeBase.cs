using BlackRose.Core.Models.States;
using HighElixir.Timers;
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
        protected AIController _parent;
        public UnitStatusData StatusData => _status;
        protected IStateMachine SM => _parent.StateMachine;
        protected Timer Timer => _parent.Timer;
        public abstract void Register();

        // Grounded Event
        public virtual void OnGrounded()
        {
            _jump.ResetLeaptFlag();
            //Debug.Log("Jump Reset");
        }
        // Input Action
        public void OnShoot(InputValue value)
        {
            InvokeShoot();
        }
        public virtual void OnReleaseShoot(InputValue value)
        {
            if (!Timer.Stop("chargeTime", out var t)) return;

            if (t > _chargeTime[1])
            {
                InvokeFullShoot();
            }
            else if (t > _chargeTime[0])
            {
                InvokeHalfShoot();
            }
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