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
            _stateMachine.RegisterState(SubState.Shoot, _shoot, "Shoot")
                .OnEnter.Subscribe(_ => _parent.PlaySE(_shootSE.SEName, _shootSE.Volume));
            _stateMachine.RegisterState(SubState.Half, _half, "Shoot")
                .OnEnter.Subscribe(_ => _parent.PlaySE(_halfshootSE.SEName, _halfshootSE.Volume));
            _stateMachine.RegisterState(SubState.Full, _full, "Shoot")
                .OnEnter.Subscribe(_ => _parent.PlaySE(_fullshootSE.SEName, _fullshootSE.Volume));
            _stateMachine.RegisterState(SubState.Other1, new Idle<AIController>(), "")
            .OnEnter.Subscribe(_ =>
            {
                Debug.Log("LightMode: Launching Missile");       
                _parent.PlaySE(_skillSE.SEName, _skillSE.Volume);
                _parent.GetComponent<SearchAndFire>().Shoot(_parent.transform.position, _missileData, _parent.AttackLayer);
                _stateMachine.LazySend(AITriggers.shootCompleted);
            });

            _stateMachine.RegisterState(SubState.Skill, _locked, "");

            // Skill
            _stateMachine.RegisterTransition(SubState.Skill, AITriggers.skillFinished, SubState.Other1, "toMultiShot");
            _stateMachine.RegisterTransition(SubState.Other1, AITriggers.shootCompleted, SubState.Idle, "toIdle");
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