using HighElixir.Timers;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose.Core.Models.Units
{
    public partial class AIController
    {
        private ReactiveCommand _onCanceledJump = new();

        public IObservable<Unit> OnCanceledJump => _onCanceledJump;

        // === Input Action ===
        private void OnJump(InputValue value)
        {
            Debug.Log($"OnJump: CanJump={CanJump}, isPressed={value.isPressed}");
            if (CanJump)
            {
                CurrentMode.OnJump(value);
            }
            if (!value.isPressed)
            {
                _onCanceledJump.Execute();
                CurrentMode.CanceldJump(value);
            }
        }

        private void OnDash(InputValue value)
        {
            if (!value.isPressed)
            {
                _fms.LazySend(AITriggers.cancelMove);
                return;
            }
            if (!IsGrounded) return;
            _fms.LazySend(AITriggers.dashInput);
        }

        private void OnMove(InputValue value)
        {
            var d = value.Get<Vector2>();
            var tmp = d;
            tmp.y = 0;
            MoveDirection = tmp;
            CurrentMode.OnInputMove(d);
            if (d.x == 0)
            {
                _fms.LazySend(AITriggers.cancelMove);
                return;
            }
            else if (!ShouldBeBlockFlip)
            {
                Direction = d.normalized;
                _shootDirection = d;
            }
            _fms.LazySend(AITriggers.moveInput);
        }

        private void OnAttack(InputValue value)
        {
            Timer.Restart(_blockFlip, false);
            if (value.isPressed)
            {
                Debug.Log("AI Attack Pressed");
                CurrentMode.OnShoot(value);
                if (!Timer.IsRunning(_chargeTicket))
                    Timer.Start(_chargeTicket);
            }
            else
            {
                if (Timer.Stop(_chargeTicket, out var remaining))
                    CurrentMode.OnReleaseShoot(remaining);
            }
        }

        private void OnSkill(InputValue value)
        {
            CurrentMode.OnSkill(value);
        }

        private void OnModeChange1(InputValue value)
        {
            CurrentMode.ModeChange_C();
        }
        private void OnModeChange2(InputValue value)
        {
            CurrentMode.ModeChange_V();
        }
        // === GroundedUnit の抽象 ===
        protected override void OnGrounded()
        {
            Timer.Start(_coyoteTicket);
            _fms.LazySend(AITriggers.landing);
            CurrentMode.OnGrounded();
        }
        protected override void OnFall()
        {
            _fms.LazySend(AITriggers.falling, true);
        }

        public override void AfterJump()
        {
            base.AfterJump();
            // ジャンプしたのでコヨーテタイムを終了させる
            Timer.Stop(_coyoteTicket);
        }
    }
}