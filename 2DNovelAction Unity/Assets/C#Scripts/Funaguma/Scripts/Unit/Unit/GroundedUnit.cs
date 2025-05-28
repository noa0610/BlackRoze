using UnityEngine;

namespace BlackRose
{
    public abstract class GroundedUnit : UnitBase
    {
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;           // 足元チェック用のTransform
        [SerializeField] private float groundCheckRadius = 0.1f;  // チェック半径
        [SerializeField] private LayerMask groundLayer;           // 地面Layer

        public bool IsGrounded { get; private set; }
        private void FixedUpdate()
        {
            GroundCheck();              // 毎フレーム地面判定＆コヨーテタイム更新
        }

        private void GroundCheck()
        {
            Collider2D hit = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer.value       // LayerMaskをIntに変換して渡す
            );

            bool grounded = hit != null;

            if (grounded)
            {
                IsGrounded = true;
                OnGrounded(); // 地面にいる場合の処理
            }
            else
            {
                IsGrounded = false;
                OnUnGrounded();
            }
        }

        protected abstract void OnGrounded();
        protected abstract void OnUnGrounded();
        // デバッグ用にGizmos表示
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}