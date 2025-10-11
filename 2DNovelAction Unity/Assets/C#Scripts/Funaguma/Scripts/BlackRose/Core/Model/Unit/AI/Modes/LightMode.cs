using BlackRose.Core.Models.Helper;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;
using BlackRose.Core.Models.States;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class LightMode : AIModeBase
    {
        public enum LightStates
        {
            L_Shoot,
            L_Jump,
            L_Skill,
        }

        public override void Register()
        {
            SM.AddTransitionsForLayer(
                AIController.Mode.Light,
                AIController.AIStates.Idle,
                    (Triggers.shootInput, LightStates.L_Shoot, ""),
                    (Triggers.jumpInput, LightStates.L_Jump, ""),
                    (Triggers.skillInput, LightStates.L_Skill, "")
                );

            SM.AddState(LightStates.L_Jump, _jump);
        }

        public override void OnSkill(InputValue value)
        {
        }

        public override void OnInputMove(Vector2 dir)
        {
        }

        public override void FixedUpdate(float deltaTime)
        {
           
        }

        public override void InvokeShoot()
        {
        }

        public override void InvokeHalfShoot()
        {
        }

        public override void InvokeFullShoot()
        {
        }

        public override void ModeChange_C()
        {
            _parent.SwitchModeHeavy();
        }

        public override void ModeChange_V()
        {
            _parent.SwitchModeNormal();
        }
    }
}