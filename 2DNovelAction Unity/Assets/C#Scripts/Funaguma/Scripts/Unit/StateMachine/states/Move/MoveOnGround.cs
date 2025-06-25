using UnityEngine;

namespace BlackRose
{
    // =======================
    // Move（移動）状態
    // =======================
    public class MoveOnGround : IState
    {
        private Rigidbody2D _rigidbody2D;
        private string _animationTrigger;
        private StatusAmount _statusAmount;
        private bool _inex = false; // inExitStopの代わりに使用するフラグ

        public MoveOnGround(Rigidbody2D rigidbody2D, string animationTrigger, StatusAmount status, bool inExitStop = false)
        {
            _rigidbody2D = rigidbody2D;
            _animationTrigger = animationTrigger;
            _statusAmount = status;
            _inex = inExitStop; // inExitStopの代わりに使用するフラグを設定
        }

        public bool Enter(IState previousState, IUnit parent)
        {
            parent.Animator.SetTrigger(_animationTrigger);
            return true;
        }

        public bool Exit(IState nextState, IUnit parent)
        {
            if (_inex)
                _rigidbody2D.velocity = Vector2.zero; // inExitStopの代わりに使用するフラグがtrueなら速度をゼロにする
            return true;
        }

        public bool Stay(IUnit parent)
        {
            var s = _statusAmount.ChangedMax;
            _rigidbody2D.velocity = new Vector2(s * parent.Direction.x, _rigidbody2D.velocity.y);
            return true;
        }
    }
}