using System.Linq;
using UnityEngine;

namespace BlackRose.Core.Models.States.Animators
{
    public sealed class AnimatorAnimationDriver : IAnimationDriver
    {
        private readonly Animator _animator;
        public string CurrentLayer { get; set; } = "Default";

        public AnimatorAnimationDriver(Animator animator, string defaultLayer = "Default")
        {
            _animator = animator;
            CurrentLayer = defaultLayer;
        }

        public void OnSetState(string stateKey)
        {
        }

        public void OnTransition(string fromState, string toState, string animationTrigger)
        {
            if (HasTrigger(animationTrigger))
            {
                _animator.ResetTrigger(animationTrigger);
                _animator.SetTrigger(animationTrigger);
            }
        }

        private bool HasTrigger(string animeTrigger)
        {
            if (string.IsNullOrEmpty(animeTrigger))
            {
                return false;
            }
            return _animator
                .parameters
                .Any(p =>
                p.type == AnimatorControllerParameterType.Trigger &&
                p.name == animeTrigger);
        }
    }
}
