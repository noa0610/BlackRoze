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
            L_Half,
            L_Full,
            L_Jump,
            L_Skill,
        }
        [SerializeField] private MultiShoot _shoot;
        [SerializeField] private MultiShoot _half;
        [SerializeField] private MultiShoot _full;
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
            SM.AddState(LightStates.L_Shoot, _shoot);
            SM.AddState(LightStates.L_Half, _half);
            SM.AddState(LightStates.L_Full, _full);

            _shoot.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(Triggers.shootCompleted);
            });
            _half.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(Triggers.shootCompleted);
            });
            _full.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(Triggers.shootCompleted);
            });
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
            SM.ChangeState(LightStates.L_Shoot);
        }

        public override void InvokeHalfShoot()
        {
            SM.ChangeState(LightStates.L_Half);
        }

        public override void InvokeFullShoot()
        {
            SM.ChangeState(LightStates.L_Full);
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