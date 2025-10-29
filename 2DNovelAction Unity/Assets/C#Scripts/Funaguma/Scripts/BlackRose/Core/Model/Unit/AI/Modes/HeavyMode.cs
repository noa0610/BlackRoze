using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.Objects;
using BlackRose.Core.Models.Units.State;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using static BlackRose.Core.Models.Units.AIController;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class HeavyMode : AIModeBase
    {
        [Header("Heavy Shoot")]
        [Header("")]
        [SerializeField] private ShootForward _shoot;
        [Header("Reflect")]
        [SerializeField] private Reflect _reflect;
        [SerializeField] private ReflectMono _reflectObj;

        public override AIStates Attach => AIStates.Heavy;

        protected override void RegisterStates()
        {
        }

        public override void OnSkill(InputValue value)
        {
            if (value.isPressed)
            {
                _stateMachine.Send(Triggers.skillInput);
            }
            else
            {
                _stateMachine.Send(Triggers.skillFinished);
            }
        }

        public override void InvokeShoot()
        {
            _stateMachine.Send(Triggers.shootInput);
        }

        public override void InvokeHalfShoot()
        {
        }

        public override void InvokeFullShoot()
        {
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