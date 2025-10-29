using BlackRose.Core.Models.Units.State;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static BlackRose.Core.Models.Units.AIController;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class LightMode : AIModeBase
    {
        [Header("Light Mode Settings")]
        [SerializeField] private MultiShoot _shoot;
        [SerializeField] private MultiShoot _half;
        [SerializeField] private MultiShoot _full;

        public override AIStates Attach => AIStates.Light;

        protected override void RegisterStates()
        {
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
            _stateMachine.Send(AITriggers.shootInput);
        }

        public override void InvokeHalfShoot()
        {
            _stateMachine.Send(AITriggers.halfCharge);
        }

        public override void InvokeFullShoot()
        {
            _stateMachine.Send(AITriggers.fullCharge);
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