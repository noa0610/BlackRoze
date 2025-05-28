using UnityEngine;

namespace BlackRose
{
    public class DashOnGround : IState
    {
        private IStatusManager.StatusAmount _dashSpeed;
        private Rigidbody2D _rigidbody2D;
        public DashOnGround(Rigidbody2D rigidbody2D, IStatusManager.StatusAmount dashSpeed)
        {
            _dashSpeed = dashSpeed;
            _rigidbody2D = rigidbody2D;
        }
        public bool Enter(IState previousState, IUnit parent)
        {
            return true;
        }
        public bool Stay(IUnit parent)
        {
            if (parent is not GroundedUnit grounded) return false;
            if (!grounded.IsGrounded) return true; // 地面にいない場合はDashを行わない
            // Dash中の処理
            Vector2 dashDirection = parent.Direction * _dashSpeed.ChangedMax;
            _rigidbody2D.velocity = dashDirection + _rigidbody2D.velocity * new Vector2(0,1);
            return true; // Dash中はStayを続ける
        }
        public bool Exit(IState nextState, IUnit parent)
        {
            // Dash終了時の処理（必要なら）
            return true; // 成功
        }
    }
}