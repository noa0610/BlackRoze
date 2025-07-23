using UnityEngine;
using System;

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

        public bool IsGrounded { get; private set; }
        public Action OnAirToGround { get; set; } = null; // 地面に着地したときのコールバック
        private void FixedUpdate()
        {
            if (!_isPlaying) return; // ゲームが一時停止中は処理を行わない
            GroundCheck();              // 毎フレーム地面判定＆コヨーテタイム更新
        }

        private void GroundCheck()
        {
            if (_disableCheckTime > 0f)
            {
                _disableCheckTime -= Time.fixedDeltaTime; // 地面判定を無効にする時間を減らす
                return; // 無効な場合は地面チェックを行わない
            }
            Collider2D hit = Physics2D.OverlapCircle(
                _groundCheck.position,
                _groundCheckRadius,
                _groundLayer.value       // LayerMaskをIntに変換して渡す
            );
            bool beforeGrounded = IsGrounded; // 前回の地面状態を保存
            bool grounded = hit != null;

            if (grounded)
            {
                if (!beforeGrounded && OnAirToGround != null)
                {
                    Debug.Log("GroundedUnit: OnAirToGround called");
                    OnAirToGround?.Invoke(); // 地面に着地したときのコールバックを呼び出す
                }
                IsGrounded = true;
                OnGrounded(); // 地面にいる場合の処理
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
        protected abstract void OnGrounded();
        /// <summary>
        /// 一定時間ごとに呼ばれ、着地していない場合に呼ばれる
        /// </summary>
        protected abstract void OnUnGrounded();

        protected virtual void OnFall()
        {
        }

        // デバッグ用にGizmos表示
        private void OnDrawGizmosSelected()
        {
            if (_groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
            }
        }

        public void AfterJump()
        {
            _disableCheckTime = 0.2f; // ジャンプしたら地面判定を無効にする
        }
    }
}