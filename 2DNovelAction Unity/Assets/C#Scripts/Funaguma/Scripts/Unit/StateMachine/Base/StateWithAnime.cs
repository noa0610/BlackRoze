using UnityEngine;
namespace BlackRose
{
    public class StateWithAnime : StateComp
    {
        [SerializeField] protected string _animeTriggerName;
        [SerializeField] protected bool _waitForAnimeEnd = false;

        public override void Enter(IState previousIState, UnitBase parent)
        {
            parent.Animator.SetTrigger(_animeTriggerName);
        }
        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (!_waitForAnimeEnd) return true;
            return parent.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
        }
    }
}