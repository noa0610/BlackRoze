using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    public class CustomMove : IState
    {
        private List<IMoveAssist> _assists = new List<IMoveAssist>();
        private string _animationKey;

        public CustomMove(string animationKey)
        {
            _animationKey = animationKey;
        }
        public void AddComp(IMoveAssist comp)
        {
            _assists.Add(comp);
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
            float t = Time.deltaTime;
            foreach (var assist in _assists)
                assist.Go(Time.deltaTime);
            return true;
        }
    }
}