using AIE2D;
using BlackRose.Datas.Definitions;
using HighElixir.Timers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(
        typeof(Rigidbody2D),
        typeof(UnityEngine.InputSystem.PlayerInput),
        typeof(DynamicAfterImageEffect2DPlayer)
        )]
    public partial class AIController : GroundedUnit
    {
        [Header("Reference")]
        [SerializeField] private List<BulletData> _bullets = new();
        [SerializeField] private LayerMask _targetLayer;

        [Header("Option Settings")]
        [SerializeField] private bool _canChargeCount = false;

        // 時間管理
        [SerializeField] private float _shootBlockTime = 0.6f;
        private TimerTicket _shootTicket;
        [SerializeField] private float _coyoteTime = 0.2f;
        private TimerTicket _coyoteTicket;

        // 射撃のチャージ
        private TimerTicket _chargeTicket;

        private Vector2 _shootDirection = Vector2.right;
        // ===== モード関連 =====
        [Header("Mode")]
        [SerializeField] private NormalMode _normalMode;
        [SerializeField] private LightMode _lightMode;
        [SerializeField] private HeavyMode _heavyMode;

        // ===== State Machine =====
        

        public List<BulletData> Bullets => _bullets;
        public bool CanJump => !Timer.IsFinished(_coyoteTicket);


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
                CurrentMode.CanceldJump(value);
            }
        }

        private void OnDash(InputValue value)
        {
            if (!value.isPressed)
            {
                _stateMachine.LazyChange(AITriggers.cancelMove);
                return;
            }
            if (!IsGrounded) return;
            _stateMachine.ChangeState(AITriggers.dashInput);
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
                _stateMachine.ChangeState(AITriggers.cancelMove);
                return;
            }
            else if (d.x != 0 && !_stateMachine.CurrentState.HasTag(tags[Tags.Shoot], tags[Tags.Stunned]))
            {
                Direction = d.normalized;
                _shootDirection = d;
            }
            _stateMachine.ChangeState(AITriggers.moveInput);
        }

        private void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("AI Attack Pressed");
                CurrentMode.OnShoot(value);
                Timer.Start(_chargeTicket);
            }
            else
            {
                CurrentMode.OnReleaseShoot(value);
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
            _stateMachine.LazyChange(AITriggers.landing);
            CurrentMode.OnGrounded();
        }
        protected override void OnFall()
        {
            _stateMachine.ChangeState(AITriggers.falling);
        }

        public override void AfterJump()
        {
            base.AfterJump();
            // ジャンプしたのでコヨーテタイムを終了させる
            Timer.Stop(_coyoteTicket);
        }
        // === Private ===
        protected override void BeforeAwake()
        {
            _coyoteTicket = Timer.CountDownRegister(_coyoteTime, "AI Coyote");
            _shootTicket = Timer.CountDownRegister(_shootBlockTime, "AI Shoot Block");
            _chargeTicket = Timer.CountUpRegister("AI ChargeTime");

            ModeRegist();
        }

        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            var dt = Time.deltaTime;
            Timer.Update(dt);
            CurrentMode.Update(dt);
        }
        protected override void AfterFixedUpdate()
        {
            base.AfterFixedUpdate();
            //Debug.Log("AAAa");
            CurrentMode?.FixedUpdate(Time.fixedDeltaTime);
        }
    }
}