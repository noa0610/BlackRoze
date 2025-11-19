using BlackRose.Core.Models.Units.State;
using HighElixir.StateMachine;
using HighElixir.StateMachine.Extention;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using static BlackRose.Core.Models.Units.AIController;
using BlackRose.Datas.Definitions;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class LightMode : AIModeBase
    {
        [Header("Light Mode Settings")]
        [SerializeField] private MultiShoot<AIController> _shoot;
        [SerializeField] private MultiShoot<AIController> _half;
        [SerializeField] private MultiShoot<AIController> _full;
        [SerializeField] private BulletData _missileData;
        private LockedShoot _locked = new();
        public override AIStates Attach => AIStates.Light;

        protected override void RegisterStates()
        {
            _stateMachine.RegisterState(SubState.Shoot, _shoot, "Shoot");
            _stateMachine.RegisterState(SubState.Half, _half, "Shoot");
            _stateMachine.RegisterState(SubState.Full, _full, "Shoot");
            var hook = _stateMachine.RegisterState(SubState.Other1, new Idle<AIController>(), "");
            hook.OnEnter.Subscribe(_ => {
                _parent.GetComponent<SearchAndFire>().Shoot(_parent.transform.position, _missileData, _parent.AttackLayer);
                _stateMachine.Send(AITriggers.shootCompleted);
            });

            _stateMachine.RegisterState(SubState.Skill, _locked, "");

            // Skill
            _stateMachine.RegisterTransition(SubState.Skill, AITriggers.skillFinished, SubState.Other1, "");
            _stateMachine.RegisterTransition(SubState.Other1, AITriggers.shootCompleted, SubState.Idle, "");

        }

        public override void OnSkill(InputValue value)
        {
            if (value.isPressed)
            {
                _stateMachine.Send(AITriggers.skillInput);
            }
            else
            {
                _stateMachine.Send(AITriggers.skillFinished);
            }
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