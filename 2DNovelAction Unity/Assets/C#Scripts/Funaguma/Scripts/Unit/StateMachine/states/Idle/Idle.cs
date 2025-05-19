using UnityEngine;

namespace BlackRose
{
    // =======================
    // IdleÅië“ã@ÅjèÛë‘
    // =======================
    public class Idle : IState
    {
        private string _animationKey;
        public Idle(string animationKey = null)
        {
        }
        public bool Enter(IState previousState, IUnit parent)
        {
            parent.Animator.SetTrigger(_animationKey);
            return true;
        }

        public bool Exit(IState nextState, IUnit parent)
        {
            return true;
        }

        public bool Stay(IUnit parent)
        {
            return true;
        }
    }
}
