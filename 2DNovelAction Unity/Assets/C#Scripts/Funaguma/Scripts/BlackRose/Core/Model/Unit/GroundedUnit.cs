using UnityEngine;
using System;
using BlackRose.Core.Models.Units;
using HighElixir.Timers;
using UniRx;

namespace BlackRose
{
    public enum GroundState
    {
        None,
        Landing,
        Falling,
        Rising,
    }
    [Serializable, RequireComponent(typeof(Rigidbody2D))]
    public abstract class GroundedUnit : UnitBase
    {
        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheck;           // 足元チェック用のTransform
        [SerializeField] private Vector3 _landingCheckOffset = new(0, 0, 0); // 着地判定を取る際のオフセット
        [SerializeField] private float _groundCheckRadius = 0.1f;  // チェック半径
        [SerializeField] private LayerMask _groundLayer;           // 地面Layer
        [SerializeField] private float _disableCheckTime = 0.2f; // 地面判定を無効にする
        private TimerTicket _ticket;
        public bool IsGrounded { get; private set; }
        public GroundState GroundState { get; private set; } = GroundState.Landing;
        private ReactiveCommand _onAirToGround = new();
        public IObservable<Unit> OnAirToGround => _onAirToGround; // 地面に着地したときのコールバック

        private bool IsFall => GroundState == GroundState.Falling;
        public virtual void AfterJump()
        {
            Timer.Start(_ticket, true, true);
        }
        private void GroundCheck()
        {
            if (!Timer.IsFinished(_ticket)) return;
            Collider2D hit = Physics2D.OverlapCircle(
                !IsFall ? _groundCheck.position : _groundCheck.position + _landingCheckOffset,
                _groundCheckRadius,
                _groundLayer
            );

            if (hit != null)
            {
                if (!IsGrounded && OnAirToGround != null)
                {
                    //Debug.Log("GroundedUnit: OnAirToGround called");
                    OnAirToGound();
                    _onAirToGround?.Execute(); // 地面に着地したときのコールバックを呼び出す
                    Timer.Start(_ticket, true, true);
                }
                IsGrounded = true;
                GroundState = GroundState.Landing;
                OnGrounded(); // 地面にいる場合の処理
                return;
            }
            if (Rigidbody2D.velocity.y < 0)
            {
                OnFall(); // 落下中の処理
                GroundState = GroundState.Falling;
            }
            else
                GroundState = GroundState.Rising;
            IsGrounded = false;
            OnUnGrounded();
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
            _ticket = Timer.CountDownRegister(_disableCheckTime, "DisableCheckTime", initZero: true);
        }
#if UNITY_EDITOR
        // デバッグ用にGizmos表示
        protected virtual void OnDrawGizmosSelected()
        {
            if (_groundCheck != null)
            {
                Gizmos.color = IsFall ? Color.red : Color.green;
                Gizmos.DrawWireSphere(!IsFall ? _groundCheck.position : _groundCheck.position + _landingCheckOffset, _groundCheckRadius);
            }
        }
#endif
    }
}