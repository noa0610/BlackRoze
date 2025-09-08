using BlackRose.Core.Models.Helper;
using System;
using UnityEngine.InputSystem;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;
using AIStates = BlackRose.Core.Models.Units.AIController.AIStates;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class NormalMode : AIModeBase
    {
        public override void Register()
        {
            _parent.StateMachine.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.Idle,
                    (Triggers.shootInput, AIStates.Shoot, ""),
                    (Triggers.jumpInput, AIStates.Jump, ""),
                    (Triggers.skillInput, AIStates.Skill, "")
                );

            _parent.StateMachine.AddState(AIStates.Jump, _jump);
        }

        // Input Action

        public override void OnSkill(InputValue value)
        {
            throw new System.NotImplementedException();
        }

        public override void InvokeShoot()
        {
            throw new NotImplementedException();
        }

        public override void InvokeHalfShoot()
        {
            throw new NotImplementedException();
        }

        public override void InvokeFullShoot()
        {
            throw new NotImplementedException();
        }
    }
}