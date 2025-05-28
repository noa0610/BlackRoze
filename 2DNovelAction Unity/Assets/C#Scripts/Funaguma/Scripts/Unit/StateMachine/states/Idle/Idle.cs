namespace BlackRose
{
    // =======================
    // Idle（待機）状態
    // =======================
    public class Idle : IState
    {
        private string _animationKey;
        public Idle(string animationKey = null)
        {
        }
        public virtual bool Enter(IState previousState, IUnit parent)
        {
            parent.Animator.SetTrigger(_animationKey);
            return true;
        }

        public virtual bool Exit(IState nextState, IUnit parent)
        {
            return true;
        }

        public virtual bool Stay(IUnit parent)
        {
            return true;
        }
    }
}
