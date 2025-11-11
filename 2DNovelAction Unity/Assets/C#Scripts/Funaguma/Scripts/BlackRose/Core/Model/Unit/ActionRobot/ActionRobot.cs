using Cysharp.Threading.Tasks;
using HighElixir.StateMachine.Extention;
using HighElixir.Timers;
using HighElixir.Unity.UI;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(PlayerInput)), Serializable]
    public partial class ActionRobot : GroundedUnit
    {
        [Header("Reference")]
        [SerializeField] private TextThrower _thrower;
        [SerializeField] private float _horizontalDecel;
        [SerializeField] private TrailRenderer _trailRenderer;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _forceEnableJump;
#endif

        // 地面にいるかどうか（OverlapCircle判定＆コヨーテタイム管理）
        public bool CanJump => _forceEnableJump || !Timer.IsFinished(_coyoteTicket);

        protected override bool BeforeTakeDamage(IUnit unit, ref float damage)
        {
            return !IsInvincible;
        }

        protected override void OnTakeDamage(IUnit s, float damage)
        {
            //Debug.Log($"Take Damage : {StatusManager.ReadValue(Status.HP)}/{StatusManager.ReadValue(Status.MaxHP)}");
            IsInvincible = true;
            Timer.Start(_invincibleTicket);
            if (s is not ObjectDamageWorker)
                _fms.Send(Triggers.stuned);
        }
        protected override void OnDeath()
        {
            _cancellableActionToken.Cancel();
            _fms.LazySend(Triggers.death);
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
            if (CanJump && value.isPressed)
            {
                _fms.Send(Triggers.jumpInput);
                AfterJump();
                Timer.Stop(_coyoteTicket); // ジャンプしたのでコヨーテタイムを終了させる
            }
            else
            {
                _jump.Cut();
            }
        }
        private void OnDash(InputValue value) => OnDash(value.isPressed);
        private void OnDash(bool isPressed)
        {
            if (!isPressed)
            {
                _fms.Send(Triggers.cancelDash);
                return;
            }
            if (!IsGrounded) return; // 地面にいない場合は無視

            _fms.Send(Triggers.dashInput);
        }

        private void OnMove(InputValue value) => OnMove(value.Get<Vector2>());
        private void OnMove(Vector2 dir)
        {
            if (dir != Vector2.zero)
                Direction = dir.normalized;
            MoveDirection = dir.normalized;
            if (dir.x != 0)
            {
                ShootDir = dir; // 横入力がある場合は攻撃方向を更新
            }
        }

        private void OnAttack(InputValue value) => OnAttack(value.isPressed);
        private void OnAttack(bool isPressed)
        {
            if (isPressed)
            {
                Timer.Start(_chargeTicket);
                if (_successionCount >= _maxSuccession)
                {
                    Timer.Start(_shootBlockTicket); // 連射ブロックタイムを開始
                    _thrower.Create(gameObject, "もう疲れたよ...", Color.red);
                }
                else
                {
                    if (IsGrounded)
                        _fms.Send(Triggers.shootInput);
                    else
                        _fms.Send(Triggers.shootInAir);
                    _successionCount++;
                }
            }
            else
            {
                Timer.Stop(_chargeTicket, out var time);
                if (time >= _chargeShoot[1])
                {
                    if (IsGrounded)
                        _fms.Send(Triggers.fullChargeShoot);
                    else
                        _fms.Send(Triggers.fullChargeInAir);
                }
                else if (time >= _chargeShoot[0])
                {
                    // チャージ攻撃の状態にする
                    if (IsGrounded)
                        _fms.Send(Triggers.halfChargeShoot);
                    else
                        _fms.Send(Triggers.halfChargeInAir);
                }
            }
        }
        #endregion

        // === Unity LifeCycle ===
        protected override void Start()
        {
            this.UpdateAsObservable().Where(_ => _isPlaying).Subscribe(_ =>
                {
                    if (Mathf.Abs(MoveDirection.x) < 0.05f)
                    {
                        var v = Rigidbody2D.velocity;
                        v.x = Mathf.MoveTowards(v.x, 0f, _horizontalDecel * Time.deltaTime);
                        Rigidbody2D.velocity = v;
                        if (Mathf.Abs(Rigidbody2D.velocity.x) < 0.01f)
                            _fms.Send(Triggers.cancelMove);
                    }
                    else
                    {
                        //Debug.Log("BBBBB");
                        _fms.LazySend(Triggers.moveInput, true);
                    }
                }).AddTo(this);
            CurrentGroundState.Where(x => x == GroundState.Falling).Subscribe(_ =>
            {
                Timer.Start(_coyoteTicket); // コヨーテタイム開始
                _fms.Send(Triggers.falling);
            }).AddTo(this);

            CurrentGroundState.Where(x => x == GroundState.Landing).Subscribe(_ =>
            {
                //Debug.Log("AAA");
                _trailRenderer.emitting = false;
                Timer.Reset(_coyoteTicket);
                _jump.ResetLeaptFlag();

                // 遅延でIdleに移行
                var token = Take();
                _fms.Send(Triggers.landing);
                var unused = _fms.SendEventWithDelayAsync(TimeSpan.FromSeconds(0.15f), Triggers.landed, token);
            }).AddTo(this);
        }
    }
}