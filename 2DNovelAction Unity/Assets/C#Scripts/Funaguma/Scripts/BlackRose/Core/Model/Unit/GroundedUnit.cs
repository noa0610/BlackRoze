using UnityEngine;
using System;
using BlackRose.Core.Models.Units;
using HighElixir.Timers;
using UniRx;

namespace BlackRose
{
    [Serializable, RequireComponent(typeof(Rigidbody2D))]
    public abstract class GroundedUnit : UnitBase
    {
        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheck;           // 足元チェック用のTransform
        [SerializeField] private float _groundCheckRadius = 0.1f;  // チェック半径
        [SerializeField] private LayerMask _groundLayer;           // 地面Layer
        [SerializeField] private float _disableCheckTime = 0.2f; // 地面判定を無効にする
        private TimerTicket _ticket;
        public bool IsGrounded { get; private set; }

        private ReactiveCommand _onAirToGround = new();
        public IObservable<Unit> OnAirToGround => _onAirToGround; // 地面に着地したときのコールバック


        public virtual void AfterJump()
        {
            Timer.Start(_ticket, true, true);
        }
        private void GroundCheck()
        {
            if (!Timer.IsFinished(_ticket)) return;
            Collider2D hit = Physics2D.OverlapCircle(
                _groundCheck.position,
                _groundCheckRadius,
                _groundLayer
            );
            bool beforeGrounded = IsGrounded; // 前回の地面状態を保存
            bool grounded = hit != null;

            if (grounded)
            {
                if (!beforeGrounded && OnAirToGround != null)
                {
                    Debug.Log("GroundedUnit: OnAirToGround called");
                    OnAirToGound();
                    _onAirToGround?.Execute(); // 地面に着地したときのコールバックを呼び出す
                }
                IsGrounded = true;
                OnGrounded(); // 地面にいる場合の処理
            }
            else if (TryGetComponent<Rigidbody2D>(out var rb) && rb.velocity.y < 0)
            {
                OnFall(); // 落下中の処理
                IsGrounded = false;
                OnUnGrounded(); // 地面にいない場合の処理
            }
            else
            {
                IsGrounded = false;
                OnUnGrounded();
            }
        }
        /// <summary>
        /// 一定時間ごとに呼ばれ、着地している場合に呼ばれる
        /// </summary>
        protected virtual void OnGrounded() { }
        /// <summary>
        /// 一定時間ごとに呼ばれ、着地していない場合に呼ばれる
        /// </summary>
        protected virtual void OnUnGrounded() { }

        protected virtual void OnFall() { }
        protected virtual void OnAirToGound() { }
        protected override void AfterFixedUpdate()
        {
            GroundCheck();              // 毎フレーム地面判定＆コヨーテタイム更新
        }
        protected override void AfterAwake()
        {
            _ticket = Timer.CountDownRegister(_disableCheckTime, "DisableCheckTime", initZero:true);
        }
#if UNITY_EDITOR
        // デバッグ用にGizmos表示
        protected virtual void OnDrawGizmosSelected()
        {
            if (_groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
            }
        }
#endif
    }
}