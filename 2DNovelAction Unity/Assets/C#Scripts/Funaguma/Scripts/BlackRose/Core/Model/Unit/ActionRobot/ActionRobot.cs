using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using HighElixir.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose
{
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput)), Serializable]
    public partial class ActionRobot : GroundedUnit
    {
        [Header("Reference")]
        [SerializeField] private List<BulletData> _bullets = new List<BulletData>();
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private TextThrower _thrower;
        private Stun _stunState;
        private Rigidbody2D _rigidbody;

        [Header("Option Settings")]
        [SerializeField] private float coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        [SerializeField] private float[] _chargeShoot = new float[2] { 1.8f, 3.4f }; // チャージ攻撃用の時間配列
        [SerializeField] private bool _canChargeCount = false;
        [SerializeField] private Vector2 _stunKnockback = Vector2.zero;
        private float coyoteTimeCounter;
        private float _shootPressTime = 0f; // 攻撃ボタンを押した時間



#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _forceEnableJump;
#endif
        // 地面にいるかどうか（OverlapCircle判定＆コヨーテタイム管理）
        public bool canJump => _forceEnableJump || coyoteTimeCounter > 0f;

        public List<BulletData> Bullets
        {
            get => _bullets;
            set => _bullets = value;
        }

        protected override void OnGrounded()
        {
            coyoteTimeCounter = coyoteTime;
            _jump.HadLeapt = false;
            _stateMachine.ChangeState(Triggers.landing);
        }
        protected override void OnUnGrounded()
        {
            coyoteTimeCounter -= Mathf.Max(0, Time.fixedDeltaTime);
        }
        protected override void OnFall()
        {
            _stateMachine.ChangeState(Triggers.falling);
        }

        public override void TakeDamage(float damage)
        {
            if (IsInvincible) return;
            base.TakeDamage(damage);
            _stateMachine.ChangeState(Triggers.stuned.ToString());
        }

        public override void Pause()
        {
            GetComponent<UnityEngine.InputSystem.PlayerInput>().currentActionMap.Disable();
            Debug.Log("Input actions disabled for pause.");
        }
        public override void Play()
        {
            GetComponent<UnityEngine.InputSystem.PlayerInput>().currentActionMap.Enable();
            Debug.Log("Input actions enabled for play.");
        }
        // === Private ===

        // === InputAction ===
        #region
        private void OnJump(InputValue value)
        {
            if (canJump)
            {
                _stateMachine.ChangeState(Triggers.jumpInput);
                coyoteTimeCounter = 0f;  // ジャンプしたら猶予リセット
            }
            if (!value.isPressed)
            {
                _jump.Cut();
            }
        }
        private void OnDash(InputValue value)
        {
            if (!value.isPressed)
            {
                _stateMachine.ChangeState(Triggers.cancelMove);
                return;
            }
            if (!IsGrounded) return; // 地面にいない場合は無視
            _stateMachine.ChangeState(Triggers.dashInput);
        }
        private void OnMove(InputValue value)
        {
            var d = value.Get<Vector2>();
            if (d == Vector2.zero)
            {
                _stateMachine.ChangeState(Triggers.cancelMove);
                Debug.Log("Canceled Move.");
                return;
            }
            Direction = d.normalized;
            _stateMachine.ChangeState(Triggers.moveInput);
        }
        private void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                _bullets[0].originalstatus.direction = Direction; // 攻撃方向を設定
                Debug.Log("Shoot");
                // 入力時に一度通常攻撃を行い、その後チャージを行う
                _stateMachine.ChangeState(Triggers.shootInput);
                _canChargeCount = true; // 攻撃ボタンを押したのでチャージ可能状態にする
            }
            else
            {
                _shootPressTime = 0f; // 攻撃ボタンを離したので時間をリセット
                _canChargeCount = false; // 攻撃ボタンを離したのでチャージ不可状態にする
                if (_chargeShoot[0] <= _shootPressTime && _shootPressTime < _chargeShoot[1])
                {
                    Debug.Log("チャージ１");
                    // チャージ攻撃の状態にする
                    _stateMachine.ChangeState(Triggers.chargeShoot);
                    _bullets[1].originalstatus.direction = Direction; // 攻撃方向を設定
                }
                else if (_shootPressTime >= _chargeShoot[1])
                {
                    Debug.Log("フルチャージ");
                    _stateMachine.ChangeState(Triggers.fullChargeShoot);
                    _bullets[2].originalstatus.direction = Direction; // 攻撃方向を設定
                }
                else
                {
                    _stateMachine.ChangeState(Triggers.shootComplete);
                }
            }
        }
        #endregion
        // === Unity LifeCycle ===
        protected override void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            base.Awake();
        }
        protected virtual void Start()
        {
            this.UpdateAsObservable()
                .Where(_ => _stateMachine.CurrentState.state is Stun stun && stun.StunTimer <= 0f)
                .Subscribe(_ => _stateMachine.LazyChange(Triggers.finishedStun))
                .AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => _canChargeCount)
                .Subscribe(_ => _shootPressTime += Time.deltaTime)
                .AddTo(this);
        }
    }
}