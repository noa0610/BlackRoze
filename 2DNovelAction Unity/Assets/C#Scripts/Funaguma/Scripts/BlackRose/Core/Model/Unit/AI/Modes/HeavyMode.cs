using BlackRose.Core.Models.Units.State;
using HighElixir.StateMachine.Extention;
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
        [Header("Anim")]
        [SerializeField] private string _frontWalk;
        [SerializeField] private string _backwardWalk;
        [Header("Heavy Shoot")]
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
            hook.OnEnter.Subscribe(_ =>
            {
                _parent.AutoFlipper.Enable = false;
                _parent.PlaySE(_shootSE.SEName, _shootSE.Volume);
            });
            hook.OnExit.Subscribe(_ => _parent.AutoFlipper.Enable = true);

            hook = _stateMachine.RegisterState(SubState.Half, _half, "Shoot");
            hook.OnEnter.Subscribe(_ =>
            {
                _parent.AutoFlipper.Enable = false;
                _parent.PlaySE(_halfshootSE.SEName, _halfshootSE.Volume);
            });
            hook.OnExit.Subscribe(_ => _parent.AutoFlipper.Enable = true);

            hook = _stateMachine.RegisterState(SubState.Full, _laser, "Shoot");
            hook.OnEnter.Subscribe(_ =>
            {
                _parent.AutoFlipper.Enable = false;
                _parent.Rigidbody2D.simulated = false;
                _parent.Rigidbody2D.velocity = Vector3.zero;
                _parent.PlaySE(_fullshootSE.SEName, _fullshootSE.Volume);
            });
            hook.OnExit.Subscribe(_ =>
            {
                _parent.AutoFlipper.Enable = true;
                _parent.Rigidbody2D.simulated = true;
            });

            _stateMachine.RegisterState(SubState.Skill, _reflect, "Skill");
        }

        public override void OnGrounded() { }
        public override void OnInputMove(Vector2 dir)
        {
            base.OnInputMove(dir);
            if (!_parent.AutoFlipper.Enable && !NonEquableDir(dir.x))
            {
                _parent.Animator.SetTrigger(_backwardWalk);
            }
            else if (!_stateMachine.Current.info.HasTagOnChild("Dash"))
                _parent.Animator.SetTrigger(_frontWalk);
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
            int dir = 0;
            if (_parent.ShootDir.y > 0) dir = 2;
            if (_parent.ShootDir.y < 0) dir = 1;
            Debug.Log(dir);
            _parent.Animator.SetInteger("LaserDir", dir);
            _ = _stateMachine.SendEventAndLockExitAsync(
                Triggers.fullCharge,
                TimeSpan.FromSeconds(1.6),
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

        private bool NonEquableDir(float x)
        {
            var movedirx = _parent.MoveDirection.x;
            return Mathf.Sign(x) == Mathf.Sign(movedirx);
        }
    }
}