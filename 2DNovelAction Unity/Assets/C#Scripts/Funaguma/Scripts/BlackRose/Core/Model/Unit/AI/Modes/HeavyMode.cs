using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.Objects;
using BlackRose.Core.Models.Units.State;
using HighElixir.StateMachine.Extensions;
using HighElixir.StateMachine.Extention;
using HighElixir.Timers;
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
        [SerializeField] private ShootWithMove<AIController> _shoot;
        [SerializeField] private ShootForward<AIController> _half;
        [SerializeField] private LaserState<AIController> _laser;
        [Header("Reflect")]
        [SerializeField] private Reflect _reflect;
        public override AIStates Attach => AIStates.Heavy;

        protected override void RegisterStates()
        {

            // Skill
            _stateMachine.RegisterAnyTransition(AITriggers.skillFinished, SubState.Idle, "");

            var hook = _stateMachine.RegisterState(SubState.Shoot, _shoot, "Shoot");
            hook.OnEnter.Subscribe(_ => _parent.AutoFlipper.Enable = false);
            hook.OnExit.Subscribe(_ => _parent.AutoFlipper.Enable = true);

            hook = _stateMachine.RegisterState(SubState.Half, _half, "Shoot");
            hook.OnEnter.Subscribe(_ => _parent.AutoFlipper.Enable = false);
            hook.OnExit.Subscribe(_ => _parent.AutoFlipper.Enable = true);

            hook = _stateMachine.RegisterState(SubState.Full, _laser, "Shoot");
            hook.OnEnter.Subscribe(_ =>
            {
                _parent.AutoFlipper.Enable = false;
                _parent.Rigidbody2D.simulated = false;
                _parent.Rigidbody2D.velocity = Vector3.zero;
            });
            hook.OnExit.Subscribe(_ =>
            {
                _parent.AutoFlipper.Enable = true;
                _parent.Rigidbody2D.simulated = true;
            });

            _stateMachine.RegisterState(SubState.Skill, _reflect, "Skill");
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
            _ = _stateMachine.SendEventAndLockExitAsync(
                Triggers.shootInput,
                TimeSpan.FromSeconds(0.4f),
                onUnlock: () => _stateMachine.LazySend(Triggers.shootCompleted));
        }

        public override void InvokeHalfShoot()
        {
            _stateMachine.Send(Triggers.halfCharge);
        }

        public override void InvokeFullShoot()
        {
            _ = _stateMachine.SendEventAndLockExitAsync(
                Triggers.fullCharge,
                TimeSpan.FromSeconds(2),
                onUnlock: () => _stateMachine.LazySend(Triggers.shootCompleted));
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