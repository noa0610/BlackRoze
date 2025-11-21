using BlackRose.Core.Models.Units;
using HighElixir;
using UnityEngine;

namespace BlackRose.Core.Models.Systems
{
    public sealed class PlayerPauser : SingletonBehavior<PlayerPauser>, IPlayerFollower
    {
        private UnitBase _target;
        public void SetTarget(UnitBase target)
        {
            _target = target;
        }

        public void Pause()
        {
            _target?.Pause();
            _target.Rigidbody2D.velocity = Vector2.zero;
        }
        public void Play()
        {
            _target?.Play();
        }
    }
}