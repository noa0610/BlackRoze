using BlackRose.Core.Models.Helper;
using System;
using UnityEngine.InputSystem;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class HeavyMode : AIModeBase
    {
        public enum HeavyState
        {
            H_Shoot,
            H_Jump,
            H_Skill
        }
        public override void Register()
        {
            _parent.StateMachine.AddTransitionsForLayer(
                AIController.Mode.Heavy,
                AIController.AIStates.Idle,
                    (Triggers.shootInput, HeavyState.H_Shoot, ""),
                    (Triggers.jumpInput, HeavyState.H_Jump, ""),
                    (Triggers.skillInput, HeavyState.H_Skill, "")
                );

            _parent.StateMachine.AddState(HeavyState.H_Jump, _jump);
        }

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