using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using BlackRose.Core.Models.Objects;
using System;
using UnityEngine;
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
        [Header("Reflect")]
        [SerializeField] private Reflect _reflect;
        [SerializeField] private ReflectMono _reflectObj;
        public override void Register()
        {
            _parent.StateMachine.AddTransitionsForLayer(
                AIController.Mode.Heavy,
                AIController.AIStates.Idle,
                    (Triggers.shootInput, HeavyState.H_Shoot, ""),
                    (Triggers.jumpInput, HeavyState.H_Jump, ""),
                    (Triggers.skillInput, HeavyState.H_Skill, "")
                );

            SM.AddState(HeavyState.H_Jump, _jump);
            SM.AddState(HeavyState.H_Skill, _reflect);

            _reflect.SetGameObject(_reflectObj.gameObject);
        }

        public override void OnSkill(InputValue value)
        {
            SM.ChangeState(Triggers.skillInput);
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

        public override void ModeChange_C()
        {
            _parent.SwitchModeNormal();
        }

        public override void ModeChange_V()
        {
            _parent.SwitchModeLight();
        }
    }
}