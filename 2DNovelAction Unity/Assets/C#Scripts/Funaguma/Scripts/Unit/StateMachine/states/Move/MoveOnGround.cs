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
        private IStatusManager.StatusAmount _statusAmount;

        public MoveOnGround(Rigidbody2D rigidbody2D, string animationTrigger, IStatusManager.StatusAmount status)
        {
            _rigidbody2D = rigidbody2D;
            _animationTrigger = animationTrigger;
            _statusAmount = status;
        }

        public bool Enter(IState previousState, IUnit parent)
        {
            parent.Animator.SetTrigger(_animationTrigger);
            return true;
        }

        public bool Exit(IState nextState, IUnit parent)
        {
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