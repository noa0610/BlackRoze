using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    [SerializeField]
    public class CustomMove : StateComp
    {
        [SerializeReference, SubclassSelector]
        private List<IMoveAssist> _assists = new List<IMoveAssist>();
        [SerializeField]
        private string _animationKey;

        public CustomMove(string animationKey)
        {
            _animationKey = animationKey;
        }
        public CustomMove() { }
        public void AddComp(IMoveAssist comp)
        {
            _assists.Add(comp);
        }
        public override void Enter(IState previousIState, IUnit parent)
        {
            parent.Animator.SetTrigger(_animationKey);
        }

        public override void Exit(IState nextIState, IUnit parent)
        {
            parent.Animator.ResetTrigger(_animationKey);
        }

        public override void Stay(IUnit parent)
        {
            float t = Time.deltaTime;
            foreach (var assist in _assists)
                assist.Go(Time.deltaTime);
        }
    }
}