using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static BlackRose.SpriteEffectHolders;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class NormalMode : IAIState
    {
        private AIController _parent;
        public enum State
        {
            N_Shoot,
            N_Jump,
            N_Skill
        }
        public enum Trigger
        {
            Shoot,
            Jump,
            Skill
        }

        [SerializeField] private UnitStatusData _normalStatus;
        [SerializeField] private Jump _jump;

        public UnitStatusData StatusData => _normalStatus;

        public bool CanJump => !_parent.TimeHolders.IsFinished("_coyoteTime");

        public void Register()
        {
            _parent.StateMachine.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIController.AIStates.Idle,
                    (Trigger.Shoot, State.N_Shoot),
                    (Trigger.Jump, State.N_Jump),
                    (Trigger.Skill, State.N_Skill)
                );

            _parent.StateMachine.AddState(State.N_Jump, _jump);
        }

        // Grounded Event
        public void OnGrounded()
        {
            _jump.HadLeapt = false;
        }
        // Input Action
        public void OnShoot(InputValue value)
        {
            throw new System.NotImplementedException();
        }

        public void OnJump(InputValue value)
        {
            Debug.Log($"NormalMode OnJump: CanJump={CanJump}, isPressed={value.isPressed}");
            if (CanJump)
            {
                _parent.StateMachine.ChangeState(Trigger.Jump);
                _parent.AfterJump();
            }
            if (!value.isPressed)
            {
                _jump.Cut();
            }
        }

        public void OnDash(InputValue value)
        {
            throw new System.NotImplementedException();
        }

        public void OnSkill(InputValue value)
        {
            throw new System.NotImplementedException();
        }

        public void OnReleaseShoot(InputValue value)
        {
            throw new System.NotImplementedException();
        }

        public void Bind(AIController parent)
        {
            _parent = parent;
        }
    }
}